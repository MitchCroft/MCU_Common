using UnityEngine;

#if UNITY_EDITOR
using System;
using System.Text;
using System.Collections.Generic;

using UnityEditor;
#endif

namespace MCU.Helpers {
    /// <summary>
    /// Manage a collection of scriptable objects that can be created and assigned like enumeration values
    /// </summary>
    public abstract class ScriptableEnum : ScriptableObject {
        /*----------Functions----------*/
        //PUBLIC

        /// <summary>
        /// Use the name of the asset for the string representation
        /// </summary>
        /// <returns>Returns the name of the object</returns>
        public override string ToString() { return name; }

#if UNITY_EDITOR
        /// <summary>
        /// Display the Scriptable Enum field as a dropdown selection of options based on the assets in the project
        /// </summary>
        [CustomPropertyDrawer(typeof(ScriptableEnum), true)]
        private sealed class ScriptableEnumDrawer : PropertyDrawer {
            /*----------Types----------*/
            //PRIVATE

            /// <summary>
            /// Store a pre-cached collection of values that can be used for enum object selection
            /// </summary>
            private sealed class EnumOptionsCache {
                /*----------Properties----------*/
                //PUBLIC

                /// <summary>
                /// The type of <see cref="ScriptableEnum"/> assets that are stored in this cache
                /// </summary>
                public Type EnumType { get; private set; }

                /// <summary>
                /// Flags if all of the options within this cache are of the same type
                /// </summary>
                public bool ContainsMixedTypes { get; private set; }

                /// <summary>
                /// The collection of enumeration assets that have been identified for use
                /// </summary>
                public ScriptableEnum[] EnumOptions { get; private set; }

                /// <summary>
                /// Provide a quick method of looking up the index of the currently selected enum in the popup
                /// </summary>
                public Dictionary<UnityEngine.Object, int> EnumToIndex { get; private set; }

                /// <summary>
                /// The collection of enumeration labels that will be displayed in the popup
                /// </summary>
                /// <remarks>This collection has the length of <see cref="EnumOptions"/> + an additional at the beginning for the null option</remarks>
                public GUIContent[] EnumLabels { get; private set; }

                /// <summary>
                /// Store a collection of the different types that can be created to fit into this option
                /// </summary>
                public Type[] CreateableTypes { get; private set; }

                /*----------Functions----------*/
                //PUBLIC

                /// <summary>
                /// Initialise this cache object for the assets of the specified type
                /// </summary>
                /// <param name="enumType">The type of <see cref="ScriptableEnum"/> assets that are stored in this cache</param>
                public EnumOptionsCache(Type enumType) {
                    // Stash the enum type to be used
                    EnumType = enumType;

                    // Compile a collection of all of the assets of the type in the project
                    List<ScriptableEnum> options = EditorHelper.GetAssetsOfType<ScriptableEnum>(enumType);

                    // Check if all of the assets are of the same type
                    ContainsMixedTypes = false;
                    Type checkType = (options.Count > 0 ? options[0].GetType() : null);
                    for (int i = 1; i < options.Count; ++i) {
                        if (options[i].GetType() != checkType) {
                            ContainsMixedTypes = true;
                            break;
                        }
                    }

                    // Determine the strings that will be used to represent the options
                    Dictionary<ScriptableEnum, string> nameLookup = new Dictionary<ScriptableEnum, string>(options.Count);
                    if (ContainsMixedTypes) {
                        foreach (ScriptableEnum val in options)
                            nameLookup[val] = string.Format("{0}/{1}", val.GetType().FullName.Replace('.', '/').Replace('+', '/'), val.name);
                    } else {
                        foreach (ScriptableEnum val in options)
                            nameLookup[val] = val.name;
                    }

                    // Sort the options based on their paths
                    options.Sort((l, r) => nameLookup[l].CompareTo(nameLookup[r]));

                    // Finalise the lookup elements
                    EnumOptions = options.ToArray();
                    EnumToIndex = new Dictionary<UnityEngine.Object, int>();
                    EnumLabels  = new GUIContent[EnumOptions.Length + 1]; // Need the initial slot for the clear entry
                    for (int i = 0; i < EnumOptions.Length; ++i) {
                        EnumToIndex[EnumOptions[i]] = i;
                        EnumLabels[i + 1] = new GUIContent(nameLookup[EnumOptions[i]]);
                    }

                    // Find the types that can be used to create an asset for the field
                    List<Type> createableTypes = new List<Type>(
                        AssemblyTypeScanner.ForTypesWithinAssembly(
                            enumType,
                            x => !x.IsAbstract
                        )
                    );
                    createableTypes.Sort((l, r) => l.Name.CompareTo(r.Name));
                    CreateableTypes = createableTypes.ToArray();
                }
            }

            /*----------Variables----------*/
            //CONST

            /// <summary>
            /// The array format that is stored within a property path to be removed
            /// </summary>
            private const string ARRAY_PATH_FORMAT = ".Array.data[";

            /// <summary>
            /// Cache the scriptable enum type for validity checks
            /// </summary>
            private static readonly Type SCRIPT_ENUM_TYPE = typeof(ScriptableEnum);

            /// <summary>
            /// Store the normalised percentage value for the amount of space the create button should take up
            /// </summary>
            private const float CREATE_BTN_NORM_WIDTH = .1f;

            //PRIVATE

            /// <summary>
            /// Cache the type that will be used to lookup the type of enumeration object that is to be selectable
            /// </summary>
            private Dictionary<int, Type> propertyToTypeLookup = new Dictionary<int, Type>();

            /// <summary>
            /// Cache the different options for possible types for re-use
            /// </summary>
            private Dictionary<Type, EnumOptionsCache> typeToCacheLookup = new Dictionary<Type, EnumOptionsCache>();

            /// <summary>
            /// The labels that will be recycled as needed for the different properties that are displayed
            /// </summary>
            private GUIContent emptyLabel   = new GUIContent("<-- None -->", "No reference to a Scriptable Enum value"),
                               missingLabel = new GUIContent("<-- Missing -->", "The enum option that was referenced is misisng"),
                               errorLabel   = new GUIContent("Error: Can't list", "Something went wrong when identifying the types to display. See the console for more information"),
                               createLabel  = new GUIContent("+", "Create a new asset that can be assigned to this reference");

            /// <summary>
            /// Reusable string buffer to use when simplifying property paths
            /// </summary>
            private StringBuilder stringBuffer = new StringBuilder();

            /*----------Functions----------*/
            //PRIVATE

            /// <summary>
            /// Retrieve the enum options cache data required to display for the specified property
            /// </summary>
            /// <param name="property">The property whose type will be determined for available popup options</param>
            /// <returns>Returns an enumeration cache object that matches the property type</returns>
            private EnumOptionsCache GetPropertyCacheData(SerializedProperty property) {
                // Get the cleaned up path string for cache access
                int pathHash = GetSimplifiedPathHash(property);

                // Get the type of scriptable enum that is to be displayed
                Type propertyType = null;
                if (!propertyToTypeLookup.TryGetValue(pathHash, out propertyType)) {
                    // Get the underlying type of the property
                    propertyType = property.GetUnderlyingType();

                    // If this isn't assignable to a scriptable enum type, then something has gone wrong
                    if (!SCRIPT_ENUM_TYPE.IsAssignableFrom(propertyType)) {
                        Debug.LogError($"Unable to display cached options for type '{propertyType.FullName}' as it doesn't inherit ScriptableEnum. something went wrong");
                        propertyType = null;
                    }

                    // Store the type for later
                    propertyToTypeLookup[pathHash] = propertyType;
                }

                // If there is no property type, then we can't do anything
                if (propertyType == null) return null;

                // Otherwise, return the options available
                return (typeToCacheLookup.ContainsKey(propertyType) ?
                    typeToCacheLookup[propertyType] :
                    typeToCacheLookup[propertyType] = new EnumOptionsCache(propertyType)
                );
            }

            /// <summary>
            /// Retrieve the simplified version of the property's as a hash for cache lookup
            /// </summary>
            /// <param name="property">The property that is to have its path simplified</param>
            /// <returns>The hash code for the properties path</returns>
            private int GetSimplifiedPathHash(SerializedProperty property) {
                // Get the path that will be tested
                string path = property.propertyPath;

                // Check if the property path has array markers in it
                if (!path.Contains(ARRAY_PATH_FORMAT))
                    return path.GetHashCode();

                // Otherwise the array elements need to be removed
                stringBuffer.Clear();
                for (int prog = 0; prog < path.Length;) {
                    // Find the next array element
                    int end = path.IndexOf(ARRAY_PATH_FORMAT, prog);

                    // If there was nothing else, just take the rest of the text
                    if (end == -1) end = path.Length;

                    // Append the leading up text
                    stringBuffer.Append(path.Substring(prog, end - prog));

                    // Look for the closing part of the array section
                    end = path.IndexOf(']', end);
                    if (end == -1) break;

                    // Otherwise, progress past the array closing
                    prog = end + 1;
                }

                // Return the final compiled string
                return stringBuffer.ToString().GetHashCode();
            }

            /// <summary>
            /// Create a new ScriptableEnum asset for use in the project
            /// </summary>
            /// <param name="enumType">The type of the enumeration asset to create</param>
            /// <returns>Returns a reference to the new asset that was created</returns>
            private static ScriptableEnum CreateEnumOption(Type enumType) {
                // Create a new instance of the type
                ScriptableEnum asset = ScriptableObject.CreateInstance(enumType) as ScriptableEnum;

                // Ensure that the path is unique
                string path = AssetDatabase.GenerateUniqueAssetPath(string.Format("Assets/{0}.asset", ObjectNames.NicifyVariableName(enumType.Name)));

                // Create the asset as a file in the project
                ProjectWindowUtil.CreateAsset(
                    asset,
                    path
                );

                // Save the changes to the asset database
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return asset;
            }

            //PUBLIC

            /// <summary>
            /// Display the popup options for the specified Scriptable Enumeration value
            /// </summary>
            /// <param name="position">The position within the inspector that the property field should be displayed</param>
            /// <param name="property">The property object that is to be displayed</param>
            /// <param name="label">The label that has has been assigned to the property to be displayed</param>
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
                // Get the cache data for this property
                EnumOptionsCache cache = GetPropertyCacheData(property);
                if (cache == null) {
                    EditorGUI.LabelField(position, label, errorLabel);
                    return;
                }

                // Using BeginProperty / EndProperty on the parent property means that
                // prefab override logic works on the entire property.
                EditorGUI.BeginProperty(position, label, property);

                // Draw label
                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

                // Don't make child fields be indented
                var indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                // Find the index of the item currently selected
                int curInd = (property.objectReferenceValue && cache.EnumToIndex.ContainsKey(property.objectReferenceValue) ?
                    cache.EnumToIndex[property.objectReferenceValue] + 1 :
                    0
                );

                // Substitute in the label that is needed for this 
                cache.EnumLabels[0] = (property.objectReferenceValue == null && property.objectReferenceInstanceIDValue != 0 ?
                    missingLabel :
                    emptyLabel
                );

                // If the type isn't abstract then we can have a create option
                bool canCreate = cache.CreateableTypes.Length > 0;

                // Display the list of options for selection
                int newInd = EditorGUI.Popup(
                    EditorHelper.GetSubPositionRect(position, 0, 0, 1, 0, canCreate ? 1f - CREATE_BTN_NORM_WIDTH : 1f), 
                    curInd, 
                    cache.EnumLabels
                );

                // If the index was modified, update the object reference
                if (newInd != curInd) {
                    property.objectReferenceValue = (newInd > 0 && newInd - 1 < cache.EnumOptions.Length ?
                        cache.EnumOptions[newInd - 1] :
                        null
                    );
                }

                // If we can create, offer a button to start the process
                if (canCreate) {
                    if (GUI.Button(EditorHelper.GetSubPositionRect(position, 0, 0, 1, 1f - CREATE_BTN_NORM_WIDTH, CREATE_BTN_NORM_WIDTH), createLabel)) {
                        // If there is only one option then we can just create off of that
                        if (cache.CreateableTypes.Length == 1) 
                            property.objectReferenceValue = CreateEnumOption(cache.CreateableTypes[0]);

                        // Otherwise, we need to create a menu of options that can be selected from
                        else {
                            // Create the generic menu to add possible options to
                            GenericMenu createMenu = new GenericMenu();
                            foreach (Type assetType in cache.CreateableTypes) {
                                Type type = assetType;
                                createMenu.AddItem(new GUIContent(type.Name), false, () => {
                                    property.objectReferenceValue = CreateEnumOption(type);
                                    property.serializedObject.ApplyModifiedProperties();
                                });
                            }

                            // Show the menu as context from the current location
                            createMenu.ShowAsContext();
                        }
                    }
                }

                // Set indent back to what it was
                EditorGUI.indentLevel = indent;

                EditorGUI.EndProperty();
            }
        }
#endif
    }
}