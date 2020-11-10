using AVP2.DAT.Reader.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AVP2.DAT.Reader
{
    public partial class Form1 : Form
    {
        public datreader.DATHeader header;
        byte[] bufferFile;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            

            OpenFileDialog myOpenFileDialog = new OpenFileDialog();

            myOpenFileDialog.Filter = "AVP2 World|*.dat";
            if (myOpenFileDialog.ShowDialog() != DialogResult.Cancel)
            {
                // If the file name is not an empty string open it for saving.
                if (myOpenFileDialog.FileName != "")
                {
                    // Saves the Image via a FileStream created by the OpenFile method.
                    System.IO.FileStream fs =
                        (System.IO.FileStream)myOpenFileDialog.OpenFile();

                            //Do something else for .tex
                    bufferFile = File.ReadAllBytes(myOpenFileDialog.FileName);
                    header = RawDataToObject<datreader.DATHeader>(bufferFile);

                    //Debug

                    richTextBox1.Text += "World Name: " + myOpenFileDialog.FileName;
                    richTextBox1.Text += "\n World Version: " + header.nVersion.ToString();
                    richTextBox1.Text += "\n World Object Data Position: " + header.ObjectDataPos.ToString();
                    richTextBox1.Text += "\n World Render Data Position: " + header.RenderDataPos.ToString();


                    //Read the world properties
                    byte[] temp = new byte[4];

                    //Set position to WorldInfo
                    fs.Position = 44;
                    //read World Info length into temp
                    fs.Read(temp, 0, 4);

                    //Convert bytes into int length
                    int tempLength = BitConverter.ToInt32(temp,0);

                    //Resize the array to get a string
                    Array.Resize(ref temp, 1024);
                    fs.Read(temp, 0, tempLength);
                    //Convert the string
                    string tempString = System.Text.Encoding.ASCII.GetString(temp);
                    //Show the string
                    textBox3.Text = tempString;
                    richTextBox1.Text += "\n World Info String: " + tempString;

                    //Read objects
                    fs.Position = header.ObjectDataPos;

                    Array.Resize(ref temp, 4);
                    fs.Read(temp, 0, 4);

                    tempLength = BitConverter.ToInt32(temp,0);

                    fs.Read(temp, 0, 4);

                    var tempDataLength = BitConverter.ToInt16(temp, 0);
                    var tempStringLength = BitConverter.ToInt16(temp, 2);

                    richTextBox1.Text += "\n\n\n Object Count: " + tempLength.ToString();

                    int position = 0;

                    World.WorldObjects tempWorld = datreader.ReadObjects(fs, tempLength, (header.ObjectDataPos + 4));
                }
            }

            textBox1.Text = header.nVersion.ToString();
            textBox2.Text = header.ObjectDataPos.ToString();
            //textBox3.Text = header.RenderDataPos.ToString();
             
        }

        


        public static T RawDataToObject<T>(byte[] rawData) where T : struct
        {
            var pinnedRawData = GCHandle.Alloc(rawData,
                                               GCHandleType.Pinned);
            try
            {
                // Get the address of the data array
                var pinnedRawDataPtr = pinnedRawData.AddrOfPinnedObject();

                // overlay the data type on top of the raw data
                return (T)Marshal.PtrToStructure(pinnedRawDataPtr, typeof(T));
            }
            finally
            {
                // must explicitly release
                pinnedRawData.Free();
            }
        }
    }
}
