using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightObjectPool
{
    public interface IPoolPolicy<T>
    {
        /// <summary>
        /// Determines the maximum number of items allowed in the pool. A value of zero indicates no explicit limit.
        /// </summary>
         int MaximumPoolSize { get; }

        /// <summary>
        /// A function that returns a new item for the pool. Used when the pool is empty and a new item is requested.
        /// </summary>
        T Create(IPool<T> pool);

        /// <summary>
        /// An action that re-initialises a pooled object (cleans up/resets object state) before it is reused by callers.
        /// </summary>
        void Reinitialize(T obj);
    }
}
