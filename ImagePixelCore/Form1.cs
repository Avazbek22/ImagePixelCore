using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Imaging;
using AForge.Imaging.Filters;
namespace ImagePixelCore
{

    public partial class Form1 : Form
    {
        private List<Bitmap> _bitmaps = new List<Bitmap>(100);
        private Random _rnd = new Random(DateTime.Now.Millisecond);
        private Bitmap? _currentImage = null;
        private Bitmap? _originalImage = null;
        private string _imagePath;
        private ImageAction _imageAction;

        private enum ImageAction
        {
            Opened,
            Deleted,
            PixelsDeleted,
            ImageBlured
        }
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
                _imagePath = openFileDialog1.FileName;
                _currentImage = new Bitmap(_imagePath);
                _originalImage = _currentImage;
                await Task.Run(() => { pictureBox1.Image = _currentImage; });
                _imageAction = ImageAction.Opened;
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
                _imageAction = ImageAction.Deleted;
            }
        }


        private async void deletePixelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(_imageAction != ImageAction.PixelsDeleted)
                splitContainer1.Panel2Collapsed ^= true;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            menuStrip1.Enabled = false;
            UpdateForm();
            await Task.Run(() => { DeletePixels(_currentImage!, trackBar1.Maximum); });
            (menuStrip1.Enabled, trackBar1.Enabled) = (true, true);
            sw.Stop();
            Text = $"Time passed: {sw.Elapsed.ToString().Remove(10)}";

            void DeletePixels(Bitmap bitmap, int steps)
            {
                int totalPixelsCount = bitmap.Height * bitmap.Width, pixelsInStep = totalPixelsCount / steps;

                int[] Shuffle(int[] array)
                {
                    int length = array.Length - 1;
                    for (int i = 0, rand; i < length; i++)
                    {
                        rand = _rnd.Next(i, length);

                        (array[rand], array[i]) = (array[i], array[rand]); // Swap
                    }

                    return array;
                }

                int[] indexes = Shuffle(Enumerable.Range(0, totalPixelsCount + 1).ToArray());

                Bitmap currentBitmap = new Bitmap(bitmap.Width, bitmap.Height);

                for (int i = 1, coor, j, bw = bitmap.Width; i < steps; i++)
                {
                    for (j = 0; j < pixelsInStep; j++)
                    {
                        coor = indexes[i * pixelsInStep + j];
                        currentBitmap.SetPixel(x: (coor % bw), y: (coor / bw), bitmap.GetPixel(coor % bw, coor / bw));
                    }

                    _bitmaps.Add((Bitmap)currentBitmap.Clone());

					this.BeginInvoke(new Action(() => {
						Text = $"{i} %";
						pictureBox1.Image = _bitmaps[trackBar1.Value++];
					}));
				}

                _bitmaps.Add(bitmap);
                this.BeginInvoke(new Action(() => { pictureBox1.Image = _bitmaps[trackBar1.Value++]; }));
                _imageAction = ImageAction.PixelsDeleted;
            }
        }

		private async void blurToolStripMenuItem_Click(object sender, EventArgs e)
		{
			splitContainer1.Panel2Collapsed = true;
			int value = trackBar1.Value - 1;
            Bitmap currentBitmap = (Bitmap)_bitmaps[value].Clone() ??  _currentImage!; 

			Stopwatch sw = new Stopwatch();
			sw.Start();

			// Запускаем блюр в фоновом потоке
			Task<Bitmap> blurTask = Task.Run(() =>
			{
				return new GaussianBlur(2000, 2000).Apply(currentBitmap);
			});

			// Обновляем UI в основном потоке, не блокируя его
			while (!blurTask.IsCompleted)
			{
				Text = $"Processing: {sw.Elapsed.ToString().Remove(10)}";
				await Task.Delay(1);
			}

			sw.Stop();

			// Получаем результат блюра и обновляем UI
			_currentImage = await blurTask;

			// Обновление UI в основном потоке
			Text = $"Processing complete: {sw.Elapsed.ToString().Remove(10)}";
			pictureBox1.Image = _currentImage;
            _imageAction = ImageAction.ImageBlured;
		}

		private void getTextToolStripMenuItem_Click(object sender, EventArgs e)
        {

           

        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null && saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
        }
    }
}