#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using UnityEditor;

namespace MCU.Helpers {
    /// <summary>
    /// Provide additional common functionality that is largely shared between all custom editors
    /// </summary>
    public static class EditorHelper {
        /*----------Variables----------*/
        //CONST

        /// <summary>
        /// Store the flag types that will be used to search the the serialized property path structure
        /// </summary>
        private const BindingFlags SEARCH_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        /// <summary>
        /// Store the characters that will be split on when extracting the value index when path tracing
        /// </summary>
        private static readonly char[] INDEX_SPLIT_CHARS = { '[', ']' };

        /*----------Properties----------*/
        //PUBLIC

        /// <summary>
        /// Store the pixel indentation size that is used per indent level
        /// </summary>
        public static float IndentPerLevel { get; private set; }

        /// <summary>
        /// Store the pixel height that is used to display a standard single line property within the inspector
        /// </summary>
        public static float StandardLineHeight { get { return EditorGUIUtility.singleLineHeight; } }

        /// <summary>
        /// Store the pixel buffer space that is used between standard lines displayed
        /// </summary>
        public static float StandardVerticalSpacing { get; private set; }

        /// <summary>
        /// The combined vertical height that is used by a standard line
        /// </summary>
        public static float StandardLineVerticalTotal { get; private set; }

        /// <summary>
        /// Get the standard length of a label that is displayed within the inspector
        /// </summary>
        public static float LabelWidth { get { return EditorGUIUtility.labelWidth; } }

        /*----------Functions----------*/
        //PRIVATE

        /// <summary>
        /// Start the process of retrieving the required information that is needed for extraction
        /// </summary>
        static EditorHelper() { RecalculateValues(); }

        //PUBLIC

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////--------------------------------Property Drawing--------------------------------//////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Recalculate the stored value information, updating them from the internal sources
        /// </summary>
        public static void RecalculateValues() {
            // Store the flags that will be used to search for the internal values required
            BindingFlags searchFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            // Information being grabbed from EditorGUI
            Type editorGUI = typeof(EditorGUI);
            IndentPerLevel = (float)editorGUI.GetField("kIndentPerLevel", searchFlags).GetValue(null);
            StandardVerticalSpacing = 2;
            StandardLineVerticalTotal = StandardLineHeight + StandardVerticalSpacing;
        }

        /// <summary>
        /// Retrieve the total number of pixels high required to display the supplied number of standard lines
        /// </summary>
        /// <param name="lines">The number of lines that are to be displayed</param>
        /// <returns>Returns the total number of pixels required to display the lines</returns>
        public static float GetStandardPixelsForLines(int lines) { return StandardLineVerticalTotal * lines; }

        /// <summary>
        /// Retrieve a sub rect area from a master defined area
        /// </summary>
        /// <param name="master">The master rectangle that is being sub-divided</param>
        /// <param name="line">The standard line within the master rect that is to be retrieved</param>
        /// <param name="indent">The level of indentation that is to be added onto the current position</param>
        /// <param name="lineCount">The number of standard lines that this operation is to cover</param>
        /// <param name="xOffset">A normalised 0-1 scale additional indent as a percentage of the overall width</param>
        /// <param name="widthOffset">A normalised 0-1 scale width as a percentage of the overall width evaluated</param>
        /// <returns>Returns a sized and positioned sub-rect based off of the master</returns>
        public static Rect GetSubPositionRect(Rect master, int line, int indent = 0, int lineCount = 1, float xOffset = 0f, float widthOffset = 1f) {
            float width = (master.width - IndentPerLevel * indent);
            return new Rect(
                (master.x + IndentPerLevel * indent) + width * xOffset,
                master.y + line * StandardLineVerticalTotal,
                width * widthOffset,
                lineCount * StandardLineVerticalTotal
            );
        }

        /// <summary>
        /// Retrieve the rectangle used to position label content 
        /// </summary>
        /// <param name="master">The master rectangle that is having the label position calculated from</param>
        /// <returns>Returns a rect defining the position that would be used to display the label</returns>
        public static Rect GetLabelPositionRect(Rect master) {
            return new Rect(
                master.x + (EditorGUI.indentLevel * IndentPerLevel),
                master.y,
                LabelWidth - (EditorGUI.indentLevel * IndentPerLevel),
                StandardLineHeight
            );
        }

        /// <summary>
        /// Get the rect area that will be used if a label has been applied ahead of an area
        /// </summary>
        /// <param name="master">The master area that is being sub-divided to find the field area</param>
        /// <param name="label">Optional label that can be accounted for ahead of the field</param>
        /// <returns>Returns a rect area that marks the area the field would be displayed in</returns>
        public static Rect GetFieldPositionRect(Rect master, GUIContent label = null) {
            //Check if there is content to be displayed
            if (!LabelHasContent(label))
                return IndentedRect(master);

            //Otherwise, calculate based on the known label size
            return new Rect(
                master.x + LabelWidth, 
                master.y, 
                master.width - LabelWidth, 
                master.height
            );
        }

        /// <summary>
        /// Indent a rect by the current indentation level
        /// </summary>
        /// <param name="source">The source rect area that is to be indented</param>
        /// <returns>Returns the source rect area indented by the current setting</returns>
        public static Rect IndentedRect(Rect source) {
            return new Rect(
                source.x + (EditorGUI.indentLevel * IndentPerLevel),
                source.y,
                source.width - (EditorGUI.indentLevel * IndentPerLevel),
                source.height
            );
        }

        /// <summary>
        /// Test to see if the supplied label has any content stored in it for display
        /// </summary>
        /// <param name="label">The label that is to be displayed</param>
        /// <returns>Returns true if there is something to be displayed for the label</returns>
        public static bool LabelHasContent(this GUIContent label) {
            return (label == null || label.text != string.Empty ?
                true :
                label.image != null
            );
        }

        /// <summary>
        /// Retrieve the display label that is used for the supplied property
        /// </summary>
        /// <param name="property">The property that is to be converted into a label</param>
        /// <returns>Returns a GUI Content object setup from the property</returns>
        public static GUIContent GetPropertyLabel(this SerializedProperty property) {
            return (property != null ?
                new GUIContent(property.displayName, property.tooltip) :
                null
            );
        }

        /// <summary>
        /// Retrieve the underlying object value of the supplied SerializedProperty object
        /// </summary>
        /// <param name="property">The property that is having its value retrieved</param>
        /// <returns>Returns the object value that exists at the SerializedProperty location</returns>
        /// <remarks>
        /// Implementation based off of the following with additional tinkering:
        /// https://forum.unity.com/threads/get-a-general-object-value-from-serializedproperty.327098/
        /// https://answers.unity.com/questions/1347203/a-smarter-way-to-get-the-type-of-serializedpropert.html
        /// https://stackoverflow.com/questions/7072088/why-does-type-getelementtype-return-null
        /// https://answers.unity.com/questions/929293/get-field-type-of-serializedproperty.html
        /// </remarks>
        public static object GetPropertyValue(this SerializedProperty property) {
            // Retrieve the base object that the property is based off of
            object obj = property.serializedObject.targetObject;

            // Retrieve the sections of the path that must be traveled by reflection
            string[] pathSections = property.propertyPath.Split('.');

            // Travel down the property path sections
            FieldInfo field;
            for (int p = 0; p < pathSections.Length && obj != null; ++p) {
                // If this section is not an array of data, process it simple
                if (pathSections[p] != "Array" && p + 1 < pathSections.Length && pathSections[p + 1].StartsWith("data[")) {
                    // Retrieve the type of the current object reference
                    Type type = obj.GetType();

                    // Search won't find the private members of base classes, need to work way up inheritance chain
                    do {
                        // Get the field for the next section
                        field = type.GetField(pathSections[p], SEARCH_FLAGS);

                        // If not found, look in base class
                        if (field == null) type = type.BaseType;
                        else break;
                    } while (type != null);

                    // Get the object value at this section
                    obj = (field != null ? field.GetValue(obj) : null);
                }

                // Otherwise, retrieve the array element within the collection
                else {
                    // Retrieve the index element of the data to extract (String format should be 'data[##]')
                    string indexString = pathSections[++p].Split(INDEX_SPLIT_CHARS)[1];

                    // Attempt to parse the index
                    int index;
                    if (!int.TryParse(indexString, out index))
                        throw new OperationCanceledException(string.Format("While attempting to parse SerializedProperty path '{0}' the array index value was unable to be parsed from the string '{1}'. Expected data format was 'data[##]'", property.propertyPath, pathSections[p]));

                    // Ensure that this element is a list
                    if (!(obj is IList)) throw new NullReferenceException(string.Format("While attempting to parse SerializedProperty path '{0}' the collection object '{1}' ({2}) could not be retrieved", property.propertyPath, obj, pathSections[p - 2]));

                    // Get the list object that can be tested
                    IList list = (obj as IList);

                    // Get the object value at this section
                    obj = (index < list.Count ? list[index] : null);
                }
            }

            // Return the object at the end of the search
            return obj;
        }

        /// <summary>
        /// Retrieve the underlying object value of the SerializedProperty for all selected objects
        /// </summary>
        /// <param name="property">The property that is having its values retrieved</param>
        /// <returns>Returns an array of object values that exist for the SerializedProperty on all selected objects</returns>
        public static object[] GetPropertyValues(this SerializedProperty property) {
            // Retrieve the base object that the property is based off of
            object[] objs = property.serializedObject.targetObjects;
            object[] buffer = new object[objs.Length];

            // Check there are objects to modify
            if (objs.Length > 0) {
                //Retrieve the sections of the path that must be traveled by reflection
                string[] pathSections = property.propertyPath.Split('.');

                // Travel down the property path sections
                FieldInfo field;
                for (int p = 0; p < pathSections.Length; ++p) {
                    // Find the index of an object that is not null
                    int notNull = -1;
                    for (int i = 0; i < objs.Length; ++i) {
                        if (!object.Equals(objs[i], null)) {
                            notNull = i;
                            break;
                        }
                    }

                    // If there is no non-null object, can't search anymore
                    if (notNull == -1) break;

                    // If this is not an array of data, process it simple
                    if (pathSections[p] != "Array" && p + 1 < pathSections.Length && pathSections[p + 1].StartsWith("data[")) {
                        // Retrieve the type of the current object
                        Type type = objs[notNull].GetType();

                        // Search won't find the private members of base classes, need to work way up inheritance chain
                        do {
                            // Get the field for the next section
                            field = type.GetField(pathSections[p], SEARCH_FLAGS);

                            // If not found, look in base class
                            if (field == null) type = type.BaseType;
                            else break;
                        } while (type != null);

                        // Get the object values at the this section
                        for (int i = 0; i < objs.Length; ++i) {
                            buffer[i] = (objs[i] != null ?
                                field.GetValue(objs[i]) :
                                null
                            );
                        }

                        // Swap over the buffers for subsequent operations
                        object[] temp = objs;
                        objs = buffer;
                        buffer = temp;
                    }

                    // Otherwise, retrieve the array element within the collection
                    else {
                        // Retrieve the index element of the data to extract (String format should be 'data[##]')
                        string indexString = pathSections[++p].Split(INDEX_SPLIT_CHARS)[1];

                        // Attempt to parse the index
                        int index;
                        if (!int.TryParse(indexString, out index))
                            throw new OperationCanceledException(string.Format("While attempting to parse SerializedProperty path '{0}' the array index value was unable to be parsed from the string '{1}'. Expected data format was 'data[##]'", property.propertyPath, pathSections[p]));

                        // Ensure that this element is a list
                        if (!(objs[notNull] is IList)) throw new NullReferenceException(string.Format("While attempting to parse SerializedProperty path '{0}' the collection object '{1}' ({2}) could not be retrieved", property.propertyPath, objs[notNull], pathSections[p - 2]));

                        // Get the object values at this section
                        for (int i = 0; i < objs.Length; i++) {
                            // Get the list object that can be tested
                            IList list = (objs[i] as IList);

                            // Get the object value at this section
                            if (list != null)
                                objs[i] = (index < list.Count ? list[index] : null);
                        }
                    }
                }
            }

            // Return the final object values
            return objs;
        }

        /// <summary>
        /// Retrieve the underlying object value of the supplied SerializedProperty object cast to the expected type
        /// </summary>
        /// <typeparam name="T">The type that the evaluated value should be returned as</typeparam>
        /// <param name="property">The property that is having its value retrieved</param>
        /// <param name="value">The variable that will be filled with the retrieved value if it is of T</param>
        /// <returns>Returns true if the retrieved type was of the specified type, otherwise false</returns>
        public static bool GetPropertyValue<T>(this SerializedProperty property, out T value) {
            // Set the default value
            value = default(T);

            // Get the regular value from the property
            object val = property.GetPropertyValue();

            // If the value isn't of T, failed
            if (!(val is T)) return false;

            // Cast the value back
            value = (T)val;
            return true;
        }

        /// <summary>
        /// Retrieve the underlying object values of the SerializedProperty for all target objects, cast to the expected type
        /// </summary>
        /// <typeparam name="T">The type that the evaluated value should be returned as</typeparam>
        /// <param name="property">The property that is having its values retrieved</param>
        /// <param name="value">The array that will be filled with the retrieved value data if it is of T</param>
        /// <returns>Returns true if the retrieved type was of the specified type, otherwise false</returns>
        public static bool GetPropertyStructValues<T>(this SerializedProperty property, out T[] value) where T : struct {
            // Get the regular values from the property
            object[] vals = property.GetPropertyValues();

            // Create the return array to match size
            value = new T[vals.Length];

            // Loop through and assign the cast values
            for (int i = 0; i < vals.Length; i++) {
                // Check that the value will work
                if (!object.Equals(vals[i], null) && vals[i] is T)
                    value[i] = (T)vals[i];

                // Otherwise, type is a mismatch
                else {
                    value = null;
                    return false;
                }
            }

            // If gotten this far, success
            return true;
        }

        /// <summary>
        /// Retrieve the underlying object values of the SerializedProperty for all target objects, cast to the expected type
        /// </summary>
        /// <typeparam name="T">The type that the evaluated value should be returned as</typeparam>
        /// <param name="property">The property that is having its values retrieved</param>
        /// <param name="value">The array that will be filled with the retrieved value data if it is of T</param>
        /// <returns>Returns true if the retrieved type was of the specified type, otherwise false</returns>
        public static bool GetPropertyValues<T>(this SerializedProperty property, out T[] value) where T : class {
            // Get the regular values from the property
            object[] vals = property.GetPropertyValues();

            // Create the return array to match size
            value = new T[vals.Length];

            // Loop through and assign the cast values
            for (int i = 0; i < vals.Length; i++) {
                // Check that the value will work
                if (object.Equals(vals[i], null) || vals[i] is T)
                    value[i] = (T)vals[i];

                // Otherwise, type is a mismatch
                else {
                    value = null;
                    return false;
                }
            }

            // If gotten this far, success
            return true;
        }

        /// <summary>
        /// Reflect into the object structure of the specified property to find the type value for the element that it represents
        /// </summary>
        /// <param name="property">The property that is to have its type retrieved</param>
        /// <returns>Returns a Type object for the property or null if unable to find</returns>
        public static Type GetUnderlyingType(this SerializedProperty property) {
            // Retrieve the starting type to work down from
            Type type = property.serializedObject.targetObject.GetType();

            // Split the path based on the '.' character that separates the elements
            string[] segments = property.propertyPath.Split('.');

            // Cascade down the path
            FieldInfo info = null;
            for (int i = 0; i < segments.Length; ++i) {
                // If this path segment is 'Array', skip over it
                if (segments[i] == "Array" && i + 1 < segments.Length && segments[i + 1].StartsWith("data[")) {
                    // Skip over the array elements index in the file path
                    i += 2;

                    // Retrieve the type from this element
                    if (type.IsGenericType) {
                        // Retrieve the generic parameters of this object
                        Type[] genericArgs = type.GetGenericArguments();

                        // If there is not one, then we have a problem
                        if (genericArgs.Length != 1) {
                            Debug.LogError($"EditorHelper.GetUnderlyingType is not setup handle the retrieval of types from properties with generics with more then one type (E.g. IList<Type>). Unable to parse '{property.propertyPath}' due to the type {type.FullName}");
                            return null;
                        }

                        // Grab the type
                        type = genericArgs[0];
                    }

                    // Otherwise, just grab the base element type
                    else type = type.GetElementType();
                }

                // Get the current field info
                if (i < segments.Length) {
                    do {
                        // Look for the field within the specified object
                        info = type.GetField(segments[i], SEARCH_FLAGS);

                        // If no field found, look in the base class
                        if (info == null) type = type.BaseType;
                        else break;
                    } while (type != null);
                }

                // If the root is a collection, use the underlying type
                else break;

                // If the info was found, update the current type
                if (info != null) type = info.FieldType;

                // Otherwise, nothing could be found
                else {
                    Debug.LogError($"EditorHelper.GetUnderlyingType was unable to parse the type chain segment '{segments[i]}' in the relative path '{property.propertyPath}'. Could not identify the base type of the property");
                    return null;
                }
            }
            return type;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////-------------------------------Asset Manipulation-------------------------------//////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create a new Scriptable Object asset within the project
        /// </summary>
        /// <typeparam name="T">The type of scriptable object that is to be created</typeparam>
        /// <param name="defaultName">The default name that is to be assigned to the scriptable object</param>
        /// <returns>Returns a reference to the Scriptable Object asset that was created</returns>
        public static T CreateScriptableAsset<T>(string defaultName) where T : ScriptableObject { return CreateScriptableAsset(defaultName, typeof(T)) as T; }

        /// <summary>
        /// Create a new Scriptable Object asset within the project
        /// </summary>
        /// <param name="defaultName">The default name that is to be assigned to the scriptable object</param>
        /// <param name="assetType">The type of scriptable object asset that is to be created</param>
        /// <returns>Returns a reference to the Scriptable Object asset that was created</returns>
        public static ScriptableObject CreateScriptableAsset(string defaultName, Type assetType) {
            // Check that the type is valid
            if (!typeof(ScriptableObject).IsAssignableFrom(assetType)) {
                Debug.LogErrorFormat("Unable to create a Scriptable Object asset from type '{0}'. Type does not inherit ScriptableObject", assetType.FullName);
                return null;
            }

            // Create a new instance of the type
            ScriptableObject asset = ScriptableObject.CreateInstance(assetType);

            // Get the path of the currently selected object
            string path = AssetDatabase.GetAssetOrScenePath(Selection.activeObject);

            // If there is no path, use the default
            if (string.IsNullOrEmpty(path)) path = "Assets/";

            // If the path has an extension (I.e is a file) get the directory
            else if (!string.IsNullOrEmpty(Path.GetExtension(path)))
                path = path.Replace(Path.GetFileName(path), string.Empty);

            // If the default name is blank, use the type name
            if (string.IsNullOrEmpty(defaultName))
                defaultName = ObjectNames.NicifyVariableName(assetType.Name);

            // Ensure that hte path is a unique asset path
            path = AssetDatabase.GenerateUniqueAssetPath(string.Format("{0}/{1}.asset", path, defaultName));

            // Create the asset as a file in the project
            ProjectWindowUtil.CreateAsset(
                asset,
                path
            );

            // Save the changes to the asset database
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Bring the new asset into focus
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;

            return asset;
        }

        /// <summary>
        /// Search through all of the assets within the project and retrieve the first of the specified type
        /// </summary>
        /// <typeparam name="T">The type of asset to search for</typeparam>
        /// <returns>Returns the first instance of T that was found or null if none</returns>
        public static T FindFirstAssetOfType<T>() where T : UnityEngine.Object {
            // Find the GUIDs of all of the assets of the specified type
            string[] ids = AssetDatabase.FindAssets("t:" + typeof(T).Name);

            // Check all of the IDs that were found
            T asset;
            for (int i = 0; i < ids.Length; ++i) {
                // Get the path of the object
                string path = AssetDatabase.GUIDToAssetPath(ids[i]);

                // If the asset can be loaded, it can be used
                if (asset = AssetDatabase.LoadAssetAtPath<T>(path))
                    return asset;
            }

            // If got this far, nothing found
            return null;
        }

        /// <summary>
        /// Retrieve all of the assets of the specified type within the assets of this project
        /// </summary>
        /// <typeparam name="T">The type of assets to search for</typeparam>
        /// <returns>Returns a collection of the instances of T that were found in the project</returns>
        public static List<T> GetAssetsOfType<T>() where T : UnityEngine.Object {
            // Find the GUIDs of all of the assets of the specified type
            string[] ids = AssetDatabase.FindAssets("t:" + typeof(T).Name);

            // Create a list to hold the found elements
            List<T> found = new List<T>(ids.Length);

            // Loop through and check all of the identified elements
            T asset;
            for (int i = 0; i < ids.Length; ++i) {
                // Get the path of the object
                string path = AssetDatabase.GUIDToAssetPath(ids[i]);

                // Attempt to load the asset
                if (asset = AssetDatabase.LoadAssetAtPath<T>(path))
                    found.Add(asset);
            }
            return found;
        }

        /// <summary>
        /// Retrieve all of the assets of the specified implementation type, cast to a specified base
        /// </summary>
        /// <typeparam name="TBase">The base type of the assets that the assets should be retrieved as</typeparam>
        /// <param name="implementationType">The type of asset that is to be retrieved</param>
        /// <returns>Returns a collection of the instances of implementationType that could be found</returns>
        public static List<TBase> GetAssetsOfType<TBase>(Type implementationType) where TBase : UnityEngine.Object {
            // Check that the implementation type can be cast to base
            if (!typeof(TBase).IsAssignableFrom(implementationType))
                return new List<TBase>();

            // Find the GUIDs of all of the implementation type assets
            string[] ids = AssetDatabase.FindAssets("t:" + implementationType.Name);

            // Create a list to hold the found elements
            List<TBase> found = new List<TBase>(ids.Length);

            // Loop through and check all of the identified elements
            TBase asset;
            for (int i = 0; i < ids.Length; ++i) {
                // Get the path of the object
                string path = AssetDatabase.GUIDToAssetPath(ids[i]);

                // Attempt to load the asset
                if (asset = AssetDatabase.LoadAssetAtPath<TBase>(path))
                    found.Add(asset);
            }
            return found;
        }

        /// <summary>
        /// Iterate over all assets of a specified type
        /// </summary>
        /// <typeparam name="T">The type of assets to search for</typeparam>
        /// <returns>Iterates over all assets of the specified type that could be found</returns>
        public static IEnumerator<T> ForAssetsOfType<T>() where T : UnityEngine.Object {
            // Find the GUIDs of all of the assets of the specified type
            string[] ids = AssetDatabase.FindAssets("t:" + typeof(T).Name);

            // Loop through and check all of the identified elements
            T asset;
            for (int i = 0; i < ids.Length; ++i) {
                // Get the path of the object
                string path = AssetDatabase.GUIDToAssetPath(ids[i]);

                // Attempt to load the asset
                if (asset = AssetDatabase.LoadAssetAtPath<T>(path))
                    yield return asset;
            }
        }

        /// <summary>
        /// Iterate over all assets of a specified type, cast to a specified base
        /// </summary>
        /// <typeparam name="TBase">The base type of the assets that the assets should be retrieved as</typeparam>
        /// <param name="implementationType">The type of asset that is to be retrieved</param>
        /// <returns>Iterates over all assets of the specified type that could be found</returns>
        public static IEnumerator<TBase> ForAssetsOfType<TBase>(Type implementationType) where TBase : UnityEngine.Object {
            // Find the GUIDs of all of the assets of the specified type
            string[] ids = AssetDatabase.FindAssets("t:" + implementationType.Name);

            // Loop through and check all of the identified elements
            TBase asset;
            for (int i = 0; i < ids.Length; ++i) {
                // Get the path of the object
                string path = AssetDatabase.GUIDToAssetPath(ids[i]);

                // Attempt to load the asset
                if (asset = AssetDatabase.LoadAssetAtPath<TBase>(path))
                    yield return asset;
            }
        }
    }
}
#endif