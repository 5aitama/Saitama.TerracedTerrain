using Unity.Mathematics;
using Unity.Collections;

using Saitama.ProceduralMesh;

namespace Terraced
{
    /// <summary>
    /// Contains the algorithms to generate a terraced tile mesh data based on different shapes.
    /// </summary>
    public static class TerracedTile
    {
        /// <summary>
        /// Generate a terraced tile mesh data from a square.
        /// </summary>
        /// <param name="square">The shape on which the generation of the terraced tile is based.</param>
        /// <param name="vertices">The list that would be contain vertices</param>
        /// <param name="triangles">The list that would be contain triangles</param>
        public static void BuildFrom(Shapes.Square square, ref NativeList<Vertex> vertices, ref NativeList<Triangle> triangles)
        {
            var minHeight = square.MinHeight();
            var maxHeight = square.MaxHeight();
            
            var color = new float4(1, 0.4748217f, 0, 1);

            for(float h = (int)minHeight; h < maxHeight; h += 0.5f)
            {
                var index           = 0;
                var points_above    = 0;
                var saddle          = false;

                index |= square[0].y >= h ? 1 : 0;
                index |= square[1].y >= h ? 2 : 0;
                index |= square[2].y >= h ? 4 : 0;
                index |= square[3].y >= h ? 8 : 0;
                
                var p = new NativeArray<float3>(4, Allocator.Temp);
                p[0] = square[0];
                p[1] = square[1];
                p[2] = square[2];
                p[3] = square[3];

                if(index == 0)
                {
                    points_above = 0;
                }
                else if(index == 1 || index == 2 || index == 4 || index == 8)
                {
                    points_above = 1;
                    switch(index)
                    {
                        case 0b0001: p[0] = square[3]; p[1] = square[0]; p[2] = square[1]; p[3] = square[2]; break;
                        case 0b0010: p[0] = square[0]; p[1] = square[1]; p[2] = square[2]; p[3] = square[3]; break;
                        case 0b0100: p[0] = square[1]; p[1] = square[2]; p[2] = square[3]; p[3] = square[0]; break;
                        case 0b1000: p[0] = square[2]; p[1] = square[3]; p[2] = square[0]; p[3] = square[1]; break;
                    }
                }
                else if(index == 9 || index == 3 || index == 6 || index == 12 || index == 5 || index == 10)
                {
                    points_above = 2;
                    switch(index)
                    {
                        case 0b1001: p[0] = square[2]; p[1] = square[3]; p[2] = square[0]; p[3] = square[1]; break;
                        case 0b0011: p[0] = square[3]; p[1] = square[0]; p[2] = square[1]; p[3] = square[2]; break;
                        case 0b0110: p[0] = square[0]; p[1] = square[1]; p[2] = square[2]; p[3] = square[3]; break;
                        case 0b1100: p[0] = square[1]; p[1] = square[2]; p[2] = square[3]; p[3] = square[0]; break;

                        case 0b1010: p[0] = square[0]; p[1] = square[1]; p[2] = square[2]; p[3] = square[3]; saddle = true; break;
                        case 0b0101: p[0] = square[1]; p[1] = square[2]; p[2] = square[3]; p[3] = square[0]; saddle = true; break;
                    }
                }
                else if(index == 14 || index == 13 || index == 11 || index == 7)
                {
                    points_above = 3;
                    switch(index)
                    {
                        case 0b1110: p[0] = square[0]; p[1] = square[1]; p[2] = square[2]; p[3] = square[3]; break;
                        case 0b1101: p[0] = square[1]; p[1] = square[2]; p[2] = square[3]; p[3] = square[0]; break;
                        case 0b1011: p[0] = square[2]; p[1] = square[3]; p[2] = square[0]; p[3] = square[1]; break;
                        case 0b0111: p[0] = square[3]; p[1] = square[0]; p[2] = square[1]; p[3] = square[2]; break;
                    }
                }
                else
                {
                    points_above = 4;
                }

                var heights = new NativeArray<float>(4, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                heights[0] = p[0].y;
                heights[1] = p[1].y;
                heights[2] = p[2].y;
                heights[3] = p[3].y;

                // Current plane
                var cplane = new NativeArray<float3>(4, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                cplane[0] = new float3(p[0].x, h, p[0].z);
                cplane[1] = new float3(p[1].x, h, p[1].z);
                cplane[2] = new float3(p[2].x, h, p[2].z);
                cplane[3] = new float3(p[3].x, h, p[3].z);

                // The plane below
                var bplane = new NativeArray<float3>(4, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                bplane[0] = new float3(p[0].x, h - 0.5f, p[0].z);
                bplane[1] = new float3(p[1].x, h - 0.5f, p[1].z);
                bplane[2] = new float3(p[2].x, h - 0.5f, p[2].z);
                bplane[3] = new float3(p[3].x, h - 0.5f, p[3].z);

                if(points_above == 4)
                {
                    triangles.Add(new Triangle(0, 1, 2) + vertices.Length);
                    triangles.Add(new Triangle(0, 2, 3) + vertices.Length);

                    vertices.Add(new Vertex { pos = cplane[0], col = color });
                    vertices.Add(new Vertex { pos = cplane[1], col = color });
                    vertices.Add(new Vertex { pos = cplane[2], col = color });
                    vertices.Add(new Vertex { pos = cplane[3], col = color });
                }
                else if(points_above == 1)
                {
                    var t0 = Interpolation(heights[0], heights[1], h);
                    var t1 = Interpolation(heights[2], heights[1], h);

                    var cplane0n = math.lerp(cplane[0], cplane[1], t0);
                    var cplane1n = math.lerp(cplane[2], cplane[1], t1);

                    var bplane0n = math.lerp(bplane[0], bplane[1], t0);
                    var bplane1n = math.lerp(bplane[2], bplane[1], t1);

                    triangles.Add(new Triangle(0, 1, 2) + vertices.Length);
                    triangles.Add(new Triangle(0, 2, 3) + vertices.Length);
                    triangles.Add(new Triangle(4, 5, 6) + vertices.Length);
                    
                    vertices.Add(new Vertex { pos = bplane0n, col = color });
                    vertices.Add(new Vertex { pos = cplane0n, col = color });
                    vertices.Add(new Vertex { pos = cplane1n, col = color });
                    vertices.Add(new Vertex { pos = bplane1n, col = color });

                    vertices.Add(new Vertex { pos = cplane0n , col = color });
                    vertices.Add(new Vertex { pos = cplane[1], col = color });
                    vertices.Add(new Vertex { pos = cplane1n , col = color });
                }
                else if(points_above == 2)
                {
                    if(!saddle)
                    {
                        var t0 = Interpolation(heights[0], heights[1], h);
                        var t1 = Interpolation(heights[3], heights[2], h);

                        var cplane0n = math.lerp(cplane[0], cplane[1], t0);
                        var cplane1n = math.lerp(cplane[3], cplane[2], t1);

                        var bplane0n = math.lerp(bplane[0], bplane[1], t0);
                        var bplane1n = math.lerp(bplane[3], bplane[2], t1);

                        triangles.Add(new Triangle(0, 1, 2) + vertices.Length);
                        triangles.Add(new Triangle(0, 2, 3) + vertices.Length);

                        triangles.Add(new Triangle(4, 5, 6) + vertices.Length);
                        triangles.Add(new Triangle(4, 6, 7) + vertices.Length);

                        vertices.Add(new Vertex { pos = bplane0n, col = color });
                        vertices.Add(new Vertex { pos = cplane0n, col = color });
                        vertices.Add(new Vertex { pos = cplane1n, col = color });
                        vertices.Add(new Vertex { pos = bplane1n, col = color });

                        vertices.Add(new Vertex { pos = cplane0n , col =  color });
                        vertices.Add(new Vertex { pos = cplane[1], col =  color });
                        vertices.Add(new Vertex { pos = cplane[2], col =  color });
                        vertices.Add(new Vertex { pos = cplane1n , col =  color });
                    }
                    else
                    {
                        var t0 = Interpolation(heights[0], heights[1], h);
                        var t1 = Interpolation(heights[2], heights[1], h);

                        var t2 = Interpolation(heights[2], heights[3], h);
                        var t3 = Interpolation(heights[0], heights[3], h);

                        var cplane0n = math.lerp(cplane[0], cplane[1], t0);
                        var cplane1n = math.lerp(cplane[2], cplane[1], t1);
                        var cplane2n = math.lerp(cplane[2], cplane[3], t2);
                        var cplane3n = math.lerp(cplane[0], cplane[3], t3);

                        var bplane0n = math.lerp(bplane[0], bplane[1], t0);
                        var bplane1n = math.lerp(bplane[2], bplane[1], t1);
                        var bplane2n = math.lerp(bplane[2], bplane[3], t2);
                        var bplane3n = math.lerp(bplane[0], bplane[3], t3);

                        triangles.Add(new Triangle( 0,  1,  2) + vertices.Length);
                        triangles.Add(new Triangle( 0,  2,  3) + vertices.Length);
                        triangles.Add(new Triangle( 4,  5,  6) + vertices.Length);
                        triangles.Add(new Triangle( 7,  8,  9) + vertices.Length);
                        triangles.Add(new Triangle( 7,  9, 10) + vertices.Length);
                        triangles.Add(new Triangle(11, 12, 13) + vertices.Length);

                        vertices.Add(new Vertex { pos = bplane0n,  col = color });
                        vertices.Add(new Vertex { pos = cplane0n,  col = color });
                        vertices.Add(new Vertex { pos = cplane1n,  col = color });
                        vertices.Add(new Vertex { pos = bplane1n,  col = color });

                        vertices.Add(new Vertex { pos = cplane0n , col = color });
                        vertices.Add(new Vertex { pos = cplane[1], col = color });
                        vertices.Add(new Vertex { pos = cplane1n , col = color });

                        vertices.Add(new Vertex { pos = bplane2n,  col = color });
                        vertices.Add(new Vertex { pos = cplane2n,  col = color });
                        vertices.Add(new Vertex { pos = cplane3n,  col = color });
                        vertices.Add(new Vertex { pos = bplane3n,  col = color });

                        vertices.Add(new Vertex { pos = cplane2n , col = color });
                        vertices.Add(new Vertex { pos = cplane[3], col = color });
                        vertices.Add(new Vertex { pos = cplane3n , col = color });
                    }
                }
                else if(points_above == 3)
                {
                    var t0 = Interpolation(heights[1], heights[0], h);
                    var t1 = Interpolation(heights[3], heights[0], h);

                    var cplane0n = math.lerp(cplane[1], cplane[0], t0);
                    var cplane1n = math.lerp(cplane[3], cplane[0], t1);

                    var bplane0n = math.lerp(bplane[1], bplane[0], t0);
                    var bplane1n = math.lerp(bplane[3], bplane[0], t1);

                    triangles.Add(new Triangle(0, 1, 2) + vertices.Length);
                    triangles.Add(new Triangle(0, 2, 3) + vertices.Length);
                    triangles.Add(new Triangle(6, 4, 5) + vertices.Length);
                    triangles.Add(new Triangle(6, 7, 8) + vertices.Length);
                    triangles.Add(new Triangle(6, 8, 4) + vertices.Length);

                    vertices.Add(new Vertex { pos = bplane0n, col = color });
                    vertices.Add(new Vertex { pos = cplane0n, col = color });
                    vertices.Add(new Vertex { pos = cplane1n, col = color });
                    vertices.Add(new Vertex { pos = bplane1n, col = color });

                    vertices.Add(new Vertex { pos = cplane0n , col = color });
                    vertices.Add(new Vertex { pos = cplane[1], col = color });
                    vertices.Add(new Vertex { pos = cplane[2], col = color });
                    vertices.Add(new Vertex { pos = cplane[3], col = color });
                    vertices.Add(new Vertex { pos = cplane1n , col = color });
                }
            }
        }

        /// <summary>
        /// Linear interpolation.
        /// </summary>
        private static float Interpolation(float a, float b, float t) => (a - t) / (a - b);
    }
}