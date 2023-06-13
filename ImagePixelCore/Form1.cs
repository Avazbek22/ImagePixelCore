using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Imaging;
using AForge.Imaging.Filters;
using Tesseract;
namespace ImagePixelCore
{

    public partial class Form1 : Form
    {
        private List<Bitmap> _bitmaps = new List<Bitmap>(100);
        private Random rnd = new Random(DateTime.Now.Millisecond);
        private Bitmap? image = null;
        private string imagePath;
        public Form1()
        {
            InitializeComponent();
            splitContainer1.Panel2Collapsed ^= true;
        }
        private void UpdateForm()
        {
            _bitmaps.Clear();
            // image = null;
            pictureBox1.Image = null;
            trackBar1.Value = 0;
            trackBar1.Enabled = false;
            Text = "0 %";
            GC.Collect();
        }

        private async void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                imagePath = openFileDialog1.FileName;
                image = new Bitmap(imagePath);
                await Task.Run(() => { pictureBox1.Image = image; });
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e) =>
                pictureBox1.Image = (trackBar1.Value == 0) ? null : _bitmaps[trackBar1.Value - 1];

        private void deleteImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null &&
                MessageBox.Show("Delete this image?", "Program", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                UpdateForm();
            }
        }


        private async void deletePixelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            splitContainer1.Panel2Collapsed ^= true;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            menuStrip1.Enabled = false;
            UpdateForm();
            await Task.Run(() => { DeletePixels(image, trackBar1.Maximum); });
            (menuStrip1.Enabled, trackBar1.Enabled) = (true, true);
            sw.Stop();
            Text = $"Прошедшее время: {sw.Elapsed.ToString().Remove(10)}";

            void DeletePixels(Bitmap bitmap, int steps)
            {
                int totalPixelsCount = bitmap.Height * bitmap.Width, pixelsInStep = totalPixelsCount / steps;

                int[] Shuffle(int[] array)
                {
                    int length = array.Length - 1;
                    for (int i = 0, rand; i < length; i++)
                    {
                        rand = rnd.Next(i, length);

                        (array[rand], array[i]) = (array[i], array[rand]); // Swap
                    }

                    return array;
                }

                int[] indexes = Enumerable.Range(0, totalPixelsCount + 1).ToArray();

                Shuffle(indexes);

                Bitmap currentBitmap = new Bitmap(bitmap.Width, bitmap.Height);

                for (int i = 1, coor, j, bw = bitmap.Width; i < steps; i++)
                {
                    for (j = 0, coor = indexes[i * pixelsInStep + j]; j < pixelsInStep; j++)
                    {
                        coor = indexes[i * pixelsInStep + j];
                        currentBitmap.SetPixel(x: (coor % bw), y: (coor / bw), bitmap.GetPixel(coor % bw, coor / bw));
                    }

                    _bitmaps.Add((Bitmap)currentBitmap.Clone());

                    this.Invoke(new Action(() => { Text = $"{i} %"; }));
                    this.Invoke(new Action(() => { pictureBox1.Image = _bitmaps[trackBar1.Value++]; }));
                }

                _bitmaps.Add(bitmap);
                this.Invoke(new Action(() => { pictureBox1.Image = _bitmaps[trackBar1.Value++]; }));
            }
        }

        private async void blurToolStripMenuItem_Click(object sender, EventArgs e)
        {
            splitContainer1.Panel2Collapsed = true;
            image = (Bitmap)new GaussianBlur(2000, 2000).Apply(image);
            pictureBox1.Image = image;

            MessageBox.Show("Succesfull");
        }

        private void getTextToolStripMenuItem_Click(object sender, EventArgs e)
        {

            var ocr = new TesseractEngine("./tessdata", "eng", EngineMode.Default);
            var img = Pix.LoadFromFile(openFileDialog1.FileName);
            var page = ocr.Process(img);
            MessageBox.Show(page.GetText());

        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                pictureBox1.Image.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
        }
    }
}