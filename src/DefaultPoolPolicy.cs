using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectPool
{
    /// <summary>
    /// Provides configuration controlling how an object pool works.
    /// Will call empty constructor for creation of object.
    /// </summary>
    /// <typeparam name="T">The type of item being pooled.</typeparam>
    /// <seealso cref="PooledItemInitialization"/>
    /// <seealso cref="ObjectPool.Pool{T}"/>
    public class DefaultPoolPolicy<T> : IPoolPolicy<T> where T : class, new()
    {
        private readonly Action<T> _reinitializeObject = null;

        public int MaximumPoolSize { get; }

        public PooledItemInitialization InitializationPolicy { get; }
        
        
        public DefaultPoolPolicy(Action<T> reinitializeObject=null, int maxPoolSize = 10, PooledItemInitialization initializePolicy = PooledItemInitialization.Return)
        {
            _reinitializeObject = reinitializeObject;
            MaximumPoolSize = maxPoolSize > 0 ? maxPoolSize : Environment.ProcessorCount * 2;
            this.InitializationPolicy = initializePolicy;
            
        }


        public T Create(IPool<T> pool)
        {
            return new T();
        }

        public void Reinitialize(T obj)
        {
            _reinitializeObject?.Invoke(obj);
        }
    }
}
