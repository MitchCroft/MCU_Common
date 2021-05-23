using System;
using System.Reflection;
using System.Collections.Generic;

namespace MCU.Helpers {
    /// <summary>
    /// Provide a generic method for cloning objects
    /// </summary>
    /// <remarks>Implementation inspired by https://stackoverflow.com/a/11308879/4608292 </remarks>
    public static class CopyFactory {
        /*----------Types----------*/
        //PRIVATE

        /// <summary>
        /// Traverse through all of the elements within an array object, across all dimensions
        /// </summary>
        private sealed class ArrayTraverse {
            /*----------Variables----------*/
            //PRIVATE

            /// <summary>
            /// The maximum lengths of the array across its dimensions
            /// </summary>
            private int[] maxLengths;

            /*----------Properties----------*/
            //PUBLIC

            /// <summary>
            /// The current traversal position of the object across the array dimensions
            /// </summary>
            public int[] Position { get; private set; }

            /*----------Functions----------*/
            //PUBLIC

            /// <summary>
            /// Create this traversal object based on an array object
            /// </summary>
            /// <param name="array">The base array to be traversed</param>
            public ArrayTraverse(Array array) {
                maxLengths = new int[array.Rank];
                for (int i = 0; i < array.Rank; ++i)
                    maxLengths[i] = array.GetLength(i) - 1;
                Position = new int[array.Rank];
            }

            /// <summary>
            /// Increase the step through the internal array adjust the current traversal position
            /// </summary>
            /// <returns>Returns true if the current step is valid for use</returns>
            public bool Step() {
                for (int i = 0; i < Position.Length; ++i) {
                    if (Position[i] < maxLengths[i]) {
                        ++Position[i];
                        for (int j = 0; j < i; j++)
                            Position[j] = 0;
                        return true;
                    }
                }
                return false;
            }
        }

        //PUBLIC

        /// <summary>
        /// Compare generic object references to determine if their references are equal
        /// </summary>
        public sealed class ReferenceEqualityComparer : EqualityComparer<object> {
            /*----------Functions----------*/
            //PUBLIC

            /// <summary>
            /// Check to see if the two objects reference the same object in memory
            /// </summary>
            /// <param name="x">The first object to be tested</param>
            /// <param name="y">The second object to be tested</param>
            /// <returns>Returns true if both objects refer to the same memory object</returns>
            public override bool Equals(object x, object y) { return ReferenceEquals(x, y); }

            /// <summary>
            /// Retrieve the hashcode of the supplied object
            /// </summary>
            /// <param name="obj">The object that is to be hashed</param>
            /// <returns>Returns an integral representation of the supplied object</returns>
            public override int GetHashCode(object obj) {
                return (obj == null ?
                    0 :
                    obj.GetHashCode()
                );
            }
        }

        /// <summary>
        /// Define an object that can be used to specifically handle the copying of specific types
        /// </summary>
        public abstract class SpecialisedCopyProcessor {
            /*----------Functions----------*/
            //PUBLIC

            /// <summary>
            /// Test to see if this processor can copy an object of the specified type
            /// </summary>
            /// <param name="type">The type of the object that is to be copied</param>
            /// <param name="original">The original object itself that is to be copied. Could be null if value is unknown at time of test</param>
            /// <returns>Returns true if this processor can handle the copying of the object</returns>
            public abstract bool CanCopy(Type type, object original);

            /// <summary>
            /// Handle the process of making a copy of the supplied object
            /// </summary>
            /// <param name="original">The object that is to be copied</param>
            /// <param name="type">The type of the object that is to be copied</param>
            /// <returns>Returns a copy of the original object for use</returns>
            public abstract object ProcessCopy(object original, Type type);
        }

        /// <summary>
        /// Manage a collection of specialised copy processors that can be used to retrieve specialised processors
        /// </summary>
        public sealed class SpecialisedCopyManager {
            /*----------Variables----------*/
            //PRIVATE

            /// <summary>
            /// Store the collection of processors that can be retrieved from this manager
            /// </summary>
            /// <remarks>The processors are included in this array in priority order</remarks>
            private SpecialisedCopyProcessor[] processors;

            /*----------Functions----------*/
            //PUBLIC
            
            /// <summary>
            /// Initialise this manager with a collection of processors to use for assignments
            /// </summary>
            /// <param name="processors">The collection of processors that can be used to handle the copying process</param>
            public SpecialisedCopyManager(params SpecialisedCopyProcessor[] processors) {
                this.processors = processors;
            }

            /// <summary>
            /// Find a contained specialised processor that can be used to handle the copying of the supplied object
            /// </summary>
            /// <param name="type">The type of the object that is to be copied</param>
            /// <param name="original">The object that is to be copied. Could be null if value is unknown at time of test</param>
            /// <returns>Returns an internal copy processor if a valid match was found else null</returns>
            public SpecialisedCopyProcessor FindProcessor(Type type, object original) {
                for (int i = 0; i < processors.Length; ++i) {
                    if (processors[i].CanCopy(type, original))
                        return processors[i];
                }
                return null;
            }
        }

        /// <summary>
        /// Store a collection of settings that are to be shared for a factory copy operation
        /// </summary>
        public sealed class CopySettings {
            /*----------Properties----------*/
            //PRIVATE

            /// <summary>
            /// A collection of previously copied objects that can be reused in the process
            /// </summary>
            public IDictionary<object, object> ObjectCache { get; private set; }

            /// <summary>
            /// A collection of specialised copy managers that will be used to handle copy operations for specific types specifically
            /// </summary>
            public SpecialisedCopyManager SpecialisedProcessors { get; private set; }

            /*----------Functions----------*/
            //PUBLIC

            /// <summary>
            /// Initialise the copy settings object with default settings
            /// </summary>
            public CopySettings() {
                ObjectCache = new Dictionary<object, object>(new ReferenceEqualityComparer());
                SpecialisedProcessors = new SpecialisedCopyManager();
            }

            /// <summary>
            /// Initialise the copy settings object with a specific object cache reference
            /// </summary>
            /// <param name="objectCache">The dictionary object that will be used to facilitate cached object re-use</param>
            public CopySettings(IDictionary<object, object> objectCache) {
                ObjectCache = objectCache;
                SpecialisedProcessors = new SpecialisedCopyManager();
            }

            /// <summary>
            /// Initialise the copy settings object with defined specialised processors
            /// </summary>
            /// <param name="specialisedProcessors">The specialised processor manager that will be used to handle copying of specific objects</param>
            public CopySettings(SpecialisedCopyManager specialisedProcessors) {
                ObjectCache = new Dictionary<object, object>(new ReferenceEqualityComparer());
                SpecialisedProcessors = specialisedProcessors;
            }

            /// <summary>
            /// Initialise the copy settings object with defined elements
            /// </summary>
            /// <param name="objectCache">The dictionary object that will be used to facilitate cached object re-use</param>
            /// <param name="specialisedProcessors">The specialised processor manager that will be used to handle copying of specific objects</param>
            public CopySettings(IDictionary<object, object> objectCache, SpecialisedCopyManager specialisedProcessors) {
                ObjectCache = objectCache;
                SpecialisedProcessors = specialisedProcessors;
            }
        }

        /*----------Variables----------*/
        //PRIVATE

        /// <summary>
        /// Cache a reference to the memberwise clone method that will be used to copy values
        /// </summary>
        private static readonly MethodInfo CLONE_METHOD = typeof(System.Object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);

        /*----------Functions----------*/
        //PRIVATE

        /// <summary>
        /// Recurse into the original object to copy the contained information
        /// </summary>
        /// <param name="original">The original object that is to be copied</param>
        /// <param name="settings">The copy settings that define how this operation is handled</param>
        /// <returns>Returns a copy of the original object for use</returns>
        private static object InternalCopy(object original, CopySettings settings) {
            // If the original is null, don't bother
            if (original == null)
                return null;

            // Get the type of the object to copied
            Type typeValue = original.GetType();

            // Check if there is a processor that needs to handle this
            SpecialisedCopyProcessor copyProcessor = settings.SpecialisedProcessors.FindProcessor(typeValue, original);
            if (copyProcessor != null) {
                // Get the copy that will be used for this object
                object specialCopy = copyProcessor.ProcessCopy(original, typeValue);

                // Cache the value if it isn't a primitive
                if (!IsPrimitive(typeValue))
                    settings.ObjectCache[original] = specialCopy;
                return specialCopy;
            }

            // If there is a cached version of the copy, use that
            if (settings.ObjectCache.ContainsKey(original))
                return settings.ObjectCache[original];

            // If the object is a primitive type, can just use the original
            if (IsPrimitive(typeValue))
                return original;

            // If the original is a delegate, don't clone those
            if (typeof(Delegate).IsAssignableFrom(typeValue))
                return null;

            // Array doesn't like the memberwise clone, handle specifically
            object objectCopy;
            if (!typeValue.IsArray) 
                objectCopy = CLONE_METHOD.Invoke(original, null);
            else {
                // Get the original array to be processed
                Array originalArray = (Array)original;

                // Create a shallow copy of the array
                Array copyArray = (Array)originalArray.Clone();
                objectCopy = copyArray;

                // Get the underlying type of the array that is being copied
                Type arrayType = typeValue.GetElementType();

                // Check if there is a specialised processor for this type
                copyProcessor = settings.SpecialisedProcessors.FindProcessor(arrayType, null);

                // If there is a specialised copy processor for the array type or the type isn't a primitive, handle the copying
                if (copyProcessor != null || !IsPrimitive(arrayType)) {
                    copyArray.ForEach((array, indices) =>
                        array.SetValue(InternalCopy(copyArray.GetValue(indices), settings), indices)
                    );
                }
            }

            // Add this copied object to the cache collection
            settings.ObjectCache[original] = objectCopy;

            // Copy the values within the copied value
            CopyFields(original, settings, objectCopy, typeValue);
            RecursiveCopyBaseTypePrivateFields(original, settings, objectCopy, typeValue);
            return objectCopy;
        }

        /// <summary>
        /// Reflect the fields that are contained within the supplied object and create a copy of them
        /// </summary>
        /// <param name="original">The original object that is being copied</param>
        /// <param name="settings">The copy settings that define how this operation is handled</param>
        /// <param name="objectCopy">The copy of the original that is being setup</param>
        /// <param name="typeValue">The type of the original object being copied</param>
        /// <param name="bindingFlags">The reflection flags that are being used to determine the fields that will be collected</param>
        /// <param name="filter">Callback function that can be used to determine fields that shouldn't be copied</param>
        private static void CopyFields(object original,
                                       CopySettings settings, 
                                       object objectCopy, 
                                       Type typeValue, 
                                       BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy, 
                                       Func<FieldInfo, bool> filter = null) {
            // Run the copy operation for all fields that are contained within this object
            foreach (FieldInfo field in typeValue.GetFields(bindingFlags)) {
                // Check if this field is filtered out
                if (filter != null && !filter(field))
                    continue;

                // If the field is a primitive type, doesn't need special treatment
                if (IsPrimitive(field.FieldType))
                    continue;

                // Create a copy of the field value
                object fieldOriginal = field.GetValue(original);
                object fieldCopy = InternalCopy(fieldOriginal, settings);
                field.SetValue(objectCopy, fieldCopy);
            }
        }

        /// <summary>
        /// Recurse down into the base types of the supplied object to copy their private fields
        /// </summary>
        /// <param name="original">The original object that is being copied</param>
        /// <param name="settings">The copy settings that define how this operation is handled</param>
        /// <param name="objectCopy">The copy of the original that is being setup</param>
        /// <param name="typeValue">The type of the original object being copied</param>
        private static void RecursiveCopyBaseTypePrivateFields(object original, CopySettings settings, object objectCopy, Type typeValue) {
            if (typeValue.BaseType != null) {
                RecursiveCopyBaseTypePrivateFields(original, settings, objectCopy, typeValue.BaseType);
                CopyFields(
                    original,
                    settings,
                    objectCopy,
                    typeValue.BaseType,
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    info => info.IsPrivate
                );
            }
        }

        /// <summary>
        /// Traverse through the array and raise a callback action
        /// </summary>
        /// <param name="array">The array that is to be traversed</param>
        /// <param name="action">The action that is to be raised for all of the elements in the array</param>
        private static void ForEach(this Array array, Action<Array, int[]> action) {
            // If the array is empty, don't bother
            if (array.LongLength == 0) return;

            // Traverse the array to raise the callback action
            ArrayTraverse walker = new ArrayTraverse(array);
            do { action(array, walker.Position); }
            while (walker.Step());
        }

        //PUBLIC

        /// <summary>
        /// Check if the type object that is supplied is a primitive type
        /// </summary>
        /// <param name="type">The type object that is to be checked</param>
        /// <returns>Returns true if the supplied type is a primitive type</returns>
        public static bool IsPrimitive(this Type type) {
            return (
                type == typeof(string) || 
                (type.IsValueType && type.IsPrimitive)
            );
        }

        /// <summary>
        /// Create a copy of the original object using a deep member clone by default
        /// </summary>
        /// <param name="original">The original object that is to be copied</param>
        /// <param name="settings">[Optional] The copy settings object that will be used to drive the copy operation</param>
        /// <returns>Returns a copy of the original object for use</returns>
        public static object Copy(this object original, CopySettings settings = null) {
            // Ensure that there is a settings object that can be used
            if (settings == null)
                settings = new CopySettings();

            // Create the copy of the object
            return InternalCopy(original, settings);
        }

        /// <summary>
        /// Create a copy of the original object using a deep member clone by default
        /// </summary>
        /// <typeparam name="T">The type of the object that is to be copied</typeparam>
        /// <param name="original">The original object that is to be copied</param>
        /// <param name="settings">[Optional] The copy settings object that will be used to drive the copy operation</param>
        /// <returns>Returns a copy of the original object for use</returns>
        public static T Copy<T>(this T original, CopySettings settings = null) {
            return (T)Copy((object)original, settings);
        }
    }
}