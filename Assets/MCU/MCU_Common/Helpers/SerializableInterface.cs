using System;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MCU.Helpers {
    /// <summary>
    /// Store information pertaining to the type of interface that should be stored
    /// </summary>
    /// <typeparam name="T">The type of interface that has been stored within the object</typeparam>
    public abstract class SerializableInterface<T> : SerializableBase where T : class {
        /*----------Properties----------*/
        //PROTECTED 

        /// <summary>
        /// Get the type of interface that is being serialized within this object
        /// </summary>
        protected sealed override Type InterfaceType { get { return typeof(T); } }

        //PUBLIC

        /// <summary>
        /// Retrieve the interface that is stored within the object
        /// </summary>
        public T Interface { get { return reference as T; } }

        /*----------Functions----------*/
        //PUBLIC
        
        /// <summary>
        /// Provide implicit conversion between a serialised interface and its contained value
        /// </summary>
        /// <param name="inter">The SerializableInterface object that is to be converted</param>
        public static implicit operator T(SerializableInterface<T> inter) { return inter.Interface; }

        /// <summary>
        /// Provide an implicit conversion test to determine if an interface has a valid interface reference assigned
        /// </summary>
        /// <param name="inter">The SerializableInterface object that is to be tested</param>
        public static implicit operator bool(SerializableInterface<T> inter) { return inter.reference && inter.reference is T; }
    }

    /// <summary>
    /// Define the base values that are required for storing serialised Interface references
    /// </summary>
    [Serializable] public abstract class SerializableBase {
        /*----------Variables----------*/
        //PRIVATE

        /// <summary>
        /// Store a reference to the serializable instance of the interface that can be retrieved
        /// </summary>
        [SerializeField, HideInInspector] protected UnityEngine.Object reference;

        /*----------Properties----------*/
        //PROTECTED

        /// <summary>
        /// Retrieve the interface that is to be stored within this object
        /// </summary>
        protected abstract Type InterfaceType { get; }

#if UNITY_EDITOR
        /// <summary>
        /// Display a field that allows for the assigning of an interface implementing object
        /// </summary>
        [CustomPropertyDrawer(typeof(SerializableBase), true)]
        private sealed class SerializableInterfaceDrawer : PropertyDrawer {
            /*----------Variables----------*/
            //PRIVATE

            /// <summary>
            /// Label to be used if the the assignment field type can't be determined
            /// </summary>
            private GUIContent invalidLabel = new GUIContent("Invalid Property Value");

            /*----------Functions----------*/
            //PUBLIC

            /// <summary>
            /// Display an object field that allows for the assigning of 
            /// </summary>
            /// <param name="position">The position within the inspector to display the object field</param>
            /// <param name="property">The property that is being displayed</param>
            /// <param name="label">The label that has been assigned to the property</param>
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
                // Ensure that the object actual represents the property being displayed
                property.serializedObject.ApplyModifiedProperties();

                // Retrieve the array of objects that that are being modified
                if (!property.GetPropertyValue(out SerializableBase baseInterface)) {
                    EditorGUI.LabelField(position, invalidLabel);
                    return;
                }

                // Using BeginProperty / EndProperty on the parent property means that
                // prefab override logic works on the entire property.
                EditorGUI.BeginProperty(position, label, property);

                // Shift the property to the internal reference
                property = property.FindPropertyRelative("reference");
                
                // Display an object field that lets the user add an object reference
                UnityEngine.Object newVal = EditorGUI.ObjectField(position, label, property.objectReferenceValue, typeof(UnityEngine.Object), true);

                // Check if the object reference has changed
                if (newVal != property.objectReferenceValue) {
                    // Check there is an object to test
                    if (newVal != null) {
                        // If this object isn't assignable to the interface, try to search for it
                        if (!baseInterface.InterfaceType.IsAssignableFrom(newVal.GetType())) {
                            // If the object is a component, get the Game Object
                            if (newVal is Component) newVal = ((Component)newVal).gameObject;

                            // If the object is a Game Object, try to find an attached component that implements the interface
                            if (newVal is GameObject) newVal = ((GameObject)newVal).GetComponent(baseInterface.InterfaceType);

                            // Otherwise, can't use this object
                            else newVal = null;
                        }
                    }

                    // Save the new value to the property
                    property.objectReferenceValue = newVal;
                }

                EditorGUI.EndProperty();
            }
        }
#endif
    }
}