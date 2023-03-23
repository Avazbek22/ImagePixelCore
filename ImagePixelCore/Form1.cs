using System.Diagnostics;

namespace ImagePixelCore
{

    public partial class Form1 : Form
    {
        private List<Bitmap> _bitmaps = new List<Bitmap>(100);
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
            GC.Collect();
        }

        private async void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                menuStrip1.Enabled = false;
                UpdateForm();
                int steps = trackBar1.Maximum;
                await Task.Run(() => { RunProcessing(new Bitmap(openFileDialog1.FileName), steps); });
                (menuStrip1.Enabled, trackBar1.Enabled) = (true, true);
                sw.Stop();
                Text = $"Прошедшее время: {sw.Elapsed.ToString().Remove(10)}";
            }
        }

        private void RunProcessing(Bitmap bitmap, int steps)
        {
            
            int cnt = bitmap.Height * bitmap.Width, pixelsInStep = cnt / steps;

            int[] indexes = Enumerable.Range(0, cnt).ToArray();

            Bitmap currentBitmap = new Bitmap(bitmap.Width, bitmap.Height);
            
            Parallel.Invoke(
                () =>
                {
                    for (int i = 0, idx; i < cnt - 1; i++)
                    {
                        idx = rnd.Next(i + 1, cnt - 1);
                        (indexes[idx], indexes[i]) = (indexes[i], indexes[idx]);
                    }
                },
                () =>
                {
                    for (int i = 1, coor, j, bw = bitmap.Width; i < steps; i++)
                    {
                        for (j = 0, coor = indexes[i * pixelsInStep + j]; j < pixelsInStep; j++) {
                            coor = indexes[i * pixelsInStep + j];
                            currentBitmap.SetPixel(coor % bw, coor / bw, bitmap.GetPixel(coor % bw, coor / bw));
                        }

                        _bitmaps.Add((Bitmap)currentBitmap.Clone());

                        this.Invoke(new Action(() => { Text = $"{i} %"; }));
                        this.Invoke(new Action(() => { pictureBox1.Image = _bitmaps[trackBar1.Value++]; }));

                    }

                    _bitmaps.Add(bitmap);
                    this.Invoke(new Action(() => { pictureBox1.Image = _bitmaps[trackBar1.Value++]; }));
                });
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