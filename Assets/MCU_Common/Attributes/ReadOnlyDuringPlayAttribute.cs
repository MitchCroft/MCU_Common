using System;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MCU.Attributes {
    /// <summary>
    /// Provide a method for making variables in the Inspector not editable during runtime
    /// </summary>
    /// <remarks>
    /// Implementation taken from It3ration 'https://answers.unity.com/questions/489942/how-to-make-a-readonly-property-in-inspector.html'
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ReadOnlyDuringPlayAttribute : PropertyAttribute {
        #if UNITY_EDITOR
        /// <summary>
        /// Display a property while preventing write operations at run time
        /// </summary>
        [CustomPropertyDrawer(typeof(ReadOnlyDuringPlayAttribute))]
        private sealed class ReadOnlyDuringPlayAttributeDrawer : PropertyDrawer {
            /*----------Functions----------*/
            //PUBLIC

            /// <summary>
            /// Display the property while managing write access
            /// </summary>
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
                // Stash the current GUI enabled state
                bool prevState = GUI.enabled;

                // Flag the current write access
                GUI.enabled &= !Application.isPlaying;

                // Display the property
                EditorGUI.PropertyField(position, property, label, property.hasVisibleChildren);

                // Restore the GUI state
                GUI.enabled = prevState;
            }
        }
        #endif
    }
}
