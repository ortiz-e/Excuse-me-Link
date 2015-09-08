using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExcuseMe_Link
{
    public partial class Form1 : Form
    {
        public static Boolean fileOpen = false;
        public static String fileName = "";
        public static byte[] theROM;
        public static int selectedTile = -1;
        public static int tileToEdit = -1;
        public static List<int> currentMapTiles;
        public static int tileToDraw = -1;
        public static int currentMap = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private Bitmap getTile(int tileNumber)
        {
            Bitmap theSource = new Bitmap(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("ExcuseMe_Link.Resources.tiles.BMP"));
            Rectangle coordinates = new Rectangle(tileNumber * 16, 0, 16, 16);
            return theSource.Clone(coordinates, theSource.PixelFormat);
        }

        private Bitmap loadTileSet(int tileSetID)
        {
            Bitmap tileset = new Bitmap(16 * 15, 16);
            if (tileSetID == 0)
            {
                using (Graphics buildingTiles = Graphics.FromImage(tileset))
                {
                    int j = 0;
                    for (int i = 0; i < 16; i += 1)
                    {
                        buildingTiles.DrawImage(getTile(i), j * 16, 0, 16, 16);
                        j += 1;
                    }
                }
            }
            return tileset;
        }

        private Bitmap loadMap(int mapID)
        {
            Bitmap theMap = new Bitmap(64 * 16, 200 * 16);
            currentMapTiles = new List<int>();
            int x = 0;
            int y = 0;
            String startOffset = (mapID == 0 ? "506C" : mapID == 1 ? "665C" : mapID == 2 ? "9056" : mapID == 3 ? "A65C" : "506C");
            String endOffset = (mapID == 0 ? "538C" : mapID == 1 ? "6942" : mapID == 2 ? "936F" : mapID == 3 ? "A942" : "538C");
            using (Graphics buildingMap = Graphics.FromImage(theMap))
            {
                for (int i = Convert.ToInt32(startOffset, 16); i <= Convert.ToInt32(endOffset, 16); i += 1)
                {
                    int[] bits = new int[2];
                    if (Convert.ToInt32(theROM[i]) < 16)
                    {
                        bits[0] = 0;
                        bits[1] = Convert.ToInt32(theROM[i]);

                    }
                    else
                    {
                        bits[0] = Convert.ToInt32((theROM[i].ToString("X")).Substring(0, 1), 16);
                        bits[1] = Convert.ToInt32((theROM[i].ToString("X")).Substring(1, 1), 16);
                    }
                    Bitmap curTile = getTile(bits[1]);
                    for (int j = 0; j <= bits[0]; j += 1)
                    {
                        buildingMap.DrawImage(curTile, x, y, 16, 16);
                        currentMapTiles.Add(bits[1]);
                        x += 16;
                        if (x > 63 * 16)
                        {
                            x = 0;
                            y += 16;
                        }
                    }
                }
            }
            return theMap;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog loadFile = new OpenFileDialog();
            if (loadFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                fileOpen = true;
                fileName = loadFile.FileName;
                theROM = System.IO.File.ReadAllBytes(fileName);
                pictureBox1.Image = loadMap(0);
                mapList.Items.Insert(0, "West Hyrule");
                mapList.Items.Insert(1, "Death Mountain");
                mapList.Items.Insert(2, "East Hyrule");
                mapList.Items.Insert(3, "Maze Island");
                mapList.Enabled = true;
                tileSetBox.Width = 15 * 16;
                tileSetBox.Height = 16;
                tileSetBox.Image = loadTileSet(0);
            }
            
        }

        private void mapList_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentMap = mapList.SelectedIndex;
            pictureBox1.Image = loadMap(mapList.SelectedIndex);
        }


        private void tileSetBox_Paint(object sender, PaintEventArgs e)
        {
            if (selectedTile != -1)
            {
                Pen pen = Pens.Black;
                e.Graphics.DrawRectangle(pen, selectedTile * 16, 0, 16, 16);
            }
        }

        private void tileSetBox_MouseUp(object sender, MouseEventArgs e)
        {
            selectedTile = (int)Math.Floor(e.X / 16.0);
            tileSetBox.Invalidate();
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            tileToEdit = (int)(Math.Floor(e.X / 16.0) + (Math.Floor(e.Y / 16.0) * 64));
            if (tileToEdit != -1 && selectedTile != -1)
            {
                currentMapTiles[tileToEdit] = selectedTile;
                using (Graphics g = Graphics.FromImage(pictureBox1.Image))
                {
                    g.DrawImage(getTile(selectedTile), (tileToEdit % 64) * 16, (int)(Math.Floor(tileToEdit / 64.0) * 16), 16, 16);
                }
                tileToEdit = -1;
            }
            pictureBox1.Invalidate();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (fileOpen == false)
            {
                MessageBox.Show("No ROM open!");
                return;
            }
            int previous = -1;
            int count = -1;
            List<String> changes = new List<String>();

            String startOffset = (currentMap == 0 ? "506C" : currentMap == 1 ? "665C" : currentMap == 2 ? "9056" : currentMap == 3 ? "A65C" : "506C");
            String endOffset = (currentMap == 0 ? "538C" : currentMap == 1 ? "6942" : currentMap == 2 ? "936F" : currentMap == 3 ? "A942" : "538C");
            for (int j = 0; j < currentMapTiles.Count; j+= 1)
            {
                if (previous != currentMapTiles[j])
                {
                    if (count >= 0) changes.Add( (count.ToString("X") + previous.ToString("X")) );
                    previous = currentMapTiles[j];
                    count = 0;
                }
                else
                {
                    if (count == 15)
                    {
                        changes.Add("F" + currentMapTiles[j].ToString("X"));
                        count = 0;
                    }
                    else
                    {
                        count += 1;
                    }
                }
            }

            if (changes.Count > (Convert.ToInt32(endOffset, 16) - Convert.ToInt32(startOffset, 16)))
            {
                MessageBox.Show("Not enough space in the ROM to save this map!");
                return;
            }
            int k = 0;
            for (int i = Convert.ToInt32(startOffset, 16); i <= Convert.ToInt32(endOffset, 16); i += 1)
            {
                if (k == changes.Count) break;
                theROM[i] = Convert.ToByte(Convert.ToInt32(changes[k],16));
                k += 1;
            }
            System.IO.File.WriteAllBytes(fileName, theROM);
            MessageBox.Show("ROM saved successfully!");
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

    }
}
