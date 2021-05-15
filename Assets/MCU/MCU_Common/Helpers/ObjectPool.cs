using MCU.Extensions;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace MCU.Helpers {
    /// <summary>
    /// Store a pooled collection of objects that can be reused
    /// </summary>
    /// <typename="T">The type of object that is stored within the pool</typename>
    public sealed class ObjectPool<T> where T : class {
        /*----------Variables----------*/
        //PRIVATE

        /// <summary>
        /// The collection of items that are to be reused
        /// </summary>
        private List<T> itemPool = new List<T>();

        /// <summary>
        /// Function that is used to create a new item for the pool
        /// </summary>
        private Func<T> onCreateItem = null;

        /*----------Events----------*/
        //PUBLIC

        /// <summary>
        /// Action that is raised when an item is enabled for use
        /// </summary>
        public event Action<T> OnEnabled;

        /// <summary>
        /// Action that is raised when an item is disabled after use
        /// </summary>
        public event Action<T> OnDisabled;

        /// <summary>
        /// Action that is raised before an item is released (for cleanup operations)
        /// </summary>
        public event Action<T> OnDestroy;

        /*----------Properties----------*/
        //PUBLIC

        /// <summary>
        /// The number of items within the pool that are currently alive
        /// </summary>
        public int AliveObjects { get; private set; }

        /// <summary>
        /// The maximum number of objects that can be created in the pool
        /// </summary>
        public int PoolLimit { get; private set; } = -1;

        /// <summary>
        /// Get the *alive* object at the specified index
        /// </summary>
        public T this[int _index] {
            get {
                if (_index < 0 || _index >= AliveObjects) 
                    throw new IndexOutOfRangeException();
                return itemPool[_index];
            }
        }

        /*----------Functions----------*/
        //PUBLIC

        /// <summary>
        /// Create a new object pool for use
        /// </summary>
        /// <param="onCreate">Function that is used to create new objects for the pool</param>
        /// <param="onEnabled">[Optional] Function that can be raised for an object that will be used</param>
        /// <param="onDisabled">[Optional] Function that can be raised for an object no longer in use</param>
        /// <param="onDestroy">[Optional] Function that can be raised for an object that will no longer be used</param>
        /// <param="poolLimit">[Optional] An optional maximum size limit that the pool can have. Use less than 0 to have no limit</param>
        public ObjectPool(Func<T> onCreate, Action<T> onEnabled = null, Action<T> onDisabled = null, Action<T> onDestroy = null, int poolLimit = -1) {
            onCreateItem = onCreate ?? throw new ArgumentNullException(nameof(onCreate));
            OnEnabled = onEnabled;
            OnDisabled = onDisabled;
            OnDestroy = onDestroy;
            AliveObjects = 0;
            PoolLimit = poolLimit;
        }

        /// <summary>
        /// Retrieve the next item from the pool to be used
        /// </summary>
        /// <param="item">Pass out a reference to the item to be used. Null if this returns false</param>
        /// <returns>Returns true if there was an item that could be used, otherwise false</returns>
        public bool GetNextItem(out T item) {
            // Check if an existing item can be retrieved
            item = null;
            bool cleanedOut = false;
            do {
                // Can re-use an item
                if (AliveObjects < itemPool.Count) {
                    if (itemPool[AliveObjects] != null) 
                        item = itemPool[AliveObjects];

                    // Remove the null entry and let it try again
                    else itemPool.RemoveAt(AliveObjects);
                }

                // Need to create a new item
                else if (PoolLimit < 0 || itemPool.Count < PoolLimit) {
                    item = onCreateItem() ?? throw new NullReferenceException($"Unable to create a new item of type {nameof(T)} for the pool");
                    itemPool.Add(item);
                }

                // Check if the items have had any null entries cleaned out
                else if (!cleanedOut) {
                    // Clean out any null entries to see if there is any room left that can be found
                    cleanedOut = true;
                    for (int i = itemPool.Count; i >= 0; --i) {
                        if (itemPool[i] == null) 
                            itemPool.RemoveAt(i);
                    }
                }

                // Nothing to be done, out of items
                else break;
            } while (item == null);

            // If there is an item, do final setup
            if (item != null) {
                OnEnabled?.Invoke(item);
                ++AliveObjects;
                return true;
            } else {
                Debug.LogError($"Unable to create a new pool entry for use. The pool has reached the maximum size of {PoolLimit}");
                return false;
            }
        }

        /// <summary>
        /// Return an active pool member so that it can be used again later
        /// </summary>
        /// <param name="item">The reference to the item that is to be returned. This must be alive as currently marked by this pool</param>
        /// <returns>Returns true if the item was returned to the pool. Otherwise false</returns>
        /// <remarks>
        /// Using this functionality will modify the order of items in the pool from their original use order. 
        /// This can effect functionality that is dependent on maintaining that order
        /// </remarks>
        public bool ReturnItem(T item) {
            // Find the index of the item in the pool
            int index = FindIndex(x => x == item);
            if (index == -1) return false;

            // Swap it around with the last entry
            T temp = itemPool[AliveObjects - 1];
            itemPool[AliveObjects - 1] = item;
            itemPool[index] = temp;
            --AliveObjects;

            // Disable the element as needed
            OnDisabled?.Invoke(item);
            return true;
        }

        /// <summary>
        /// Reset the items that are in the pool to a default state
        /// </summary>
        public void ResetPool() { ResetPool(false); }

        /// <summary>
        /// Reset the items that are in the pool to a default state
        /// </summary>
        /// <param="_destroyItems">Flags if the pooled items should be destroyed instead<param>
        public void ResetPool(bool _destroyItems) {
            if (_destroyItems) {
                // Just destroy and clear the list
                for (int i = 0; i < itemPool.Count; ++i) {
                    if (itemPool[i] != null) {
                        if (i < AliveObjects) 
                            OnDisabled?.Invoke(itemPool[i]);
                        OnDestroy?.Invoke(itemPool[i]);
                    }
                }
                itemPool.Clear();
            } else {
                // Disable the items for later re-use
                for (int i = itemPool.Count - 1; i >= 0; --i) {
                    if (itemPool[i] != null) {
                        if (i < AliveObjects) 
                            OnDisabled?.Invoke(itemPool[i]);
                    } else itemPool.RemoveAt(i);
                }
            }

            // Nothing left active
            AliveObjects = 0;
        }

        /// <summary>
        /// Find the first *alive* object that matches the criteria predicate
        /// </summary>
        /// <param name="_criteria">The callback method that will be used to evaluate the items</param>
        /// <param name="_startIndex">The index that the search should start from</param>
        /// <returns>Returns the first object matching the criteria or null if unable to find</returns>
        public T Find(Predicate<T> _criteria, int _startIndex = 0) {
            for (int i = _startIndex; i < AliveObjects; ++i) {
                if (itemPool[i] != null) {
                    if (_criteria(itemPool[i])) 
                        return itemPool[i];
                }
            }

            // Nothing found
            return null;
        }

        /// <summary>
        /// Find the index of the first *alive* object that matches the criteria predicate
        /// </summary>
        /// <param name="_criteria">The callback method that will be used to evaluate the items</param>
        /// <param name="_startIndex">The index that the search should start from</param>
        /// <returns>Returns the index of the first object matching the criteria or -1 if unable to find</returns>
        public int FindIndex(Predicate<T> _criteria, int _startIndex = 0) {
            for (int i = _startIndex; i < AliveObjects; ++i) {
                if (itemPool[i] != null) {
                    if (_criteria(itemPool[i])) 
                        return i;
                }
            }

            // Nothing found
            return -1;
        }

        /// <summary>
        /// Create a MonoBehaviour pool with the standard behaviour
        /// </summary>
        /// <param="_prefab">The prefab that is to be instantiated for use</param>
        /// <param="_parent">[Optional] The parent that the object should be parented to</param>
        /// <param="_poolLimit">[Optional] An optional maximum size limit that the pool can have. Use less than 0 to have no limit</param>
        public static ObjectPool<U> CreateMonoPool<U>(U _prefab, Transform _parent = null, int _poolLimit = -1) where U : MonoBehaviour {
            return new ObjectPool<U>(
                () => {
                    U obj = GameObject.Instantiate(_prefab.gameObject, _parent).GetComponent<U>();
                    obj.transform.Reset();
                    return obj;
                },
                x => x.gameObject.SetActive(true),
                x => x.gameObject.SetActive(false),
                x => GameObject.Destroy(x.gameObject),
                _poolLimit
            );
        }

        /// <summary>
        /// Create a MonoBehaviour pool with the standard behaviour
        /// </summary>
        /// <param="_onCreate">The function that is used to instantiate the new object</param>
        /// <param="_poolLimit">[Optional] An optional maximum size limit that the pool can have. Use less than 0 to have no limit</param>
        public static ObjectPool<U> CreateMonoPool<U>(Func<U> _onCreate, int _poolLimit = -1) where U : MonoBehaviour {
            return new ObjectPool<U>(
                _onCreate,
                x => x.gameObject.SetActive(true),
                x => x.gameObject.SetActive(false),
                x => GameObject.Destroy(x.gameObject),
                _poolLimit
            );
        }
    }
}