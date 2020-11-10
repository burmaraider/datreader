using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTTypes
{
    public class LTTypes
    {
        /*
         * type
      LTFloat = Single;
      LTVector = record
        x: LTFloat;
        y: LTFloat;
        z: LTFloat;
      end;

      LTRotation = record
        x: LTFloat;
        y: LTFloat;
        z: LTFloat;
        w: LTFloat;
      end;

      TUVPair = record
        a: LTFloat;
        b: LTFloat;
      end;

      PLTVector = ^LTVector;
      PLTRotation = ^LTRotation;
      PLTFloat = ^LTFloat;
      PUVPair = ^TUVPair;

      TLTUnknownStruct1 = record
        vVec1: LTVector;
        vVec2: LTVector;
        nWord1: Word;
      end;*/



        public enum PropType
        {
            PT_STRING = 0,
            PT_VECTOR = 1,
            PT_COLOR = 2,
            PT_REAL = 3,
            PT_FLAGS = 4,
            PT_BOOL = 5,
            PT_LONGINT = 6,
            PT_ROTATION = 7
        }

        public struct LTFloat
        {
            public LTFloat(float i)
            {
                I = i;
            }
            public float I { get; set; }

        }

        /// <summary>
        /// Return type of X, Y, Z floats
        /// </summary>
        public struct LTVector
        {
            public LTVector(LTFloat x, LTFloat y, LTFloat z)
            {
                X = x;
                Y = y;
                Z = z;
            }
            public LTFloat X { get; set; }
            public LTFloat Y { get; set; }
            public LTFloat Z { get; set; }
        }
        public struct LTRotation
        {
            public LTRotation(LTFloat x, LTFloat y, LTFloat z, LTFloat w)
            {
                X = x;
                Y = y;
                Z = z;
                W = w;
            }
            public LTFloat X { get; set; }
            public LTFloat Y { get; set; }
            public LTFloat Z { get; set; }
            public LTFloat W { get; set; }
        }

        public struct TUVPair
        {
            public TUVPair(LTFloat u, LTFloat v)
            {
                U = u;
                V = v;
            }
            public LTFloat U { get; set; }
            public LTFloat V { get; set; }
        }
    }
}