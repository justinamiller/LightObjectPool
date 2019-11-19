# ObjectPool

## What is ObjectPool ?
A simple, light weight, thread safe object pool.

It also supports pooling of disposable types, managing the life time of pooled objects and performing early dispose when possible.
Pool implementations implement a simple common interface, so they can be mocked or replaced with alternatives.

## Supported Platforms
Currently;

* .Net Framework 4.0+
* .Net Standard 2.0+

## How do I use it?
*We got your samples right here*

Install the Nuget package like this;

```powershell
    PM> Install-Package ObjectPool
```

[![NuGet Badge](https://buildstats.info/nuget/ObjectPool)](https://www.nuget.org/packages/ObjectPool/)

Or reference the ObjectPool.dll assembly that matches your app's platform.

### Creating a Pool
Create a PoolPolicy<T> instance to configure options and behaviour for the pool, T is the type of item being pooled.
Create a new Pool<T> instance passing the pool policy you created. Pool policies can be re-used across pools so long as the assigned Function and Action delegates are thread-safe.

```C#

    using ObjectPool;
    // Define a policy. This policy;
    //  Is for a StringBuilder pool.
    //  Synchronously resets the StringBuilder state when the item is returned to the pool.
    //  Pools at most 10 instances

    var policy = new PoolPolicy<System.Text.StringBuilder>()
    {
    	Factory = (poolInstance) => new System.Text.StringBuilder(),
    	InitializationPolicy = PooledItemInitialization.Return,
    	MaximumPoolSize = 10,
    	ReinitializeObject = (sb) => sb.Clear()
    };
    
```

### Using a Pool
Use the Take method to retrieve an instance from the pool. Use the Add method to return an instance to the pool so it can be re-used.

```C#
    //Retrieve an instance from the pool
    var stringbuilder = pool.Get();
 
    //Do something with the stringbuilder   
    
    //Return the string builder to the pool
    pool.Return(stringbuilder);    
```

#### Using a Pool with Auto-Return Semantics
Instead of creating a pool for your specific type, create the pool for PooledObject<T> where T is the type you actually want.
Then you can use auto-return like this;

```C#
    //Retrieve an instance from the pool
    using (var pooledItem= pool.GetPooledObject())
    {
        //pooledItem.Value is the object you actually want.
        //If the pool is for tyhe type PooledObject<System.Text.StringBuilder> then
        //you can access the string builder instance like this;
        pooledItem.Value.Append("Some text to add to the builder");
        
    } // The item will automatically be returned to the pool here.
```
