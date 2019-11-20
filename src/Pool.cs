using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LightObjectPool
{
    public static class Pool
    {
        public static Pool<T> Create<T>(IPoolPolicy<T> policy = null) where T : class, new()
        {
            return new Pool<T>(policy ?? new DefaultPoolPolicy<T>());
        }


        public static Pool<T> Create<T>(Action<T> reinitializeObject, int maximumPoolSize) where T : class, new()
        {
            return new Pool<T>(new DefaultPoolPolicy<T>(reinitializeObject, maximumPoolSize));
        }
        /// <summary>
        /// Creates StringBuilder with reset object state on return.
        /// </summary>
        /// <returns></returns>
        public static IPool<StringBuilder> CreateStringBuilderPool()
        {
            return Pool.Create<StringBuilder>((sb) => sb.Clear(), Environment.ProcessorCount * 2);
        }
    }


    /// <summary>
    /// A non-blocking object pool optimised for situations involving heavily concurrent access.
    /// </summary>
    /// <remarks>
    /// <para>This pool does not block when a new item is requested and the pool is empty, instead a new will be allocated and returned.</para>
    /// <para>By default the pool starts empty and items are allocated as needed. The <see cref="Expand()"/> method can be used to pre-load the pool if required.</para>
    /// <para>Objects returned to the pool are added on a first come first serve basis. If the pool is full when an object is returned, it is ignored (and will be garbage collected if there are no other references to it). In this case, if the item implements <see cref="IDisposable"/> the pool will ensure the item is disposed before being 'ignored'.</para>
    /// <para>The pool makes a best effort attempt to avoid going over the specified <see cref="PoolPolicy{T}.MaximumPoolSize"/>, but does not strictly enforce it. Under certain multi-threaded scenarios it's possible for a few items more than the maximum to be kept in the pool.</para>
    /// <para>Disposing the pool will also dispose all objects currently in the pool, if they support <see cref="IDisposable"/>.</para>
    /// </remarks>
    /// <typeparam name="T">The type of value being pooled.</typeparam>
    /// <seealso cref="PoolPolicy{T}"/>
    /// <seealso cref="IPool{T}"/>
    /// <seealso cref="PooledObject{T}"/>
    public class Pool<T> : PoolBase<T> where T : class
    {
        /// <summary>
        /// struct wrapper avoids array-covariance-checks from the runtime when assigning to elements of the array.
        /// </summary>
        private protected struct ObjectWrapper
        {
            public T Element;
        }

        #region Fields
        private protected readonly int _poolSize;
        private protected readonly ObjectWrapper[] _pool;
        private int _poolInstancesCount;


        #endregion

        #region Constructors

        /// <summary>
        /// Full constructor.
        /// </summary>
        /// <param name="poolPolicy">A <seealso cref="PoolPolicy{T}"/> instance containing configuration information for the pool.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="poolPolicy"/> argument is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if the <see cref="PoolPolicy{T}._factory"/> property of the <paramref name="poolPolicy"/> argument is null.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "HAA0603:Delegate allocation from a method group", Justification = "<Pending>")]
        public Pool(IPoolPolicy<T> poolPolicy) : base(poolPolicy)
        {
            _pool = new ObjectWrapper[PoolPolicy.MaximumPoolSize];
            _poolSize = _pool.Length;
        }

        #endregion

        #region IPool<T> Members


        /// <summary>
        /// Gets an item from the pool.
        /// </summary>
        /// <remarks>
        /// <para>If the pool is empty when the request is made, a new item is instantiated and returned. Otherwise an instance from the pool will be used.</para>
        /// <para>This method is thread safe.</para>
        /// </remarks>
        /// <returns>Returns an instance of {T} from the pool, or a new instance if the pool is empty.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the pool has been disposed.</exception>
        public override T Get()
        {
            CheckDisposed();

            T retVal;
            var pool = _pool;
            for (var i = 0; i < _poolSize; i++)
            {
                retVal = pool[i].Element;
                if (retVal != null && Interlocked.CompareExchange(ref pool[i].Element, null, retVal) == retVal)
                {
                    Interlocked.Decrement(ref _poolInstancesCount);
                    PoolPolicy.Reinitialize(retVal);

                    return retVal;
                }
            }

            //need to create new instance
            return PoolPolicy.Create(this);
        }


        /// <summary>
        /// Wrapped gets an item from the pool.
        /// </summary>
        /// <remarks>
        /// <para>If the pool is empty when the request is made, a new item is instantiated and returned. Otherwise an instance from the pool will be used.</para>
        /// <para>This method is thread safe.</para>
        /// </remarks>
        /// <returns>Returns an instance of {T} from the pool, or a new instance if the pool is empty.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the pool has been disposed.</exception>
        public override PooledObject<T> GetPooledObject()
        {
            return new PooledObject<T>(this, this.Get());
        }


        /// <summary>
        /// Returns/adds an object to the pool so it can be reused.
        /// </summary>
        /// <param name="value"></param>
        /// <remarks>
        /// <para>Items will be returned to the pool if it is not full, otherwise no action is taken and no error is reported.</para>
        /// <para>If the policy for the pool specifies <see cref="PooledItemInitialization.AsyncReturn"/> the item will be queued for re-intialisation on a background thread before being returned to the pool, control will return to the caller once the item has been queued even if it has not yet been fully re-initialised and returned to the pool.</para>
        /// <para>If the item is NOT returned to the pool, and {T} implements <see cref="System.IDisposable"/>, the instance will be disposed before the method returns.</para>
        /// <para>Calling this method on a disposed pool will dispose the returned item if it supports <see cref="IDisposable"/>, but takes no other action and throws no error.</para>
        /// <para>This method is 'thread safe', though it is possible for multiple threads returning items at the same time to add items beyond the maximum pool size. This should be rare and have few ill effects. Over time the pool will likely return to it's normal size.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="value"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the <see cref="PoolPolicy{T}.ErrorOnIncorrectUsage"/> is true and the same instance already exists in the pool.</exception>
        public override bool Return(T value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            if (IsDisposed)
            {
                SafeDispose((object)value);
                return false;
            }

            //check if pool is full
            if (!IsPoolFull())
            {
                Add(value);

                return true;
            }
            else
            {
                SafeDispose((object)value);
                return false;
            }
        }

        /// <summary>
        /// adds to pool array.
        /// </summary>
        /// <param name="value"></param>
        private void Add(T value)
        {
            //check if value has been alraedy returned
            if (Contains(value))
            {
                return;
            }

            var pool = _pool;
            for (var i = 0; i < _poolSize; ++i)
            {
                if (Interlocked.CompareExchange(ref pool[i].Element, value, null) == null)
                {
                    //found empty element to use
                    Interlocked.Increment(ref _poolInstancesCount);
                    return;
                }
            }

            //pool is full will just disposed this element.
            SafeDispose((object)value);
        }

        /// <summary>
        /// used to capture if value has already been returned
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool Contains(T value)
        {
            if (_poolInstancesCount == 0)
            {
                return false;
            }

            var pool = _pool;
            for (var i = 0; i < _poolSize; ++i)
            {
                var item = pool[i].Element;
                if (item != null && value.Equals(item))
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Performs dispose logic, can be overridden by derivded types.
        /// </summary>
        /// <param name="disposing">True if the pool is being explicitly disposed, false if it is being disposed from a finalizer.</param>
        /// <seealso cref="PoolBase{T}.Dispose()"/>
        /// <seealso cref="PoolBase{T}.IsDisposed"/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (IsPooledTypeDisposable)
                {
                    for (var i = 0; i < _poolSize; i++)
                    {
                        if (_pool[i].Element != null)
                        {
                            SafeDispose((object)_pool[i].Element);
                        }
                    }
                }
            }
        }

        public override string ToString()
        {
            return string.Format("Usage {0}/{1}", _poolInstancesCount.ToString(), this._poolSize.ToString());
        }

        #endregion

        #region Private Methods
#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool IsPoolFull()
        {
            return _poolInstancesCount >= _poolSize;
        }

        #endregion
    }
}
