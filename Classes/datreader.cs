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
    public static WorldExtents worldExtents;
    public static WorldTreeNode m_pRootNode;
    public static WorldTree worldTree;

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

    public struct WorldExtents
    {
        public WorldExtents(int i, LTVector o, LTVector p)
        {
            unk = i;
            vExtentsMin = o;
            vExtentsMax = p;
        }

        public int unk;
        public LTVector vExtentsMin;
        public LTVector vExtentsMax;
    }

    public class WorldTreeNode
    {
        public WorldTreeNode(List<object> nodeList)
        {
            pNodeList = nodeList;
        }

        public LTVector vBBoxMin { get; set; }
        public LTVector vBBoxMax { get; set; }
        public LTFloat fCenterX { get; set; }
        public LTFloat fCenterZ { get; set; }
        public LTFloat fSmallestDim { get; set; }
        public WorldTreeNode pParent { get; set; }
        public Int32 nChildren { get; set; }
        public List<object> pNodeList { get; set; }

        public void SetBB(LTVector a, LTVector b)
        {
            vBBoxMin = a;
            vBBoxMax = b;

            fCenterX = (LTFloat)((LTFloat)(b.X + a.X) * 0.5f);
            fCenterZ = (LTFloat)((LTFloat)(b.Z + a.Z) * 0.5f);
            fSmallestDim = (LTFloat)Math.Min(b.X - a.X, b.Z - a.Z);
        }

    }

    public class WorldTree
    {
        public Int32 nNumNode { get; set; }
        public WorldTreeNode pRootNode { get; set; }
        public List<object> pNodes { get; set; }

        public WorldTree()
        {
            pNodes = new List<object>();
            pRootNode = new WorldTreeNode(pNodes);
        }

        public WorldTreeNode ReadWorldTree(FileStream file)
        {
            int nDummyTerrainDepth, nCurOffset, i;
            LTVector vBoxMin, vBoxMax;
            byte nCurByte, nCurBit;
            WorldTreeNode pNewNode;

            nDummyTerrainDepth = 0;
            vBoxMin = ReadLTVector(ref file);
            vBoxMax = ReadLTVector(ref file);

            nNumNode = ReadInt(ref file);
            nDummyTerrainDepth = ReadInt(ref file);

            if (nNumNode > 1)
            {
                for (int t = 0; t < nNumNode - 2; t++)
                {
                    pNewNode = new WorldTreeNode(pNodes);
                    pNodes.Add(pNewNode);
                }
            }

            nCurByte = 0;
            nCurBit = 8;

            pRootNode.SetBB(vBoxMin, vBoxMax);

            nCurOffset = 0;

            return new WorldTreeNode(pNodes);
        }

    }

    public static WorldObjects ReadObjects(FileStream file, int objectCount, int lastPosition)
    {
        WorldObjects temp = new WorldObjects();
        temp.obj = new List<WorldObject>();
        int tempDataLength;
        byte propertyType;

        byte[] tempByte = new byte[4];

        file.Position = lastPosition;

        for (int i = 0; i < objectCount; i++)
        {
            //Make a new object to store this info
            WorldObject obj = new WorldObject();

            //Make a dictionary to make things easier
            Dictionary<string, object> tempData = new Dictionary<string, object>();
            obj.dataOffset = (int)file.Position;
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

            Array.Resize(ref tempByte, 4);
            //Read how many entries this object has
            file.Read(tempByte, 0, 4);
            obj.objectEntries = BitConverter.ToInt16(tempByte, 0);


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

    /// <summary>
    /// Read the World Extents from the .DAT file
    /// </summary>
    /// <param name="file"></param>
    /// <param name="lastPosition"></param>
    /// <returns></returns>
    public static WorldExtents ReadWorldExtents(FileStream file, int lastPosition)
    {
        int tempunk = ReadInt(ref file);
        LTVector temp1 = ReadLTVector(ref file);
        LTVector temp2 = ReadLTVector(ref file);

        worldExtents = new WorldExtents(tempunk, temp1, temp2);

        return worldExtents;

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

    /// <summary>
    /// Get the integer in the bitstream
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    private static int ReadInt(ref FileStream file)
    {
        //Read the int
        byte[] tempByte = new byte[4];
        file.Read(tempByte, 0, tempByte.Length);
        return BitConverter.ToInt32(tempByte, 0);
    }
}