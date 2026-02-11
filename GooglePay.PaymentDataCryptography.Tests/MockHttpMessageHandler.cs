using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GooglePay.PaymentDataCryptography.Tests
{
    internal class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _responseContent;
        private readonly TimeSpan? _maxAge;

        public Uri RequestUri { get; private set; }
        public int RequestCount { get; private set; }

        public MockHttpMessageHandler(string responseContent, TimeSpan? maxAge = null)
        {
            _responseContent = responseContent;
            _maxAge = maxAge;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            RequestCount++;

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_responseContent, Encoding.UTF8, "application/json")
            };

            if (_maxAge.HasValue)
            {
                response.Headers.CacheControl = new CacheControlHeaderValue
                {
                    MaxAge = _maxAge.Value
                };
            }

            return Task.FromResult(response);
        }
    }
}
