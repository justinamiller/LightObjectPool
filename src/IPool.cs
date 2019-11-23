using System;
using System.Collections.Generic;
using System.Text;

namespace LightObjectPool
{
    public interface IPool<T> : IDisposable
    {
        /// <summary>
        /// Gets an item from the pool.
        /// </summary>
     
        T Get();

        /// <summary>
        /// Gets an <see cref="PooledObject{T}"/> from the pool.
        /// </summary>

        PooledObject<T> GetPooledObject();

        /// <summary>
        /// Returns/adds an object to the pool so it can be reused.
        /// </summary>
 
        bool Return(T value);

        /// <summary>
        /// Returns true of the <see cref="IDisposable.Dispose"/> method has been called on this instance.
        /// </summary>
        bool IsDisposed { get; }
    }
}
