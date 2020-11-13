using AVP2.DAT.Reader.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static LTTypes.LTTypes;

namespace AVP2.DAT.Reader
{
    public partial class Form1 : Form
    {
        public datreader.DATHeader header;
        byte[] bufferFile;

        public World.WorldObjects worldObjectList;
        public datreader.WorldExtents worldExtents;
        public datreader.WorldTreeNode worldNodeTree;
        public Form1()
        {
            InitializeComponent();
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        public void MakeList()
        {

            //Build the string
            ListViewItem tempItems = new ListViewItem();
            var items = new ListViewItem[worldObjectList.obj.Count()];

            int i = 0;
            foreach (World.WorldObject t in worldObjectList.obj)
            {
                items[i] = new ListViewItem(t.options["Name"].ToString());
                items[i].SubItems.Add(t.objectType.ToString());
                i++;
            }

            if (listView1.InvokeRequired)
            {
                listView1.Invoke(new MethodInvoker(delegate
                {
                    listView1.Items.AddRange(items);
                   

                }));
            }
            else
            {
                listView1.Items.AddRange(items);
            }

        }

        public static T RawDataToObject<T>(byte[] rawData) where T : struct
        {
            var pinnedRawData = GCHandle.Alloc(rawData, GCHandleType.Pinned);
            try
            {
                var pinnedRawDataPtr = pinnedRawData.AddrOfPinnedObject();
                return (T)Marshal.PtrToStructure(pinnedRawDataPtr, typeof(T));
            }
            finally
            {
                pinnedRawData.Free();
            }
        }

        public string GetFileName(string filePath)
        {
            string[] item = filePath.Split('\\');
            return item.Last();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            for (int i =0; i < 40; i++)
            {
                Label temp = new Label { Name = "TEST " + i, Text = "id#" + i };
                temp.Location = new Point(panel1.Location.X, panel1.Location.Y + (25 * i));

                panel1.Controls.Add(temp);
            }

            //dataGroupBox.Controls.sc
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            //Remove all previous controls
            panel1.Controls.Clear();

            //Set out group box label to the object name
            dataGroupBox.Text = (string)worldObjectList.obj[e.ItemIndex].options["Name"];

            //Add some items to the panel

            foreach(KeyValuePair<string, object> temp in worldObjectList.obj[e.ItemIndex].options)
            {
                Point tempLocation;
                int panelItemCount = panel1.Controls.Count;

                if(panelItemCount == 0)
                    tempLocation = new Point(panel1.Location.X, 0);
                else 
                    tempLocation = new Point(panel1.Location.X, panel1.Controls[panelItemCount -1].Location.Y + 25);

                //Create base label
                Label tempLabel = new Label { Text = temp.Key.ToString() };
                tempLabel.Location = tempLocation;
                //tempLabel.Width = tempLabel.Text.Length * 2;

                
                if (temp.Value is LTVector && temp.Key.Contains("Color"))
                {
                    LTVector vectorItem = (LTVector)temp.Value;
                    TextBox xTextBox = new TextBox { Text = vectorItem.X.ToString(), Width = 50 };
                    TextBox yTextBox = new TextBox { Text = vectorItem.Y.ToString(), Width = 50 };
                    TextBox zTextBox = new TextBox { Text = vectorItem.Z.ToString(), Width = 50 };

                    xTextBox.Location = new Point(tempLocation.X + 20 + tempLabel.Width, tempLocation.Y);
                    yTextBox.Location = new Point(xTextBox.Location.X + 70, tempLocation.Y);
                    zTextBox.Location = new Point(yTextBox.Location.X + 70, tempLocation.Y);

                    Color tempColor = new Color();
                    tempColor = Color.FromArgb(255, (int)vectorItem.X, (int)vectorItem.Y, (int)vectorItem.Z);
                    //Add color picker
                    PictureBox colorBox = new PictureBox { Size = new Size(20, 20), BackColor = tempColor};


                    colorBox.Location = new Point(zTextBox.Location.X + 70, tempLocation.Y);

                    panel1.Controls.Add(tempLabel);
                    panel1.Controls.Add(xTextBox);
                    panel1.Controls.Add(yTextBox);
                    panel1.Controls.Add(zTextBox);
                    panel1.Controls.Add(colorBox);
                }
                else if (temp.Value is LTVector)
                {
                    LTVector vectorItem = (LTVector)temp.Value;
                    TextBox xTextBox = new TextBox { Text = vectorItem.X.ToString(), Width = 50 };
                    TextBox yTextBox = new TextBox { Text = vectorItem.Y.ToString(), Width = 50 };
                    TextBox zTextBox = new TextBox { Text = vectorItem.Z.ToString(), Width = 50 };

                    xTextBox.Location = new Point(tempLocation.X + 20 + tempLabel.Width, tempLocation.Y);
                    yTextBox.Location = new Point(xTextBox.Location.X + 70, tempLocation.Y);
                    zTextBox.Location = new Point(yTextBox.Location.X + 70, tempLocation.Y);

                    panel1.Controls.Add(tempLabel);
                    panel1.Controls.Add(xTextBox);
                    panel1.Controls.Add(yTextBox);
                    panel1.Controls.Add(zTextBox);
                }
                else if (temp.Value is LTRotation)
                {
                    LTRotation vectorItem = (LTRotation)temp.Value;
                    TextBox xTextBox = new TextBox { Text = vectorItem.X.ToString(), Width = 50 };
                    TextBox yTextBox = new TextBox { Text = vectorItem.Y.ToString(), Width = 50 };
                    TextBox zTextBox = new TextBox { Text = vectorItem.Z.ToString(), Width = 50 };
                    TextBox wTextBox = new TextBox { Text = vectorItem.Z.ToString(), Width = 50 };

                    xTextBox.Location = new Point(tempLocation.X + 20 + tempLabel.Width, tempLocation.Y);
                    yTextBox.Location = new Point(xTextBox.Location.X + 70, tempLocation.Y);
                    zTextBox.Location = new Point(yTextBox.Location.X + 70, tempLocation.Y);
                    wTextBox.Location = new Point(zTextBox.Location.X + 70, tempLocation.Y);

                    panel1.Controls.Add(tempLabel);
                    panel1.Controls.Add(xTextBox);
                    panel1.Controls.Add(yTextBox);
                    panel1.Controls.Add(zTextBox);
                    panel1.Controls.Add(wTextBox);
                }
                else if (temp.Value is LTFloat)
                {
                    LTFloat vectorItem = (LTFloat)temp.Value;

                    TextBox xFloat= new TextBox { Text = vectorItem.ToString(), Width = 50 };
                    xFloat.Location = new Point(tempLocation.X + 20 + tempLabel.Width, tempLocation.Y);

                    //If allow gametype
                    if (!temp.Key.Contains("AllowIn"))
                        xFloat.Enabled = false;

                    panel1.Controls.Add(tempLabel);
                    panel1.Controls.Add(xFloat);
                }
                else if(temp.Value is string)
                {
                    string stringItem = (string)temp.Value;

                    TextBox stringBox = new TextBox { Text = stringItem, Width = 260 };
                    stringBox.Location = new Point(tempLocation.X + 20 + tempLabel.Width, tempLocation.Y);

                    panel1.Controls.Add(tempLabel);
                    panel1.Controls.Add(stringBox);
                }
                else if (temp.Value is bool)
                {
                    bool boolItem = (bool)temp.Value;

                    CheckBox boolButton = new CheckBox { Checked = boolItem };

                    boolButton.Location = new Point(tempLocation.X + 20 + tempLabel.Width, tempLocation.Y);

                    panel1.Controls.Add(tempLabel);
                    panel1.Controls.Add(boolButton);
                }
                else if (temp.Value is float)
                {
                    float vectorItem = (float)temp.Value;

                    TextBox xFloat = new TextBox { Text = vectorItem.ToString(), Width = 50 };
                    xFloat.Location = new Point(tempLocation.X + 20 + tempLabel.Width, tempLocation.Y);

                    panel1.Controls.Add(tempLabel);
                    panel1.Controls.Add(xFloat);
                }
                else if (temp.Value is Int64)
                {
                    Int64 vectorItem = (Int64)temp.Value;

                    byte[] tempByte = new byte[sizeof(Int64)];
                    tempByte = BitConverter.GetBytes(vectorItem);
                    float tempFloat = BitConverter.ToSingle(tempByte, sizeof(Int64)-4);

                    if (vectorItem == -4647714815446089728)
                        vectorItem = -1;
                    TextBox xFloat = new TextBox { Text = tempFloat.ToString(), Width = 50 };
                    xFloat.Location = new Point(tempLocation.X + 20 + tempLabel.Width, tempLocation.Y);

                    panel1.Controls.Add(tempLabel);
                    panel1.Controls.Add(xFloat);
                }
            }
        }
        void ColorDialogOpen(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() != DialogResult.Cancel)
            {
                
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            richTextBox1.Text = "";
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

                    bufferFile = File.ReadAllBytes(myOpenFileDialog.FileName);

                    int tempFileVersion = BitConverter.ToInt32(bufferFile, 4);
                    byte[] temp = new byte[4];
                    //Convert bytes into int length
                    int tempLength = BitConverter.ToInt32(temp, 0);

                    header = RawDataToObject<datreader.DATHeader>(bufferFile);

                    //Is this Shogo?
                    if (header.nVersion == 56)
                        fs.Position = 12;
                    else if (header.nVersion == 50)
                        fs.Position = 16;
                    //AVP2, NOLF, KISS
                    else
                        fs.Position = 44;

                    richTextBox1.Text += "World Name: " + myOpenFileDialog.FileName;
                    richTextBox1.Text += "\n World Version: " + header.nVersion.ToString();
                    richTextBox1.Text += "\n World Object Data Position: " + header.ObjectDataPos.ToString();
                    richTextBox1.Text += "\n World Render Data Position: " + header.RenderDataPos.ToString();
                    richTextBox1.SelectionStart = richTextBox1.Text.Length;
                    richTextBox1.ScrollToCaret();

                    statusFileInfo.Text = "File Info | FileSize: " + fs.Length.ToString() + " | Map Version: " + header.nVersion +  " | Object Data Offset: " + header.ObjectDataPos.ToString();

                    //DIRECT ENGINE 1.0
                    if (header.nVersion != 50)
                    {
                        //read World Info length into temp
                        fs.Read(temp, 0, 4);

                        tempLength = BitConverter.ToInt32(temp, 0);

                        //Resize the array to get a string
                        Array.Resize(ref temp, 1024);
                        fs.Read(temp, 0, tempLength);
                        //Convert the string
                        string tempString = System.Text.Encoding.ASCII.GetString(temp);
                        //Show the string
                        richTextBox1.Text += "\n World Info String: " + tempString;
                        richTextBox1.SelectionStart = richTextBox1.Text.Length;
                        richTextBox1.ScrollToCaret();
                    }

                    //Read World Extents
                    worldExtents = datreader.ReadWorldExtents(fs, (int)fs.Position);
                    worldNodeTree = datreader.ReadWorldNodeTree(fs);

                    //Read objects
                    fs.Position = header.ObjectDataPos;

                    Array.Resize(ref temp, 4);
                    fs.Read(temp, 0, 4);

                    tempLength = BitConverter.ToInt32(temp, 0);

                    fs.Read(temp, 0, 4);

                    var tempDataLength = BitConverter.ToInt16(temp, 0);
                    var tempStringLength = BitConverter.ToInt16(temp, 2);

                    richTextBox1.Text += "\n\n\n Object Count: " + tempLength.ToString();
                    richTextBox1.SelectionStart = richTextBox1.Text.Length;
                    richTextBox1.ScrollToCaret();


                    List<object> arguments = new List<object>();
                    arguments.Add(fs);
                    arguments.Add(tempLength);


                    worldObjectList = datreader.ReadObjects(fs, tempLength, (header.ObjectDataPos + 4));


                    Thread childThread = new Thread(MakeList);
                    childThread.IsBackground = true;
                    childThread.Start();

                    statusFileName.Text = GetFileName(myOpenFileDialog.FileName);

                }
            }
        }
    }
}
