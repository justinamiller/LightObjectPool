using LightObjectPool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleFrameworkConsole
{
    class Program
    {

        public static IPool<StringBuilder> Pool { get; private set; }
        static void Main(string[] args)
        {
            System.Threading.Thread.Sleep(100);
            //   var p = Microsoft.Extensions.ObjectPool.ObjectPool.Create<System.Threading.Tasks.Task>();
            var poolPolicy1 = new PoolPolicy<System.IO.MemoryStream>((poolInstance) => new System.IO.MemoryStream(), (jw) => jw.Position = 0, 10);
            var pool1 = new Pool<System.IO.MemoryStream>(poolPolicy1);


            var poolPolicy = new PoolPolicy<StringBuilder>((poolInstance) => new StringBuilder(), (jw) => jw.Clear(), 10);
            //{
            //    Factory = (poolInstance) => new StringBuilder(),
            //    InitializationPolicy = PooledItemInitialization.Return,
            //    MaximumPoolSize = 10,
            //    ReinitializeObject = (jw) => jw.Clear()
            //};

            Pool = new Pool<StringBuilder>(poolPolicy);

            var test = LightObjectPool.Pool.Create<StringBuilder>((s) => s.Clear(), 10);

            var sb = Pool.Get();
            Pool.Return(sb);
            sb.Append("Test");
            Pool.Return(sb);
            var sb1 = Pool.Get();
            sb1.Append("HELLO");
            var sb2 = Pool.Get();
            sb2.Append("Test");
            Pool.Return(sb1);
            Pool.Return(sb2);
            var sb3 = Pool.Get();
            Pool.Return(sb1);
            var sb4 = Pool.Get();

            for (var i = 0; i < 100; i++)
            {
                var z = Pool.Get();
                if (z.Length != 0)
                {
                    throw new Exception();
                }
                z.Append("TEsting 12345");
                Pool.Return(z);
            }

           using(var item = Pool.GetPooledObject())
            {
                item.Value.Append("Testl");
            }


            ThreadingTest();
        }

       private static void ThreadingTest()
        {
            var pool = LightObjectPool.Pool.Create<StringBuilder>((s) => s.Clear(), 10);

            var t1 = System.Threading.Tasks.Task.Run(() =>
            {
                System.Threading.Tasks.Parallel.For(1, 10000, (i) =>
                {
                    var sb = pool.Get();
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
                    sb.Append("testing");
                    pool.Return(sb);
                });
            });

            System.Threading.Tasks.Task.WaitAll(t1, t2);
        }
    }
}
