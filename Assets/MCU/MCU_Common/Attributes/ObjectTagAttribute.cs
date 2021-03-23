using System;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MCU.Attributes {
    /// <summary>
    /// Display the tag options within a field inside the Inspector
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class ObjectTagAttribute : PropertyAttribute {
        #if UNITY_EDITOR
        /// <summary>
        /// Handle the displaying of the tag options for a property within the inspector
        /// </summary>
        [CustomPropertyDrawer(typeof(ObjectTagAttribute))]
        private sealed class ObjectTagAttributeDrawer : PropertyDrawer {
            /*----------Functions----------*/
            //PRIVATE

            /// <summary>
            /// Display the elements of the property within the designated area on the inspector area
            /// </summary>
            /// <param name="position">The position within the inspector that the property should be drawn to</param>
            /// <param name="property">The property that is to be displayed within the inspector</param>
            /// <param name="label">The label that has been assigned to the property</param>
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
                // Ensure this field is a string
                if (property.propertyType != SerializedPropertyType.String) {
                    EditorGUI.LabelField(position, label, new GUIContent("ObjectTag Attribute is string only"));
                    return;
                }

                // Using BeginProperty / EndProperty on the parent property means that
                // prefab override logic works on the entire property.
                EditorGUI.BeginProperty(position, label, property);

                // Display the tag options list
                property.stringValue = EditorGUI.TagField(position, label, property.stringValue);

                // End the prefab-able section
                EditorGUI.EndProperty();
            }
        }
        #endif
    }
}