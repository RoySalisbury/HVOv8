using HVO.Weather;
using System;

namespace HVO
{
    public sealed class Direction
    {

        public Direction(short degree)
        {
            if ((degree < 0) || (degree > 360))
            {
                throw new ArgumentOutOfRangeException(nameof(degree));
            }

            // If the degree is 360, then we want to reset it to 0
            if (degree == 360)
            {
                degree = 0;
            }

            this.Degree = degree;
        }
        public Direction(CompassPoint cardinalPoint)
        {
            this.Degree = (short)cardinalPoint;
        }

        public short Degree
        {
            get;
            private set;
        }

        public CompassPoint CardinalPoint
        {
            get
            {
                // Get the closest cardinal point possible from the compass degree
                if ((this.Degree >= (short)CompassPoint.N) & (this.Degree < (short)CompassPoint.NNE))
                {
                    return CompassPoint.N;
                }
                else if ((this.Degree >= (short)CompassPoint.NNE) & (this.Degree < (short)CompassPoint.NE))
                {
                    return CompassPoint.NNE;
                }
                else if ((this.Degree >= (short)CompassPoint.NE) & (this.Degree < (short)CompassPoint.ENE))
                {
                    return CompassPoint.NE;
                }
                else if ((this.Degree >= (short)CompassPoint.ENE) & (this.Degree < (short)CompassPoint.E))
                {
                    return CompassPoint.ENE;
                }
                else if ((this.Degree >= (short)CompassPoint.E) & (this.Degree < (short)CompassPoint.ESE))
                {
                    return CompassPoint.E;
                }
                else if ((this.Degree >= (short)CompassPoint.ESE) & (this.Degree < (short)CompassPoint.SE))
                {
                    return CompassPoint.ESE;
                }
                else if ((this.Degree >= (short)CompassPoint.SE) & (this.Degree < (short)CompassPoint.SSE))
                {
                    return CompassPoint.SE;
                }
                else if ((this.Degree >= (short)CompassPoint.SSE) & (this.Degree < (short)CompassPoint.S))
                {
                    return CompassPoint.SSE;
                }
                else if ((this.Degree >= (short)CompassPoint.S) & (this.Degree < (short)CompassPoint.SSW))
                {
                    return CompassPoint.S;
                }
                else if ((this.Degree >= (short)CompassPoint.SSW) & (this.Degree < (short)CompassPoint.SW))
                {
                    return CompassPoint.SSW;
                }
                else if ((this.Degree >= (short)CompassPoint.SW) & (this.Degree < (short)CompassPoint.WSW))
                {
                    return CompassPoint.SW;
                }
                else if ((this.Degree >= (short)CompassPoint.WSW) & (this.Degree < (short)CompassPoint.W))
                {
                    return CompassPoint.WSW;
                }
                else if ((this.Degree >= (short)CompassPoint.W) & (this.Degree < (short)CompassPoint.WNW))
                {
                    return CompassPoint.W;
                }
                else if ((this.Degree >= (short)CompassPoint.WNW) & (this.Degree < (short)CompassPoint.NW))
                {
                    return CompassPoint.WNW;
                }
                else if ((this.Degree >= (short)CompassPoint.NW) & (this.Degree < (short)CompassPoint.NNW))
                {
                    return CompassPoint.NW;
                }
                else
                {
                    return CompassPoint.NNW;
                }
            }
        }
    }
}
