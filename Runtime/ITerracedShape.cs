using Unity.Mathematics;

namespace Terraced.Shapes
{
    public interface ITerracedShape<T> where T : System.IEquatable<float3>, System.IFormattable
    {
        /// <summary>
        /// Returns the lowest y-value between all vertex positions.
        /// </summary>
        float MinHeight();

        /// <summary>
        /// Returns the highest y-value between all vertex positions.
        /// </summary>
        float MaxHeight();

        /// <summary>
        /// Get or Set values like an array
        /// </summary>
        T this[int index] { get; }
    }
}