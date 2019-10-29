using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace bird
{
    public partial class Form1 : Form
    {
        const float MAX_HEALTH_REGAIN = 1;

        int frameRate = 20;
        Timer timer = new Timer();
        int numberOfBirds = 0;
        PointF destPos = new Point(0, 0);
        Random rd = new Random();
        const int DESTMOVESPEED = 5;

        List<Bird> birds = new List<Bird>();
        bool collision = true;

        bool paused = false;
        bool freeFlying = false;
        bool combat = true;

        Bitmap backgroundBitmap;
        PointF backgroundRenderPos = new Point(0, 0);

        private float randomInRange(float l, float r)
        {
            return (float)rd.NextDouble() * (r - l) + l;
        }

        private float mapPoint(float l1, float r1, float l2, float r2, float x)
        {
            float d = (x - l1) / (r1 - l1);
            return l2 + d * (r2 - l2);
        }

        private void createNewBird()
        {
            //Environment.CurrentDirectory
            numberOfBirds++;
            float posX = randomInRange(1, 1000);
            float posY = randomInRange(1, 1000);
            float force = randomInRange(0, 30);
            float friction = randomInRange(0.01f, 0.09f);
            float radius = 65f;
            float colforce = randomInRange(10, 40);
            float mass = randomInRange(1f, 5f);

            //float rT = randomInRange(0, 0.8f);
            //float gT = randomInRange(0, 0.8f);
            //float bT = randomInRange(0, 0.8f);
            float rT = mapPoint(10, 40, 0, 1f, colforce);
            float gT = mapPoint(0, 30, 0, 1f, force);
            float bT = mapPoint(1f, 5f, 0, 1f, mass);

            float aimlessness = randomInRange(0.0f, 0.1f);
            float freForce = randomInRange(1, force);

            float health = 5000;
            float defence = randomInRange(1, 10);

            birds.Add(new Bird(posX, posY, force, friction, radius, colforce, mass, rT, gT, bT, aimlessness, freForce, health, defence));
        }

        private void clearDeadBirds()
        {
            for (int i = 0; i < numberOfBirds; i++)
            {
                if (birds[i].disappeared(this.Top - 100, this.Bottom + 100))
                {
                    birds.Remove(birds[i]);
                    numberOfBirds--;
                }
            }
        }

        private void moveEverything(float X, float Y)
        {
            foreach(Bird bird_ in birds)
            {
                bird_.bringPos(X, Y);
            }

            destPos.X += X;
            destPos.Y += Y;
        }

        public Form1()
        {
            //InitializeComponent();
            DoubleBuffered = true;
            run();
        }

        private void run()
        {
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            this.Paint += Form1_Paint;
            timer.Tick += Timer_Tick;
            this.KeyDown += Form1_KeyDown;
            timer.Interval = frameRate = 20;
            timer.Start();
            this.MouseClick += Form1_Click;
            this.MouseWheel += Form1_MouseWheel;

            destPos = new PointF(0, 0);
            combat = Bird.combat;
            freeFlying = Bird.freeFlying;
            backgroundRenderPos = new Point(0, 0);

            screenshotEffect();
            this.Refresh();
        }

        void screenshotEffect()
        { 
            Rectangle bounds = new Rectangle(0, 0, 2000, 2000);
            //MessageBox.Show(bounds.Width.ToString(), "Hanh dang iu", new MessageBoxButtons(), new MessageBoxIcon());
            using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(new Point(0, 0), new Point(0, 0), bounds.Size);
                }

                int resolutionX = 0, resolutionY = 0;
                for(int x = 0; x < bitmap.Width; x++)
                    for(int y = 0; y < bitmap.Height; y++)
                    {
                        Color color = bitmap.GetPixel(x, y);
                        if(color.A != 0)
                        {
                            resolutionX = Math.Max(resolutionX, x);
                            resolutionY = Math.Max(resolutionY, y);
                        }
                    }

                Rectangle rect = new Rectangle(0, 0, resolutionX + 1, resolutionY + 1);
                Bitmap croppedBitmap = bitmap.Clone(rect, bitmap.PixelFormat);
                croppedBitmap.Save(Application.StartupPath + "/background.png", ImageFormat.Png);
            }

            backgroundBitmap = new Bitmap(Application.StartupPath + "/Background.png");
        }

        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            moveEverything(0, e.Delta / 5);
        }

        private void Form1_Click(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                destPos = new Point(e.X, e.Y);
            }
            if(e.Button == MouseButtons.Right)
            {
                foreach(Bird bird_ in birds)
                {
                    bird_.shoot(e.X, e.Y);
                }
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!paused)
            {
                foreach (Bird bird_ in birds)
                {
                    bird_.regainHealth(MAX_HEALTH_REGAIN, destPos);
                }

                if (collision)
                {
                    for (int i = 0; i < numberOfBirds - 1; i++)
                        for (int j = i + 1; j < numberOfBirds; j++)
                        {
                            Bird.collision(birds[i], birds[j]);
                        }
                }

                foreach (Bird bird_ in birds)
                {
                    bird_.moveInFrame(destPos);
                }

                clearDeadBirds();
            }
            //this.Invalidate();
            this.Refresh();
            //MessageBox.Show(cnt.ToString());
        }

        private void showInstruction(Graphics g)
        {
            SolidBrush brush = new SolidBrush(Color.FromArgb(150, 0, 0, 0));
            Rectangle rect = new Rectangle(100, 50, this.Width-200, this.Height-80);
            g.FillRectangle(brush, rect);

            Font drawFont = new Font("Arial", 50, FontStyle.Bold);
            SolidBrush drawBrush = new SolidBrush(Color.HotPink);
            g.DrawString("~~~ BIRDS INSTRUCTION ~~~", drawFont, drawBrush, new PointF(250,50));

            drawFont = new Font("Consolas", 20, FontStyle.Bold);
            drawBrush = new SolidBrush(Color.Yellow);
            string[] instructionList =
            {
                "<> LEFTCLICK: . . . . . . . . . . change destination point",
                "<> RIGHTCLICK:. . . . . . . . . . shoot!",
                "<> MOUSE SCROLL:. . . . . . . . . move camera up or down",
                "<> ARROW KEYS:. . . . . . . . . . move destination point",
                "<> SPACE BAR: . . . . . . . . . . create a new bird",
                "<> BACKSPACE: . . . . . . . . . . kill all birds",
                "<> C: . . . . . . . . . . . . . . turn on/off collision",
                "<> F: . . . . . . . . . . . . . . turn on/off free flying mode",
                "<> H: . . . . . . . . . . . . . . turn on/off combat mode",
                "<> P: . . . . . . . . . . . . . . pause",
                "<> L: . . . . . . . . . . . . . . turn on/off default color (improve performance)",
                "<> +/-: . . . . . . . . . . . . . speed up/down",
                "<> ESCAPE:. . . . . . . . . . . . exit",
            };
            float startingY = 150;
            float endingY = this.Height - 150;
            float lineSpacing = (endingY - startingY)/instructionList.Length;
            
            for(int i = 0; i<instructionList.Length; i++)
            {
                g.DrawString(instructionList[i], drawFont, drawBrush, new PointF(180, startingY + i*lineSpacing));
            }

            g.DrawString("this instruction is shown whenever there's no bird left!", drawFont, drawBrush, new PointF(200, this.Height-50));
        }

        private void drawBackground(Graphics g)
        {
            float scaleX = this.Width;
            float scaleY = backgroundBitmap.Height * this.Width / backgroundBitmap.Width;
            g.DrawImage(backgroundBitmap, backgroundRenderPos.X, backgroundRenderPos.Y         , scaleX, scaleY);
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            drawBackground(g);

            Font drawFont = new Font("Arial", 50);
            SolidBrush drawBrush = new SolidBrush(Color.HotPink);
            PointF destPosRender = new PointF(destPos.X - 30, destPos.Y - 35);
            g.DrawString("+", drawFont, drawBrush, destPosRender);

            foreach (Bird bird_ in birds)
            {
                bird_.render(g);
            }

            if(numberOfBirds == 0)
            {
                showInstruction(g);
            }
            else
            {
                drawBrush = new SolidBrush(Color.Black);
                drawFont = new Font("Arial", 30);
                g.DrawString("population: " + Bird.numberOfBirds, drawFont, drawBrush, 0, 0);
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Space:
                    {
                        createNewBird();
                        break;
                    }

                case Keys.Escape:
                    {
                        this.Close();
                        break;
                    }

                case Keys.Up:
                    {
                        destPos.Y -= DESTMOVESPEED;
                        break;
                    }
                case Keys.Down:
                    {
                        destPos.Y += DESTMOVESPEED;
                        break;
                    }
                case Keys.Left:
                    {
                        destPos.X -= DESTMOVESPEED;
                        break;
                    }
                case Keys.Right:
                    {
                        destPos.X += DESTMOVESPEED;
                        break;
                    }
                case Keys.Back:
                    {
                        foreach (Bird bird_ in birds)
                            bird_.die();
                        break;
                    }
                case Keys.C:
                    {
                        collision = !collision;
                        break;
                    }
                case Keys.P:
                    {
                        paused = !paused;

                        // Player might want to do something else while pausing the animation
                        //if(paused)
                        //{
                        //    timer.Stop();
                        //}
                        //else
                        //{
                        //    timer.Start();
                        //}
                        break;
                    }

                case Keys.F:
                    {
                        freeFlying = !freeFlying;
                        Bird.freeFlying = freeFlying;

                        break;
                    }

                case Keys.H:
                    {
                        combat = !combat;
                        Bird.combat = combat;
                        break;
                    }

                case Keys.L:
                    {
                        Bird.reColoring = !Bird.reColoring;
                        break;
                    }

                case Keys.Add:
                    {
                        frameRate = frameRate > 1 ? --frameRate : 1;
                        timer.Interval = frameRate;
                        break;
                    }

                case Keys.Subtract:
                    {
                        frameRate++;
                        frameRate = frameRate < 100 ? ++frameRate : 100;
                        timer.Interval = frameRate;
                        break;
                    }

                default:
                    {
                        break;
                    }
            }
        }
    }
}