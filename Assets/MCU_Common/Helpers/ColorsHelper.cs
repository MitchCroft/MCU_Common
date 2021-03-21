using UnityEngine;

namespace MCU.Helpers {
    /// <summary>
    /// Provide additional functionality for the <see cref="UnityEngine.Color"/> class
    /// </summary>
    public static class ColorsHelper {
        /*----------Functions----------*/
        //PUBLIC

        /// <summary>
        /// Retrieve a random color generated for use
        /// </summary>
        /// <param name="includeAlpha">Flags if the alpha should be randomised as well. If false, will leave alpha at 1</param>
        /// <returns>Returns a new color object with randomly assigned color values</returns>
        public static Color GetRandomColor(bool includeAlpha) {
            return new Color(
                Random.value,
                Random.value,
                Random.value,
                (includeAlpha ? Random.value : 1f)
            );
        }

        /// <summary>
        /// Retrieve a new Color value with the alpha value adjusted
        /// </summary>
        /// <param name="color">The color that is to be modified for the new alpha</param>
        /// <param name="alpha">The new alpha value that is to be used for the color</param>
        /// <returns>Returns a new Color with the same RGB as the supplied and adjusted alpha</returns>
        public static Color GetAdjustedAlpha(Color color, float alpha) {
            return new Color(
                color.r,
                color.g,
                color.b,
                alpha
            );
        }

        /// <summary>
        /// Retrieve a new Color value with the alpha value adjusted
        /// </summary>
        /// <param name="color">The color that is to be modified for the new alpha</param>
        /// <param name="alpha">The new alpha value (0-255 scale) that is to be used for the color</param>
        /// <returns>Returns a new Color with the same RGB as the supplied and adjusted alpha</returns>
        public static Color GetAdjustedAlpha(Color color, int alpha) {
            return new Color(
                color.r,
                color.g,
                color.b,
                alpha / (float)byte.MaxValue
            );
        }

        /// <summary>
        /// Create a new color from 0-255 color channel values
        /// </summary>
        /// <param name="r">The value of the red color channel</param>
        /// <param name="g">The value of the red color channel</param>
        /// <param name="b">The value of the red color channel</param>
        /// <returns>Returns a color object that has been initialised for the supplied color values</returns>
        public static Color MakeRGBColor(int r, int g, int b) {
            return new Color(
                r / (float)byte.MaxValue,
                g / (float)byte.MaxValue,
                b / (float)byte.MaxValue
            );
        }

        /// <summary>
        /// Create a new color from 0-255 color channel values
        /// </summary>
        /// <param name="r">The value of the red color channel</param>
        /// <param name="g">The value of the red color channel</param>
        /// <param name="b">The value of the red color channel</param>
        /// <param name="a">The value of the alpha channel</param>
        /// <returns>Returns a color object that has been initialised for the supplied color values</returns>
        public static Color MakeRGBAColor(int r, int g, int b, int a) {
            return new Color(
                r / (float)byte.MaxValue,
                g / (float)byte.MaxValue,
                b / (float)byte.MaxValue,
                a / (float)byte.MaxValue
            );
        }
    }
}