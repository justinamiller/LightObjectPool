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
                pool.Return(sb);
                pool.Return(sb);
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

        [TestMethod]
        public void TestOversubscribe()
        {
            var pool = LightObjectPool.Pool.Create<System.IO.MemoryStream>(null, 1);
            var a = pool.Get();
            var b = pool.Get();
            var c = pool.Get();

            pool.Return(a);
            Assert.IsTrue(a.CanRead);
            pool.Return(b);
            Assert.IsFalse(b.CanRead);
            pool.Return(c);
            Assert.IsFalse(c.CanRead);
        }

        [TestMethod]
        public void TestThreading()
        {
            var pool = LightObjectPool.Pool.Create<StringBuilder>((s) => s.Clear(), 10);

            var t1 = System.Threading.Tasks.Task.Run(() =>
            {
                System.Threading.Tasks.Parallel.For(1, 10000, (i) =>
                {
                    var sb = pool.Get();
                    Assert.IsTrue(sb.Length == 0);
                    sb.Append("this is a test message");
                    sb.AppendLine("this is a test message");
                    pool.Return(sb);
                });
            });

            var t2 = System.Threading.Tasks.Task.Run(() =>
            {
                System.Threading.Tasks.Parallel.For(1, 10000, (i) =>
                {
                    var sb = pool.Get();
                    Assert.IsTrue(sb.Length == 0);
                    sb.Append("testing");
                    pool.Return(sb);
                });
            });

            System.Threading.Tasks.Task.WaitAll(t1, t2);
        }

        [TestMethod]
        public void TestReturnNull()
        {
            var pool = LightObjectPool.Pool.Create<StringBuilder>(null, 1);
            try
            {
                pool.Return(null);
                Assert.Fail("Should failed");
            }
            catch (Exception)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TestDisposed()
        {
            var pool = LightObjectPool.Pool.Create<System.IO.MemoryStream>(null, 1);
            var sb = pool.Get();
            var sb1 = pool.Get();
            var sb2 = pool.Get();
            var sb3 = pool.Get();
            Assert.IsFalse(pool.IsDisposed);
            pool.Return(sb);
            pool.Return(sb1);
            pool.Return(sb2);
            pool.Dispose();
            Assert.IsTrue(pool.IsDisposed);
            try
            {
                sb = pool.Get();
                Assert.Fail("Should failed");
            }
            catch (Exception)
            {
                Assert.IsTrue(true);
            }

            pool.Return(sb3);
            Assert.IsFalse(sb3.CanRead);
        }
    }
}
