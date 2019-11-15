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
    /// <seealso cref="PoolSharp.Pool{T}"/>
    public class PoolPolicy<T>
    {
        /// <summary>
        /// A function that returns a new item for the pool. Used when the pool is empty and a new item is requested.
        /// </summary>
        /// <remarks>
        /// <para>Should return a new, clean item, ready for use by the caller. Takes a single argument being a reference to the pool that was asked for the object, useful if you're creating <see cref="PooledObject{T}"/> instances.</para>
        /// <para>Cannot be null. If null when provided to a <see cref="PoolSharp.Pool{T}"/> instance, an exception will be thrown.</para>
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Func<IPool<T>, T> Factory { get; }


        /// <summary>
        /// An action that re-initialises a pooled object (cleans up/resets object state) before it is reused by callers.
        /// </summary>
        /// <remarks>
        /// <para>Can be null if no re-initialisation is required, or if the client is expected to perform it's own initialisation.</para>
        /// <para>This action is not performed against newly created items, it is a *re*-initialisation, not an initialisation action.</para>
        /// <para>The action proided may be called from a background thread. If the pooled object has thread affinity, invoking to the appropriate thread may be required within the action itself.</para>
        /// </remarks>
        public Action<T> ReinitializeObject { get; }
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
        public PooledItemInitialization InitializationPolicy { get;}
        /// <summary>
        /// If true the system will throw an <see cref="InvalidOperationException"/> when incorrect usage is detected. Normally this setting should be false (the default).
        /// </summary>
        /// <remarks>
        /// <para>Enabling this feature may significantly impact performance and increase allocations, it should only be enabled in debug or test builds and only when a problem is suspected.</para>
        /// <para>If enabled the <see cref="IPool{T}.Return(T)"/> function will throw if an item already in the pool as added a second time.</para>
        /// </remarks>
        public bool ErrorOnIncorrectUsage { get; }

        //public PoolPolicy<T>()
        //{
        //    //var type = this.GetType().GetGenericArguments()[0];
        //    //if (type.GetConstructor(Type.EmptyTypes) != null)
        //    //{
        //    //    var obj = type.InvokeMember(type.FullName, BindingFlags.CreateInstance, null, Type.EmptyTypes, null);
        //    //    this.Factory = (a) =>
        //    //    {
        //    //        var a = default;
        //    //        a;
        //    //    };
        //}


        private readonly Type _typeInstance;
        private T CreateInstance()
        {
                return (T)_typeInstance.InvokeMember(_typeInstance.FullName, BindingFlags.CreateInstance, null, Type.EmptyTypes, null);
        }

        public PoolPolicy(int maxPoolSize = 10, PooledItemInitialization initializePolicy = PooledItemInitialization.Return, bool errorOnIncorrectUsage = false)
        {
            if (0 >= maxPoolSize)
            {
                throw new ArgumentException($"{ nameof(maxPoolSize)} must be greater than 0");
            }
            this.MaximumPoolSize = maxPoolSize;
            this.InitializationPolicy = initializePolicy;
            this.ErrorOnIncorrectUsage = errorOnIncorrectUsage;

            _typeInstance = this.GetType().GetGenericArguments()[0];
            if (_typeInstance.GetConstructor(Type.EmptyTypes) == null)
            {
                throw new NotImplementedException("missing parameterless constructor");
            }

            Factory = (poolInstance) => CreateInstance();
        }

        public PoolPolicy(Func<IPool<T>, T> factory, Action<T> reinitializeObject=null, int maxPoolSize = 10, PooledItemInitialization initializePolicy = PooledItemInitialization.Return, bool errorOnIncorrectUsage = false)
        {
            if (0 >= maxPoolSize)
            {
                throw new ArgumentException($"{ nameof(maxPoolSize)} must be greater than 0");
            }
            this.MaximumPoolSize = maxPoolSize;
            this.InitializationPolicy = initializePolicy;
            this.ErrorOnIncorrectUsage = errorOnIncorrectUsage;

            if(factory== null)
            {
                throw new NullReferenceException(nameof(factory));
            }

            this.Factory = factory;
            this.ReinitializeObject = reinitializeObject;

        }


        //            Factory = (poolInstance) => new JsonStringWriter(),
        ////        InitializationPolicy = PooledItemInitialization.Return,
        ////        MaximumPoolSize = 10,
        ////        ReinitializeObject = (jw) => jw.Clear()
    }
}
