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
    public class Pool<T> : PoolBase<T> where T : class
    {
        private protected struct ObjectWrapper
        {
            public T Element;
        }

        private protected readonly int _poolSize;
        private protected readonly ObjectWrapper[] _pool;
        private int _poolInstancesCount;

        public Pool(IPoolPolicy<T> poolPolicy) : base(poolPolicy)
        {
            _pool = new ObjectWrapper[PoolPolicy.MaximumPoolSize];
            _poolSize = _pool.Length;
        }

        /// <summary>
        /// Gets an item from the pool.
        /// </summary>
  
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
      
        public override PooledObject<T> GetPooledObject()
        {
            return new PooledObject<T>(this, this.Get());
        }


        /// <summary>
        /// Returns/adds an object to the pool so it can be reused.
        /// </summary>

        public override bool Return(T value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            if (IsDisposed)
            {
                SafeDispose(value);
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
                SafeDispose(value);
                return false;
            }
        }

        /// <summary>
        /// adds to pool array.
        /// </summary>

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
            SafeDispose(value);
        }

        /// <summary>
        /// used to capture if value has already been returned
        /// </summary>

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


        /// <summary>
        /// Performs dispose logic, can be overridden by derivded types.
        /// </summary>

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
                            SafeDispose(_pool[i].Element);
                        }
                    }
                }
            }
        }

        public override string ToString()
        {
            return string.Format("Usage {0}/{1}", _poolInstancesCount.ToString(), this._poolSize.ToString());
        }

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private bool IsPoolFull()
        {
            return _poolInstancesCount >= _poolSize;
        }
    }
}
