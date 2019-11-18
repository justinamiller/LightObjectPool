using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;

namespace ObjectPool
{
    /// <summary>
    /// Base class providing code re-use among multiple pool implementations. Should not be used directly by calling code, instead use <see cref="IPool{T}"/> for references.
    /// </summary>
    /// <typeparam name="T">The type of value being pooled.</typeparam>
    public abstract class PoolBase<T> : IPool<T> where T:class
    {

        #region Fields

        private readonly IPoolPolicy<T> _poolPolicy;
        private readonly bool _isPooledTypeDisposable;
        private bool _isDisposed;

        #endregion

        #region Constructors

        /// <summary>
        /// Full constructor.
        /// </summary>
        /// <param name="poolPolicy">A <seealso cref="PoolPolicy{T}"/> instance containing configuration information for the pool.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="poolPolicy"/> argument is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if the <see cref="PoolPolicy{T}._factory"/> property of the <paramref name="poolPolicy"/> argument is null.</exception>
        protected PoolBase(IPoolPolicy<T> poolPolicy)
        {
            _poolPolicy = poolPolicy ?? throw new ArgumentNullException(nameof(poolPolicy));
            _isPooledTypeDisposable =   typeof(IDisposable).IsAssignableFrom(typeof(T)); 
        }

        #endregion

        #region IDisposable & Related Implementation 

        /// <summary>
        /// Disposes this pool and all contained objects (if they are disposable).
        /// </summary>
        /// <remarks>
        /// <para>A pool can only be disposed once, calling this method multiple times will have no effect after the first invocation.</para>
        /// </remarks>
        /// <seealso cref="IsDisposed"/>
        /// <seealso cref="Dispose(bool)"/>
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
                              /// </summary>
                              /// <remarks>
                              /// <para>A pool can only be disposed once, calling this method multiple times will have no effect after the first invocation.</para>
                              /// </remarks>
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
        /// <param name="disposing">True if the pool is being explicitly disposed, false if it is being disposed from a finalizer.</param>
        /// <seealso cref="Dispose()"/>
        /// <seealso cref="IsDisposed"/>
        protected abstract void Dispose(bool disposing);

        /// <summary>
        /// Throws a <see cref="ObjectDisposedException"/> if the <see cref="Dispose()"/> method has been called.
        /// </summary>
        protected void CheckDisposed()
        {
            if (_isDisposed) throw new ObjectDisposedException(this.GetType().FullName);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Provides access to the <see cref="IPoolPolicy"/> passed in the constructor.
        /// </summary>
        /// <seealso cref="PoolBase{T}"/>
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

        #endregion

        #region Public Methods

        /// <summary>
        /// Disposes <paramref name="pooledObject"/> if it is not null and supports <see cref="IDisposable"/>, otherwise does nothing. If <paramref name="pooledObject"/> is actually a <see cref="PooledObject{T}"/> instance, then disposes the <see cref="PooledObject{T}.Value"/> property instead.
        /// </summary>
        /// <param name="pooledObject"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "object")]
        protected void SafeDispose(object pooledObject)
        {
            (pooledObject as IDisposable)?.Dispose();
        }

        #endregion

        #region Abstract IPool<T> Members

        /// <summary>
        /// Abstract method for adding or returning an instance to the pool.
        /// </summary>
        /// <param name="value">The instance to add or return to the pool.</param>
        public abstract bool Return(T value);

        /// <summary>
        /// Abstract method for retrieving an item from the pool. 
        /// </summary>
        /// <returns>An instance of {T}.</returns>
        public abstract T Get();

        /// <summary>
        /// Abstract method for retrieving an PooledObject from the pool. 
        /// </summary>
        /// <returns>An instance of {T}.</returns>
        public abstract PooledObject<T> GetPooledObject();

        #endregion
    }
}
