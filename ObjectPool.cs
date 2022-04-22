using System.Collections.Generic;

public class ObjectPool<T>
{
    public delegate T CreateCallback();
    public delegate void Callback(T element);

    CreateCallback onCreate;
    Callback onTake, onReturn, onDestroy;
    Stack<T> pool;
    int maxSize;
    int numObjects;

    public ObjectPool(CreateCallback createFunc, Callback takeFunc, Callback returnFunc,
        Callback destroyFunc, int initialSize, int maximumSize)
    {
        onCreate = createFunc;
        onTake = takeFunc;
        onReturn = returnFunc;
        onDestroy = destroyFunc;

        // Create the initial number of objects and add them to the pool.
        pool = new Stack<T>(initialSize);
        for (int i = 0; i < initialSize; ++i)
        {
            pool.Push(onCreate());
        }

        maxSize = maximumSize;
        numObjects = initialSize;
    }

    public T Take()
    {
        T element;

        if (pool.Count < 1 && numObjects < maxSize)
        {
            // Create and return a new object if the pool is empty 
            // and the max size has not been reached.
            element = onCreate();
            numObjects += 1;
        }
        else
        {
            // Remove element from the pool.
            element = pool.Pop();
        }

        // Run onTake() and return element.
        onTake(element);
        return element;
    }

    public void Return(T element)
    {
        if (pool.Count >= maxSize)
        {
            // Destroy the object if the pool is full.
            onDestroy(element);
            numObjects -= 1;
        }
        else
        {
            // Run onReturn() and add the object back to the pool.
            onReturn(element);
            pool.Push(element);
        }
    }

    public void Clear()
    {
        // Destroy all objects.
        foreach (T elemement in pool)
        {
            onDestroy(elemement);
        }

        pool.Clear();
        numObjects = 0;
    }

    public int CountAll()
    {
        return numObjects;
    }

    public int CountActive()
    {
        return numObjects - pool.Count;
    }

    public int CountInactive()
    {
        return pool.Count;
    }
}
