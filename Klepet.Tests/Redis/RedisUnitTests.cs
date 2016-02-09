using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackExchange.Redis;

namespace Klepet.Tests.Redis
{
    [TestClass]
    public class RedisUnitTests
    {
        private static ConnectionMultiplexer _connectionMultiplexer;

        [ClassInitialize]
        public static void UnitTestsInit(TestContext testContext)
        {
            _connectionMultiplexer = ConnectionMultiplexer.Connect(new ConfigurationOptions() {EndPoints = { "127.0.0.1"},AbortOnConnectFail = false});
        }

        [TestMethod]
        public void RedisTest()
        {
            var db = _connectionMultiplexer.GetDatabase();
            const string k = "1";
            const string v = "Hello";
            db.KeyDelete(k);
            Assert.IsTrue(db.StringSet(k, v), "Set failed");
            var vg = db.StringGet(k);
            Assert.IsTrue(v == vg, $"Values mismatch {v}:{vg}");
        }

        [TestMethod]
        public void RedisPubSubTest()
        {
            const string name = "push";
            const string v = "Hello";
            var reset = new AutoResetEvent(false);
            var sub = _connectionMultiplexer.GetSubscriber();
            sub.Subscribe(name, (c, m) =>
            {
                Assert.IsTrue(v == m, $"Values mismatch {v}:{m}");
                reset.Set();
            });
            sub.Publish(name, v);
            reset.WaitOne();
        }

        [TestMethod]
        public void RedisListTest()
        {
            var db = _connectionMultiplexer.GetDatabase();
            const string k = "myList";
            var values = new[] {"a","b","123"};
            db.KeyDelete(k);
            for (var i = 1; i <= values.Length; i++)
            {
                Assert.AreEqual(i,db.ListRightPush(k,values[i-1]));
            }
            foreach (var value in values)
            {
                Assert.AreEqual(value, db.ListLeftPop(k).ToString());
            }
        }

        [ClassCleanup]
        public static void UnitTestsClean()
        {
            _connectionMultiplexer?.Close();
        }
    }
}
