using System;
using System.Threading.Tasks;
using Xunit;

using GooglePay.PaymentDataCryptography;

namespace GooglePay.PaymentDataCryptography.Tests
{
    public class PassCallbackValidatorTest
    {
        private const string ValidKeyJson =
            "{\"keys\":[{\"keyValue\":\"MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEPYnHwS8uegWAewQtlxizmLFynwHcxRT1PK07cDA6/C4sXrVI1SzZCUx8U8S0LjMrT6ird/VW7be3Mz6t/srtRQ==\",\"protocolVersion\":\"ECv1\"}]}";

        // Structurally valid payment data that will parse correctly but fail signature verification
        private const string PaymentDataJson =
            "{\"protocolVersion\":\"ECv1\"," +
            "\"signedMessage\":\"{\\\"tag\\\":\\\"ZVwlJt7dU8Plk0+r8rPF8DmPTvDiOA1UAoNjDV+SqDE\\\\u003d\\\"," +
            "\\\"ephemeralPublicKey\\\":\\\"BPhVspn70Zj2Kkgu9t8+ApEuUWsI/zos5whGCQBlgOkuYagOis7qsrcbQrcprjvTZO3XOU+Qbcc28FSgsRtcgQE\\\\u003d\\\"," +
            "\\\"encryptedMessage\\\":\\\"12jUObueVTdy\\\"}\"," +
            "\"signature\":\"MEQCIDxBoUCoFRGReLdZ/cABlSSRIKoOEFoU3e27c14vMZtfAiBtX3pGMEpnw6mSAbnagCCgHlCk3NcFwWYEyxIE6KGZVA\\u003d\\u003d\"}";

        [Fact]
        public void ParameterlessConstructor()
        {
            var validator = new PassCallbackValidator();
            Assert.NotNull(validator);
        }

        [Fact]
        public void NullOptions_Constructs()
        {
            var validator = new PassCallbackValidator((GoogleKeyProviderOptions)null);
            Assert.NotNull(validator);
        }

        [Fact]
        public void WithOptions_UsesGooglePassesUrl()
        {
            var handler = new MockHttpMessageHandler(ValidKeyJson);
            var validator = new PassCallbackValidator(new GoogleKeyProviderOptions
            {
                MessageHandler = handler
            });

            // Verify triggers key fetch; signature won't match so SecurityException is expected
            Assert.Throws<SecurityException>(() =>
                validator.Verify("someRecipient", PaymentDataJson));

            Assert.Equal("https://pay.google.com/gp/m/issuer/keys", handler.RequestUri.ToString());
        }

        [Fact]
        public void WithOptions_IgnoresUrlAndIsTestProperties()
        {
            var handler = new MockHttpMessageHandler(ValidKeyJson);
            var validator = new PassCallbackValidator(new GoogleKeyProviderOptions
            {
                IsTest = true,
                Url = "https://should-be-ignored.example.com/keys.json",
                MessageHandler = handler
            });

            Assert.Throws<SecurityException>(() =>
                validator.Verify("someRecipient", PaymentDataJson));

            // Should still use Google Passes URL, not the one from options
            Assert.Equal("https://pay.google.com/gp/m/issuer/keys", handler.RequestUri.ToString());
        }

        [Fact]
        public void WithOptions_ForwardsMessageHandler()
        {
            var handler = new MockHttpMessageHandler(ValidKeyJson);
            var validator = new PassCallbackValidator(new GoogleKeyProviderOptions
            {
                MessageHandler = handler
            });

            Assert.Throws<SecurityException>(() =>
                validator.Verify("someRecipient", PaymentDataJson));

            Assert.Equal(1, handler.RequestCount);
        }
    }
}
