using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
// ReSharper disable StaticMemberInGenericType

namespace Utils.Singleton
{
    /// <summary>
    /// Simple base class for implementing a singleton.
    /// </summary>
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T? _instance;
        private static readonly object _instanceLock = new ();
        private static bool _quitting;
        
        /// <summary>
        /// True if an instance of this singleton exists.
        /// </summary>
        public static bool HasInstance => _instance != null;
        
        private static SingletonAttribute? _cachedAttribute;
        private static List<Action<T>> _pendingActions = new();
        
        public static T? Instance 
        {
            get 
            {
                lock(_instanceLock)
                {
                    if (_instance != null || _quitting)
                    {
                        return _instance;
                    }

                    CacheAttribute();
                        
                    switch (_cachedAttribute!.CreationParams.creationMode)
                    {
                        case SingletonCreationMode.Throw:
                            Debug.LogError("Instance of " + typeof(T).FullName + " does not exist, but it should. Are you sure this can't be auto-created? If not, make sure you call it later!");
                            return null;
                        case SingletonCreationMode.Wait:
                            return null;
                    }

                    //Create the singleton and get creation params if we don't already have them.
                    var go = new GameObject(typeof(T).ToString());
                    _instance = go.AddComponent<T>();

                    if (_cachedAttribute.CreationParams.dontDestroyOnLoad)
                    {
                        DontDestroyOnLoad(go);
                    }
                        
                    InvokePendingActions();

                    return _instance;
                }
            }
        }

        /// <summary>
        /// Invokes the supplied action with the instance of this singleton if it exists, otherwise caches the action to be invoked when the instance is created.
        /// </summary>
        public static void OnInstanceCreated(Action<T> action)
        {
            if (_instance != null)
            {
                action(_instance);
            }
            else
            {
                _pendingActions.Add(action);
            }
        }
        
        private static void InvokePendingActions()
        {
            if (_instance == null)
            {
                Debug.LogError("Don't call InvokePendingActions before the instance is created!");
                return;
            }
            
            foreach (var action in _pendingActions)
            {
                action(_instance);
            }
            
            _pendingActions.Clear();
        }
        
        private static void CacheAttribute() 
            => _cachedAttribute ??= Attribute.GetCustomAttribute(typeof(T), typeof(SingletonAttribute)) as SingletonAttribute ?? new SingletonAttribute();

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = gameObject.GetComponent<T>();

                if (_instance == null)
                {
                    return;
                }
                
                CacheAttribute();
                if(_cachedAttribute!.CreationParams.dontDestroyOnLoad)
                {
                    DontDestroyOnLoad(gameObject);
                }
                
                InvokePendingActions();
            }
            else if(_instance.GetInstanceID() != GetInstanceID())
            {
                Destroy(gameObject);
                Debug.LogError($"Instance of {GetType().FullName} already exists, removing {ToString()}");
            }
        }

        protected virtual void OnApplicationQuit() 
        {
            _quitting = true;
        }

    }
}