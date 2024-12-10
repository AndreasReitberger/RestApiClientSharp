#if !NETFRAMEWORK
using System;
#if DEBUG
using System.Diagnostics;
#endif
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace AndreasReitberger.API.REST.Utilities
{
    /// <summary>
    /// Limits the API calls to not violate the rate limits from LexOffice.
    /// Docs: https://developers.lexoffice.io/docs/#api-rate-limits
    /// Bases on: https://devblogs.microsoft.com/dotnet/announcing-rate-limiting-for-dotnet/
    /// </summary>
    /// <param name="limiter"></param>
    public class RateLimitedHandler(RateLimiter limiter) : DelegatingHandler(new HttpClientHandler())
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            using RateLimitLease lease = await limiter.AcquireAsync(1, cancellationToken);
            if (lease.IsAcquired)
            {
                return await base.SendAsync(request, cancellationToken);
            }
            HttpResponseMessage response = new(System.Net.HttpStatusCode.TooManyRequests);
            if (lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
            {
                response.Headers.Add(HeaderNames.RetryAfter, ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo));
            }
#if DEBUG
            Debug.WriteLine($"Response: {response.StatusCode}");
#endif
            return response;
        }
    }
}
#endif