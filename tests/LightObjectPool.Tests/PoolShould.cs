using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;

namespace LightObjectPool.Tests
{
    [TestClass]
    public class PoolShould
    {
        [TestMethod]
        public void TestPool()
        {
            var pool = LightObjectPool.Pool.Create<StringBuilder>((s) => s.Clear(), 10);
           var sb=pool.Get();
            Assert.IsNotNull(sb);
           pool.Return(sb);
            sb = pool.Get();
            Assert.IsNotNull(sb);
            sb = pool.Get();

            for(var i = 0; i < 100; i++)
            {
                sb = pool.Get();
                Assert.IsNotNull(sb);
            }

            pool = LightObjectPool.Pool.Create<StringBuilder>();

            for (var i = 0; i < 100; i++)
            {
                sb = pool.Get();
                Assert.IsNotNull(sb);
                pool.Return(sb);
                Assert.IsNotNull(sb);
            }
        }

        [TestMethod]
        public void TestPooledObject()
        {
            var pool = LightObjectPool.Pool.Create<StringBuilder>((s) => s.Clear(), 10);

            for (var i = 0; i < 100; i++)
            {
                using (var item = pool.GetPooledObject())
                {
                    Assert.IsNotNull(item);
                    Assert.IsNotNull(item.Value);

                    Assert.IsTrue(item.Value.Length == 0);
                    item.Value.Append("Testing");
                    Assert.IsTrue(item.Value.Length > 0);
                }
            }
        }
    }
}
