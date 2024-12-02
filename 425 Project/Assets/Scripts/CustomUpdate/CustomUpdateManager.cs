using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.Singleton;


/// <summary>
/// Manages custom update loops in place of Unity's built-in update methods to save on performance.
/// </summary>
public class CustomUpdateManager : SingletonMonoBehaviour<CustomUpdateManager>
{
    private static readonly UpdateQueue<ICustomUpdate> _updates = new();
    private static readonly UpdateQueue<ICustomFixedUpdate> _fixedUpdates = new();
    private static readonly UpdateQueue<ICustomLateUpdate> _lateUpdates = new();

    /// <summary>
    /// Registers an object to all custom update types it implements.
    /// </summary>
    public static void Register(IManagedUpdate update)
    {
        if (update is ICustomUpdate customUpdate)
        {
            RegisterUpdate(customUpdate);
        }

        if (update is ICustomFixedUpdate customFixedUpdate)
        {
            RegisterFixedUpdate(customFixedUpdate);
        }

        if (update is ICustomLateUpdate customLateUpdate)
        {
            RegisterLateUpdate(customLateUpdate);
        }
    }

    /// <summary>
    /// Unregisters an object from all custom update types it implements.
    /// </summary>
    /// <param name="update"></param>
    public static void Unregister(IManagedUpdate update)
    {
        if (update is ICustomUpdate customUpdate)
        {
            UnregisterUpdate(customUpdate);
        }

        if (update is ICustomFixedUpdate customFixedUpdate)
        {
            UnregisterFixedUpdate(customFixedUpdate);
        }

        if (update is ICustomLateUpdate customLateUpdate)
        {
            UnregisterLateUpdate(customLateUpdate);
        }
    }

    /// <summary>
    /// Registers an object to receive custom updates.
    /// </summary>
    public static void RegisterUpdate(ICustomUpdate update)
        => _updates.Add(update);

    /// <summary>
    /// Unregisters an object from receiving custom updates.
    /// </summary>
    public static void UnregisterUpdate(ICustomUpdate update)
        => _updates.Remove(update);

    /// <summary>
    /// Registers an object to receive custom fixed updates.
    /// </summary>
    public static void RegisterFixedUpdate(ICustomFixedUpdate update)
        => _fixedUpdates.Add(update);

    /// <summary>
    /// Unregisters an object from receiving custom fixed updates.
    /// </summary>
    public static void UnregisterFixedUpdate(ICustomFixedUpdate update)
        => _fixedUpdates.Remove(update);

    /// <summary>
    /// Registers an object to receive custom late updates.
    /// </summary>
    public static void RegisterLateUpdate(ICustomLateUpdate update)
        => _lateUpdates.Add(update);

    /// <summary>
    /// Unregisters an object from receiving custom late updates.
    /// </summary>
    public static void UnregisterLateUpdate(ICustomLateUpdate update)
        => _lateUpdates.Remove(update);

    private void Update()
    {

        _updates.ProcessQueue();

        float deltaTime = Time.deltaTime;

        foreach (var update in _updates)
        {
            try
            {
                update.CustomUpdate(deltaTime);
            }
            catch (Exception exc)
            {
                Debug.LogError(exc);
            }
        }
    }

    private void FixedUpdate()
    {
        _fixedUpdates.ProcessQueue();

        float fixedDeltaTime = Time.fixedDeltaTime;
        foreach (var update in _fixedUpdates)
        {
            try
            {
                update.CustomFixedUpdate(fixedDeltaTime);
            }
            catch (Exception exc)
            {
                Debug.LogError(exc);
            }
        }
    }

    private void LateUpdate()
    {
        _lateUpdates.ProcessQueue();

        float deltaTime = Time.deltaTime;
        foreach (var update in _lateUpdates)
        {
            try
            {
                update.CustomLateUpdate(deltaTime);
            }
            catch (Exception exc)
            {
                Debug.LogError(exc);
            }
        }
    }


    private class UpdateQueue<T> : IEnumerable<T>
    {
        private readonly HashSet<T> _items = new();
        private readonly HashSet<T> _addQueue = new();
        private readonly HashSet<T> _removeQueue = new();

        /// <summary>
        /// Adds an item to the update queue.
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
            => _addQueue.Add(item);

        /// <summary>
        /// Removes an item from the update queue.
        /// </summary>
        /// <param name="item"></param>
        public void Remove(T item)
            => _removeQueue.Add(item);

        /// <summary>
        /// Processes the update queue, adding and removing items as necessary.
        /// </summary>
        public void ProcessQueue()
        {
            foreach (var item in _addQueue)
            {
                _items.Add(item);
            }

            _addQueue.Clear();

            foreach (var item in _removeQueue)
            {
                _items.Remove(item);
            }

            _removeQueue.Clear();
        }
        
        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}