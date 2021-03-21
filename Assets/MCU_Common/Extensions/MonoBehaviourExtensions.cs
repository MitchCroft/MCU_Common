using System;
using System.Collections;

using UnityEngine;

namespace MCU.Extensions {
    /// <summary>
    /// Provide additional functionality to <see cref="UnityEngine.MonoBehaviour"/> objects 
    /// </summary>
    public static class MonoBehaviourExtensions {
        /*----------Functions----------*/
        //PRIVATE

        /// <summary>
        /// Wait for the completion of a yield instruction before raising an action
        /// </summary>
        /// <param name="instruction">The instruction that must be completed before the callback is raised</param>
        /// <param name="action">The callback function that is run when the yield instruction is complete</param>
        private static IEnumerator WaitToExecute_CR(IEnumerator instruction, Action action) {
            // Yield for the waiting instruction
            yield return instruction;

            // Raise the callback
            action();
        }

        /// <summary>
        /// Wait for the completion of a yield instruction before raising an action
        /// </summary>
        /// <param name="instruction">The instruction that must be completed before the callback is raised</param>
        /// <param name="action">The callback function that is run when the yield instruction is complete</param>
        private static IEnumerator WaitToExecute_CR(YieldInstruction instruction, Action action) {
            // Yield for the waiting instruction
            yield return instruction;

            // Raise the callback
            action();
        }

        //PUBLIC

        /// <summary>
        /// Wait for a condition to occur before an action is undertaken
        /// </summary>
        /// <param name="comp">The component that the Coroutine will be attached to</param>
        /// <param name="condition">The callback functionality that is used to evaluate if the action should be run</param>
        /// <param name="action">The callback function that is run when the condition is evaluated to true</param>
        /// <returns>Returns a reference to the Coroutine object that is created for yielding purposes</returns>
        public static Coroutine WaitToExecute(this MonoBehaviour comp, Func<bool> condition, Action action) {
            // Check that the condition is a valid function
            if (condition == null || condition.Method == null) {
                Debug.LogError("WaitToExecute can't be run without a valid conditional callback function", comp);
                return null;
            }

            // Check there is an action to use
            else if (action == null || action.Method == null) {
                Debug.LogError("WaitToExecute can't be run without a valid callback action", comp);
                return null;
            }

            // Start the operation
            return comp.StartCoroutine(WaitToExecute_CR(new WaitUntil(condition), action));
        }

        /// <summary>
        /// Wait for a condition to occur before an action is undertaken
        /// </summary>
        /// <param name="author">The component that will has 'created' the operation</param>
        /// <param name="condition">The callback functionality that is used to evaluate if the action should be run</param>
        /// <param name="action">The callback function that is run when the condition is evaluated to true</param>
        /// <returns>Returns a reference to an enumerable coroutine operation</returns>
        public static IEnumerator CreateWaitToExecute(this MonoBehaviour author, Func<bool> condition, Action action) {
            // Check that the condition is a valid function
            if (condition == null || condition.Method == null) {
                Debug.LogError("CreateWaitToExecute can't be run without a valid conditional callback function", author);
                return null;
            }

            // Check there is an action to use
            else if (action == null || action.Method == null) {
                Debug.LogError("CreateWaitToExecute can't be run without a valid callback action", author);
                return null;
            }

            // Create the operation
            return WaitToExecute_CR(new WaitUntil(condition), action);
        }

        /// <summary>
        /// Wait for an asynchronous operation to complete before an action is undertaken
        /// </summary>
        /// <param name="comp">The component that the Coroutine will be attached to</param>
        /// <param name="operation">The async-operation that must be completed before the action is raised</param>
        /// <param name="action">The callback function that is run when the condition is evaluated to true</param>
        /// <returns>Returns a reference to the Coroutine object that is created for yielding purposes</returns>
        public static Coroutine WaitToExecute(this MonoBehaviour comp, AsyncOperation operation, Action action) {
            // Check the operation is valid
            if (operation == null) {
                Debug.LogError("WaitToExecute can't be run without a valid AsyncOperation", comp);
                return null;
            }

            // Check there is an action to use
            else if (action == null || action.Method == null) {
                Debug.LogError("WaitToExecute can't be run without a valid callback action", comp);
                return null;
            }

            // Start the operation
            return comp.StartCoroutine(WaitToExecute_CR(operation, action));
        }

        /// <summary>
        /// Wait for an asynchronous operation to complete before an action is undertaken
        /// </summary>
        /// <param name="author">The component that will has 'created' the operation</param>
        /// <param name="operation">The async-operation that must be completed before the action is raised</param>
        /// <param name="action">The callback function that is run when the condition is evaluated to true</param>
        /// <returns>Returns a reference to an enumerable coroutine operation</returns>
        public static IEnumerator CreateWaitToExecute(this MonoBehaviour author, AsyncOperation operation, Action action) {
            // Check the operation is valid
            if (operation == null) {
                Debug.LogError("CreateWaitToExecute can't be run without a valid AsyncOperation", author);
                return null;
            }

            // Check there is an action to use
            else if (action == null || action.Method == null) {
                Debug.LogError("CreateWaitToExecute can't be run without a valid callback action", author);
                return null;
            }

            // Create the operation
            return WaitToExecute_CR(operation, action);
        }

        /// <summary>
        /// Wait for a period of time to pass before an action is undertaken
        /// </summary>
        /// <param name="comp">The component that the Coroutine will be attached to</param>
        /// <param name="duration">The amount of time (in seconds) that must pass before the action should be run</param>
        /// <param name="action">The callback function to run after the specified time has elapsed</param>
        /// <param name="realtime">Flags if the unscaled real-time should be used</param>
        /// <returns>Returns a reference to the Coroutine object that is created for yielding purposes</returns>
        public static Coroutine WaitToExecute(this MonoBehaviour comp, float duration, Action action, bool realtime = false) {
            // Check the duration is valid
            if (duration <= 0f) {
                Debug.LogError("WaitToExecute can't be run without a valid duration greater than 0 seconds", comp);
                return null;
            }

            // Check there is an action to use
            else if (action == null || action.Method == null) {
                Debug.LogError("WaitToExecute can't be run without a valid callback action", comp);
                return null;
            }

            // Return the operation
            return comp.StartCoroutine(realtime ?
                WaitToExecute_CR(new WaitForSecondsRealtime(duration), action) :
                WaitToExecute_CR(new WaitForSeconds(duration), action)
            );
        }

        /// <summary>
        /// Wait for a period of time to pass before an action is undertaken
        /// </summary>
        /// <param name="author">The component that will has 'created' the operation</param>
        /// <param name="duration">The amount of time (in seconds) that must pass before the action should be run</param>
        /// <param name="action">The callback function to run after the specified time has elapsed</param>
        /// <param name="realtime">Flags if the unscaled real-time should be used</param>
        /// <returns>Returns a reference to an enumerable coroutine operation</returns>
        public static IEnumerator CreateWaitToExecute(this MonoBehaviour author, float duration, Action action, bool realtime = false) {
            // Check the duration is valid
            if (duration <= 0f) {
                Debug.LogError("CreateWaitToExecute can't be run without a valid duration greater than 0 seconds", author);
                return null;
            }

            // Check there is an action to use
            else if (action == null || action.Method == null) {
                Debug.LogError("CreateWaitToExecute can't be run without a valid callback action", author);
                return null;
            }

            // Return the operation
            return (realtime ?
                WaitToExecute_CR(new WaitForSecondsRealtime(duration), action) :
                WaitToExecute_CR(new WaitForSeconds(duration), action)
            );
        }

        /// <summary>
        /// Wait until the end of the frame before an action is undertaken
        /// </summary>
        /// <param name="comp">The component that the Coroutine will be attached to</param>
        /// <param name="action">The callback function to run after the current frame has been rendered</param>
        /// <returns>Returns a reference to the Coroutine object that is created for yielding purposes</returns>
        public static Coroutine ExecuteAtEndOfFrame(this MonoBehaviour comp, Action action) {
            // Check that the action is valid
            if (action == null || action.Method == null) {
                Debug.LogError("ExecuteAtEndOfFrame can't be run without a valid callback action", comp);
                return null;
            }

            // Return the operation
            return comp.StartCoroutine(WaitToExecute_CR(new WaitForEndOfFrame(), action));
        }

        /// <summary>
        /// Wait until the end of the frame before an action is undertaken
        /// </summary>
        /// <param name="author">The component that will has 'created' the operation</param>
        /// <param name="action">The callback function to run after the current frame has been rendered</param>
        /// <returns>Returns a reference to an enumerable coroutine operation</returns>
        public static IEnumerator CreateExecuteAtEndOfFrame(this MonoBehaviour author, Action action) {
            // Check that the action is valid
            if (action == null || action.Method == null) {
                Debug.LogError("CreateExecuteAtEndOfFrame can't be run without a valid callback action", author);
                return null;
            }

            // Create the operation
            return WaitToExecute_CR(new WaitForEndOfFrame(), action);
        }

        /// <summary>
        /// Wait for the specified number of frames before an action is undertaken
        /// </summary>
        /// <param name="comp">The component that the Coroutine will be attached to</param>
        /// <param name="frameCount">The number of frames that the Coroutine will wait for before executing the callback action</param>
        /// <param name="action">The callback function to run after the frame count has been reached</param>
        /// <returns>Returns a reference to the Coroutine object that is created for yielding purposes</returns>
        public static Coroutine ExecuteAfterFrames(this MonoBehaviour comp, int frameCount, Action action) {
            // Check that the action is valid
            if (action == null || action.Method == null) {
                Debug.LogError("ExecuteAfterFrames can't be run without a valid callback action", comp);
                return null;
            }

            // Store a counter for the number of frames
            int count = 0;

            // Return the operation
            return comp.StartCoroutine(WaitToExecute_CR(new WaitUntil(() => count++ >= frameCount), action));
        }

        /// <summary>
        /// Wait for the specified number of frames before an action is undertaken
        /// </summary>
        /// <param name="author">The component that will has 'created' the operation</param>
        /// <param name="frameCount">The number of frames that the Coroutine will wait for before executing the callback action</param>
        /// <param name="action">The callback function to run after the frame count has been reached</param>
        /// <returns>Returns a reference to an enumerable coroutine operation</returns>
        public static IEnumerator CreateExecuteAfterFrames(this MonoBehaviour author, int frameCount, Action action) {
            // Check that the action is valid
            if (action == null || action.Method == null) {
                Debug.LogError("CreateExecuteAfterFrames can't be run without a valid callback action", author);
                return null;
            }

            // Store a counter for the number of frames
            int count = 0;

            // Return the operation
            return WaitToExecute_CR(new WaitUntil(() => count++ >= frameCount), action);
        }

        /// <summary>
        /// Wait for the next fixed update before an action is undertaken
        /// </summary>
        /// <param name="comp">The component that the Coroutine will be attached to</param>
        /// <param name="action">The callback function to run after the frame count has been reached</param>
        /// <returns>Returns a reference to the Coroutine object that is created for yielding purposes</returns>
        public static Coroutine ExecuteOnFixedUpdate(this MonoBehaviour comp, Action action) {
            // Check that the action is valid
            if (action == null || action.Method == null) {
                Debug.LogError("ExecuteOnFixedUpdate can't be run without a valid callback action", comp);
                return null;
            }

            // Return the operation
            return comp.StartCoroutine(WaitToExecute_CR(new WaitForFixedUpdate(), action));
        }

        /// <summary>
        /// Wait for the next fixed update before an action is undertaken
        /// </summary>
        /// <param name="author">The component that will has 'created' the operation</param>
        /// <param name="action">The callback function to run after the frame count has been reached</param>
        /// <returns>Returns a reference to an enumerable coroutine operation</returns>
        public static IEnumerator CreateExecuteOnFixedUpdate(this MonoBehaviour author, Action action) {
            // Check that the action is valid
            if (action == null || action.Method == null) {
                Debug.LogError("CreateExecuteOnFixedUpdate can't be run without a valid callback action", author);
                return null;
            }

            // Create the operation
            return WaitToExecute_CR(new WaitForFixedUpdate(), action);
        }
    }
}
