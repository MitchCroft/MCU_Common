#if UNITY_EDITOR
using System;
using System.Reflection;

using UnityEngine;
using UnityEditor;

namespace MCU.EditorScreens {
    /// <summary>
    /// Handle the generation of label content for display
    /// </summary>
    public static class LabelGeneration {
        /*----------Functions----------*/
        //PUBLIC

        /// <summary>
        /// Retrieve the nicified names for enumeration elements in the supplied type
        /// </summary>
        /// <param name="enumType">The <see cref="System.Enum"/> type that is to be processed</param>
        /// <param name="customReplacements">[Optional] A callback function that can be used to provide additional processing to the label text that is to be returned (E.g. replacing characters)</param>
        /// <returns>Returns the labels in the order they were defined in the enumeration type, or null if not an enumeration type</returns>
        /// <remarks>
        /// Names are nicified through the application of <see cref="UnityEditor.ObjectNames.NicifyVariableName(string)"/>
        /// </remarks>
        public static string[] GetNicifiedEnumNames(Type enumType, Func<string, string> additionalProcessing = null) {
            // If the type isn't an enumeration, nothing we can do
            if (!enumType.IsEnum) {
                Debug.LogErrorFormat("Unable to get the nicified enumeration names for the type '{0}'. Type is not an enumeration", enumType);
                return null;
            }

            // If there is no processing callback, just use what is supplied
            if (additionalProcessing == null)
                additionalProcessing = x => x;

            // Get the labels for the enumeration
            string[] labels = Enum.GetNames(enumType);
            for (int i = 0; i < labels.Length; ++i) {
                labels[i] = additionalProcessing(
                    ObjectNames.NicifyVariableName(labels[i])
                );
            }
            return labels;
        }

        /// <summary>
        /// Retrieve the nicified names for enumeration elements in the supplied type
        /// </summary>
        /// <typeparam name="TEnum">The <see cref="System.Enum"/> type that is to be processed</typeparam>
        /// <param name="customReplacements">[Optional] A callback function that can be used to provide additional processing to the label text that is to be returned (E.g. replacing characters)</param>
        /// <returns>Returns the labels in the order they were defined in the enumeration type</returns>
        /// <remarks>
        /// Names are nicified through the application of <see cref="UnityEditor.ObjectNames.NicifyVariableName(string)"/>
        /// </remarks>
        public static string[] GetNicifiedEnumNames<TEnum>(Func<string, string> additionalProcessing = null) where TEnum : Enum { return GetNicifiedEnumNames(typeof(TEnum), additionalProcessing); }

        /// <summary>
        /// Retrieve the GUI Content objects that can be used to represent the specified names
        /// </summary>
        /// <param name="names">The names that are to be created into content objects to be displayed</param>
        /// <returns>Returns a GUIContent object for each supplied name in the matching index location</returns>
        public static GUIContent[] CreateLabelsContent(params string[] names) {
            GUIContent[] content = new GUIContent[names.Length];
            for (int i = 0; i < names.Length; ++i)
                content[i] = new GUIContent(names[i]);
            return content;
        }

        /// <summary>
        /// Retrieve GUI Content objects with nicified names for enumeration elements in the supplied type
        /// </summary>
        /// <param name="enumType">The <see cref="System.Enum"/> type that is to be processed</param>
        /// <param name="customReplacements">[Optional] A callback function that can be used to provide additional processing to the label text that is to be returned (E.g. replacing characters)</param>
        /// <returns>Returns a GUIContent object for each entry in the numeration type in the order they are defined, or null if not an enumeration type</returns>
        /// <remarks>
        /// Names are nicified through the application of <see cref="UnityEditor.ObjectNames.NicifyVariableName(string)"/>
        /// </remarks>
        public static GUIContent[] CreateEnumContent(Type enumType, Func<string, string> additionalProcessing = null) {
            // If the type isn't an enumeration, nothing we can do
            if (!enumType.IsEnum) {
                Debug.LogErrorFormat("Unable to get the nicified enumeration names for the type '{0}'. Type is not an enumeration", enumType);
                return null;
            }

            // If there is no processing callback, just use what is supplied
            if (additionalProcessing == null)
                additionalProcessing = x => x;

            // Get the entrys within the enumeration type
            string[] labels = Enum.GetNames(enumType);
            GUIContent[] contents = new GUIContent[labels.Length];
            for (int i = 0; i < contents.Length; ++i) {
                // Look for a tooltip to be assigned to the label
                string tooltip = string.Empty;
                MemberInfo[] memberInfos = enumType.GetMember(labels[i]);
                if (memberInfos.Length > 0) {
                    // Look for the enum member entry for this array
                    MemberInfo enumEntry = Array.Find(memberInfos, x => x.DeclaringType == enumType);
                    if (enumEntry != null) {
                        // Look for a tooltip associated with this member value
                        TooltipAttribute attribute = enumEntry.GetCustomAttribute<TooltipAttribute>(false);
                        if (attribute != null)
                            tooltip = attribute.tooltip;
                    }
                }

                // Get the display name for this entry
                labels[i] = additionalProcessing(
                    ObjectNames.NicifyVariableName(labels[i])
                );

                // Create the content object 
                contents[i] = new GUIContent(labels[i], tooltip);
            }
            return contents;
        }

        /// <summary>
        /// Retrieve GUI Content objects with nicified names for enumeration elements in the supplied type
        /// </summary>
        /// <typeparam name="TEnum">The <see cref="System.Enum"/> type that is to be processed</typeparam>
        /// <param name="customReplacements">[Optional] A callback function that can be used to provide additional processing to the label text that is to be returned (E.g. replacing characters)</param>
        /// <returns>Returns a GUIContent object for each entry in the numeration type in the order they are defined</returns>
        /// <remarks>
        /// Names are nicified through the application of <see cref="UnityEditor.ObjectNames.NicifyVariableName(string)"/>
        /// </remarks>
        public static GUIContent[] CreateEnumContent<TEnum>(Func<string, string> additionalProcessing = null) where TEnum : Enum { return CreateEnumContent(typeof(TEnum), additionalProcessing); }
    }
}
#endif