using System;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MCU.Attributes {
    /// <summary>
    /// Mark a <see cref="String"/> or <see cref="UnityEngine.Color"/> field object as a web color field in the inspector
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class WebColorAttribute : PropertyAttribute {
        /*----------Properties----------*/
        //PUBLIC

        /// <summary>
        /// Flag if alpha values are allowed to be set in this property
        /// </summary>
        /// <remarks>If false the alpha will be kept at 255/1f</remarks>
        public bool AllowAlpha { get; private set; }

        /*----------Functions----------*/
        //PUBLIC

        /// <summary>
        /// Initialise this object with default values
        /// </summary>
        /// <param name="allowAlpha">Flag if the alpha channel should be adjustable. If false, will be kept at 255/1f</param>
        public WebColorAttribute(bool allowAlpha = true) { AllowAlpha = allowAlpha; }

#if UNITY_EDITOR
        /// <summary>
        /// Manage the displaying of a field marked as a Web Color Attribute
        /// </summary>
        [CustomPropertyDrawer(typeof(WebColorAttribute))]
        private sealed class WebColorAttributeDrawer : PropertyDrawer {
            /*----------Functions----------*/
            //PUBLIC

            /// <summary>
            /// Display the elements of the property within the designated area on the inspector area
            /// </summary>
            /// <param name="position">The position within the inspector that the property should be drawn to</param>
            /// <param name="property">The property that is to be displayed within the inspector</param>
            /// <param name="label">The label that has been assigned to the property</param>
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
                // Using BeginProperty / EndProperty on the parent property means that
                // prefab override logic works on the entire property.
                EditorGUI.BeginProperty(position, label, property);

                // Retrieve the reference to the attached attribute
                WebColorAttribute att = (WebColorAttribute)attribute;

                // Retrieve the current color value stored in the field
                string text = null;
                Color color;
                switch (property.propertyType) {
                    case SerializedPropertyType.String:
                        text = property.stringValue;
                        if (!ColorUtility.TryParseHtmlString(property.stringValue, out color))
                            text = (att.AllowAlpha ? ColorUtility.ToHtmlStringRGBA(color) : ColorUtility.ToHtmlStringRGB(color));
                        break;
                    case SerializedPropertyType.Color:
                        color = property.colorValue;
                        text = '#' + (att.AllowAlpha ? ColorUtility.ToHtmlStringRGBA(color) : ColorUtility.ToHtmlStringRGB(color));
                        break;
                    default:
                        EditorGUI.LabelField(position, label, new GUIContent("Invalid Field Type " + property.propertyType));
                        return;
                }

                // Prefix the label for this field onto it
                position = EditorGUI.PrefixLabel(position, label);

                // Flag if anything has changed
                bool hasChanged = false;

                // Display a delayed text field to grab entered text
                string newVal = EditorGUI.DelayedTextField(new Rect(position.x, position.y, position.width * .75f, position.height), text);
                if (newVal != text) {
                    // Try to parse the text into a color value that can be used
                    Color buffer;
                    if (ColorUtility.TryParseHtmlString(newVal, out buffer)) {
                        if (!att.AllowAlpha) buffer.a = 1f;
                        color = buffer;
                        text = '#' + (att.AllowAlpha ? ColorUtility.ToHtmlStringRGBA(buffer) : ColorUtility.ToHtmlStringRGB(buffer));
                        hasChanged = true;
                    }
                }

                // Display a color picker field for specific selection
                Color newCol = EditorGUI.ColorField(new Rect(position.x + position.width * .75f, position.y, position.width * .25f, position.height), color);
                if (newCol != color) {
                    text = '#' + (att.AllowAlpha ? ColorUtility.ToHtmlStringRGBA(newCol) : ColorUtility.ToHtmlStringRGB(newCol));
                    color = newCol;
                    hasChanged = true;
                }

                // If anything has changed, update the value
                if (hasChanged) {
                    switch (property.propertyType) {
                        case SerializedPropertyType.String:
                            property.stringValue = text;
                            break;
                        case SerializedPropertyType.Color:
                            property.colorValue = color;
                            break;
                    }
                }

                EditorGUI.EndProperty();
            }
        }
#endif
    }
}