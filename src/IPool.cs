using System;
using System.Collections.Generic;
using System.Text;

namespace LightObjectPool
{
    /// <summary>
    /// Interface for a simple object pool.
    /// </summary>
    /// <typeparam name="T">The type of value being pooled.</typeparam>
    /// <seealso cref="LightObjectPool.Pool{T}"/>
    /// <seealso cref="PooledObject{T}"/>
    /// <seealso cref="PoolPolicy{T}"/>
    public interface IPool<T> : IDisposable
    {
        /// <summary>
        /// Gets an item from the pool.
        /// </summary>
        /// <remarks>
        /// <para>If the pool is empty when the request is made, a new item is instantiated and returned.</para>
        /// </remarks>
        /// <returns>Returns an instance of {T} from the pool, or a new instance if the pool is empty.</returns>
        T Get();

        /// <summary>
        /// Gets an <see cref="PooledObject{T}"/> from the pool.
        /// </summary>
        /// <remarks>
        /// <para>If the pool is empty when the request is made, a new item is instantiated and returned.</para>
        /// </remarks>
        /// <returns>Returns an wrapper of <see cref="PooledObject{T}"/> with an instance of {T} from the pool, or a new instance if the pool is empty.</returns>
        PooledObject<T> GetPooledObject();

        /// <summary>
        /// Returns/adds an object to the pool so it can be reused.
        /// </summary>
        /// <param name="value"></param>
        /// <remarks>
        /// <para>Items will be returned to the pool if it is not full and the item is not already in the pool, otherwise no action is taken and no error is reported.</para>
        /// <para>If the policy for the pool specifies <see cref="PooledItemInitialization.AsyncReturn"/> the item will be queued for re-intialisation on a background thread before being returned to the pool, control will return to the caller once the item has been queued even if it has not yet been fully re-initialised and returned to the pool.</para>
        /// <para>If the item is NOT returned to the pool, and {T} implements <see cref="System.IDisposable"/>, the instance will be disposed before the method returns.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="value"/> is null.</exception>
        bool Return(T value);

        /// <summary>
        /// Returns true of the <see cref="IDisposable.Dispose"/> method has been called on this instance.
        /// </summary>
        bool IsDisposed { get; }
    }
}
