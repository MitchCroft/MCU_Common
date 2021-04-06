#if UNITY_EDITOR
using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace MCU.EditorScreens {
    /// <summary>
    /// Store a collection of screen definitions that can be displayed within the editor for use
    /// </summary>
    /// <typeparam name="TEnum">The enumeration type that this state machine is based on</typeparam>
    public class ScreenStateMachine<TEnum> : ScreenStateMachine where TEnum : struct, Enum {
        /*----------Types----------*/
        //PUBLIC

        /// <summary>
        /// Delegate that is raised when the screen state is changed
        /// </summary>
        /// <param name="to">The state that the state machine has been changed to</param>
        /// <param name="toScreen">The screen object that will be displayed for the new state. This can be null if there was no screen assigned</param>
        /// <param name="from">The state the state machine was in previous to the change. This can be null for the first instance</param>
        /// <param name="fromScreen">The screen object that was used to display the screen previously. This can be null for the first instance or if no screen was assigned</param>
        /// <param name="data">The group information that is accessed and written to by the screens in the state machine</param>
        public delegate void StateChangedDel(TEnum to, ScreenBase toScreen, TEnum? from, ScreenBase fromScreen, Dictionary<string, object> data);

        /// <summary>
        /// Delegate that is raised when the state machine is cleaned up for final processing
        /// </summary>
        /// <param name="stateMachine">The state machine that is being disposed of</param>
        public delegate void StateMachineDisposedDel(ScreenStateMachine<TEnum> stateMachine);

        /// <summary>
        /// A collection of parameters that are used to format how the state machine is initialised
        /// </summary>
        public struct InitParams {
            /*----------Variables----------*/
            //CONST

            /// <summary>
            /// The default collection of parameters that will be used to initialise a state machine
            /// </summary>
            public static readonly InitParams Default = new InitParams {
                initialScreen = default,
                stateScreens = new Dictionary<TEnum, ScreenBase>(),
                generateNavigationLabels = true
            };
               
            //PUBLIC

            /// <summary>
            /// The initial screen that will be displayed
            /// </summary>
            public TEnum initialScreen;

            /// <summary>
            /// The collection of screens that will be available for show initially
            /// </summary>
            public IDictionary<TEnum, ScreenBase> stateScreens;

            /// <summary>
            /// Flags if the navigation labels should be created for the state options
            /// </summary>
            public bool generateNavigationLabels;
        }

        /*----------Variables----------*/
        //CONST

        /// <summary>
        /// Store the values that denominate the display order for elements in this machine
        /// </summary>
        private static readonly TEnum[] STATE_VALUES = (TEnum[])Enum.GetValues(typeof(TEnum));

        //PROTECTED

        /// <summary>
        /// The collection of screen objects that can be displayed with this state machine
        /// </summary>
        protected Dictionary<TEnum, ScreenBase> specifiedScreens;

        /// <summary>
        /// The current screen that is activly being displayed
        /// </summary>
        protected TEnum? activeScreen;

        /// <summary>
        /// Flag the type of screen that is to be transitioned to on the next display
        /// </summary>
        protected TEnum? transitionTo;

        /*----------Events----------*/
        //PUBLIC

        /// <summary>
        /// Event that is raised when this state machine changes screens
        /// </summary>
        public event StateChangedDel OnScreenChange;

        /// <summary>
        /// Event that is raised just before the data is cleared within the state machine
        /// </summary>
        public event StateMachineDisposedDel OnDisposed;
    
        /*----------Properties----------*/
        //PUBLIC
    
        /// <summary>
        /// Get and set the specified screens that will be used for the nominated state
        /// </summary>
        /// <param name="key">The key of the screen that is to be accessed</param>
        public ScreenBase this[TEnum key] {
            get {
                // Check if the object has been disposed of
                if (disposedValue) throw new ObjectDisposedException(nameof(ScreenStateMachine<TEnum>));
                return specifiedScreens.ContainsKey(key) ? specifiedScreens[key] : null;
            }
            set {
                // Check if the object has been disposed of
                if (disposedValue) throw new ObjectDisposedException(nameof(ScreenStateMachine<TEnum>));

                // If there was a previous callback assigned remove it
                if (specifiedScreens.ContainsKey(key) && specifiedScreens[key] != null)
                    specifiedScreens[key].OnStateChange -= ScreenStateChangeRequestCallback;

                // Assign the new state entry
                specifiedScreens[key] = value;

                // If there is a new screen, subscribe to the callbacks
                if (value != null)
                    specifiedScreens[key].OnStateChange += ScreenStateChangeRequestCallback;
            }
        }

        /// <summary>
        /// The screen that is currently actively being displayed
        /// </summary>
        public TEnum ActiveScreen {
            get {
                // Check if the object has been disposed of
                if (disposedValue) throw new ObjectDisposedException(nameof(ScreenStateMachine<TEnum>));
                return activeScreen.HasValue ? activeScreen.Value : default;
            }
            set {
                // Check if the object has been disposed of
                if (disposedValue) throw new ObjectDisposedException(nameof(ScreenStateMachine<TEnum>));

                // Don't bother if transitioning to the same screen
                if (activeScreen.Equals(value))
                    return;

                // Stash the state to move to on next display
                transitionTo = value;
            }
        }

        /// <summary>
        /// Content labels that will be used to display information for the different enum states via <see cref="DoLayoutNavigation"/>
        /// </summary>
        public Dictionary<TEnum, GUIContent> StateLabels { get; private set; }

        /*----------Functions----------*/
        //PROTECTED

        /// <summary>
        /// Attempt to modify the display state based on a request from a screen
        /// </summary>
        /// <param name="state">The state that is requested to be changed to</param>
        protected void ScreenStateChangeRequestCallback(Enum state) {
            // Check that this state useable for this state machine
            Type enumType  = typeof(TEnum),
                 stateType = state.GetType();

            // If the types don't match, can't transition to state
            if (!enumType.IsAssignableFrom(stateType)) {
                Debug.LogWarningFormat("Unable to transition state machine of type '{0}' to the state '{1}' ({2})", enumType.FullName, state, stateType.FullName);
                return;
            }

            // Transition the display to the specified state
            ActiveScreen = (TEnum)state;
        }

        /// <summary>
        /// Process the transition between active screens
        /// </summary>
        protected void TransitionScreen() {
            // Reset the active control elements 
            GUIUtility.hotControl =
            GUIUtility.keyboardControl = 0;

            // Close out the previous screen if it was in use
            if (activeScreen.HasValue &&
                specifiedScreens.ContainsKey(activeScreen.Value) &&
                specifiedScreens[activeScreen.Value] != null)
                specifiedScreens[activeScreen.Value].OnClose(Data);

            // Raise the event for screen change
            OnScreenChange?.Invoke(
                transitionTo.Value,
                specifiedScreens.ContainsKey(transitionTo.Value) ? specifiedScreens[transitionTo.Value] : null,
                activeScreen,
                activeScreen.HasValue && specifiedScreens.ContainsKey(activeScreen.Value) ? specifiedScreens[activeScreen.Value] : null,
                Data
            );

            // Set the required values
            activeScreen = transitionTo.Value;
            transitionTo = null;

            // Open up the new screen for display if it can
            if (specifiedScreens.ContainsKey(activeScreen.Value) &&
                specifiedScreens[activeScreen.Value] != null)
                specifiedScreens[activeScreen.Value].OnOpened(Data);
        }

        /// <summary>
        /// Diplay basic UI information to show that there is no screen assigned for the current state
        /// </summary>
        protected void DoNoScreenLayout() {
            EditorGUILayout.HelpBox(activeScreen.HasValue ?
                string.Format("There is no screen registered for the screen state '{0}'", activeScreen.Value) :
                "There is no currently designated active screen",
                MessageType.Error
            );
        }

        /// <summary>
        /// Try to force a clearing of the state machine data when the object is being destroyed
        /// </summary>
        ~ScreenStateMachine() { Dispose(); }

        //PUBLIC

        /// <summary>
        /// Initialise this object with it's base values
        /// </summary>
        public ScreenStateMachine() : this(InitParams.Default) {}

        /// <summary>
        /// Initialise this object with a defined starting state
        /// </summary>
        /// <param name="initialState">The state that should initially be shown for this state machine</param>
        public ScreenStateMachine(InitParams initParams) {
            specifiedScreens = new Dictionary<TEnum, ScreenBase>(initParams.stateScreens);
            transitionTo = initParams.initialScreen;
            Data = new Dictionary<string, object>();
            StateLabels = new Dictionary<TEnum, GUIContent>();
            if (initParams.generateNavigationLabels)
                ResetNavigationLabels();
        }

        /// <summary>
        /// Reset all of the navigation display lables to the specified 
        /// </summary>
        public void ResetNavigationLabels() {
            // Check if the object has been disposed of
            if (disposedValue) throw new ObjectDisposedException(nameof(ScreenStateMachine<TEnum>));

            // Stash the default labels that will be shown
            GUIContent[] labels = LabelGeneration.CreateEnumContent<TEnum>();
            StateLabels.Clear();
            for (int i = 0; i < labels.Length; ++i)
                StateLabels[STATE_VALUES[i]] = labels[i];
        }

        /// <summary>
        /// Display the standard navigation bar for the current state machine
        /// </summary>
        public override void DoLayoutNavigation() {
            // Check if the object has been disposed of
            if (disposedValue) throw new ObjectDisposedException(nameof(ScreenStateMachine<TEnum>));

            // Display the navigation elements along a single line
            EditorGUILayout.BeginHorizontal(); {
                // Add an entry for all of the possible states
                for (int i = 0; i < STATE_VALUES.Length; ++i) {
                    // Check that there is a state to display
                    if (!specifiedScreens.ContainsKey(STATE_VALUES[i]) ||
                         specifiedScreens[STATE_VALUES[i]] == null ||
                        !StateLabels.ContainsKey(STATE_VALUES[i]) ||
                         StateLabels[STATE_VALUES[i]] == null)
                        continue;

                    // Display the toggle to show th current navigation state
                    bool state = GUILayout.Toggle(ActiveScreen.Equals(STATE_VALUES[i]), StateLabels[STATE_VALUES[i]], EditorStyles.toolbarButton);
                    if (state && !ActiveScreen.Equals(STATE_VALUES[i]))
                        ActiveScreen = STATE_VALUES[i];
                }
            } EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Display the active screen for user interaction
        /// </summary>
        public override void DoLayoutActiveScreen() {
            // Check if the object has been disposed of
            if (disposedValue) throw new ObjectDisposedException(nameof(ScreenStateMachine<TEnum>));

            // Check if there is a new screen that is to be displayed
            if (transitionTo.HasValue) 
                TransitionScreen();

            // If there is an active screen display it
            if (activeScreen.HasValue &&
                specifiedScreens.ContainsKey(activeScreen.Value) &&
                specifiedScreens[activeScreen.Value] != null)
                specifiedScreens[activeScreen.Value].DisplayLayout(Data);

            // Otherwise, there is no scene to be displayed
            else DoNoScreenLayout();
        }
        
        /// <summary>
        /// Handle the closing of the contained screens that will no longer be used to display
        /// </summary>
        public override void Dispose() {
            // Only dispose of the elements once
            if (!disposedValue) {
                // Raise the disposal behaviour for final value clear up
                OnDisposed?.Invoke(this);

                // Close out and destroy the specified screens
                foreach (var pair in specifiedScreens) {
                    // Check there is a screen instance to process
                    if (pair.Value != null) {
                        // If this is the active screen close it
                        if (activeScreen.HasValue && activeScreen.Value.Equals(pair.Key))
                            pair.Value.OnClose(Data);

                        // Destroy the screen state
                        pair.Value.OnDestroy(Data);

                        // Clear the callback assigned
                        pair.Value.OnStateChange -= ScreenStateChangeRequestCallback;
                    }
                }

                // Clear all stored values
                specifiedScreens = null;
                activeScreen = null;
                transitionTo = null;
                Data = null;
                StateLabels = null;

                // Prevent disposal from re-occurring
                disposedValue = true;
                GC.SuppressFinalize(this);
            }
        }
    }

    /// <summary>
    /// Provide a base point for referencing screen state machines common behaviour
    /// </summary>
    public abstract class ScreenStateMachine : IDisposable {
        /*----------Variables----------*/
        //PROTECTED

        /// <summary>
        /// Determine if the state machine has been disposed of
        /// </summary>
        protected bool disposedValue = false;

        /*----------Properties----------*/
        //PUBLIC

        /// <summary>
        /// Data information that can be accessed from any of the screens in the state machine
        /// </summary>
        public Dictionary<string, object> Data { get; protected set; }

        /*----------Functions----------*/
        //PUBLIC

        /// <summary>
        /// Display the standard navigation bar for the current state machine
        /// </summary>
        public abstract void DoLayoutNavigation();

        /// <summary>
        /// Display the active screen for user interaction
        /// </summary>
        public abstract void DoLayoutActiveScreen();

        /// <summary>
        /// Clear up screens contained within the state machine
        /// </summary>
        public abstract void Dispose();
    }
}
#endif