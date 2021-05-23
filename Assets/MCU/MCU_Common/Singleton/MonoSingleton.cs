using System;
using System.Reflection;

using UnityEngine;

namespace MCU.Singleton {
    /// <summary>
    /// Provides a base point for defining a MonoBehaviour Singleton object that can be used
    /// </summary>
    /// <typeparam name="T">The <see cref="MonoBehaviour"/> type that is to be accessed via this base</typeparam>
    [DisallowMultipleComponent] 
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T> {
        /*----------Variables----------*/
        //SHARED

        /// <summary>
        /// Track the active singleton instance that will be used throughout operation
        /// </summary>
        private static T instance = null;

        /// <summary>
        /// Flag if the application is quitting to prevent new instances being created
        /// </summary>
        private static bool applicationHasQuit = false;

        //VISIBLE

        [Header("Singleton Behaviour")]

        [SerializeField, Tooltip("Flags if this object should automatically be initialised when the Singleton instance is identified")]
        protected bool autoInitOnIdentify = true;

        [SerializeField, Tooltip("State flag that indicates how multiple instances of a Singleton type will be handled")]
        protected MultipleSingletonHandler doForMultipleInstances = MultipleSingletonHandler.DestroyNew;

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
                // Check if there is an instance that to be served
                if (!instance) {
                    // Look for an instance that can be used
                    T[] found = GameObject.FindObjectsOfType<T>();
                    if (found.Length > 0) {
                        if (found.Length > 1) Debug.LogWarning($"Multiple instances of {typeof(T).Name} found in scene. Using {found[0]}...", found[0]);
                        instance = found[0];
                    }

                    // Nothing found, check if we are making one
                    else if (!applicationHasQuit) {
                        // Look for a CreateDefaultSingletonAttribute attribute to determine if a default asset should be created
                        CreateDefaultSingletonAttribute defaultAtt = typeof(T).GetCustomAttribute<CreateDefaultSingletonAttribute>(true);

                        // Check if we are creating a new instance for use
                        if (defaultAtt != null && defaultAtt.CreateDefault) {
                            // Notify dev of operation
                            Debug.Log($"No Singleton object of {typeof(T).Name} could be found in scene. Creating a default...");

                            // Create the new object instance to be used
                            GameObject obj = new GameObject(typeof(T).Name);
                            T inst = obj.AddComponent<T>();

                            // Make sure that that this instance was activated properly
                            if (instance == null) instance = inst;
                            else if (instance != inst) throw new InvalidOperationException($"A new instance of {typeof(T).Name} was created but the instance {instance} was somehow assigned instead");
                        }

                        // Otherwise, there is just nothing to use
                        else Debug.LogWarning($"No Singleton object of {typeof(T).Name} could be found in scene. Add one to the scene to use");
                    }

                    // Check if it needs to be initialised
                    if (instance && !instance.IsInitialised && instance.autoInitOnIdentify && !instance.Init()) 
                        Debug.LogError($"Singleton object of {typeof(T).Name} tried to be initialised but failed", instance);
                }
                return instance;
            }
        }

        /// <summary>
        /// Flag if this instance has been initialised and is ready for use
        /// </summary>
        public bool IsInitialised { get; protected set; }

        /// <summary>
        /// Flags if this object should automatically be initialised when the Singleton instance is identified
        /// </summary>
        public bool AutoInitOnIdentify {
            get { return autoInitOnIdentify; }
            set { autoInitOnIdentify = value; }
        }

        /// <summary>
        /// State flag that indicates how multiple instances of a Singleton type will be handled
        /// </summary>
        public MultipleSingletonHandler DoForMultipleInstances {
            get { return doForMultipleInstances; }
            set { doForMultipleInstances = value; }
        }

        /*----------Functions----------*/
        //PROTECTED

        /// <summary>
        /// Initialise this objects internal information
        /// <summary>
        protected virtual void Awake() {
            // If there is already a singleton then we have a potential problem and it's not this, something needs to happen
            if (instance && instance != this) {
                // Switch on how the active singleton wants to handle this
                Debug.Log($"Singleton instance of {instance} is handling multiple instances with {instance.doForMultipleInstances}. Processing...", this);
                switch (instance.doForMultipleInstances) {
                    // Destroy this object
                    default:
                    case MultipleSingletonHandler.DestroyNew:
                        // Prevent this object from being used
                        enabled = false;
                        GameObject.Destroy(this);
                        return;

                    // Shutdown and destroy the old one
                    case MultipleSingletonHandler.DestroyOld:
                        // If the old instance is initialised, shut it down
                        if (instance.IsInitialised) instance.Shutdown();
                        GameObject.Destroy(instance);
                        break;

                    // Copy the values of the old instance
                    case MultipleSingletonHandler.InheritOldValues:
                        // Try to inherit the values of the original
                        if (!InheritValues(instance)) Debug.LogError($"New Singleton instance {this} failed to inherit the previous values of {instance}", this);

                        // Clean up the old object
                        IsInitialised = instance.IsInitialised;
                        instance.IsInitialised = false;
                        GameObject.Destroy(instance);
                        break;

                    // Pass the new values onto the old object
                    case MultipleSingletonHandler.InheritNewValues:
                        // try to inherit this objects values
                        if (!instance.InheritValues(this as T)) Debug.LogError($"Original Singleton instance {instance} failed to inherit the new values of {this}", instance);

                        // Prevent this object from being used
                        enabled = false;
                        GameObject.Destroy(this);
                        return;
                }
            }

            // Save this instance for use as the singleton
            instance = this as T;

            // Check if this needs to be initialised
            if (!IsInitialised && autoInitOnIdentify && !Init())
                Debug.LogError($"Singleton object of {typeof(T).Name} tried to be initialised but failed", this);
        }
    
        /// <summary>
        /// Clear allocated memory / references
        /// </summary>
        protected virtual void OnDestroy() {
            // Clear the singleton instance if it is this
            if (instance == this) instance = null;

            // If this object is still active, kill it
            if (IsInitialised) Shutdown();
        }

        /// <summary>
        /// Flag that the application is quitting and shouldn't create a new instance
        /// </summary>
        /// <remarks>
        /// This method will fail in the rare situation where no instance is created at all
        /// until the application is quitting
        /// </remarks>
        protected virtual void OnApplicationQuit() { applicationHasQuit = true; }

        /// <summary>
        /// Inherit the values contained within the supplied instance to migrate operations to new object reference
        /// </summary>
        /// <param name="toCopy">The object instance that is to be copied to migrate the values to</param>
        /// <returns>Returns true if the copy process was successful and this object can handle continuing operation</returns>
        protected virtual bool InheritValues(T toCopy) { return true; }

        //PUBLIC

        /// <summary>
        /// Initialise the singleton values needed to operate
        /// </summary>
        /// <returns>Returns true if the initialisation process was completed successfully</returns>
        public virtual bool Init() {
            // If we're already initialised, something went wrong
            if (IsInitialised) throw new InvalidOperationException($"{this} is unable to initialise as it has already been initialised");

            // Base doesn't do anything, should be handled by implementing child classes
            IsInitialised = true;
            return true;
        }

        /// <summary>
        /// Clear up the data involved in this object
        /// </summary>
        public virtual void Shutdown() {
            // If not initialised, something went wrong
            if (!IsInitialised) throw new InvalidOperationException($"{this} is unable to shutdown as it hasn't been initialised");

            // Base dosen't do anything, should be handled by implementing child classes
            IsInitialised = false;
        }
    }

    /// <summary>
    /// Define the operation that will be used for handling multiple instances of a <see cref="MonoSingleton{T}"/> type
    /// </summary>
    /// <remarks>
    /// The handle operation is based on the value of the original singleton instance
    /// 
    /// E.g. If first instance has a value of <see cref="DestroyNew"/> and the second has a value of <see cref="InheritOldValues"/>
    /// then the first instances destroy operation will be used
    /// </remarks>
    public enum MultipleSingletonHandler
    {
        /// <summary>
        /// The new duplicate instance element will be destroyed
        /// </summary>
        DestroyNew,

        /// <summary>
        /// The old instance element will be destroyed so the new one can take over
        /// </summary>
        /// <remarks>
        /// The old instance will be shutdown before the new one is started
        /// </remarks>
        DestroyOld,

        /// <summary>
        /// The new instance will try to inherit the values of the old one, and then destroy the old one if successful
        /// </summary>
        /// <remarks>
        /// Inherit operations should transfer the current "session" to the recieving instance so references are
        /// maintained without interuppting the data collection
        /// </remarks>
        InheritOldValues,

        /// <summary>
        /// The old instance will try to inherit the values of the new one, and the one will be destroyed
        /// </summary>
        /// <remarks>
        /// Inherit operations should transfer the current "session" to the recieving instance so references are
        /// maintained without interuppting the data collection
        /// </remarks>
        InheritNewValues,
    }
}