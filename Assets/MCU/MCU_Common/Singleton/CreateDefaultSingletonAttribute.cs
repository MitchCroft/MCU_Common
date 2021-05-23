using System;

namespace MCU.Singleton {
    /// <summary>
    /// Specifiy for Singleton objects, which should have default instances created for them if none can be found
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class CreateDefaultSingletonAttribute : Attribute {
        /*----------Properties----------*/
        //PUBLIC
    
        /// <summary>
        /// Flags if the Singleton Type attached should generate a default instance for use if none can be found
        /// </summary>
        public bool CreateDefault { get; private set; }
    
        /*----------Functions----------*/
        //PUBLIC
    
        /// <summary>
        /// Initialise this object with it's base values
        /// </summary>
        /// <param name="createDefault">Flag that indicates if a default instance of the singleton should be created</param>
        public CreateDefaultSingletonAttribute(bool createDefault = true) { CreateDefault = createDefault; }
    }
}