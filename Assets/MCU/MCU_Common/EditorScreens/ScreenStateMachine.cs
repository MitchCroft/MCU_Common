#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace MCU.EditorScreens {
    /// <summary>
    /// Handle the assigning of state screens to the values defined by an enumeration type
    /// </summary>
    /// <typeparam name="TEnum">The type of enumeration that will hold the different states that can be displayed</typeparam>
    public class EnumScreenStateMachine<TEnum> : ScreenStateMachine where TEnum : struct, Enum {
        /*----------Types----------*/
        //PUBLIC

        /// <summary>
        /// Information about the screen transition event that is being raised
        /// </summary>
        public sealed class ScreenStateTransitionEventArgs : EventArgs {
            /*----------Properties----------*/
            //PUBLIC

            /// <summary>
            /// The enumeration state that is being transitioned to
            /// </summary>
            public TEnum ToState { get; private set; }

            /// <summary>
            /// The screen object that is being transitioned to in this process
            /// </summary>
            public ScreenBase ToScreen { get; private set; }

            /// <summary>
            /// The enumeration state that is being transitioned from
            /// </summary>
            public TEnum? FromState { get; private set; }

            /// <summary>
            /// The screen object that is being transitioned from in this process
            /// </summary>
            public ScreenBase FromScreen { get; private set; }

            /*----------Functions----------*/
            //PUBLIC

            /// <summary>
            /// Initialise this object with the information needed for the event
            /// </summary>
            /// <param name="to">The state that the state machine has been changed to</param>
            /// <param name="toScreen">The screen object that will be displayed for the new state. This can be null if there was no screen assigned</param>
            /// <param name="from">The state the state machine was in previous to the change. This can be null for the first instance</param>
            /// <param name="fromScreen">The screen object that was used to display the screen previously. This can be null for the first instance or if no screen was assigned</param>
            public ScreenStateTransitionEventArgs(TEnum to, ScreenBase toScreen, TEnum? from, ScreenBase fromScreen) {
                ToState = to;
                ToScreen = toScreen;
                FromState = from;
                FromScreen = fromScreen;
            }
        }

        /// <summary>
        /// Delegate that is raised when the state machine is cleaned up for final processing
        /// </summary>
        /// <param name="stateMachine">The state machine that is being disposed of</param>
        public delegate void StateMachineDisposedDel(EnumScreenStateMachine<TEnum> stateMachine);

        /// <summary>
        /// A collection of values that can define the initial starting parameters for this state machine
        /// </summary>
        public struct InitParams {
            /*----------Variables----------*/
            //SHARED

            /// <summary>
            /// The default values that will be used to initialise an Enum Screen State Machine
            /// </summary>
            public static readonly InitParams Default = new InitParams {
                initialScreen = (ENUM_STATE_VALUES.Length > 0 ? ENUM_STATE_VALUES[0] : default),
                stateScreens = null,
                generateDefaultNavLabels = true,
                navigationLabels = null
            };

            //PUBLIC

            /// <summary>
            /// The first screen that should be displayed by this state machine
            /// </summary>
            public TEnum? initialScreen;

            /// <summary>
            /// The collection of state screens that are to be managed by this state screen
            /// </summary>
            public Dictionary<TEnum, ScreenBase> stateScreens;

            /// <summary>
            /// Flags if the default navigation labels should be generated based on the enumeration values
            /// </summary>
            public bool generateDefaultNavLabels;

            /// <summary>
            /// The overriding collection of labels that should be applied to the different states
            /// </summary>
            /// <remarks>If <see cref="generateDefaultNavLabels"/> is true, the defaults will be generated before the overrides are assigned</remarks>
            public Dictionary<TEnum, GUIContent> navigationLabels;
        }

        /*----------Variables----------*/
        //SHARED

        /// <summary>
        /// Track the collection of enumeration state values that can be displayed within this state machine
        /// </summary>
        protected static TEnum[] ENUM_STATE_VALUES = (TEnum[])Enum.GetValues(typeof(TEnum));

        //PROTECTED

        /// <summary>
        /// The current screen that is actively being displayed
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
        public event EventHandler<ScreenStateTransitionEventArgs> OnScreenTransition;

        /// <summary>
        /// Event that is raised just before the data is cleared within the state machine
        /// </summary>
        public event StateMachineDisposedDel OnDisposed;

        /*----------Properties----------*/
        //PROTECTED

        /// <summary>
        /// Retrieve the collection of possible state values that can be found within the state machine
        /// </summary>
        protected override IList StateValues { get { return ENUM_STATE_VALUES; } }

        /// <summary>
        /// Get the hash of the element that is currently active for use
        /// </summary>
        protected override int? ActiveScreenHashID {
            get { return (activeScreen.HasValue ? (int?)activeScreen.Value.GetHashCode() : null); }
            set {
                // If there is a value assigned, need to try and find a match
                if (value.HasValue) {
                    // Look for a matching hash value
                    for (int i = 0; i < ENUM_STATE_VALUES.Length; ++i) {
                        if (ENUM_STATE_VALUES[i].GetHashCode() == value.Value) {
                            activeScreen = ENUM_STATE_VALUES[i];
                            return;
                        }
                    }
                }

                // If got this far, just clear the value
                activeScreen = null;
            }
        }

        /// <summary>
        /// Is there a pending screen transition that needs to be performed
        /// </summary>
        protected override bool HasTransitionPending { get { return transitionTo.HasValue; } }

        //PUBLIC

        /// <summary>
        /// The screen that is currently being actively displayed
        /// </summary
        public TEnum ActiveScreen {
            get {
                if (IsDisposed) throw new ObjectDisposedException(nameof(EnumScreenStateMachine<TEnum>));
                return (activeScreen.HasValue ? activeScreen.Value : default);
            } set {
                // If disposed, can't do anything else
                if (IsDisposed) throw new ObjectDisposedException(nameof(EnumScreenStateMachine<TEnum>));

                // If already transitioning, if we know would go back to the active, we don't need to transition
                if (transitionTo.HasValue && activeScreen.HasValue && activeScreen.Value.Equals(value)) {
                    transitionTo = null;
                    return;
                }

                // If transitioning to the same value don't bother
                if (activeScreen.HasValue && activeScreen.Value.Equals(value))
                    return;

                // Flag the state to be transitioned to
                transitionTo = value;
            }
        }

        /// <summary>
        /// Get and set the specified screens that will be used for the nominated state
        /// </summary>
        /// <param name="key">The key of the screen that is to be accessed</param>
        public ScreenBase this[TEnum key] {
            get {
                if (IsDisposed) throw new ObjectDisposedException(nameof(EnumScreenStateMachine<TEnum>));
                int hash = key.GetHashCode();
                return (stateScreens.ContainsKey(hash) ? stateScreens[hash] : null);
            } set {
                // If disposed, can't do anything else
                if (IsDisposed) throw new ObjectDisposedException(nameof(EnumScreenStateMachine<TEnum>));

                // If there was a previous callback assigned remove it
                int hash = key.GetHashCode();
                if (stateScreens.ContainsKey(hash) && stateScreens[hash] != null)
                    stateScreens[hash].OnStateChange -= TransitionToStateScreen;

                // If there is no new screen, clear the state entry
                if (value == null) stateScreens.Remove(hash);

                // Otherwise, setup the object for use
                else {
                    // Save the object in the lookup for display
                    stateScreens[hash] = value;

                    // Subscribe to the state change callback that can be raised
                    value.OnStateChange += TransitionToStateScreen;
                }
            }
        }

        /*----------Functions----------*/
        //PROTECTED

        /// <summary>
        /// Process the transition between active screens
        /// </summary>
        protected override void TransitionToPendingScreen() {
            // Reset the active control elements 
            GUIUtility.hotControl =
            GUIUtility.keyboardControl = 0;

            // Get the hash keys for the different elements
            int toHash   = transitionTo.Value.GetHashCode();
            int fromHash = (activeScreen.HasValue ? activeScreen.Value.GetHashCode() : 0);

            // Close out the previous screen if it was in use
            if (activeScreen.HasValue &&
                stateScreens.ContainsKey(fromHash) &&
                stateScreens[fromHash] != null)
                stateScreens[fromHash].OnClose(Data);

            // Raise the state changed event
            OnScreenTransition?.Invoke(this, new ScreenStateTransitionEventArgs(
                transitionTo.Value,
                stateScreens.ContainsKey(toHash) ? stateScreens[toHash] : null,
                activeScreen,
                activeScreen.HasValue && stateScreens.ContainsKey(fromHash) ? stateScreens[fromHash] : null
            ));

            // Set the required values
            activeScreen = transitionTo.Value;
            transitionTo = null;

            // Open up the new screen for display if it can
            if (stateScreens.ContainsKey(toHash) &&
                stateScreens[toHash] != null)
                stateScreens[toHash].OnOpened(Data);
        }

        /// <summary>
        /// Attempt to modify the display state based on a request from a screen
        /// </summary>
        /// <param name="state">The state that is requested to be changed to</param>
        protected override void TransitionToStateScreen(object state) {
            // Check that the supplied state value exists
            if (object.Equals(state, null)) {
                Debug.LogWarning("Unable to transition state machine to supplied state value. State value is null");
                return;
            }

            // Check that this state useable for this state machine
            Type enumType = typeof(TEnum),
                 stateType = state.GetType();

            // If the types don't match, can't transition to state
            if (!enumType.IsAssignableFrom(stateType)) {
                Debug.LogWarningFormat("Unable to transition {0} to the state '{1}' ({2})", nameof(EnumScreenStateMachine<TEnum>), state, stateType.FullName);
                return;
            }

            // Transition the display to the specified state
            ActiveScreen = (TEnum)state;
        }

        /// <summary>
        /// Diplay basic UI information to show that there is no screen assigned for the current state
        /// </summary>
        /// <param name="data">The current collection of information that has been set for the state machine</param>
        protected override void DisplayNoScreenLayout(Dictionary<string, object> data) {
            EditorGUILayout.HelpBox(activeScreen.HasValue ?
                string.Format("There is no screen registered for the screen state '{0}'", activeScreen.Value) :
                "There is no currently designated active screen",
                MessageType.Error
            );
        }

        //PUBLIC

        /// <summary>
        /// Initialise the state machine with default settings
        /// </summary>
        public EnumScreenStateMachine() : this(InitParams.Default) {}

        /// <summary>
        /// Setup the state machine with the specified starting settings
        /// </summary>
        /// <param name="initParams">The collection of initialisation settings that will be processed</param>
        public EnumScreenStateMachine(InitParams initParams) {
            // Setup the screen elements that are to be displayed
            stateScreens = new Dictionary<int, ScreenBase>(initParams.stateScreens != null ? initParams.stateScreens.Count : 0);
            if (initParams.stateScreens != null) {
                foreach (var pair in initParams.stateScreens)
                    this[pair.Key] = pair.Value;
            }

            // Assign the screen that is to be displayed first
            transitionTo = initParams.initialScreen;

            // Create the labels collection for storage
            stateLabels = new Dictionary<int, GUIContent>(Math.Max(
                initParams.generateDefaultNavLabels ? ENUM_STATE_VALUES.Length : 0,
                initParams.navigationLabels != null ? initParams.navigationLabels.Count : 0
            ));

            // If the basic labels are being generated, get them sorted
            if (initParams.generateDefaultNavLabels)
                ResetNavigationLabels();

            // Apply the override labels as needed
            if (initParams.navigationLabels != null) {
                foreach (var pair in initParams.navigationLabels)
                    SetNavigationLabel(pair.Key, pair.Value);
            }
        }

        /// <summary>
        /// Reset the navigation lables to the defaults for the enumeration
        /// </summary>
        public virtual void ResetNavigationLabels() {
            // Check if this state machine has been disposed of already
            if (IsDisposed) throw new ObjectDisposedException(nameof(EnumScreenStateMachine<TEnum>));

            // Get the labels that can be used for the enumeration entries
            GUIContent[] labels = LabelGeneration.CreateEnumContent<TEnum>();
            stateLabels.Clear();
            for (int i = 0; i < labels.Length; ++i)
                stateLabels[ENUM_STATE_VALUES[i].GetHashCode()] = labels[i];
        }

        /// <summary>
        /// Assign the specified GUIContent object to the specified state
        /// </summary>
        /// <param name="state">The state that the label should be used to display</param>
        /// <param name="label">The label that will be assigned to the state. NOTE: Use null to clear the label for the state</param>
        public virtual void SetNavigationLabel(TEnum state, GUIContent label) {
            // Check if this state machine has been disposed of already
            if (IsDisposed) throw new ObjectDisposedException(nameof(EnumScreenStateMachine<TEnum>));

            // Get the hash for the state
            int hash = state.GetHashCode();

            // Check if clearing or assigning
            if (label == null) stateLabels.Remove(hash);
            else stateLabels[hash] = label;
        }

        /// <summary>
        /// Handle the raising of disposal events 
        /// </summary>
        public override void Dispose() {
            // If this object hasn't been disposed yet, raise the callback(s)
            if (!IsDisposed) OnDisposed?.Invoke(this);

            // Handle the base disposal behaviour
            base.Dispose();
        }
    }

    /// <summary>
    /// Handle the assigning of state screens to string values
    /// </summary>
    public class StringScreenStateMachine : ScreenStateMachine {
        /*----------Types----------*/
        //PUBLIC

        /// <summary>
        /// Information about the screen transition event that is being raised
        /// </summary>
        public sealed class ScreenStateTransitionEventArgs : EventArgs {
            /*----------Properties----------*/
            //PUBLIC

            /// <summary>
            /// The state that is being transitioned to
            /// </summary>
            public string ToState { get; private set; }

            /// <summary>
            /// The screen object that is being transitioned to in this process
            /// </summary>
            public ScreenBase ToScreen { get; private set; }

            /// <summary>
            /// The state that is being transitioned from
            /// </summary>
            public string FromState { get; private set; }

            /// <summary>
            /// The screen object that is being transitioned from in this process
            /// </summary>
            public ScreenBase FromScreen { get; private set; }

            /*----------Functions----------*/
            //PUBLIC

            /// <summary>
            /// Initialise this object with the information needed for the event
            /// </summary>
            /// <param name="to">The state that the state machine has been changed to</param>
            /// <param name="toScreen">The screen object that will be displayed for the new state. This can be null if there was no screen assigned</param>
            /// <param name="from">The state the state machine was in previous to the change. This can be null for the first instance</param>
            /// <param name="fromScreen">The screen object that was used to display the screen previously. This can be null for the first instance or if no screen was assigned</param>
            public ScreenStateTransitionEventArgs(string to, ScreenBase toScreen, string from, ScreenBase fromScreen) {
                ToState = to;
                ToScreen = toScreen;
                FromState = from;
                FromScreen = fromScreen;
            }
        }

        /// <summary>
        /// Delegate that is raised when the state machine is cleaned up for final processing
        /// </summary>
        /// <param name="stateMachine">The state machine that is being disposed of</param>
        public delegate void StateMachineDisposedDel(StringScreenStateMachine stateMachine);

        /// <summary>
        /// A collection of values that can define the initial starting parameters for this state machine
        /// </summary>
        public struct InitParams {
            /*----------Variables----------*/
            //SHARED

            /// <summary>
            /// The default values that will be used to initialise an Enum Screen State Machine
            /// </summary>
            public static readonly InitParams Default = new InitParams {
                initialScreen = null,
                stateScreens = null,
                navigationLabels = null
            };

            //PUBLIC

            /// <summary>
            /// The first screen that should be displayed by this state machine
            /// </summary>
            public string initialScreen;

            /// <summary>
            /// The collection of state screens that are to be managed by this state screen
            /// </summary>
            public Dictionary<string, ScreenBase> stateScreens;

            /// <summary>
            /// The overriding collection of labels that should be applied to the different states
            /// </summary>
            /// <remarks>If <see cref="generateDefaultNavLabels"/> is true, the defaults will be generated before the overrides are assigned</remarks>
            public Dictionary<string, GUIContent> navigationLabels;
        }

        /*----------Variables----------*/
        //PROTECTED

        /// <summary>
        /// The collection of screen state names that have been assigned to this state machine
        /// </summary>
        protected List<string> stateNames;

        /// <summary>
        /// The name of the current screen that is actively being displayed
        /// </summary>
        protected string activeScreen;

        /// <summary>
        /// The name of the screen that is to be shown next
        /// </summary>
        protected string transitionTo;

        /*----------Events----------*/
        //PUBLIC

        /// <summary>
        /// Event that is raised when this state machine changes screens
        /// </summary>
        public event EventHandler<ScreenStateTransitionEventArgs> OnScreenTransition;

        /// <summary>
        /// Event that is raised just before the data is cleared within the state machine
        /// </summary>
        public event StateMachineDisposedDel OnDisposed;

        /*----------Properties----------*/
        //PROTECTED

        /// <summary>
        /// Retrieve the collection of possible state values that can be found within the state machine
        /// </summary>
        protected override IList StateValues { get { return stateNames; } }
        
        /// <summary>
        /// Get the hash of the currently displayed element
        /// </summary>
        protected override int? ActiveScreenHashID {
            get { return (activeScreen != null ? (int?)activeScreen.GetHashCode() : null); }
            set {
                // If there is a value assigned, need to try and find a match
                if (value != null) {
                    // Look for a matching hash value
                    for (int i = 0; i < stateNames.Count; ++i) {
                        if (stateNames[i].GetHashCode() == value.Value) {
                            activeScreen = stateNames[i];
                            return;
                        }
                    }
                }

                // If got this far, just clear the value
                activeScreen = null;
            }
        }

        /// <summary>
        /// Is there a pending screen transition that needs to be performed
        /// </summary>
        protected override bool HasTransitionPending { get { return transitionTo != null; } }

        //PUBLIC

        /// <summary>
        /// The screen that is currently being actively displayed
        /// </summary>
        public string ActiveScreen {
            get {
                if (IsDisposed) throw new ObjectDisposedException(nameof(StringScreenStateMachine));
                return activeScreen;
            } set {
                // If disposed, can't do anything else
                if (IsDisposed) throw new ObjectDisposedException(nameof(StringScreenStateMachine));

                // Can't transition to a null state
                if (value == null) throw new ArgumentNullException(nameof(value));

                // If already transitioning, if we know we would go back to the active, we don't need to transition
                if (transitionTo != null && activeScreen != null && activeScreen == value) {
                    transitionTo = null;
                    return;
                }

                // If transitioning to the same value, don't bother
                if (activeScreen == value)
                    return;

                // Flag the state that is to be transitioned to
                transitionTo = value;
            }
        }

        /// <summary>
        /// Get and set the specified screens that will be used for the nominated state
        /// </summary>
        /// <param name="key">The key of the screen that is to be accessed</param>
        public ScreenBase this[string key] {
            get {
                if (IsDisposed) throw new ObjectDisposedException(nameof(StringScreenStateMachine));
                if (key == null) throw new ArgumentNullException(nameof(key));
                int hash = key.GetHashCode();
                return (stateScreens.ContainsKey(hash) ? stateScreens[hash] : null);
            } set {
                // If disposed, can't do anything
                if (IsDisposed) throw new ObjectDisposedException(nameof(StringScreenStateMachine));

                // Can't assign to a null value
                if (key == null) throw new ArgumentNullException(nameof(key));

                // If there was a previous callback assigned remove it
                int hash = key.GetHashCode();
                if (stateScreens.ContainsKey(hash) && stateScreens[hash] != null)
                    stateScreens[hash].OnStateChange -= TransitionToStateScreen;

                // If there is no new screen, clear the state entry
                if (value == null) {
                    stateScreens.Remove(hash);
                    stateNames.Remove(key);
                }

                // Otherwise, setup the object for use
                else {
                    // Save the object in the lookup for display
                    stateScreens[hash] = value;

                    // Add this entry to the end of the list
                    if (!stateNames.Contains(key))
                        stateNames.Add(key);

                    // Subscribe to the state change callback that can be raised
                    value.OnStateChange += TransitionToStateScreen;
                }
            }
        }

        /*----------Functions----------*/
        //PROTECTED

        /// <summary>
        /// Process the transition between active screens
        /// </summary>
        protected override void TransitionToPendingScreen() {
            // Reset the active control elements
            GUIUtility.hotControl =
            GUIUtility.keyboardControl = 0;

            // Get the hash keys for the different elements
            int toHash = transitionTo.GetHashCode();
            int fromHash = (activeScreen != null ? activeScreen.GetHashCode() : 0);

            // Close out the previous screen if it was in use
            if (activeScreen != null &&
                stateScreens.ContainsKey(fromHash) &&
                stateScreens[fromHash] != null)
                stateScreens[fromHash].OnClose(Data);

            // Raise the state changed event
            OnScreenTransition?.Invoke(this, new ScreenStateTransitionEventArgs(
                transitionTo,
                stateScreens.ContainsKey(toHash) ? stateScreens[toHash] : null,
                activeScreen,
                activeScreen != null && stateScreens.ContainsKey(fromHash) ? stateScreens[fromHash] : null
            ));

            // Set the required values
            activeScreen = transitionTo;
            transitionTo = null;

            // Open the new screen for display if it can
            if (stateScreens.ContainsKey(toHash) &&
                stateScreens[toHash] != null)
                stateScreens[toHash].OnOpened(Data);
        }

        /// <summary>
        /// Attempt to modify the display state based on a request from a screen
        /// </summary>
        /// <param name="state">The state that is requested to be changed to</param>
        protected override void TransitionToStateScreen(object state) {
            // Check that the supplied state value exists
            if (object.Equals(state, null)) {
                Debug.LogWarning("Unable to transition state machine to supplied state value. State value is null");
                return;
            }

            // Check that this state is usable for this state machine
            Type stringType = typeof(string),
                  stateType = state.GetType();
            
            // If the types don't match, can't transition the state
            if (!stringType.IsAssignableFrom(stateType)) {
                Debug.LogWarningFormat("Unable to transition {0} to the state '{1}' ({2})", nameof(StringScreenStateMachine), state, stateType.FullName);
                return;
            }

            // Transition the display to the specified state
            ActiveScreen = (string)state;
        }

        /// <summary>
        /// Display basic UI information to show that there is no screen assigned for the current state
        /// </summary>
        /// <param name="data">The current collection of information that has been set for the state machine</param>
        protected override void DisplayNoScreenLayout(Dictionary<string, object> data) {
            EditorGUILayout.HelpBox(activeScreen != null ?
                string.Format("There is no screen registered for the screen state '{0}'", activeScreen) :
                "There is no currently designated active screen",
                MessageType.Error
            );
        }

        //PUBLIC

        /// <summary>
        /// Initialise the state machine with default settings
        /// </summary>
        public StringScreenStateMachine() : this(InitParams.Default) {}

        /// <summary>
        /// Setup the state machine with the specified starting settings
        /// </summary>
        /// <param name="initParams">The collection of initialisation settings that will be processed</param>
        public StringScreenStateMachine(InitParams initParams) {
            // Setup the screen elements that are to be displayed
            stateNames = new List<string>(initParams.stateScreens != null ? initParams.stateScreens.Count : 0);
            stateScreens = new Dictionary<int, ScreenBase>(stateNames.Capacity);
            if (initParams.stateScreens != null) {
                foreach (var pair in initParams.stateScreens)
                    this[pair.Key] = pair.Value;
            }

            // Assign the screen that is to be displayed first
            transitionTo = initParams.initialScreen;

            // Create the labels collection for storage
            stateLabels = new Dictionary<int, GUIContent>(initParams.navigationLabels != null ? initParams.navigationLabels.Count : 0);
            if (initParams.navigationLabels != null) {
                foreach (var pair in initParams.navigationLabels)
                    SetNavigationLabel(pair.Key, pair.Value);
            }
        }

        /// <summary>
        /// Assign the specified GUIContent object to the specified state
        /// </summary>
        /// <param name="state">The state that the label should be used to display</param>
        /// <param name="label">The label that will be assigned to the state. NOTE: Use null to clear the label for the state</param>
        public virtual void SetNavigationLabel(string state, GUIContent label) {
            // Check if this state machine has been disposed of already
            if (IsDisposed) throw new ObjectDisposedException(nameof(StringScreenStateMachine));

            // If the state is null then we can't do anything about it
            if (state == null) throw new ArgumentNullException(nameof(state));

            // Get the hash for the state
            int hash = state.GetHashCode();

            // Check if clearing or assigning
            if (label == null) stateLabels.Remove(hash);
            else stateLabels[hash] = label;
        }

        /// <summary>
        /// handle the raising of disposal events
        /// </summary>
        public override void Dispose() {
            // If this object hasn't be disposed yet, raise the callback(s)
            if (!IsDisposed) OnDisposed?.Invoke(this);

            // Handle the base disposal behaviour
            base.Dispose();
        }
    }

    /// <summary>
    /// Provide a base point for referencing screen state machines common behaviour
    /// </summary>
    public abstract class ScreenStateMachine : IDisposable {
        /*----------Variables----------*/
        //PROTECTED

        /// <summary>
        /// Store the screens that will be displayed by the 
        /// </summary>
        protected Dictionary<int, ScreenBase> stateScreens;

        /// <summary>
        /// Determine if the state machine has been disposed of
        /// </summary>
        protected bool disposedValue = false;

        /// <summary>
        /// Store the labels that will be shown for state navigation
        /// </summary>
        protected Dictionary<int, GUIContent> stateLabels;

        /*----------Properties----------*/
        //PROTECTED

        /// <summary>
        /// The collection of values that are currently or could be contained within the state machine for display
        /// </summary>
        protected abstract IList StateValues { get; }

        /// <summary>
        /// The hash of the screen that is currently being displayed
        /// </summary>
        protected abstract int? ActiveScreenHashID { get; set; }

        /// <summary>
        /// Flags if there is a pending screen transition that needs to be resolved
        /// </summary>
        protected abstract bool HasTransitionPending { get; }

        //PUBLIC

        /// <summary>
        /// Flags if this state machine has been disposed of
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Data information that can be accessed from any of the screens in the state machine
        /// </summary>
        public Dictionary<string, object> Data { get; protected set; }

        /*----------Functions----------*/
        //PRIVATE

        /// <summary>
        /// Try to force a clearing of the state machine data when the object is being destroyed
        /// </summary>
        ~ScreenStateMachine() { Dispose(); }

        //PROTECTED

        /// <summary>
        /// Handle the process of transitioning the current state machine to the pending value
        /// </summary>
        protected abstract void TransitionToPendingScreen();

        /// <summary>
        /// Handle the navigation to another state screen from an assigned screen element
        /// </summary>
        /// <param name="state">The value whose hash will be converted into an ID of the state that is to be transitioned to</param>
        protected abstract void TransitionToStateScreen(object state);

        /// <summary>
        /// Handle the displaying of information that describes that there is no screen to be shown currently
        /// </summary>
        /// <param name="data">The current collection of information that has been set for the state machine</param>
        protected abstract void DisplayNoScreenLayout(Dictionary<string, object> data);

        //PUBLIC

        /// <summary>
        /// Initialise the machine with the default required values
        /// </summary>
        public ScreenStateMachine() {
            IsDisposed = false;
            Data = new Dictionary<string, object>();
        }

        /// <summary>
        /// Display the standard navigation bar for the current state machine
        /// </summary>
        public virtual void DoLayoutNavigation() {
            // Check if this state machine has been disposed of already
            if (IsDisposed) throw new ObjectDisposedException(this.GetType().Name);

            // Display the navigation elements along a single line
            int? active = ActiveScreenHashID;
            EditorGUILayout.BeginHorizontal(); {
                // Iterate over all of the state values that need to be shown
                foreach (object state in StateValues) {
                    // If this state is null then can't show anything
                    if (object.Equals(state, null))
                        continue;

                    // Check that there are elements to be shown for this
                    int hash = state.GetHashCode();
                    if (!stateScreens.ContainsKey(hash) ||
                         stateScreens[hash] == null ||
                         !stateLabels.ContainsKey(hash) ||
                         stateLabels[hash] == null)
                        continue;

                    // Display the toggle to show the current navigation state
                    bool isCurrent = (active.HasValue && active.Value == hash);
                    bool isActive = GUILayout.Toggle(isCurrent, stateLabels[hash], EditorStyles.toolbarButton);
                    if (isActive && !isCurrent)
                        TransitionToStateScreen(state);
                }
            } EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Display the active screen for user interaction
        /// </summary>
        public virtual void DoLayoutActiveScreen() {
            // Check if this state machine has been disposed of already
            if (IsDisposed) throw new ObjectDisposedException(this.GetType().Name);

            // If there is a new screen to be displayed then handle it
            if (HasTransitionPending) TransitionToPendingScreen();

            // If there is a screen to display, display it
            int? active = ActiveScreenHashID;
            if (active.HasValue &&
                stateScreens.ContainsKey(active.Value) &&
                stateScreens[active.Value] != null)
                stateScreens[active.Value].DisplayLayout(Data);

            // Otherwise, there is nothing to show
            else DisplayNoScreenLayout(Data);
        }

        /// <summary>
        /// Clear up screens contained within the state machine
        /// </summary>
        public virtual void Dispose() {
            if (!IsDisposed) {
                // Clear up the screens that are stored within the state machine
                int? active = ActiveScreenHashID;
                foreach (var pair in stateScreens) {
                    // Check that there is a screen instance to process
                    if (pair.Value != null) {
                        // If this is the active screen, close it
                        if (active.HasValue && pair.Key == active.Value)
                            pair.Value.OnClose(Data);

                        // Raise the destroy behaviour for the screen
                        pair.Value.OnDestroy(Data);

                        // Remove the callback that is assigned for the screen
                        pair.Value.OnStateChange -= TransitionToStateScreen;
                    }
                }

                // Clear all stored values
                stateScreens = null;
                ActiveScreenHashID = null;
                Data = null;
                stateLabels = null;

                // Prevent disposal from re-occurring
                disposedValue = true;
                GC.SuppressFinalize(this);
            }
        }
    }
}
#endif