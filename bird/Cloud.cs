using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace bird
{
    class Cloud
    {
        float X, Y;
        int type;
        public int isFront;
        const float SPD = -3;
        static Random RAND = new Random();
        static List<Bitmap> sourceCloud = new List<Bitmap> { new Bitmap(Application.StartupPath + "/backCloud.png"), new Bitmap(Application.StartupPath + "/frontCloud.png") };

        //contant infomation
        public static List<int> Width = new List<int> { 150, 300 };
        public static List<int> Height = new List<int> { 100, 200 };

        static int ClientW, ClientH;

        public Cloud(int Y, int CW, int CH, int isNotBack)
        {

            this.X = CW;
            this.Y = (float)Y;
            ClientW = CW;
            ClientH = CH;
            this.isFront = isNotBack;

            //this.SPD = (RAND.Next() % 10) + 1;
            this.type = RAND.Next() % 7;
        }

        public void flow()
        {
            X += isFront == 0 ? SPD : (SPD * 1.2f);
        }

        public void render(Graphics g)
        {
            g.DrawImage(sourceCloud[isFront], X, Y, new Rectangle(type * Width[isFront], 0, Width[isFront], Height[isFront]), GraphicsUnit.Pixel);
        }

        public void bringPos(float X, float Y)
        {
            this.X += X;
            this.Y += Y;
        }

        public bool outOfScreen()
        {
            const int delta = 200;
            return (X > ClientW || X + Width[isFront] < -delta);
        }
    }
}
