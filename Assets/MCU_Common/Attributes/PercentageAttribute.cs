using System;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MCU.Attributes {
    /// <summary>
    /// Display a floating point property as a percentage
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class PercentageAttribute : PropertyAttribute {
        /*----------Properties----------*/
        //PUBLIC

        /// <summary>
        /// The minimum value that will be displayed on the slider bar
        /// </summary>
        public float Min { get; private set; }

        /// <summary>
        /// The maximum value that will be displayed on the slider bar
        /// </summary>
        public float Max { get; private set; }

        /// <summary>
        /// Flag if the value should be bound to the specified min/max range
        /// </summary>
        public bool BindMinMax { get; private set; }

        /*----------Functions----------*/
        //PUBLIC

        /// <summary>
        /// Initialise this object with the default display settings
        /// </summary>
        public PercentageAttribute() {
            Min = 0f;
            Max = 1f;
            BindMinMax = true;
        }

        /// <summary>
        /// Initialise this object with the display settings
        /// </summary>
        /// <param name="min">The minimum value that will be used as the left of the slider bar</param>
        /// <param name="max">The maximum value that will be used as the right of the slider bar</param>
        /// <param name="bindMinMax">Flags if the specified value should be bound to the min/max range</param>
        public PercentageAttribute(float min, float max, bool bindMinMax = true) {
            Min = min;
            Max = max;
            BindMinMax = bindMinMax;
        }

        /// <summary>
        /// Initialise this object with the display settings
        /// </summary>
        /// <param name="min">The minimum value that will be used as the left of the slider bar</param>
        /// <param name="max">The maximum value that will be used as the right of the slider bar</param>
        /// <param name="bindMinMax">Flags if the specified value should be bound to the min/max range</param>
        public PercentageAttribute(int min, int max, bool bindMinMax = true) {
            Min = min;
            Max = max;
            BindMinMax = bindMinMax;
        }

        #if UNITY_EDITOR
        /// <summary>
        /// Display the percentage value within the inspector area
        /// </summary>
        [CustomPropertyDrawer(typeof(PercentageAttribute))]
        private sealed class PercentageAttributeDrawer : PropertyDrawer {
            /*----------Functions----------*/
            //PUBLIC

            /// <summary>
            /// Display the elements of the property within the designated area on the inspector area
            /// </summary>
            /// <param name="position">The position within the inspector that the property should be drawn to</param>
            /// <param name="property">The property that is to be displayed within the inspector</param>
            /// <param name="label">The label that has been assigned to the property</param>
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
                // Check the property is a float
                if (property.propertyType != SerializedPropertyType.Float &&
                    property.propertyType != SerializedPropertyType.Integer) {
                    EditorGUI.LabelField(position, label, new GUIContent("Percentage Attribute valid on number only"));
                    return;
                }

                // Using BeginProperty / EndProperty on the parent property means that
                // prefab override logic works on the entire property.
                EditorGUI.BeginProperty(position, label, property);

                // Prefix the area with the label
                position = EditorGUI.PrefixLabel(position, label);

                // Calculate the positions for different control areas
                Rect sliderPosition = new Rect(position.x, position.y, position.width * .65f, position.height);
                Rect fieldPosition = new Rect(position.x + position.width * .7f, position.y, position.width * .3f, position.height);

                // Get the attribute that is being drawn
                PercentageAttribute att = (PercentageAttribute)attribute;

                // Switch on the property type
                if (property.propertyType == SerializedPropertyType.Float) {
                    // Get the percentage value in 0-100 style format
                    float percentage = property.floatValue * 100f;

                    // Start checking for changes
                    EditorGUI.BeginChangeCheck();

                    // Display the slider field for the value
                    percentage = GUI.HorizontalSlider(sliderPosition, percentage, att.Min * 100f, att.Max * 100f);

                    // Display the regular field for the value
                    percentage = EditorGUI.FloatField(fieldPosition, percentage);

                    // If anything changed, apply the value
                    if (EditorGUI.EndChangeCheck())
                        property.floatValue = (att.BindMinMax ? Mathf.Clamp(percentage * .01f, att.Min, att.Max) : percentage * .01f);
                } else {
                    // Get the percentage value in 0-100 style format
                    int percentage = property.intValue;

                    // Start checking for changes
                    EditorGUI.BeginChangeCheck();

                    // Display the slider field for the value
                    percentage = (int)GUI.HorizontalSlider(sliderPosition, percentage, (int)att.Min, (int)att.Max);

                    // Display the regular field for the value
                    percentage = EditorGUI.IntField(fieldPosition, percentage);

                    // If anything changed, apply the value
                    if (EditorGUI.EndChangeCheck())
                        property.intValue = (att.BindMinMax ? Mathf.Clamp(percentage, (int)att.Min, (int)att.Max) : percentage);
                }

                // End the prefab-able section
                EditorGUI.EndProperty();
            }
        }
        #endif
    }
}