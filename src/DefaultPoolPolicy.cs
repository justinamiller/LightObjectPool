﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightObjectPool
{
    /// <summary>
    /// Provides configuration controlling how an object pool works.
    /// Will call empty constructor for creation of object.
    /// </summary>
    public class DefaultPoolPolicy<T> : IPoolPolicy<T> where T : class, new()
    {
        private readonly Action<T> _reinitializeObject = null;

        public int MaximumPoolSize { get; }

        
        public DefaultPoolPolicy(Action<T> reinitializeObject=null, int maxPoolSize = 10)
        {
            _reinitializeObject = reinitializeObject;
            MaximumPoolSize = maxPoolSize > 0 ? maxPoolSize : Environment.ProcessorCount * 2;
        }

        public T Create(IPool<T> pool)
        {
#pragma warning disable HAA0502 // Explicit new reference type allocation
            return new T();
#pragma warning restore HAA0502 // Explicit new reference type allocation
        }

        public void Reinitialize(T obj)
        {
            _reinitializeObject?.Invoke(obj);
        }
    }
}
