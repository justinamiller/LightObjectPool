using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LightObjectPool
{
    /// <summary>
    /// Base class providing code re-use among multiple pool implementations.
    /// </summary>
   
     public abstract class PoolBase<T> : IPool<T> where T:class
    {
        private readonly IPoolPolicy<T> _poolPolicy;
        private readonly bool _isPooledTypeDisposable;
        private bool _isDisposed;


        protected PoolBase(IPoolPolicy<T> poolPolicy)
        {
            _poolPolicy = poolPolicy ?? throw new ArgumentNullException(nameof(poolPolicy));
            _isPooledTypeDisposable =   typeof(IDisposable).IsAssignableFrom(typeof(T)); 
        }

        /// <summary>
        /// Disposes this pool and all contained objects (if they are disposable).
        /// </summary>
        public bool IsDisposed
        {
            get
            {
                return _isDisposed;
            }
        }

#pragma warning disable CA1063 // Implement IDisposable Correctly
                              /// <summary>
                              /// Disposes this pool and all contained objects (if they are disposable).
        public void Dispose()
#pragma warning restore CA1063 // Implement IDisposable Correctly
        {
            if (_isDisposed) return;

            try
            {
                _isDisposed = true;

                Dispose(true);
            }
            finally
            {
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Performs dispose logic, can be overridden by derivded types.
        /// </summary>
 
        protected abstract void Dispose(bool disposing);

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        protected void CheckDisposed()
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);
        }


        /// <summary>
        /// Provides access to the <see cref="IPoolPolicy"/> passed in the constructor.
        /// </summary>
        protected IPoolPolicy<T> PoolPolicy
        {
            get
            {
                return _poolPolicy;
            }
        }

        /// <summary>
        /// Returns a boolean indicating if {T} is disposale.
        /// </summary>
        protected bool IsPooledTypeDisposable
        {
            get
            {
                return _isPooledTypeDisposable;
            }
        }


        /// <summary>
        /// Disposes <paramref name="pooledObject"/> if it is not null and supports <see cref="IDisposable"/>, otherwise does nothing. If <paramref name="pooledObject"/> is actually a <see cref="PooledObject{T}"/> instance, then disposes the <see cref="PooledObject{T}.Value"/> property instead.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "object")]
        protected void SafeDispose(object pooledObject)
        {
            (pooledObject as IDisposable)?.Dispose();
        }

        /// <summary>
        /// Abstract method for adding or returning an instance to the pool.
        /// </summary>

        public abstract bool Return(T value);

        /// <summary>
        /// Abstract method for retrieving an item from the pool. 
        /// </summary>

        public abstract T Get();

        /// <summary>
        /// Abstract method for retrieving an PooledObject from the pool. 
        /// </summary>

        public abstract PooledObject<T> GetPooledObject();

    }
}
