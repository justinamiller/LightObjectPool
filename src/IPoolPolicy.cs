using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectPool
{
    public interface IPoolPolicy<T>
    {
        /// <summary>
        /// Determines the maximum number of items allowed in the pool. A value of zero indicates no explicit limit.
        /// </summary>
        /// <remarks>
        /// <para>This restricts the number of instances stored in the pool at any given time, it does not represent the maximum number of items that may be generated or exist in memory at any given time. If the pool is empty and a new item is requested, a new instance will be created even if pool was previously full and all it's instances have been taken already.</para>
        /// </remarks>
         int MaximumPoolSize { get; }

        /// <summary>
        /// A value from the <see cref="PooledItemInitialization"/> enum specifying when and how pooled items are re-initialised.
        /// </summary>
        PooledItemInitialization InitializationPolicy { get; }

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
