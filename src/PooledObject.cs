﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightObjectPool
{
    /// <summary>
    /// A wrapper for a pooled object that allows for easily retrieving and returning the item to the pool via the using statement.
    /// </summary>
    public sealed class PooledObject<T> : IDisposable
    {
        private bool _isDisposed;
        private readonly IPool<T> _pool;
        private readonly T _value;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public PooledObject(IPool<T> pool, T value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
            _value = value;
        }

        /// <summary>
        /// The actual value of interest.
        /// </summary>
        public T Value { get { return _value; } }

        /// <summary>
        /// Rather than disposing the wrapper or the <see cref="Value"/>, returns the wrapper to the pool specified in the wrapper's constructor.
        /// </summary>
        public void Dispose()
        {
            //check if we already disposed of this object
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;
            try
            {
                _pool.Return(_value);
            }
            catch (ObjectDisposedException)
            {
                (this.Value as IDisposable)?.Dispose();
            }
        }
    }
}
