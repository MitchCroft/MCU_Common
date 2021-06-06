using System.Reflection;

using UnityEngine;

namespace MCU.Singleton {
    /// <summary>
    /// Provides a base point for defining a Scriptable Object singleton instance that can be used
    /// </summary>
    /// <typeparam name="T">The <see cref="ScriptableObject"/> type that is to be accessed via this base</typeparam>
    public abstract class ScriptableSingleton<T> : ScriptableObject where T : ScriptableSingleton<T> {
        /*----------Variables----------*/
        //SHARED

        /// <summary>
        /// Track the active singleton instance that will be used throughout operation
        /// </summary>
        private static T instance = null;
    
        /*----------Properties----------*/
        //PUBLIC
    
        /// <summary>
        /// Get a flag indicating if there is a singleton instance currently
        /// </summary>
        public static bool HasInstance { get { return instance; } }

        /// <summary>
        /// Retrieve the singleton instance for use
        /// </summary>
        public static T Instance {
            get {
                if (!instance) {
                    // Find all of the instances in Resources
                    T[] assets = Resources.FindObjectsOfTypeAll<T>();
                    if (assets.Length > 0) {
                        // There shouldn't be more then one
                        if (assets.Length > 1)
                            Debug.LogWarning($"Multiple instances of {typeof(T).Name} found in resources. Using {assets[0]}...", assets[0]);

                        // Store the reference to the asset that will be used
                        if (typeof(T).GetCustomAttribute<DuplicateAssetAttribute>() != null)
                            instance = Instantiate(assets[0]);
                        else instance = assets[0];
                    }

                    // If none was found check to see if a default should be created
                    else {
                        // Determine how no asset should be handled
                        if (typeof(T).GetCustomAttribute<CreateDefaultSingletonAttribute>() != null) {
                            Debug.Log($"No Singleton asset of {typeof(T).Name} could be found in Resources. Creating a default...");
                            instance = ScriptableObject.CreateInstance<T>();
                        }

                        // Otherwise, nothing to use
                        else Debug.LogWarning($"No Singleton asset of {typeof(T).Name} could be found in Resources. Add one to resources to use");
                    }

                    // If there is an asset to be used, prevent it from being unloaded
                    if (instance) instance.hideFlags = HideFlags.DontUnloadUnusedAsset;
                }
                return instance;
            }
        }
    }
}