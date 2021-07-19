using UnityEngine;

namespace SkyFramework
{
    public static class ColorHelper
    {
        private const byte MAX_BYTE_FOR_OVEREXPOSED_COLOR = 191; //internal Unity const

        public static float GetColorIntensity(Color color)
        {
            var maxColorComponent = color.maxColorComponent;
            var scaleFactor = MAX_BYTE_FOR_OVEREXPOSED_COLOR / maxColorComponent;
            return Mathf.Log(255f / scaleFactor) / Mathf.Log(2f); 
        }

        public static Color SetColorIntensity(Color color, float intensity)
        {
            float factor = Mathf.Pow(2,intensity);
            return new Color(color.r*factor,color.g*factor,color.b*factor,color.a);
        }
    }
}