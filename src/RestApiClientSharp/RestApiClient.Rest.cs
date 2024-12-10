using AndreasReitberger.API.REST.Events;
using AndreasReitberger.API.REST.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AndreasReitberger.API.REST
{
    // Documentation: https://finnhub.io/docs/api
    public partial class RestApiClient : ObservableObject, IRestApiClient
    {
        #region Properties

        [ObservableProperty]
        [property: XmlIgnore]
        Dictionary<string, IAuthenticationHeader> authHeaders = [];

        #endregion

        #region Methods

        #region ValidateResult

        protected virtual bool GetQueryResult(string? result, bool emptyResultIsValid = false)
        {
            try
            {
                if ((string.IsNullOrEmpty(result) || result == "{}") && emptyResultIsValid)
                    return true;
                IQueryActionResult? actionResult = GetObjectFromJson<QueryActionResult>(result);
                return actionResult?.Ok ?? false;
            }
            catch (JsonException jecx)
            {
                OnError(new JsonConvertEventArgs()
                {
                    Exception = jecx,
                    OriginalString = result,
                    Message = jecx.Message,
                });
                return false;
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
                return false;
            }
        }

        protected virtual IRestApiRequestRespone ValidateResponse(RestResponse? respone, Uri? targetUri)
        {
            RestApiRequestRespone apiRsponeResult = new() { IsOnline = IsOnline };
            try
            {
                if (respone is null) return apiRsponeResult;
                if ((
                    respone.StatusCode == HttpStatusCode.OK || respone.StatusCode == HttpStatusCode.NoContent ||
                    respone.StatusCode == HttpStatusCode.Created || respone.StatusCode == HttpStatusCode.Accepted
                    ) && respone.ResponseStatus == ResponseStatus.Completed
                    )
                {
                    apiRsponeResult.IsOnline = true;
                    AuthenticationFailed = false;
                    apiRsponeResult.Result = respone.Content;
                    apiRsponeResult.Succeeded = true;
                    apiRsponeResult.EventArgs = new RestEventArgs()
                    {
                        Status = respone.ResponseStatus.ToString(),
                        Exception = respone.ErrorException,
                        Message = respone.ErrorMessage,
                        Uri = targetUri,
                    };
                }
                else if (respone.StatusCode == HttpStatusCode.NonAuthoritativeInformation
                    || respone.StatusCode == HttpStatusCode.Forbidden
                    || respone.StatusCode == HttpStatusCode.Unauthorized
                    )
                {
                    apiRsponeResult.IsOnline = true;
                    apiRsponeResult.HasAuthenticationError = true;
                    apiRsponeResult.EventArgs = new RestEventArgs()
                    {
                        Status = respone.ResponseStatus.ToString(),
                        Exception = respone.ErrorException,
                        Message = respone.ErrorMessage,
                        Uri = targetUri,
                    };
                }
                else if (respone.StatusCode == HttpStatusCode.Conflict)
                {
                    apiRsponeResult.IsOnline = true;
                    apiRsponeResult.HasAuthenticationError = false;
                    apiRsponeResult.EventArgs = new RestEventArgs()
                    {
                        Status = respone.ResponseStatus.ToString(),
                        Exception = respone.ErrorException,
                        Message = respone.ErrorMessage,
                        Uri = targetUri,
                    };
                }
                else
                {
                    OnRestApiError(new RestEventArgs()
                    {
                        Status = respone.ResponseStatus.ToString(),
                        Exception = respone.ErrorException,
                        Message = respone.ErrorMessage,
                        Uri = targetUri,
                    });
                }
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
            }
            return apiRsponeResult;
        }
        #endregion

        #region Rest Api
        public virtual async Task<IRestApiRequestRespone?> SendRestApiRequestAsync(
            string? requestTargetUri,
            Method method,
            string? command,
            Dictionary<string, IAuthenticationHeader> authHeaders,
            object? jsonObject = null,
            CancellationTokenSource? cts = default,
            Dictionary<string, string>? urlSegments = null
            )
        {
            RestApiRequestRespone apiRsponeResult = new() { IsOnline = IsOnline };
            try
            {
                cts ??= new(DefaultTimeout);
                requestTargetUri ??= string.Empty;
                command ??= string.Empty;
                if (RestClient == null)
                {
                    UpdateRestClientInstance();
                }
                RestRequest request = new(!string.IsNullOrEmpty(command) ? $"{requestTargetUri}/{command}" : requestTargetUri)
                {
                    RequestFormat = DataFormat.Json,
                    Method = method
                };
                if (jsonObject is not null)
                {
                    request.AddJsonBody(jsonObject, "application/json");
                }
                if (urlSegments is not null)
                {
                    foreach (KeyValuePair<string, string> pair in urlSegments)
                    {
                        request.AddParameter(pair.Key, pair.Value, ParameterType.QueryString);
                    }
                }
                if (authHeaders?.Count > 0)
                {
                    foreach (KeyValuePair<string, IAuthenticationHeader> authHeader in authHeaders)
                    {
                        switch (authHeader.Value.Target)
                        {
                            case Enums.AuthenticationHeaderTarget.UrlSegment:
                                request.AddParameter(authHeader.Key, authHeader.Value.Token, ParameterType.QueryString);
                                break;
                            case Enums.AuthenticationHeaderTarget.Header:
                            default:
                                // Examples:
                                // "X-Api-Key", $"{apiKey}"
                                // "Authorization", $"Bearer {apiKey}"
                                request.AddHeader(authHeader.Key, authHeader.Value.Token);
                                break;
                        }
                    }
                }
#if DEBUG
                Uri? fullUri = RestClient?.BuildUri(request);
                Debug.WriteLine($"REST-Request: Uri = '{fullUri}'");
#endif
                try
                {
                    if (RestClient is not null)
                    {
                        RestResponse? respone = await RestClient.ExecuteAsync(request, cts.Token).ConfigureAwait(false);
#if DEBUG
                        Debug.WriteLine($"REST: Result = '{(respone?.IsSuccessful is true ? "successfully" : "failed")} (Code: {respone?.StatusCode})'\n{respone?.Content}");
#endif
                        if (ValidateResponse(respone, fullUri) is RestApiRequestRespone res)
                            apiRsponeResult = res;
                    }
                }
                catch (TaskCanceledException texp)
                {
                    // Throws exception on timeout, not actually an error but indicates if the server is reachable.
                    if (!IsOnline)
                    {
                        OnError(new UnhandledExceptionEventArgs(texp, false));
                    }
                }
                catch (HttpRequestException hexp)
                {
                    // Throws exception on timeout, not actually an error but indicates if the server is not reachable.
                    if (!IsOnline)
                    {
                        OnError(new UnhandledExceptionEventArgs(hexp, false));
                    }
                }
                catch (TimeoutException toexp)
                {
                    // Throws exception on timeout, not actually an error but indicates if the server is not reachable.
                    if (!IsOnline)
                    {
                        OnError(new UnhandledExceptionEventArgs(toexp, false));
                    }
                }
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
            }
            return apiRsponeResult;
        }

        public virtual async Task<IRestApiRequestRespone?> SendMultipartFormDataFileRestApiRequestAsync(
            string requestTargetUri,
            Dictionary<string, IAuthenticationHeader> authHeaders,
            string? fileName = null,
            byte[]? file = null,
            Dictionary<string, string>? parameters = null,
            string? localFilePath = null,
            string contentType = "multipart/form-data",
            string fileTargetName = "file",
            string fileContentType = "application/octet-stream",
            int timeout = 100000
            )
        {
            RestApiRequestRespone apiRsponeResult = new();
            if (!IsOnline) return apiRsponeResult;

            try
            {
                // If there is no file specified
                if (file is null && localFilePath is null)
                    // and there are no additional parameters supplied, throw
                    if (parameters?.Count == 0)
                        throw new ArgumentNullException(
                            $"{nameof(file)} / {nameof(localFilePath)} / {nameof(parameters)}",
                            $"No file, localFilePath and paramaters have been provided! Set at least one of those three parameters!");
                if (RestClient is null)
                {
                    UpdateRestClientInstance();
                }
                CancellationTokenSource cts = new(new TimeSpan(0, 0, 0, 0, timeout));
                RestRequest request = new(requestTargetUri);

                if (authHeaders?.Count > 0)
                {
                    foreach (KeyValuePair<string, IAuthenticationHeader> authHeader in authHeaders)
                    {
                        switch (authHeader.Value.Target)
                        {
                            case Enums.AuthenticationHeaderTarget.UrlSegment:
                                request.AddParameter(authHeader.Key, authHeader.Value.Token, ParameterType.QueryString);
                                break;
                            case Enums.AuthenticationHeaderTarget.Header:
                            default:
                                // Examples:
                                // "X-Api-Key", $"{apiKey}"
                                // "Authorization", $"Bearer {apiKey}"
                                request.AddHeader(authHeader.Key, authHeader.Value.Token);
                                break;
                        }
                    }
                }

                request.RequestFormat = DataFormat.Json;
                request.Method = Method.Post;
                request.AlwaysMultipartFormData = true;

                //Multiform
                request.AddHeader("Content-Type", contentType ?? "multipart/form-data");
                if (file is not null && !string.IsNullOrEmpty(fileName))
                {
                    request.AddFile(fileTargetName ?? "file", file, fileName, fileContentType ?? "application/octet-stream");
                }
                else if (localFilePath is not null)
                {
                    request.AddFile(fileTargetName ?? "file", localFilePath, fileContentType ?? "application/octet-stream");
                }

                if (parameters?.Count > 0)
                {
                    foreach (KeyValuePair<string, string> para in parameters)
                    {
                        request.AddParameter(para.Key, para.Value, ParameterType.GetOrPost);
                    }
                }
#if DEBUG
                Uri? fullUrl = RestClient?.BuildUri(request);
                Debug.WriteLine($"REST-Request: Uri = '{fullUrl}'");
#endif
                try
                {
                    if (RestClient is not null)
                    {
                        RestResponse respone = await RestClient.ExecuteAsync(request, cts.Token);
                        if (ValidateResponse(respone, fullUrl) is RestApiRequestRespone res)
                            apiRsponeResult = res;
                    }
                }
                catch (TaskCanceledException texp)
                {
                    // Throws exception on timeout, not actually an error but indicates if the server is reachable.
                    if (!IsOnline)
                    {
                        OnError(new UnhandledExceptionEventArgs(texp, false));
                    }
                }
                catch (HttpRequestException hexp)
                {
                    // Throws exception on timeout, not actually an error but indicates if the server is not reachable.
                    if (!IsOnline)
                    {
                        OnError(new UnhandledExceptionEventArgs(hexp, false));
                    }
                }
                catch (TimeoutException toexp)
                {
                    // Throws exception on timeout, not actually an error but indicates if the server is not reachable.
                    if (!IsOnline)
                    {
                        OnError(new UnhandledExceptionEventArgs(toexp, false));
                    }
                }
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
            }
            return apiRsponeResult;
        }
        #endregion

        #region Download
        public virtual async Task<byte[]?> DownloadFileFromUriAsync(
            string path,
            Dictionary<string, IAuthenticationHeader> authHeaders,
            Dictionary<string, string>? urlSegments = null,
            int timeout = 10000
            )
        {
            try
            {
                if (RestClient is null)
                {
                    UpdateRestClientInstance();
                }
                RestRequest request = new(path);
                if (authHeaders?.Count > 0)
                {
                    foreach (KeyValuePair<string, IAuthenticationHeader> authHeader in authHeaders)
                    {
                        switch (authHeader.Value.Target)
                        {
                            case Enums.AuthenticationHeaderTarget.UrlSegment:
                                request.AddParameter(authHeader.Key, authHeader.Value.Token, ParameterType.QueryString);
                                break;
                            case Enums.AuthenticationHeaderTarget.Header:
                            default:
                                // Examples:
                                // "X-Api-Key", $"{apiKey}"
                                // "Authorization", $"Bearer {apiKey}"
                                request.AddHeader(authHeader.Key, authHeader.Value.Token);
                                break;
                        }
                    }
                }

                request.RequestFormat = DataFormat.Json;
                request.Method = Method.Get;
                request.Timeout = TimeSpan.FromMilliseconds(timeout);
                if (urlSegments?.Count > 0)
                {
                    foreach (KeyValuePair<string, string> segment in urlSegments)
                    {
                        request.AddParameter(segment.Key, segment.Value, ParameterType.QueryString);
                    }
                }
#if DEBUG
                Uri? fullUrl = RestClient?.BuildUri(request);
                Debug.WriteLine($"REST-Request: Uri = '{fullUrl}'");
#endif
                CancellationTokenSource cts = new(timeout);
                if (RestClient is not null)
                {
                    byte[]? respone = await RestClient.DownloadDataAsync(request, cts.Token)
                        .ConfigureAwait(false);
                    return respone;
                }
                else return null;
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
                return null;
            }
        }
        #endregion

        [Obsolete("Use the functions returning `IRestApiRequestRespone`")]
        public async Task<T?> SendRestApiRequestAsync<T>(
            string command, 
            Method method = Method.Get, 
            string body = "", 
            Dictionary<string, string>? headers = null,
            Dictionary<string, string>? urlSegments = null,
            CancellationTokenSource? cts = default) where T : class
        {
            if (cts == default)
            {
                cts = new(DefaultTimeout);
            }
            if (RestClient is null)
            {
                UpdateRestClientInstance();
            }

            RestRequest request = new(command, method)
            {
                RequestFormat = DataFormat.Json
            };
            if (headers is not null)
            {
                foreach (KeyValuePair<string, string> item in headers)
                {
                    request.AddHeader(item.Key, item.Value);
                }
            }
            if (urlSegments != null)
            {
                foreach (KeyValuePair<string, string> pair in urlSegments)
                {
                    request.AddParameter(pair.Key, pair.Value, ParameterType.QueryString);
                }
            }
            /*
            else if (!string.IsNullOrEmpty(AccessToken))
                request.AddHeader("Authorization", $"Bearer {AccessToken}");
            */
            if (!string.IsNullOrEmpty(body))
            {
                request.AddJsonBody(body);
            }
            if (RestClient is not null)
            {
                Uri? fullUri = RestClient?.BuildUri(request);
                RestResponse? response = await RestClient.ExecuteAsync(request, cts.Token).ConfigureAwait(false);
                if ((response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created) &&
                    response.ResponseStatus == ResponseStatus.Completed)
                {
                    if (typeof(T) == typeof(byte[]))
                    {
                        return response.RawBytes as T;
                    }
                    else if (typeof(T) == typeof(string))
                    {
                        return response.Content as T;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Unsupported return type: {typeof(T).Name}");
                    }
                }
                else
                {
                    string errorMessage = $"Request failed with status code {(int)response.StatusCode} ({response.StatusCode}).";

                    if (!string.IsNullOrEmpty(response.Content))
                    {
                        errorMessage += $" Response content: {response.Content}";
                    }
                    throw new HttpRequestException(errorMessage);
                }
            }
            return default;
        }

        #endregion

    }
}
