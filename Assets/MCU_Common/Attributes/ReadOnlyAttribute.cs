using System;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MCU.Attributes {
    /// <summary>
    /// Provide a method for making variables in the Inspector not editable
    /// </summary>
    /// <remarks>
    /// Implementation taken from It3ration 'https://answers.unity.com/questions/489942/how-to-make-a-readonly-property-in-inspector.html'
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ReadOnlyAttribute : PropertyAttribute {
        #if UNITY_EDITOR
        /// <summary>
        /// Display a property while preventing any write operations
        /// </summary>
        [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
        private sealed class ReadOnlyAttributeDrawer : PropertyDrawer {
            /*----------Functions----------*/
            //PUBLIC

            /// <summary>
            /// Display the property while restricting write access
            /// </summary>
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
                // Stash the current GUI enabled state
                bool prevState = GUI.enabled;

                // Disable the editor GUI
                GUI.enabled = false;

                // Display the property
                EditorGUI.PropertyField(position, property, label, property.hasVisibleChildren);

                // Restore the GUI state
                GUI.enabled = prevState;
            }
        }
        #endif
    }
}
