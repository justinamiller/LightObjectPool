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

            //var poolPolicyTest = new PoolPolicy<StringBuilder>((p) => new PooledObject<StringBuilder>(p, new StringBuilder()), (jw) => jw.Clear(), 10, PooledItemInitialization.Return);
            //var poolTest = new Pool<PooledObject<StringBuilder>>(poolPolicyTest);
            //Retrieve an instance from the pool
    //        using (var pooledItem =Pool.Get())
    //{
    //            //pooledItem.Value is the object you actually want.
    //            //If the pool is for tyhe type PooledObject<System.Text.StringBuilder> then
    //            //you can access the string builder instance like this;
    //            pooledItem.Value.Append("Some text to add to the builder");

    //        } // The item will automatically be returned to the pool here.

        }
    }
}
