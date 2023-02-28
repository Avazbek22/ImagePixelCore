using System.Diagnostics;
using System.Drawing;

namespace ImagePixelCore
{

    public partial class Form1 : Form
    {
        private List<Bitmap> _bitmaps = new List<Bitmap>();
        private Random rnd = new Random(DateTime.Now.Millisecond);
        public Form1()
        {
            InitializeComponent();
            trackBar1.Enabled = false;
        }
        private void UpdateForm()
        {
            _bitmaps.Clear();
            pictureBox1.Image = null;
            trackBar1.Value = 0;
            trackBar1.Enabled = false;
            Text = "0 %";
        }

        private async void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                menuStrip1.Enabled = false;
                UpdateForm();
                await Task.Run(() => { RunProcessing(new Bitmap(openFileDialog1.FileName)); });
                (menuStrip1.Enabled, trackBar1.Enabled) = (true, true);
                sw.Stop();
                Text = $"Прошедшее время: {sw.Elapsed.ToString().Remove(8)}";
            }
        }

        private void RunProcessing(Bitmap bitmap)
        {
            List<Pixel> pixels = GetPixels(bitmap);

            int pixelsInStep = (bitmap.Height * bitmap.Width) / 100;

            List<Pixel> currentPixelsSet = new List<Pixel>(pixels.Count - pixelsInStep);
            Bitmap currentBitmap;

            for (int i = 1, index; i < trackBar1.Maximum; i++)
            {
                for (int j = 0; j < pixelsInStep; j++)
                {
                    index = rnd.Next(pixels.Count);
                    currentPixelsSet.Add(pixels[index]);
                    pixels.RemoveAt(index);
                }
                currentBitmap = new Bitmap(bitmap.Width, bitmap.Height);

                foreach (Pixel pixel in currentPixelsSet)
                    currentBitmap.SetPixel(pixel.Point.X, pixel.Point.Y, pixel.Color);

                _bitmaps.Add(currentBitmap);

                this.Invoke(new Action(() => { Text = $"{i} %"; }));
                this.Invoke(new Action(() => { pictureBox1.Image = _bitmaps[trackBar1.Value++]; }));

            }
            _bitmaps.Add(bitmap);
            this.Invoke(new Action(() => { pictureBox1.Image = _bitmaps[trackBar1.Value++]; }));
        }

        private List<Pixel> GetPixels(Bitmap bitmap)
        {
            List<Pixel> pixels = new List<Pixel>(bitmap.Width * bitmap.Height);

            for (int y = 0; y < bitmap.Height; y++)
                for (int x = 0; x < bitmap.Width; x++)
                    pixels.Add(new Pixel(new Point(x, y), bitmap.GetPixel(x, y)));

            return pixels;
        }

        private void trackBar1_Scroll(object sender, EventArgs e) =>
                pictureBox1.Image = (trackBar1.Value == 0) ? null : _bitmaps[trackBar1.Value - 1];

        private void deleteImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Delete this image?", "Program", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                UpdateForm();
            }
        }
    }
}