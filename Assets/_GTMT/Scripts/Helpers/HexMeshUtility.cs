using UnityEngine;

namespace GTMT
{
    public static class HexMeshUtility
    {
        /* Const */
        private const float c_Radius = 0.866025404f; // Mathf.Sqrt(3) / 2;


        /* Variables */
        public static float InnerRadius;
        public static float OuterRadius;
        public static float SolidPercent;
        public static float BlendPercent;
        public static float ElevationStep;
        public static float TerraceSteps;
        public static float HorizontalTerraceStepSize;
        public static float VerticalTeraceStepSize;
        public static int ChunkSizeX;
        public static int ChunkSizeZ;
        public static Vector3[] Corners;


        /* SetUp */
        public static void SetUpUtility(float radius, float blendPercent, float elevationStep, float terracesPerSlope, int chunkSizeX, int chunkSizeZ)
        {
            OuterRadius = radius;
            InnerRadius = radius * c_Radius;

            BlendPercent = blendPercent;
            SolidPercent = 1.0f - BlendPercent;

            ElevationStep = elevationStep;
            TerraceSteps = terracesPerSlope * 2 + 1;
            HorizontalTerraceStepSize = 1f / TerraceSteps;
            VerticalTeraceStepSize = 1f / (terracesPerSlope + 1);

            ChunkSizeX = chunkSizeX;
            ChunkSizeZ = chunkSizeZ;

            Corners = new Vector3[6];
            Corners[0] = new Vector3(0f, 0f, OuterRadius);                      // Top Right
            Corners[1] = new Vector3(InnerRadius, 0f, 0.5f * OuterRadius);      // Right
            Corners[2] = new Vector3(InnerRadius, 0f, -0.5f * OuterRadius);     // Bottom Right
            Corners[3] = new Vector3(0f, 0f, -OuterRadius);                     // Bottom Left
            Corners[4] = new Vector3(-InnerRadius, 0f, -0.5f * OuterRadius);     // Left
            Corners[5] = new Vector3(-InnerRadius, 0f, 0.5f * OuterRadius);      // Top Left

        }


        /* First corner in the specific direction */
        public static ref Vector3 GetCornerA(HexDirection direction)
        {
            return ref Corners[(int)direction];
        }
       
        /* Second corner in the specific direction */
        public static ref Vector3 GetCornerB(HexDirection direction)
        {
            return ref Corners[(int)direction.Next()];
        }


        /* Bridge */
        public static Vector3 GetBridgeComplex(HexDirection direction)
        {
            return (Corners[(int)direction] + Corners[(int)direction.Next()]) * 0.5f * BlendPercent;
        }

        public static Vector3 GetBridgeSimple(HexDirection direction)
        {
            return (Corners[(int)direction] + Corners[(int)direction.Next()]) * BlendPercent;
        }


        /* Terrace Lerp */
        public static Vector3 TerraceLerp(Vector3 a, Vector3 b, int step)
        {
            float h = step * HorizontalTerraceStepSize;
            a.x += (b.x - a.x) * h;
            a.z += (b.z - a.z) * h;
            float v = ((step + 1) / 2) * VerticalTeraceStepSize;
            a.y += (b.y - a.y) * v;
            return a;
        }

        public static Color TerraceLerp(Color a, Color b, int step)
        {
            float h = step * HorizontalTerraceStepSize;
            return Color.Lerp(a, b, h);
        }


        /* Hex Edge Type */
        public static HexEdgeType GetEdgeType(int elevation_first, int elevation_second)
        {
            if (elevation_first == elevation_second)
            {
                return HexEdgeType.Flat;
            }

            int delta = elevation_second - elevation_first;
            if (delta == 1 || delta == -1)
            {
                return HexEdgeType.Slope;
            }

            return HexEdgeType.Cliff;
        }
    }
}

