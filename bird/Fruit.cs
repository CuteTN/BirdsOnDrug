using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace bird
{
    class Fruit
    {
        const float GRAVITATIONAL_ACCELERATION = 1;
        PointF pos;
        PointF veloc;
        PointF accel;
        float mass; // it is used for bonus feature

        static Bitmap img;

        public Fruit(PointF)
        {
            pos = new PointF(posX, posY);
            veloc = new PointF(0, 0);
            accel = new PointF(0, 0);
            mass = 5;
            initStaticSprites();
        }

        private void initStaticSprites()
        {
            if (img != null)
                return;

            img = new Bitmap(Application.StartupPath + "/fruit.png");
        }

        private void getNewAccel()
        {
            accel = new PointF(0,GRAVITATIONAL_ACCELERATION);
        }

        public void updatePerFrame()
        {
            getNewAccel();
            veloc = new PointF(veloc.X + accel.X, veloc.Y + accel.Y);
            pos = new PointF(pos.X + veloc.X, pos.Y + veloc.Y);
        }

        public bool isDisappeared(float formBottom)
        {
            const float OFFSET = 100;
            return (pos.X > formBottom + OFFSET);
        }

        public void render(Graphics g)
        {
            const float IMG_SIZE = 50;
            g.DrawImage(img, pos.X, pos.Y, IMG_SIZE, IMG_SIZE);
        }

        
    }
}
