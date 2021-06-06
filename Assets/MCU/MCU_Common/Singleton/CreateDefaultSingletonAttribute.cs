using System;

namespace MCU.Singleton {
    /// <summary>
    /// Specifiy for Singleton objects, which should have default instances created for them if none can be found
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CreateDefaultSingletonAttribute : Attribute {}
}