using Alkami.Contracts;
using HACK26.MS.MyMoneyRules.Contracts.Requests;
using HACK26.MS.MyMoneyRules.Data.Gemini;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HACK26.MS.MyMoneyRules.Service.Tests
{
    [TestFixture]
    public class ServiceTests
    {
        /// <summary>
        /// This will hold the instance of the service implementation we're going to test
        /// </summary>
        private readonly ServiceImp _testClass;

        public ServiceTests()
        {
            var providerName = "HACK26.MS.MyMoneyRules";
            var providerType = "HACK26";
            _testClass = new ServiceImp(providerType, providerName);
        }
        private void AugmentBaseRequest(BaseRequest request)
        {
            request.BankIdentifier = Guid.Parse("78554577-9DE6-43CD-9085-5868977156D1"); //developer dynamic
            request.BankUri = "developer.dev.alkamitech.com";
            request.UserIdentifier = Guid.Parse("3061E6D7-87F9-4EEB-A9BB-E42DB629D4CD"); //mike.brady
        }

        [SetUp]
        public void Setup()
        {
            /*
			 ...
			 do other injections here
			 ...
			 */
        }

        [Test]
        public async Task CanGetGeminiStatusWhenNotConfigured()
        {
            var request = new GetGeminiStatusRequest();
            AugmentBaseRequest(request);

            var result = await _testClass.GetGeminiStatusAsync(request);

            Assert.IsFalse(result.Configured);
            Assert.AreEqual(GeminiDefaults.DefaultModel, result.Model);
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.Message));
        }

        [Test]
        public async Task CanGetData()
        {
            var request = new GetSomethingRequest();
            AugmentBaseRequest(request);

            // Result should have two default settings
            var result = await _testClass.GetDataAsync(request);

            // Each should have a name containing these partial strings
            var hasAnotherIntProperty = result.ItemList.Any(x => x.AnotherPropertyThatsAnInt == 9999);
            var hasItemwitChildObjects = result.ItemList.Any(x => x.ChildrenObjects.Count == 2);

            Assert.IsTrue(result.ItemList.Count == 1);
            Assert.IsTrue(hasAnotherIntProperty);
            Assert.IsTrue(hasItemwitChildObjects);
        }
    }
}