using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace MCU.Helpers {
    /// <summary>
    /// Allow for the running of Coroutines on an object that is not bound to any specific scene object
    /// </summary>
    /// <remarks>Coroutines started on this will continue to run if their initial object is destroyed and/or scene is changed</remarks>
    public static class CoroutineRunner {
        /*----------Types----------*/
        //PRIVATE

        /// <summary>
        /// A base MonoBehaviour that can have Coroutines attached to it for execution
        /// </summary>
        private sealed class RunningMan : MonoBehaviour {
            /*----------Properties----------*/
            //PUBLIC

            /// <summary>
            /// Flag if the application has quit so that a new instance isn't created during cleanup
            /// </summary>
            public static bool ApplicationHasQuit { get; private set; }

            /*----------Functions----------*/
            //PRIVATE

            /// <summary>
            /// Flag when the application is quitting so it doesn't create a new one
            /// </summary>
            private void OnApplicationQuit() { ApplicationHasQuit = true; }
        }

        /*----------Variables----------*/
        //PRIVATE

        /// <summary>
        /// Store a reference to the Runner to attach MonoBehaviours to
        /// </summary>
        private static RunningMan runner;

        /// <summary>
        /// Store a reference to all of the Coroutines that have been registered to an object for execution
        /// </summary>
        private static Dictionary<System.Object, Coroutine> registeredCoroutines;

        /*----------Properties----------*/
        //PRIVATE

        /// <summary>
        /// Retrieve the MonoBehaviour instance that is used to attach Coroutine operations to
        /// </summary>
        private static RunningMan Instance {
            get {
                // Ensure that there is a runner
                if (!runner && !RunningMan.ApplicationHasQuit) {
                    // Look for a runner in the scene(s)
                    runner = GameObject.FindObjectOfType<RunningMan>();

                    // If nothing was found, then we make a new one
                    if (!runner) {
                        // Clear any previously stored registered coroutines, they are lost
                        if (registeredCoroutines.Count > 0)
                            registeredCoroutines.Clear();

                        // Create a new GameObject to put the instance onto
                        GameObject go = new GameObject("CoroutineRunner");
                        go.hideFlags = HideFlags.HideAndDontSave;

                        // Prevent this object from being destroyed by scene transition
                        GameObject.DontDestroyOnLoad(go);

                        // Add a running man behaviour to the object for use
                        runner = go.AddComponent<RunningMan>();
                    }
                }

                // Return the instance of the runner for use
                return runner;
            }
        }

        /*----------Functions----------*/
        //PRIVATE

        /// <summary>
        /// Initialise the internal container required for registered coroutine execution
        /// </summary>
        static CoroutineRunner() { registeredCoroutines = new Dictionary<System.Object, Coroutine>(); }

        /// <summary>
        /// Run a registered Coroutine operation on the runner
        /// </summary>
        /// <param name="_key">The 'key' object that is used to register the operation</param>
        /// <param name="_routine">The routine that is to be executed on the Coroutine Runner</param>
        private static IEnumerator RunRegistered_CR(System.Object _key, IEnumerator _routine) {
            // Process the supplied action
            while (_routine.MoveNext()) yield return _routine.Current;

            // Remove the operation from the register
            registeredCoroutines.Remove(_key);
        }

        //PUBLIC

        /// <summary>
        /// Check if there is a registered coroutine under the supplied key
        /// </summary>
        /// <param name="_key">The key object that is being checked for a registered operation</param>
        /// <returns>Returns true if there is an active running operation</returns>
        public static bool HasRegisteredCoroutine(System.Object _key) { return registeredCoroutines.ContainsKey(_key); }

        /// <summary>
        /// Start running a coroutine operation on the runner
        /// </summary>
        /// <param name="_routine">The routine that is to be executed on the Coroutine Runner</param>
        /// <returns>Returns a reference to the started Coroutine operation for yielding purposes</returns>
        public static Coroutine StartCoroutine(IEnumerator _routine) { return Instance.StartCoroutine(_routine); }

        /// <summary>
        /// Start running a coroutine operation that will be registered under the supplied object key
        /// </summary>
        /// <param name="_key">The object that this coroutine operation will be registered under to prevent overlapping operations</param>
        /// <param name="_routine">The routine that is to be executed on the Coroutine Runner</param>
        /// <param name="_throwOnDuplicate">If there is an existing operation with the supplied key and this is true an <see cref="System.OperationCanceledException"/> exception will be thrown</param>
        /// <returns>Returns a reference to the started Coroutine operation for yielding purposes</returns>
        public static Coroutine StartRegisteredCoroutine(System.Object _key, IEnumerator _routine, bool _throwOnDuplicate = false) {
            // Check if there is a running operation with this key
            if (HasRegisteredCoroutine(_key)) {
                // If not throwing errors, stop the running operation
                if (!_throwOnDuplicate) StopRegisteredCoroutine(_key);

                // Otherwise, exception
                else throw new OperationCanceledException(string.Format("Coroutine Runner attempted to start another coroutine for the registering object '{0}'", _key));
            }

            // Start the registered operation 
            return registeredCoroutines[_key] = StartCoroutine(RunRegistered_CR(_key, _routine));
        }

        /// <summary>
        /// Stop the supplied coroutine that is running on the Coroutine Runner
        /// </summary>
        /// <param name="_coroutine">The Coroutine operation that is to be stopped</param>
        public static void StopCoroutine(Coroutine _coroutine) { Instance.StopCoroutine(_coroutine); }

        /// <summary>
        /// Stop the Coroutine that is registered under the supplied key object
        /// </summary>
        /// <param name="_key">The object thta the coroutine operation was registered under</param>
        /// <returns>Returns true if a registered operation was found and stopped</returns>
        public static bool StopRegisteredCoroutine(System.Object _key) {
            // Check if there is an operation with the key
            if (!registeredCoroutines.ContainsKey(_key)) return false;

            // Stop the coroutine from running
            Instance.StopCoroutine(registeredCoroutines[_key]);

            // Remove the registered instance from the list
            return registeredCoroutines.Remove(_key);
        }
    }
}
