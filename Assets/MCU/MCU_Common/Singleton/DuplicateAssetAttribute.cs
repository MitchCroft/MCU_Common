using System;

namespace MCU.Singleton {
    /// <summary>
    /// Mark an asset type that should be duplicated instead of using the on disk version
    /// </summary>
    /// <remarks>
    /// Intended for <see cref="ScriptableSingleton{T}"/> to allow for flexability on how the asset is used
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class DuplicateAssetAttribute : Attribute {}
}