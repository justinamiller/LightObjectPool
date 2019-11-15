using ObjectPool;
using System;
using System.Text;

namespace SampleConsole
{
    class Program
    {
        public static IPool<StringBuilder> Pool { get; private set; }
        static void Main(string[] args)
        {

            var poolPolicy = new PoolPolicy<StringBuilder>()
            {
                Factory = (poolInstance) => new StringBuilder(),
                InitializationPolicy = PooledItemInitialization.Return,
                MaximumPoolSize = 10,
                ReinitializeObject = (jw) => jw.Clear()
            };

            Pool = new Pool<StringBuilder>(poolPolicy);

     var sb=       Pool.Get();
        }
    }
}
