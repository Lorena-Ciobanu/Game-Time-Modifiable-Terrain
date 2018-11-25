namespace GTMT
{
    public enum HexDirection
    {
        TopRight, Right, BottomRight, BottomLeft, Left, TopLeft
        // NE, E, SE, SW, W, NW
    }

    public static class HexDirectionExtensions
    {
        /**
        *  Opposite Extension Method (extend HexDirection enum)
        *  TopRight -  BottomLeft
        *  Right - Left
        *  BottomRight - TopLeft
        * 
        */

        public static HexDirection Opposite(this HexDirection direction)    // Extension method (first argument needs this keyword)
        {
            return (int)direction < 3 ? (direction + 3) : (direction - 3);
        }


        /**
         *  Previous (Extension Method)
         *  TopRight - TopLeft
         *  Right - TopLeft
         *  BottomRight - Right (etc)
         * 
         */
        public static HexDirection Previous(this HexDirection direction)
        {
            return direction == HexDirection.TopRight ? HexDirection.TopLeft : (direction - 1);
        }


        /**
         *  Next (Extension Method)
         *  TopRight - Right
         *  Right - BottomRight
         *  BottomRight - BottomLeft (etc)
         * 
         */
        public static HexDirection Next(this HexDirection direction)
        {
            return direction == HexDirection.TopLeft ? HexDirection.TopRight : (direction + 1);
        }
    }
}
