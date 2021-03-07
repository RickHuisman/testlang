using Newtonsoft.Json;
using NUnit.Framework;

namespace UnitTest
{
    public static class TestHelper
    {
        public static void AreEqualByJson(object expected, object actual)
        {
            var expectedJson = JsonConvert.SerializeObject(expected);
            var actualJson = JsonConvert.SerializeObject(actual);
            Assert.AreEqual(expectedJson, actualJson);
        }

        public static string AsJson(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}