using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GTMT
{
    [System.Serializable]
    public class HexCoordinate
    {
        [SerializeField]
        private int x, z;

        public int X
        {
            get
            {
                return x;
            }
        }

        public int Y
        {
            get
            {
                return -X - Z;
            }
        }
        public int Z
        {
            get
            {
                return z;
            }
        }

        public HexCoordinate(int x, int z)
        {
            this.x = x;
            this.z = z;
        }


        public int DistanceTo(HexCoordinate other)
        {
            return Mathf.Abs(x - other.x) + Mathf.Abs(Y- other.Y) + Mathf.Abs(z - other.z) / 2;
        }


        public static HexCoordinate FromOffsetCoordinates(int x, int z)
        {
            return new HexCoordinate(x - z / 2, z);             // (x - z / 2) (account for zig-zagging in x coord) 
        }

        public static HexCoordinate FromPosition(Vector3 position, float innerRadius, float outerRadius)
        {
            float x = position.x / (innerRadius * 2f);
            float y = -x;

            float offset = position.z / (outerRadius * 3f);
            x -= offset;
            y -= offset;

            int iX = Mathf.RoundToInt(x);
            int iY = Mathf.RoundToInt(y);
            int iZ = Mathf.RoundToInt(-x - y);

            if (iX + iY + iZ != 0)
            {
                float dX = Mathf.Abs(x - iX);
                float dY = Mathf.Abs(y - iY);
                float dZ = Mathf.Abs(-x - y - iZ);

                if (dX > dY && dX > dZ)
                {
                    iX = -iY - iZ;
                }
                else if (dZ > dY)
                {
                    iZ = -iX - iY;
                }
            }

            return new HexCoordinate(iX, iZ);
        }




        public override string ToString()
        {
            return "(" + X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ")";
        }
    }
}

