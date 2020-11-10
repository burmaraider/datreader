using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using AVP2.DAT.Reader.Classes;
using System.Runtime.CompilerServices;

public class datreader
{
    public string fileNameOpened;
    //public LTTypes.LTVector test = new LTTypes.LTVector(new LTTypes.LTFloat(2.35352f), new LTTypes.LTFloat(2.0f), new LTTypes.LTFloat(4.0f));

    [StructLayout(LayoutKind.Sequential)]
    public struct DATHeader
    {
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public readonly int nVersion;
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public readonly int ObjectDataPos;
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public readonly int RenderDataPos;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] unk1;
    }

    public static World.WorldObjects ReadObjects(FileStream file, int objectCount, int lastPosition)
    {
        World.WorldObjects temp = new World.WorldObjects();
        temp.obj = new List<World.WorldObject>();
        int tempDataLength;

        byte[] tempByte = new byte[4];

        file.Position = lastPosition;

        for (int i = 0; i < objectCount; i++)
        {
            //Make a new object to store this info
            World.WorldObject obj = new World.WorldObject();

            //Make a dictionary to make things easier
            Dictionary<string, object> tempData = new Dictionary<string, object>();

            file.Read(tempByte, 0, sizeof(Int32));
            var dataLength = BitConverter.ToInt16(tempByte, 0);

            //Get out current position in the file
            var currentPos = (int)file.Position;
            //Calculate our end position
            int endPos = (currentPos + (int)dataLength) - 2;

            var lengthOfString = BitConverter.ToInt16(tempByte, 2);
            Array.Resize(ref tempByte, lengthOfString);
            file.Read(tempByte, 0, lengthOfString);

            obj.dataLength = dataLength;
            obj.objectType = System.Text.Encoding.ASCII.GetString(tempByte);

            obj.data = new List<byte[]>();

            //Read how many entries this object has
            file.Read(tempByte, 0, 4);
            obj.objectEntries = BitConverter.ToInt16(tempByte, 0);

            //Array.Resize(ref tempByte, 4);

            for (int t = 0; t < obj.objectEntries; t++)
            {
                tempDataLength = ReadDataLength(ref file);

                Array.Resize(ref tempByte, tempDataLength);

                file.Read(tempByte, 0, tempDataLength);

                string tempKey = System.Text.Encoding.ASCII.GetString(tempByte);

                Array.Resize(ref tempByte, 1);

                //Read what type of object data this is
                file.Read(tempByte, 0, 1);



                switch (tempByte[0])
                {
                    case (byte)LTTypes.PropType.PT_STRING:
                        obj.objectType = "String";
                        file.Position += 6;

                        //Get Data Length
                        tempDataLength = ReadDataLength(ref file);

                        Array.Resize(ref tempByte, tempDataLength);
                        file.Read(tempByte, 0, tempDataLength);

                        string tempValue = System.Text.Encoding.ASCII.GetString(tempByte);

                        tempData.Add(tempKey, tempValue);

                        break;

                    case (byte)LTTypes.PropType.PT_VECTOR:
                        obj.objectType = "Vector";
                        //Skip ahead
                        //Dont care about prop flags..YET?
                        file.Position += 4;

                        tempDataLength = ReadDataLength(ref file);

                        //resize to fit float X Y Z
                        Array.Resize(ref tempByte, tempDataLength);
                        file.Read(tempByte, 0, tempDataLength);

                        float x, y, z;

                        x = BitConverter.ToSingle(tempByte, 0);
                        y = BitConverter.ToSingle(tempByte, sizeof(Single));
                        z = BitConverter.ToSingle(tempByte, sizeof(Single) + sizeof(Single));

                        LTTypes.LTVector tempVec = new LTTypes.LTVector(new LTTypes.LTFloat(x), new LTTypes.LTFloat(y), new LTTypes.LTFloat(z));

                        tempData.Add(tempKey, tempVec);

                        break;

                    case (byte)LTTypes.PropType.PT_ROTATION:
                        obj.objectType = "Rotation";


                        break;
                }
            }

            temp.obj.Add(obj);
        }
        temp.endingOffset = (int)file.Position;
        return temp;
    }

    /// <summary>
    /// Get the data length for the property of the Lithtech Object
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    private static Int16 ReadDataLength(ref FileStream file)
    {
        //Read data length 2 bytes
        byte[] tempByte = new byte[2];  
        file.Read(tempByte, 0, 2);
        return BitConverter.ToInt16(tempByte, 0);
    }

}