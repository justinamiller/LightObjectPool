using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ObjectPool
{
    /// <summary>
    /// Provides configuration controlling how an object pool works.
    /// </summary>
    /// <typeparam name="T">The type of item being pooled.</typeparam>
    /// <seealso cref="PooledItemInitialization"/>
    /// <seealso cref="ObjectPool.Pool{T}"/>
    public class PoolPolicy<T>:IPoolPolicy<T>
    {
        /// <summary>
        /// A function that returns a new item for the pool. Used when the pool is empty and a new item is requested.
        /// </summary>
        /// <remarks>
        /// <para>Should return a new, clean item, ready for use by the caller. Takes a single argument being a reference to the pool that was asked for the object, useful if you're creating <see cref="PooledObject{T}"/> instances.</para>
        /// <para>Cannot be null. If null when provided to a <see cref="ObjectPool.Pool{T}"/> instance, an exception will be thrown.</para>
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        private readonly Func<IPool<T>, T> _factory;

        /// <summary>
        /// An action that re-initialises a pooled object (cleans up/resets object state) before it is reused by callers.
        /// </summary>
        /// <remarks>
        /// <para>Can be null if no re-initialisation is required, or if the client is expected to perform it's own initialisation.</para>
        /// <para>This action is not performed against newly created items, it is a *re*-initialisation, not an initialisation action.</para>
        /// <para>The action proided may be called from a background thread. If the pooled object has thread affinity, invoking to the appropriate thread may be required within the action itself.</para>
        /// </remarks>
        private readonly Action<T> _reinitializeObject;
        /// <summary>
        /// Determines the maximum number of items allowed in the pool. A value of zero indicates no explicit limit.
        /// </summary>
        /// <remarks>
        /// <para>This restricts the number of instances stored in the pool at any given time, it does not represent the maximum number of items that may be generated or exist in memory at any given time. If the pool is empty and a new item is requested, a new instance will be created even if pool was previously full and all it's instances have been taken already.</para>
        /// </remarks>
        public int MaximumPoolSize { get; }
        /// <summary>
        /// A value from the <see cref="PooledItemInitialization"/> enum specifying when and how pooled items are re-initialised.
        /// </summary>
        public PooledItemInitialization InitializationPolicy { get; }

        public PoolPolicy(Func<IPool<T>, T> factory, Action<T> reinitializeObject = null, int maxPoolSize = 10, PooledItemInitialization initializePolicy = PooledItemInitialization.Return)
        {
            if (0 >= maxPoolSize)
            {
                throw new ArgumentException($"{ nameof(maxPoolSize)} must be greater than 0");
            }
            this._factory = factory ?? throw new NullReferenceException(nameof(factory));

            this.MaximumPoolSize = maxPoolSize;
            this.InitializationPolicy = initializePolicy;
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
