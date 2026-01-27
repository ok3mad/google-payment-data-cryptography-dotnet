using Xunit;

using GooglePay.PaymentDataCryptography;

namespace GooglePay.PaymentDataCryptography.Tests
{
    public class GoogleKeyProviderOptionsTest
    {
        [Fact]
        public void DefaultValues()
        {
            var options = new GoogleKeyProviderOptions();

            Assert.False(options.IsTest);
            Assert.Null(options.Url);
            Assert.Null(options.CacheDuration);
            Assert.Null(options.MessageHandler);
        }
    }
}
