using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

using GooglePay.PaymentDataCryptography;

namespace GooglePay.PaymentDataCryptography.Tests
{
    public class GoogleKeyProviderTest
    {
        private const string ValidKeyJson =
            "{\"keys\":[{\"keyValue\":\"MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEPYnHwS8uegWAewQtlxizmLFynwHcxRT1PK07cDA6/C4sXrVI1SzZCUx8U8S0LjMrT6ird/VW7be3Mz6t/srtRQ==\",\"protocolVersion\":\"ECv1\"}]}";

        [Fact]
        public void NullOptions_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new GoogleKeyProvider((GoogleKeyProviderOptions)null));
        }

        [Fact]
        public async Task DefaultOptions_UsesProductionUrl()
        {
            var handler = new MockHttpMessageHandler(ValidKeyJson);
            var provider = new GoogleKeyProvider(new GoogleKeyProviderOptions
            {
                MessageHandler = handler
            });

            await provider.GetPublicKeys("ECv1");

            Assert.Equal(
                "https://payments.developers.google.com/paymentmethodtoken/keys.json",
                handler.RequestUri.ToString());
        }

        [Fact]
        public async Task IsTestTrue_UsesTestUrl()
        {
            var handler = new MockHttpMessageHandler(ValidKeyJson);
            var provider = new GoogleKeyProvider(new GoogleKeyProviderOptions
            {
                IsTest = true,
                MessageHandler = handler
            });

            await provider.GetPublicKeys("ECv1");

            Assert.Equal(
                "https://payments.developers.google.com/paymentmethodtoken/test/keys.json",
                handler.RequestUri.ToString());
        }

        [Fact]
        public async Task CustomUrl_OverridesIsTest()
        {
            var handler = new MockHttpMessageHandler(ValidKeyJson);
            var provider = new GoogleKeyProvider(new GoogleKeyProviderOptions
            {
                IsTest = true,
                Url = "https://custom.example.com/keys.json",
                MessageHandler = handler
            });

            await provider.GetPublicKeys("ECv1");

            Assert.Equal("https://custom.example.com/keys.json", handler.RequestUri.ToString());
        }

        [Fact]
        public async Task CustomMessageHandler_IsUsedForRequests()
        {
            var handler = new MockHttpMessageHandler(ValidKeyJson);
            var provider = new GoogleKeyProvider(new GoogleKeyProviderOptions
            {
                MessageHandler = handler
            });

            await provider.GetPublicKeys("ECv1");

            Assert.Equal(1, handler.RequestCount);
        }

        [Fact]
        public async Task CustomMessageHandler_ReturnsKeys()
        {
            var handler = new MockHttpMessageHandler(ValidKeyJson);
            var provider = new GoogleKeyProvider(new GoogleKeyProviderOptions
            {
                MessageHandler = handler
            });

            var keys = await provider.GetPublicKeys("ECv1");

            Assert.NotNull(keys);
            Assert.Single(keys);
            Assert.Equal(
                "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEPYnHwS8uegWAewQtlxizmLFynwHcxRT1PK07cDA6/C4sXrVI1SzZCUx8U8S0LjMrT6ird/VW7be3Mz6t/srtRQ==",
                keys.First());
        }

        [Fact]
        public async Task UnknownProtocolVersion_ReturnsNull()
        {
            var handler = new MockHttpMessageHandler(ValidKeyJson);
            var provider = new GoogleKeyProvider(new GoogleKeyProviderOptions
            {
                MessageHandler = handler
            });

            var keys = await provider.GetPublicKeys("ECv99");

            Assert.Null(keys);
        }

        [Fact]
        public async Task CacheDurationSet_ServerMaxAgeIgnored()
        {
            var handler = new MockHttpMessageHandler(ValidKeyJson, maxAge: TimeSpan.Zero);
            var provider = new GoogleKeyProvider(new GoogleKeyProviderOptions
            {
                CacheDuration = TimeSpan.FromDays(30),
                MessageHandler = handler
            });

            await provider.GetPublicKeys("ECv1");
            Assert.Equal(1, handler.RequestCount);

            // Server said max-age=0 but we set 30 days — should not re-fetch
            await provider.GetPublicKeys("ECv1");
            Assert.Equal(1, handler.RequestCount);
        }

        [Fact]
        public async Task NoCacheDuration_ServerMaxAgeUsed()
        {
            var handler = new MockHttpMessageHandler(ValidKeyJson, maxAge: TimeSpan.Zero);
            var provider = new GoogleKeyProvider(new GoogleKeyProviderOptions
            {
                MessageHandler = handler
            });

            await provider.GetPublicKeys("ECv1");
            Assert.Equal(1, handler.RequestCount);

            // Server said max-age=0, no CacheDuration override — should re-fetch
            await provider.GetPublicKeys("ECv1");
            Assert.Equal(2, handler.RequestCount);
        }

        [Fact]
        public async Task NoCacheControlHeader_DefaultSevenDaysUsed()
        {
            var handler = new MockHttpMessageHandler(ValidKeyJson, maxAge: null);
            var provider = new GoogleKeyProvider(new GoogleKeyProviderOptions
            {
                MessageHandler = handler
            });

            await provider.GetPublicKeys("ECv1");
            Assert.Equal(1, handler.RequestCount);

            // No server header, default is 7 days — should use cache
            await provider.GetPublicKeys("ECv1");
            Assert.Equal(1, handler.RequestCount);
        }

        [Fact]
        public async Task HttpClientReused_AcrossMultipleFetches()
        {
            var handler = new MockHttpMessageHandler(ValidKeyJson, maxAge: TimeSpan.Zero);
            var provider = new GoogleKeyProvider(new GoogleKeyProviderOptions
            {
                MessageHandler = handler
            });

            await provider.GetPublicKeys("ECv1");
            await provider.GetPublicKeys("ECv1");
            await provider.GetPublicKeys("ECv1");

            // All 3 fetches went through the same handler instance
            Assert.Equal(3, handler.RequestCount);
        }

        [Fact]
        public async Task PrefetchKeys_FetchesViaHandler()
        {
            var handler = new MockHttpMessageHandler(ValidKeyJson);
            var provider = new GoogleKeyProvider(new GoogleKeyProviderOptions
            {
                MessageHandler = handler
            });

            await provider.PrefetchKeys();
            Assert.Equal(1, handler.RequestCount);

            // GetPublicKeys should use cached keys, no re-fetch
            var keys = await provider.GetPublicKeys("ECv1");
            Assert.Equal(1, handler.RequestCount);
            Assert.NotNull(keys);
        }

        [Fact]
        public void BackwardCompat_ParameterlessConstructor()
        {
            var provider = new GoogleKeyProvider();
            Assert.NotNull(provider);
        }

        [Fact]
        public void BackwardCompat_BoolConstructor()
        {
            var provider = new GoogleKeyProvider(isTest: true);
            Assert.NotNull(provider);
        }
    }
}
