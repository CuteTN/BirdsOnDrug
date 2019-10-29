using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

namespace bird
{
    static class Program
    {
        /*
        static void screenshotEffect()
        {
            Point pointToScreen = Control.PointToScreen(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height)
            Rectangle bounds = new Rectangle(0,0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            //MessageBox.Show(bounds.Width.ToString(), "Hanh dang iu", new MessageBoxButtons(), new MessageBoxIcon());
            using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(new Point(0,0), new Point(0,0), bounds.Size);
                }
                bitmap.Save(Application.StartupPath + "/background.png", ImageFormat.Png);
            }
        }*/

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new Form1());
        }
    }
}
