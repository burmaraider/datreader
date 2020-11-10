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
using System.Windows.Forms;
using static LTTypes.LTTypes;

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
        byte propertyType;

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


            //Read how many entries this object has
            file.Read(tempByte, 0, 4);
            obj.objectEntries = BitConverter.ToInt16(tempByte, 0);

            //Array.Resize(ref tempByte, 4);

            for (int t = 0; t < obj.objectEntries; t++)
            {
                tempDataLength = ReadDataLength(ref file);

                //Read the property name
                string tempKey = ReadString(ref file, tempDataLength);

                //Read the Property Type
                propertyType = ReadPropertyType(ref file);

                switch (propertyType)
                {
                    case (byte)PropType.PT_STRING:
                        file.Position += 6;
                        //Get Data Length
                        tempDataLength = ReadDataLength(ref file);
                        //Read the string
                        tempData.Add(tempKey, ReadString(ref file, tempDataLength));
                        break;

                    case (byte)PropType.PT_VECTOR:
                        //Skip ahead
                        //Dont care about prop flags..YET?
                        file.Position += 4;
                        //Get our data length
                        tempDataLength = ReadDataLength(ref file);
                        //Get our float data
                        LTVector tempVec = ReadLTVector(ref file);
                        //Add our object to the Dictionary
                        tempData.Add(tempKey, tempVec);
                        break;

                    case (byte)PropType.PT_ROTATION:
                        //Skip ahead
                        //Dont care about prop flags..YET?
                        file.Position += 4;
                        //Get our data length
                        tempDataLength = ReadDataLength(ref file);
                        //Get our float data
                        LTRotation tempRot = ReadLTRotation(ref file);
                        //Add our object to the Dictionary
                        tempData.Add(tempKey, tempRot);
                        break;
                    case (byte)PropType.PT_LONGINT:
                        //Skip ahead
                        //Dont care about prop flags..YET?
                        file.Position += 2;
                        //Get our data length
                        //tempDataLength = ReadDataLength(ref file);
                        //Get our float data
                        Int64 longInt = ReadLongInt(ref file);
                        //Add our object to the Dictionary
                        tempData.Add(tempKey, longInt);
                        break;
                    case (byte)PropType.PT_BOOL:
                        //Skip ahead
                        //Dont care about prop flags..YET?
                        file.Position += 6;
                        //Add our object to the Dictionary
                        tempData.Add(tempKey, ReadBool(ref file));
                        break;
                    case (byte)PropType.PT_REAL:
                        //Skip ahead
                        //Dont care about prop flags..YET?
                        file.Position += 4;
                        //Get our data length
                        tempDataLength = ReadDataLength(ref file);
                        //Add our object to the Dictionary
                        tempData.Add(tempKey, ReadReal(ref file));
                        break;
                    case (byte)PropType.PT_COLOR:
                        //Skip ahead
                        //Dont care about prop flags..YET?
                        file.Position += 4;
                        //Get our data length
                        tempDataLength = ReadDataLength(ref file);
                        //Get our float data
                        LTVector tempCol = ReadLTVector(ref file);
                        //Add our object to the Dictionary
                        tempData.Add(tempKey, tempCol);
                        break;
                }
            }

            obj.options = tempData;

            temp.obj.Add(obj);
        }

        temp.endingOffset = (int)file.Position;
        return temp;
    }

    /// <summary >
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

    /// <summary>
    /// Get the object transform X, Y, Z of the Lithtech Object
    /// </summary>
    /// <param name="file"></param>
    /// <seealso cref="LTVector">See here</seealso>
    /// <returns></returns>
    private static LTVector ReadLTVector(ref FileStream file)
    {
        //Read data length 12 bytes
        //x - single
        //y - single
        //z - single
        byte[] tempByte = new byte[12];
        file.Read(tempByte, 0, 12);

        float x, y, z;

        x = BitConverter.ToSingle(tempByte, 0);
        y = BitConverter.ToSingle(tempByte, sizeof(Single));
        z = BitConverter.ToSingle(tempByte, sizeof(Single) + sizeof(Single));

        return new LTVector(new LTFloat(x), new LTFloat(y), new LTFloat(z));
    }

    /// <summary>
    /// Get the object Rotation X, Y, Z, W of the Lithtech Object
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    private static LTRotation ReadLTRotation(ref FileStream file)
    {
        //Read data length 12 bytes
        //x - single
        //y - single
        //z - single
        //w - single
        byte[] tempByte = new byte[16];
        file.Read(tempByte, 0, 16);

        float x, y, z, w;

        x = BitConverter.ToSingle(tempByte, 0);
        y = BitConverter.ToSingle(tempByte, sizeof(Single));
        z = BitConverter.ToSingle(tempByte, sizeof(Single) + sizeof(Single));
        w = BitConverter.ToSingle(tempByte, sizeof(Single) + sizeof(Single) + sizeof(Single));

        return new LTRotation(new LTFloat(x), new LTFloat(y), new LTFloat(z), new LTFloat(w));
    }

    /// <summary>
    /// Get the objects string, either name or paramters (ie: trigger message) of the Lithtech Object
    /// </summary>
    /// <param name="file"></param>
    /// <param name="stringLength"></param>
    /// <returns></returns>
    private static string ReadString(ref FileStream file, int stringLength)
    {
        //Read the string
        byte[] tempByte = new byte[stringLength];
        file.Read(tempByte, 0, tempByte.Length);
        return System.Text.Encoding.ASCII.GetString(tempByte);
    }

    /// <summary>
    /// Get the objects property type of the Lithtech Object
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    private static byte ReadPropertyType(ref FileStream file)
    {
        //Read the string
        byte[] tempByte = new byte[1];
        file.Read(tempByte, 0, tempByte.Length);
        return tempByte[0];
    }

    /// <summary>
    /// Get the LongInt used in AllowedGameTypes of the Lithtech Object
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    private static Int64 ReadLongInt(ref FileStream file)
    {
        //Read the string
        byte[] tempByte = new byte[8];
        file.Read(tempByte, 0, tempByte.Length);
        return BitConverter.ToInt64(tempByte, 0);
    }

    /// <summary>
    /// Get the true or false flag from the property of the Lithtech Object
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    private static bool ReadBool(ref FileStream file)
    {
        //Read the string
        byte[] tempByte = new byte[1];
        file.Read(tempByte, 0, tempByte.Length);
        return BitConverter.ToBoolean(tempByte, 0);
    }
    /// <summary>
    /// Get the Real used in single float values of the Lithtech Object
    /// </summary>
    /// <param name="file"></param>
    /// <returns>description</returns>
    private static float ReadReal(ref FileStream file)
    {
        //Read the string
        byte[] tempByte = new byte[4];
        file.Read(tempByte, 0, tempByte.Length);
        return BitConverter.ToSingle(tempByte, 0);
    }
}