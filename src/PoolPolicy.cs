using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace LightObjectPool
{
    /// <summary>
    /// Provides configuration controlling how an object pool works.
    /// </summary>
    public class PoolPolicy<T>:IPoolPolicy<T>
    {
        /// <summary>
        /// A function that returns a new item for the pool. Used when the pool is empty and a new item is requested.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        private readonly Func<IPool<T>, T> _factory;

        /// <summary>
        /// An action that re-initialises a pooled object (cleans up/resets object state) before it is reused by callers.
        /// </summary>
        private readonly Action<T> _reinitializeObject;
        /// <summary>
        /// Determines the maximum number of items allowed in the pool. A value of zero indicates no explicit limit.
        /// </summary>
        public int MaximumPoolSize { get; }

        public PoolPolicy(Func<IPool<T>, T> factory, Action<T> reinitializeObject = null, int maxPoolSize = 10)
        {
            if (0 >= maxPoolSize)
            {
                throw new ArgumentException($"{ nameof(maxPoolSize)} must be greater than 0");
            }
            this._factory = factory ?? throw new NullReferenceException(nameof(factory));

            this.MaximumPoolSize = maxPoolSize;
            this._reinitializeObject = reinitializeObject;
        }

        /// <summary>
        /// A function that returns a new item for the pool. Used when the pool is empty and a new item is requested.
        /// </summary>
        public T Create(IPool<T> pool)
        {
            return this._factory(pool);
        }

        public void Reinitialize(T obj)
        {
            _reinitializeObject?.Invoke(obj);
        }
    }
}
