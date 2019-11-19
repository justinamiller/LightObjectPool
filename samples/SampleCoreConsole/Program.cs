using LightObjectPool;
using System;
using System.Text;

namespace SampleConsole
{
    class Program
    {


        public static IPool<StringBuilder> Pool { get; private set; }
        static void Main(string[] args)
        {

            //   var p = Microsoft.Extensions.ObjectPool.ObjectPool.Create<System.Threading.Tasks.Task>();
            var poolPolicy1 = new PoolPolicy<System.IO.MemoryStream>((poolInstance) => new System.IO.MemoryStream(), (jw) => jw.Position=0, 10);
            var pool1 = new Pool<System.IO.MemoryStream>(poolPolicy1);


            var poolPolicy = new PoolPolicy<StringBuilder>((poolInstance) => new StringBuilder(), (jw) => jw.Clear(), 10);
            //{
            //    Factory = (poolInstance) => new StringBuilder(),
            //    InitializationPolicy = PooledItemInitialization.Return,
            //    MaximumPoolSize = 10,
            //    ReinitializeObject = (jw) => jw.Clear()
            //};

            Pool = new Pool<StringBuilder>(poolPolicy);

     var sb=       Pool.Get();
            var sb1 = Pool.Get();

            var sb2 = Pool.Get();
            var sb3= Pool.Get();
            Pool.Return(sb1);
            var sb4 = Pool.Get();

            for(var i = 0; i < 100; i++)
            {
                var z = Pool.Get();
                if (z.Length != 0)
                {
                    throw new Exception();
                }
                z.Append("TEsting 12345");
                Pool.Return(z);
            }

        }
    }
}
