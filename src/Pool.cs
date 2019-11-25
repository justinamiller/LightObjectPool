using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

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
    /// Base class providing code re-use among multiple pool implementations.
    /// </summary>

    public class Pool<T> : IPool<T> where T:class
    {
        private protected struct ObjectWrapper
        {
            public T Element;
        }

        private readonly IPoolPolicy<T> _poolPolicy;
        private readonly bool _isPooledTypeDisposable;
        private bool _isDisposed;
        private protected readonly int _poolSize;
        private protected readonly ObjectWrapper[] _pool;


        public Pool(IPoolPolicy<T> poolPolicy)
        {
            _poolPolicy = poolPolicy ?? throw new ArgumentNullException(nameof(poolPolicy));
            _isPooledTypeDisposable =   typeof(IDisposable).IsAssignableFrom(typeof(T));

            _pool = new ObjectWrapper[PoolPolicy.MaximumPoolSize];
            _poolSize = _pool.Length;
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
        /// Disposes object
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "object")]
#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        protected void SafeDispose(T pooledObject)
        {
            if (_isPooledTypeDisposable)
            {
                ((IDisposable)pooledObject).Dispose();
            }
        }




        /// <summary>
        /// Gets an item from the pool.
        /// </summary>

        public T Get()
        {
            CheckDisposed();

            T retVal;
            var pool = _pool;
            for (var i = 0; i < _poolSize; i++)
            {
                retVal = pool[i].Element;
                if (retVal != null && Interlocked.CompareExchange(ref pool[i].Element, null, retVal) == retVal)
                {
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

        public PooledObject<T> GetPooledObject()
        {
            return new PooledObject<T>(this, this.Get());
        }


        /// <summary>
        /// Returns/adds an object to the pool so it can be reused.
        /// </summary>

        public  bool Return(T value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            if (IsDisposed)
            {
                SafeDispose(value);
                return false;
            }

            //add to the pool;
            return Add(value);
        }

        /// <summary>
        /// adds to pool array.
        /// </summary>

        private bool Add(T value)
        {
            //check if value has been alraedy returned
            if (Contains(value))
            {
                return false;
            }

            var pool = _pool;
            for (var i = 0; i < _poolSize; ++i)
            {
                if (Interlocked.CompareExchange(ref pool[i].Element, value, null) == null)
                {
                    //found empty element to use
                    return true;
                }
            }

            //pool is full will just disposed this element.
            SafeDispose(value);
            return false;
        }

        /// <summary>
        /// used to capture if value has already been returned
        /// </summary>

        private bool Contains(T value)
        {
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


        /// <summary>
        /// Performs dispose logic, can be overridden by derivded types.
        /// </summary>

        protected  void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (IsPooledTypeDisposable)
                {
                    for (var i = 0; i < _poolSize; i++)
                    {
                        if (_pool[i].Element != null)
                        {
                            SafeDispose(_pool[i].Element);
                        }
                    }
                }
            }
        }
       
    }
}
