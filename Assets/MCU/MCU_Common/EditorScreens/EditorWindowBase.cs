#if UNITY_EDITOR
using System;

using UnityEngine;
using UnityEditor;

namespace MCU.EditorScreens {
    /// <summary>
    /// Provide a base point for managers to inherit from and manage the displaying of their information
    /// </summary>
    public abstract class EditorWindowBase : EditorWindow {
        /*----------Variables----------*/
        //PROTECTED

        /// <summary>
        /// Store a reference to the state machine that will be used to display the different elements
        /// </summary>
        protected ScreenStateMachine screenStateMachine;

        /*----------Functions----------*/
        //PROTECTED

        /// <summary>
        /// Initialise this objects internal information
        /// <summary>
        protected virtual void OnEnable() { screenStateMachine = CreateWindowStateMachine(); }

        /// <summary>
        /// Render the window UI controls to the display area
        /// </summary>
        protected virtual void OnGUI() {
            if (screenStateMachine != null) {
                screenStateMachine.DoLayoutNavigation();
                screenStateMachine.DoLayoutActiveScreen();
            }
        }

        /// <summary>
        /// Save editor preferences for resuming operation
        /// </summary>
        protected virtual void OnDestroy() {
            if (screenStateMachine != null)
                screenStateMachine.Dispose();
        }

        /// <summary>
        /// Get the state machine object that will be displayed for this window
        /// </summary>
        /// <returns>Returns the state machine instance that is to be displayed</returns>
        protected abstract ScreenStateMachine CreateWindowStateMachine();

        //PUBLIC

        /// <summary>
        /// Open a window of the specified type and show it for use
        /// </summary>
        /// <param name="windowType">The type of <see cref="EditorWindowBase"/> object that is to be retrieved</param>
        /// <returns>Returns the instance of the Window Instance that is to be displayed, or null if unable</returns>
        public static EditorWindowBase GetWindowInstance(Type windowType) {
            // Check that the type can be converted
            if (!typeof(EditorWindowBase).IsAssignableFrom(windowType)) {
                Debug.LogErrorFormat("Unable to get a WindowBase instance from the type '{0}'. Type dosen't inherit from WindowBase", windowType);
                return null;
            }

            // Get the window that is to be shown
            EditorWindowBase window = GetWindow(windowType) as EditorWindowBase;
            if (window == null) {
                Debug.LogErrorFormat("Unable to get a WindowBase instance from the type '{0}'", windowType);
                return null;
            }

            // Display the window for use
            window.Show();
            return window;
        }

        /// <summary>
        /// Open a window of the specified type and show it for use
        /// </summary>
        /// <typeparam name="TWindow">The type of <see cref="EditorWindowBase"/> object that is to be retrieved</typeparam>
        /// <returns>Returns the instance of the Window Instance that is to be displayed</returns>
        public static TWindow GetWindowInstance<TWindow>() where TWindow : EditorWindowBase { return (TWindow)GetWindowInstance(typeof(TWindow)); }
    }
}
#endif