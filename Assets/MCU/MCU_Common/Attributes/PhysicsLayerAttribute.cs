using System;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MCU.Attributes {
    /// <summary>
    /// Provide a method for selecting a single Physics Layer option from within the Inspector
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class PhysicsLayerAttribute : PropertyAttribute {
        #if UNITY_EDITOR
        /// <summary>
        /// Display a property that allows for the selection of a single Physics Layer
        /// </summary>
        [CustomPropertyDrawer(typeof(PhysicsLayerAttribute))]
        private sealed class PhysicsLayerAttributeDrawer : PropertyDrawer {
            /*----------Functions----------*/
            //PUBLIC

            /// <summary>
            /// Render the options within the display area of the Inspector
            /// </summary>
            /// <param name="position">The position to place the </param>
            /// <param name="property">The property containing the information to modify</param>
            /// <param name="label">The lavel that should be displayed with the option</param>
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
                // Check that this property is the correct type
                if (property.propertyType != SerializedPropertyType.Integer) {
                    EditorGUI.LabelField(position, label, "PhysicsLayerAttribute only works on Integer Fields");
                    return;
                }

                // Using BeginProperty / EndProperty on the parent property means that
                // prefab override logic works on the entire property.
                EditorGUI.BeginProperty(position, label, property);

                // Draw the label for this property
                position = EditorGUI.PrefixLabel(position, label);

                // Display the different physics layer options
                property.intValue = EditorGUI.LayerField(position, property.intValue);

                // End the property definitions
                EditorGUI.EndProperty();
            }
        }
        #endif
    }
}