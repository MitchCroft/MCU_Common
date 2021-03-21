using System;
using System.Collections.Generic;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MCU.Helpers {
    /// <summary>
    /// Store a generic mapping object which allows for the association of one value with another
    /// </summary>
    /// <typeparam name="Key">The type of the first value that will be used as the 'key' of the mapping</typename>
    /// <typeparam name="Value">The type of the second value that will be used as the 'value' of the mapping</typename>
    /// <remarks>This class is to allow for the establishing of relations between two elements from within the inspector</remarks>
    public abstract class GenericMapping<Key, Value> : AGenericMapping {
        /*----------Variables----------*/
        //PUBLIC

        [Tooltip("The key value that will be used to link to the value of the mapping")]
        public Key key;

        [Tooltip("The value that will be accessed via the defined key value")]
        public Value value;

        /*----------Functions----------*/
        //PUBLIC

        /// <summary>
        /// Convert this mapping object to a simple representation of the data stored within
        /// </summary>
        /// <returns>Returns a display text representation of the current mapping object</returns>
        public override string ToString() { return string.Format("{0} -> {1}", key, value); }
    }

    /// <summary>
    /// Provide a base point for all of the generic mapping objects to inherit
    /// </summary>
    public abstract class AGenericMapping {
#if UNITY_EDITOR
        /// <summary>
        /// A simple property drawer for displaying a Generic Mapping object without any displayed labels
        /// </summary>
        [CustomPropertyDrawer(typeof(AGenericMapping), true)]
        private sealed class GenericMappingDrawer : PropertyDrawer {
            /*----------Variables----------*/
            //CONST

            /// <summary>
            /// Store basic representations of the main property elements for displaying within the inspector
            /// </summary>
            private static readonly GUIContent KEY_LABEL, VAL_LABEL;

            /*----------Functions----------*/
            //STATIC

            /// <summary>
            /// Initialise the readonly content elements for display
            /// </summary>
            static GenericMappingDrawer() {
                KEY_LABEL = new GUIContent("Key");
                VAL_LABEL = new GUIContent("Value");
            }

            //PUBLIC

            /// <summary>
            /// Determine the height that is needed to display the the mapping object values
            /// </summary>
            /// <param name="property">The property that is being displayed</param>
            /// <param name="label">The label that has been assigned to the property</param>
            /// <returns>Returns the height to be used for displaying the property</returns>
            public sealed override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
                // Retrieve the properties that are to be displayed
                SerializedProperty keyProp = property.FindPropertyRelative("key");
                SerializedProperty valProp = property.FindPropertyRelative("value");

                // Get the height of the properties to determine how they should be drawn
                float keyHeight = (keyProp != null ? EditorGUI.GetPropertyHeight(keyProp, true) : EditorGUIUtility.singleLineHeight),
                      valHeight = (valProp != null ? EditorGUI.GetPropertyHeight(valProp, true) : EditorGUIUtility.singleLineHeight);

                // If both of these are the same height, display them next to each other. Otherwise, draw the 'key' on top
                return (Mathf.Approximately(keyHeight, valHeight) ? keyHeight : keyHeight + valHeight);
            }

            /// <summary>
            /// Display the elements of the property within the designated area on the inspector area
            /// </summary>
            /// <param name="position">The position within the inspector that the property should be drawn to</param>
            /// <param name="property">The property that is to be displayed within the inspector</param>
            /// <param name="label">The label that has been assigned to the property</param>
            public sealed override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
                // Using BeginProperty / EndProperty on the parent property means that
                // prefab override logic works on the entire property.
                EditorGUI.BeginProperty(position, label, property);

                // Retrieve the properties that are to be displayed
                SerializedProperty keyProp = property.FindPropertyRelative("key");
                SerializedProperty valProp = property.FindPropertyRelative("value");

                // Store the values that will be used to display the properties
                Rect keyArea, valArea; GUIContent keyContent = GUIContent.none, valContent = GUIContent.none;

                // If the height of the key is the same as the height assigned for position, they can display on the same line
                float keyHeight = (keyProp != null ? EditorGUI.GetPropertyHeight(keyProp, true) : EditorGUIUtility.singleLineHeight);
                if (Mathf.Approximately(keyHeight, position.height)) {
                    // Use half of the specified area each
                    keyArea = new Rect(position.x, position.y, position.width * .5f, position.height);
                    valArea = new Rect(position.x + position.width * .5f, position.y, position.width * .5f, position.height);
                }

                // Otherwise, there needs to be some ordering 
                else {
                    // Use proper labels for the individual elements
                    keyContent = KEY_LABEL; valContent = VAL_LABEL;

                    // Stack the positions on over the other
                    keyArea = EditorGUI.IndentedRect(new Rect(position.x, position.y, position.width, keyHeight));
                    valArea = EditorGUI.IndentedRect(new Rect(position.x, position.y + keyHeight, position.width, position.height - keyHeight));
                }

                // Display the properties options
                if (keyProp != null) EditorGUI.PropertyField(keyArea, keyProp, keyContent, true);
                else EditorGUI.LabelField(keyArea, "'key' Property Not Found");
                if (valProp != null) EditorGUI.PropertyField(valArea, valProp, valContent, true);
                else EditorGUI.LabelField(valArea, "'value' Property Not Found");

                // End the prefab-able section
                EditorGUI.EndProperty();
            }
        }
#endif
    }

    /// <summary>
    /// Flag different behaviours that can be used when converting Generic Mapping arrays to Dictionary values
    /// </summary>
    public enum EMappingAssignmentBehaviour {
        /// <summary>
        /// Any duplicate key values will have their values replaced without feedback
        /// </summary>
        Silent,

        /// <summary>
        /// Any duplicate key values will cause a warning to be logged to Unity. Duplicate key values will have their values replaced
        /// </summary>
        Warning,

        /// <summary>
        /// Any duplicate key values will cause an error to be logged to Unity. Subsequent duplicate values will be skipped
        /// </summary>
        Error,

        /// <summary>
        /// Any duplicate key values will cause a <see cref="ArgumentException"/> to be thrown
        /// </summary>
        Exception,
    }

    /// <summary>
    /// Provide additional functionality to the Generic Mapping objects
    /// </summary>
    public static class GenericMappingExtensions {
        /// <summary>
        /// Convert an array of Generic Mapping objects into an established Dictionary
        /// </summary>
        /// <param name="mappings">The array of Generic Mapping Objects to be converted</param>
        /// <param name="assignmentBehaviour">Flags how duplicate key values should be handled during processing</param>
        /// <returns>Returns a Dictionary with the stored mapping values</returns>
        public static Dictionary<Key, Value> ToDictionary<Key, Value>(this GenericMapping<Key, Value>[] mappings, EMappingAssignmentBehaviour assignmentBehaviour = EMappingAssignmentBehaviour.Silent) {
            // Create the dictionary to store the values
            Dictionary<Key, Value> dict = new Dictionary<Key, Value>(mappings.Length);

            // Loop through the mapping elements
            for (int i = 0; i < mappings.Length; i++) {
                // Check if the key has a clash
                if (dict.ContainsKey(mappings[i].key)) {
                    switch (assignmentBehaviour) {
                        case EMappingAssignmentBehaviour.Warning:
                            Debug.LogWarning(string.Format("Key Value '{0}' assigned to '{1}' was found but that Key is already assigned to '{2}'", mappings[i].key, mappings[i].value, dict[mappings[i].key]));
                            break;
                        case EMappingAssignmentBehaviour.Error:
                            Debug.LogError(string.Format("Key Value '{0}' assigned to '{1}' was found but that Key is already assigned to '{2}'", mappings[i].key, mappings[i].value, dict[mappings[i].key]));
                            continue; 
                        case EMappingAssignmentBehaviour.Exception:
                            throw new ArgumentException(string.Format("Key Value '{0}' assigned to '{1}' was found but that Key is already assigned to '{2}'", mappings[i].key, mappings[i].value, dict[mappings[i].key]));
                    }
                }

                // Save the value
                dict[mappings[i].key] = mappings[i].value;
            }

            // Return the established values
            return dict;
        }
    }
}