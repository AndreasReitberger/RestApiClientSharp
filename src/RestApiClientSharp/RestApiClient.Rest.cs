using AndreasReitberger.API.REST.Enums;
using AndreasReitberger.API.REST.Events;
using AndreasReitberger.API.REST.Interfaces;
using AndreasReitberger.API.REST.Structs;
using Newtonsoft.Json;
using RestSharp;
using System.Collections.Generic;
#if DEBUG
using System.Diagnostics;
#endif
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AndreasReitberger.API.REST
{
    public partial class RestApiClient : ObservableObject, IRestApiClient
    {
        #region Properties

        [ObservableProperty]
        [XmlIgnore]
        public partial Dictionary<string, IAuthenticationHeader> AuthHeaders { get; set; } = [];

        #endregion

        #region Methods

        #region ValidateResult

        protected virtual bool GetQueryResult(string? result, bool emptyResultIsValid = false)
        {
            try
            {
                if ((string.IsNullOrEmpty(result) || result == "{}") && emptyResultIsValid)
                    return true;
                return GetObjectFromJson<QueryActionResult>(result)?.Ok ?? false;
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
                    apiRsponeResult.RawBytes = respone.RawBytes;
                    apiRsponeResult.Succeeded = true;
                    apiRsponeResult.EventArgs = new RestEventArgs()
                    {
                        Status = respone.ResponseStatus.ToString(),
                        Exception = respone.ErrorException,
                        Message = respone.ErrorMessage,
                        Uri = targetUri,
                    };
                    apiRsponeResult.Cookies = respone.Cookies;
                }
                else if (respone.StatusCode == HttpStatusCode.NonAuthoritativeInformation
                    || respone.StatusCode == HttpStatusCode.Forbidden
                    || respone.StatusCode == HttpStatusCode.Unauthorized
                    )
                {
                    apiRsponeResult.IsOnline = true;
                    apiRsponeResult.HasAuthenticationError = true;
                    apiRsponeResult.Cookies = respone.Cookies;
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
                    apiRsponeResult.Cookies = respone.Cookies;
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
                        Message = respone.Content ?? respone.ErrorMessage,
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
            object? body = null,
            RestBodyTarget target = RestBodyTarget.Json,
            CancellationTokenSource? cts = default,
            Dictionary<string, string>? urlSegments = null
            )
        {
            RestApiRequestRespone apiRsponeResult = new() { IsOnline = IsOnline };
            try
            {
                cts ??= new(TimeSpan.FromSeconds(DefaultTimeout));
                requestTargetUri ??= string.Empty;
                command ??= string.Empty;
                if (RestClient == null)
                {
                    UpdateRestClientInstance();
                }
                RestRequest request = new(!string.IsNullOrEmpty(command) ? $"{requestTargetUri}/{command}" : requestTargetUri)
                {
                    Method = method
                };
                if (body is not null)
                {
                    switch (target)
                    {
                        /*
                        case RestBodyTarget.File:
                            if (body is byte[] fileBytes)
                                request.AddFile("file", fileBytes, "", ContentType.Binary, new FileParameterOptions() {  });
                            break;
                        */
                        case RestBodyTarget.Plain:
                            request.RequestFormat = DataFormat.None;
                            if (body is string plainText)
                                request.AddStringBody(plainText, DataFormat.None);
                            break;
                        case RestBodyTarget.Xml:
                            request.RequestFormat = DataFormat.Xml;
                            if (body is string xml)
                                request.AddStringBody(xml, RestContentType.Xml);
                            else
                                request.AddXmlBody(body);
                            break;
                        case RestBodyTarget.Binary:
                            if (body is byte[] bytes)
                            {
                                request.AddBody(bytes, RestContentType.StreamOctet);
                            }
                            break;
                        case RestBodyTarget.Json:
                        default:
                            request.RequestFormat = DataFormat.Json;
                            if (body is string bodyContent)
                            {
                                request.AddJsonBody(bodyContent, RestContentType.Json);
                            }
                            else
                            {
                                bodyContent = JsonConvert.SerializeObject(body, DefaultNewtonsoftJsonSerializerSettings);
                                request.AddJsonBody(bodyContent, RestContentType.Json);
                            }
                            break;
                    }
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
                        if (authHeader.Value is null) continue;
                        switch (authHeader.Value.Target)
                        {
                            case AuthenticationHeaderTarget.UrlSegment:
                                request.AddParameter(authHeader.Key, authHeader.Value.Token, ParameterType.QueryString);
                                break;
                            case AuthenticationHeaderTarget.Header:
                            default:
                                // Examples:
                                // "X-Api-Key", $"{apiKey}"
                                // "Authorization", $"Bearer {apiKey}"
                                request.AddHeader(authHeader.Key, authHeader.Value.Token);
                                break;
                        }
                    }
                }
                Uri? fullUri = RestClient?.BuildUri(request);
#if NET6_0_OR_GREATER
                ArgumentNullException.ThrowIfNull(fullUri, nameof(fullUri));
#endif
#if DEBUG
                Debug.WriteLine($"REST-Request: Uri = '{fullUri}'");
#endif
                try
                {
                    if (RestClient is not null)
                    {
                        RestResponse? respone = await RestClient.ExecuteAsync(request, cts.Token).ConfigureAwait(false);
                        if (ValidateResponse(respone, fullUri) is RestApiRequestRespone res)
                        {
                            apiRsponeResult = res;
                            if (!apiRsponeResult.Succeeded && apiRsponeResult.EventArgs is not null)
                            {
                                OnRestApiError(new()
                                {
                                    Exception = apiRsponeResult.EventArgs.Exception,
                                    Message = apiRsponeResult.EventArgs.Message,
                                    Status = apiRsponeResult.EventArgs.Status,
                                    Uri = apiRsponeResult.EventArgs.Uri,
                                });
                            }
                        }
                    }
                }
                catch (TaskCanceledException texp)
                {
                    OnTaskCanceled(new()
                    {
                        Message = texp.Message,
                        Uri = fullUri,
                        Source = nameof(CheckOnlineAsync),
                        CancelationRequested = cts?.IsCancellationRequested ?? false,
                        Exception = texp
                    });
                    // Throws exception on timeout, not actually an error but indicates if the server is reachable.
                    if (!IsOnline)
                    {
                        OnError(new UnhandledExceptionEventArgs(texp, false));
                        apiRsponeResult.Exception = texp;
                    }
                }
                catch (HttpRequestException hexp)
                {
                    OnRestApiError(new()
                    {
                        Exception = hexp,
                        Message = hexp.Message,
#if NET6_0_OR_GREATER
                        Status = hexp.StatusCode.ToString(),
#endif
                        Uri = fullUri,
                    });
                    apiRsponeResult.Exception = hexp;                  
                }
                catch (TimeoutException toexp)
                {
                    // Throws exception on timeout, not actually an error but indicates if the server is not reachable.
                    if (!IsOnline)
                    {
                        OnError(new UnhandledExceptionEventArgs(toexp, false));
                        apiRsponeResult.Exception = toexp;
                    }
                }
                cts?.Dispose();
                cts = null;
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
                apiRsponeResult.Exception = exc;
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
            int timeout = 100
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
                CancellationTokenSource cts = new(TimeSpan.FromSeconds(timeout));
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
                Uri? fullUrl = RestClient?.BuildUri(request);
#if DEBUG
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
                    apiRsponeResult.Exception = texp;
                    OnTaskCanceled(new()
                    {
                        Message = texp.Message,
                        Uri = fullUrl,
                        Source = nameof(CheckOnlineAsync),
                        CancelationRequested = cts?.IsCancellationRequested ?? false,
                        Exception = texp
                    });
                    // Throws exception on timeout, not actually an error but indicates if the server is reachable.
                    if (!IsOnline)
                    {
                        OnError(new UnhandledExceptionEventArgs(texp, false));
                        //apiRsponeResult.Exception = texp;
                    }
                }
                catch (HttpRequestException hexp)
                {
                    // Throws exception on timeout, not actually an error but indicates if the server is not reachable.
                    if (!IsOnline)
                    {
                        OnError(new UnhandledExceptionEventArgs(hexp, false));
                        apiRsponeResult.Exception = hexp;
                    }
                }
                catch (TimeoutException toexp)
                {
                    // Throws exception on timeout, not actually an error but indicates if the server is not reachable.
                    if (!IsOnline)
                    {
                        OnError(new UnhandledExceptionEventArgs(toexp, false));
                        apiRsponeResult.Exception = toexp;
                    }
                }
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
                apiRsponeResult.Exception = exc;
            }
            return apiRsponeResult;
        }
        #endregion

        #region Download
        public virtual async Task<byte[]?> DownloadFileFromUriAsync(
            string path,
            Dictionary<string, IAuthenticationHeader> authHeaders,
            Dictionary<string, string>? headers = null,
            Dictionary<string, string>? urlSegments = null,
            int timeout = 10
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
                            case AuthenticationHeaderTarget.UrlSegment:
                                request.AddParameter(authHeader.Key, authHeader.Value.Token, ParameterType.QueryString);
                                break;
                            case AuthenticationHeaderTarget.Header:
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
                request.Timeout = TimeSpan.FromSeconds(timeout);
                if (headers?.Count > 0)
                {
                    foreach (KeyValuePair<string, string> segment in headers)
                    {
                        request.AddHeader(segment.Key, segment.Value);
                    }
                }
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
                CancellationTokenSource cts = new(TimeSpan.FromSeconds(timeout));
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

        #endregion

    }
}
