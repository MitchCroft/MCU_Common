using System.Collections.Generic;

using UnityEngine;

namespace MCU.Extensions {
    /// <summary>
    /// Provide additional functionality to Component objects
    /// </summary>
    public static class ComponentExtensions {
        /*----------Functions----------*/
        //PUBLIC

        /// <summary>
        /// Find all components (traveling down an objects hierarchy) of the specified type
        /// </summary>
        /// <typeparam name="T">The type of object that is being searched for</typeparam>
        /// <param name="component">The component that this operation is originating from</param>
        /// <param name="includeSelf">Flags if the calling object should be included in the search</param>
        /// <param name="includeInactive">Flags if inactive child objects should be included in the search</param>
        /// <returns>Returns an array of all of the objects that were found of the specified type</returns>
        /// <remarks>This is a breadth first search of the hierarchy</remarks>
        public static T[] FindAllComponentsInChildren<T>(this Component component, bool includeSelf = true, bool includeInactive = false) {
            // Store a collection of the objects that were found
            List<T> found = new List<T>();

            // Store a collection of the Transforms left to search
            Queue<Transform> unsearched = new Queue<Transform>();

            // If searching itself, add to the queue
            if (includeSelf) unsearched.Enqueue(component.transform);

            // Otherwise, add the children
            else {
                foreach (Transform child in component.transform) {
                    if (includeInactive || child.gameObject.activeSelf)
                        unsearched.Enqueue(child);
                }
            }

            // While there is something to search
            Transform current; T[] buffer;
            while (unsearched.Count > 0) {
                // Get the next object to process
                current = unsearched.Dequeue();

                // Check if there is a value to retrieve
                if ((buffer = current.GetComponents<T>()).Length != 0)
                    found.AddRange(buffer);

                // Add this transforms children to the search
                if (current.childCount > 0) {
                    foreach (Transform child in current) {
                        if (includeInactive || child.gameObject.activeSelf)
                            unsearched.Enqueue(child);
                    }
                }
            }

            // Return the found components
            return found.ToArray();
        }

        /// <summary>
        /// Find all components (traveling up an objects hierarchy) of the specified type
        /// </summary>
        /// <typeparam name="T">The type of object that is being searched for</typeparam>
        /// <param name="component">The component that this operation is originating from</param>
        /// <param name="includeSelf">Flags if the calling object should be included in the search</param>
        /// <returns>Returns an array of all of the objects that were found of the specified type</returns>
        public static T[] FindAllComponentsInParents<T>(this Component component, bool includeSelf = true) {
            // Store a collection of the objects that were found
            List<T> found = new List<T>();

            // Store the transform to search from
            Transform current = (includeSelf ? component.transform : component.transform.parent);

            // Loop up the hierarchy
            T[] buffer;
            while (current) {
                // Look for attached components
                if ((buffer = current.GetComponents<T>()).Length != 0)
                    found.AddRange(buffer);

                // Move up the hierarchy
                current = current.parent;
            }

            // Return the found objects
            return found.ToArray();
        }

        /// <summary>
        /// Look down this objects hierarchy to find the specified type object
        /// </summary>
        /// <typeparam name="T">The type of object that is being searched for</typeparam>
        /// <param name="component">The component that this operation is operating on</param>
        /// <param name="includeInactive">Flags if inactive child objects should be included in the search</param>
        /// <returns>Returns the first the instance of T found or null if not</returns>
        /// <remarks>
        /// This is a depth first search down the hierarchy tree
        /// </remarks>
        public static T FindTypeInChildren<T>(this Component component, bool includeInactive = false) where T : class {
            // Store a reference to the found object
            T buffer = null;

            // Store the elements yet to search
            Queue<Transform> unsearched = new Queue<Transform>();
            unsearched.Enqueue(component.transform);

            // Look for the first instance
            Transform current;
            while (unsearched.Count > 0) {
                // Get the next transform to process
                current = unsearched.Dequeue();

                // Check for the object
                if ((buffer = current.GetComponent<T>()) != null)
                    break;

                // Enqueue the child transforms
                foreach (Transform child in current) {
                    // Check if the child can be added
                    if (includeInactive || child.gameObject.activeSelf)
                        unsearched.Enqueue(child);
                }
            }

            // Return the found buffer
            return buffer;
        }

        /// <summary>
        /// Look up this objects hierarchy tree to find the specified type object
        /// </summary>
        /// <typeparam name="T">The type of object that is being searched for</typeparam>
        /// <param name="component">The component that this operation is operating on</param>
        /// <returns>Returns the first the instance of T found or null if not</returns>
        public static T FindTypeInParents<T>(this Component component) where T : class {
            // Store the current transform being processed
            Transform current = component.transform;

            // Loop while searching
            T buffer = null;
            do { buffer = current.GetComponent<T>();
            } while (buffer == null && (current = current.parent));

            // Return the identified element
            return buffer;
        }

        /// <summary>
        /// Look through this objects hierarchy to find the specified type
        /// </summary>
        /// <typeparam name="T">The type of object that is being searched for</typeparam>
        /// <param name="component">The component that this operation is operating on</param>
        /// <param name="includeInactive">Flags if inactive child objects should be considered in the search</param>
        /// <returns>Returns the first the instance of T found or null if not</returns>
        public static T FindTypeInHierarchy<T>(this Component component, bool includeInactive = false) where T : class {
            T buffer = null;
            if ((buffer = component.FindTypeInChildren<T>(includeInactive)) != null) return buffer;
            else return component.FindTypeInParents<T>();
        }

        /// <summary>
        /// Retrieve the component of type attached to the current component or add one if it doesn't exist
        /// </summary>
        /// <param name="component">The component that this operation is operating on</param>
        /// <param name="componentType">The type of object that is being searched for/added</param>
        /// <returns>Returns the first instance of componentType or a newly instantiated version of the object</returns>
        public static Component GetOrAddComponent(this Component component, System.Type componentType) {
            // Check if there is one already on the component
            Component buffer = null;
            if (buffer = component.GetComponent(componentType))
                return buffer;

            // Otherwise, add a new component
            return component.gameObject.AddComponent(componentType);
        }

        /// <summary>
        /// Retrieve the component of type attached to the current component or add one if it doesn't exist
        /// </summary>
        /// <typeparam name="T">The type of object that is being searched for/added</typeparam>
        /// <param name="component">The component that this operation is operating on</param>
        /// <returns>Returns the first instance of T found or a newly instantiated version of the object</returns>
        public static T GetOrAddComponent<T>(this Component component) where T : Component {
            // Check if there is one already on the component
            T buffer = null;
            if (buffer = component.GetComponent<T>())
                return buffer;

            // Otherwise add a new component
            return component.gameObject.AddComponent<T>();
        }

        /// <summary>
        /// Retrieve the game object within the current component with the specified tag
        /// </summary>
        /// <param name="component">The component that this operation is operating on</param>
        /// <param name="tag">The tag that is being searched for</param>
        /// <param name="includeInactive">Flags if inactive child objects should be considered in the search</param>
        /// <returns>Returns the first object with the tag or null if not found</returns>
        /// <remarks>This is a breadth first search</remarks>
        public static GameObject FindObjectWithTagInChildren(this Component component, string tag, bool includeInactive = false) {
            // Store a collection of objects to be searched
            Queue<Transform> unsearched = new Queue<Transform>();
            unsearched.Enqueue(component.transform);

            // Process the queue 
            Transform current;
            while (unsearched.Count > 0) {
                // Get the next object
                current = unsearched.Dequeue();

                // Check the tag of the object
                if (current.tag == tag) return current.gameObject;

                // Add all of the children to the queue
                foreach (Transform child in current) {
                    if (includeInactive || child.gameObject.activeSelf)
                        unsearched.Enqueue(child);
                }
            }

            // If reached this point, not found
            return null;
        }

        /// <summary>
        /// Retrieve the game object within the current component with the specified tag
        /// </summary>
        /// <param name="component">The component that this operation is operating on</param>
        /// <param name="tag">The tag that is being searched for</param>
        /// <returns>Returns the first object with the tag or null if not found</returns>
        public static GameObject FindObjectWithTagInParent(this Component component, string tag) {
            // Store the transform of the current object
            Transform current = component.transform;

            // Check up the hierarchy
            do {
                // Check the tag of the object
                if (current.tag == tag) return current.gameObject;
                else current = current.parent;
            } while (current);

            // If reached this point, not found
            return null;
        }

        /// <summary>
        /// Retrieve the game objects within the current component with the specified tag
        /// </summary>
        /// <param name="component">The component that this operation is operating on</param>
        /// <param name="tag">The tag that is being searched for</param>
        /// <param name="includeInactive">Flags if inactive child objects should be considered in the search</param>
        /// <returns>Returns an array of the Game Objects that were found with the tag</returns>
        /// <remarks>This is a breadth first search</remarks>
        public static GameObject[] FindObjectsWithTagInChildren(this Component component, string tag, bool includeInactive = false) {
            // Store a list of the objects that were found with the tag
            List<GameObject> found = new List<GameObject>();

            // Store a collection of objects to be searched
            Queue<Transform> unsearched = new Queue<Transform>();
            unsearched.Enqueue(component.transform);

            // Process the queue 
            Transform current;
            while (unsearched.Count > 0) {
                // Get the next object
                current = unsearched.Dequeue();

                // Check the tag of the object
                if (current.tag == tag) found.Add(current.gameObject);

                // Add all of the children to the queue
                foreach (Transform child in current) {
                    if (includeInactive || child.gameObject.activeSelf)
                        unsearched.Enqueue(child);
                }
            }

            // Return the found elements
            return found.ToArray();
        }

        /// <summary>
        /// Retrieve the game objects within the current component with the specified tag
        /// </summary>
        /// <param name="component">The component that this operation is operating on</param>
        /// <param name="tag">The tag that is being searched for</param>
        /// <returns>Returns an array of the Game Objects that were found with the tag</returns>
        public static GameObject[] FindObjectsWithTagInParent(this Component component, string tag) {
            // Store a list of the objects that were found with the tag
            List<GameObject> found = new List<GameObject>();

            // Store the transform of the current object
            Transform current = component.transform;

            // Check up the hierarchy
            do {
                // Check the tag of the object
                if (current.tag == tag) found.Add(current.gameObject);
                current = current.parent;
            } while (current);

            // Return the found elements
            return found.ToArray();
        }

        /// <summary>
        /// Retrieve the first object of the specified types that are found in the Components children
        /// </summary>
        /// <param name="component">The component that this operation is operating on</param>
        /// <param name="types">A collection of Type objects that will be looked for</param>
        /// <returns>Returns the first object of the specified types found or null if not</returns>
        /// <remarks>This is a breadth first search</remarks>
        public static Component FindFirstObjectOfTypeInChildren(this Component component, params System.Type[] types) {
            // If there are no types, don't bother
            if (types.Length == 0) return null;

            // Create a queue of the transforms that haven't been searched
            Queue<Transform> unsearched = new Queue<Transform>();
            unsearched.Enqueue(component.transform);

            // Look until something is found
            Transform current; Component buffer;
            while (unsearched.Count > 0) {
                // Get the next transform to be processed
                current = unsearched.Dequeue();

                // Look for the types on the object
                for (int i = 0; i < types.Length; ++i) {
                    if (buffer = current.GetComponent(types[i]))
                        return buffer;
                }

                // Add the children to the search list
                foreach (Transform child in current)
                    unsearched.Enqueue(child);
            }

            // If got this far, nothing to be found
            return null;
        }

        /// <summary>
        /// Retrieve the first object of the specified types that are found in the Components parents
        /// </summary>
        /// <param name="component">The component that this operation is operating on</param>
        /// <param name="types">A collection of Type objects that will be looked for</param>
        /// <returns>Returns the first object of the specified types found or null if not</returns>
        public static Component FindFirstObjectOfTypeInParents(this Component component, params System.Type[] types) {
            // If there are no types, don't bother
            if (types.Length == 0) return null;

            // Store the transform of the object to be searched
            Transform current = component.transform;

            // Look until there is nothing left to search
            Component buffer;
            do {
                // Look for the types on the object
                for (int i = 0; i < types.Length; ++i) {
                    if (buffer = current.GetComponent(types[i]))
                        return buffer;
                }

                // Shift up the hierarchy
                current = current.parent;
            } while (current);

            // If got this far, not found
            return null;
        }

        /// <summary>
        /// Retrieve the last object of the specified type that is found in the Components parents
        /// </summary>
        /// <typeparam name="T">The type of object that is being searched for</typeparam>
        /// <param name="component">The component that this operation is operating on</param>
        /// <returns>Returns the last instance of T found or null if none found</returns>
        public static T FindLastObjectOfTypeInParents<T>(this Component component) where T : class {
            // Store the transform of the object to be searched
            Transform current = component.transform;

            // Store a reference to the component found
            T found = null, buffer;

            // Look until there is nothing left to search
            do {
                // Look for the component of type
                if ((buffer = current.GetComponent<T>()) != null)
                    found = buffer;

                // Shift up the hierarchy
                current = current.parent;
            } while (current);

            // Return whatever is found at this point
            return found;
        }

        /// <summary>
        /// Retrieve the last component of the specified type that is found in the Components parents
        /// </summary>
        /// <param name="component">The component that this operation is operating on</param>
        /// <param name="types">A collection of Type objects that will be looked for. Include the type earlier in the array to have it prioritised</param>
        /// <returns>Returns the last component of the defined types or null if none found</returns>
        public static Component FindLastObjectOfTypeInParents(this Component component, params System.Type[] types) {
            // If there are no types, don't bother
            if (types.Length == 0) return null;

            // Store the transform of the object to be searched
            Transform current = component.transform;

            // Store a reference to the component found
            Component found = null, buffer;

            // Look until there is nothing left to search
            do {
                // Look for the components of the specified type
                for (int i = 0; i < types.Length; ++i) {
                    if (buffer = current.GetComponent(types[i])) {
                        found = buffer;
                        break;
                    }
                }

                // Shift up the hierarchy
                current = current.parent;
            } while (current);

            // Return the component that was found
            return found;
        }

        /// <summary>
        /// Determine if the supplied Transform object is part of this Components transform Hierarchy looking down the tree
        /// </summary>
        /// <param name="component">The component that this operation is operating on</param>
        /// <param name="obj">The Transform object that is being searched for</param>
        /// <returns>Returns true if the Transform is part of this Components child Hierarchy</returns>
        /// <remarks>This is a breadth first search</remarks>
        public static bool IsPartOfChildren(this Component component, Transform obj) {
            // Store a queue of unsearched components
            Queue<Transform> unsearched = new Queue<Transform>();
            unsearched.Enqueue(component.transform);

            // Search all objects and their children
            Transform current;
            while (unsearched.Count > 0) {
                // Get the next transform
                current = unsearched.Dequeue();

                // Check if this object is a match
                if (current == obj) return true;

                // Add all of the children
                foreach (Transform child in current)
                    unsearched.Enqueue(child);
            }

            // If this far, not part of the children
            return false;
        }

        /// <summary>
        /// Determine if the supplied Transform is part of this Components transform Hierarchy looking up the tree
        /// </summary>
        /// <param name="component">The component that this operation is operating on</param>
        /// <param name="obj">The Transform object that is being searched for</param>
        /// <returns>Returns true if the Transform is part of this Components child Hierarchy</returns>
        public static bool IsPartOfParents(this Component component, Transform obj) {
            // Store the transform that is currently being searched
            Transform current = component.transform;

            // Search until there is nothing left to test
            do {
                // Check if this is the object
                if (current == obj) return true;

                // Get the next object
                current = current.parent;
            } while (current);

            // If this far, not part of the parents
            return false;
        }

        /// <summary>
        /// Determine if the supplied Transform is part of this Components transform Hierarchy
        /// </summary>
        /// <param name="component">The component that this operation is operating on</param>
        /// <param name="obj">The Transform object that is being searched for</param>
        /// <param name="searchParentsFirst">Flags if the parents of the Component should be searched first</param>
        /// <returns>Returns true if the Transform is part of this Components child Hierarchy</returns>
        public static bool IsPartOfHierarchy(this Component component, Transform obj, bool searchParentsFirst = true) {
            return (searchParentsFirst ?
                (IsPartOfParents(component, obj) || IsPartOfChildren(component, obj)) :
                (IsPartOfChildren(component, obj) || IsPartOfParents(component, obj))
            );
        }
    }
}