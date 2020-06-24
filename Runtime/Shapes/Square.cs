using Unity.Mathematics;
using Unity.Collections;

namespace Terraced.Shapes
{
    [System.Serializable]
    /// <summary>
    /// Represent a simple square that would be used to generate a terraced tile.
    /// </summary>
    public struct Square : ITerracedShape<float3>
    {
        /// <summary>
        /// The position of the bottom left vertex of the square.
        /// </summary>
        public float3 p0;
        /// <summary>
        /// The position of the top left vertex of the square.
        /// </summary>
        public float3 p1;

        /// <summary>
        /// The position of the top right vertex of the square.
        /// </summary>
        public float3 p2;

        /// <summary>
        /// The position of the bottom right vertex of the square.
        /// </summary>
        public float3 p3;

        /// <summary>
        /// Create new square.
        /// </summary>
        /// <param name="p0">The position of the bottom left vertex of the square.</param>
        /// <param name="p1">The position of the top left vertex of the square.</param>
        /// <param name="p2">The position of the top right vertex of the square.</param>
        /// <param name="p3">The position of the bottom right vertex of the square.</param>
        public Square(float3 p0, float3 p1, float3 p2, float3 p3)
        {
            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }

        /// <summary>
        /// Create new square.
        /// </summary>
        /// <param name="p">The array of the position of the vertices of the square. ⚠ Must be respect this order: bottom left, top left, top right, bottom right</param>
        public Square(NativeArray<float3> p)
        {
            if(p.Length < 4) 
                throw new System.Exception("Array length must be >= 4");

            this.p0 = p[0];
            this.p1 = p[1];
            this.p2 = p[2];
            this.p3 = p[3];
        }

        /// <summary>
        /// Get the specific position of a vertex of this square
        /// </summary>
        /// <value></value>
        public float3 this[int index]
        {
            get {
                switch(index)
                {
                    case 0: return p0;
                    case 1: return p1;
                    case 2: return p2;
                    case 3: return p3;
                    default: throw new System.Exception("Index out of range!");
                }
            }
        }

        /// <summary>
        /// Returns the lowest y-value between all vertex positions.
        /// </summary>
        public float MinHeight() => math.min(p0.y, math.min(p1.y, math.min(p2.y, p3.y)));

        /// <summary>
        /// Returns the highest y-value between all vertex positions.
        /// </summary>
        public float MaxHeight() => math.max(p0.y, math.max(p1.y, math.max(p2.y, p3.y)));
    }
}