using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace bird
{
    class Bird
    {
        private int curRenderFrame = 0;
        private PointF curPos;
        private PointF veloc;
        private PointF accel;
        private PointF colAccel;
        
        private float force { get; }
        private float friction { get; }
        private float radius { get; }
        private float colForce { get; set; }
        private float mass { get; set; }

        private float rT, gT, bT;

        private int dirX = 1;
        private int dirY = 1;
        
        public bool dead { get; set; }
        private Bitmap soulSprite;
        public PointF soulPos;
        private PointF soulVeloc;
        private PointF soulAccel;
        private int soulRenderFrame = 0;

        private float maxHealth = 1000;
        private float health;
        private float defence;
        public const float WIN_REWARD = 1000;
        static public bool combat = false;


        // how the birds will turn around in free flying mode, value in [0..1]
        static public bool freeFlying = false;
        public float aimlessness;
        public float freForce;
        const float FREEXMAX = 1600;
        const float FREEXMIN = -100;

        const float GRAVITATIONAL_ACCELERATION = 2;
        public static Bitmap spriteStatic; // flyweight cheat  =)))
        public static Bitmap soulSpriteStatic;
        static private Random rd = new Random(); 
        public Bitmap sprite;
        private int spriteDirX = 1;
        private int spriteDirY = 1;
        static public bool reColoring = true;
        static public int numberOfBirds = 0;

        public Bird(float posX, float posY, float force_, float friction_, float radius_, float colForce_, float mass_, float rT_, float gT_, float bT_, float aimlessness_, float freForce_, float maxHealth_, float defence_)
        {
            numberOfBirds++;

            curPos = new PointF(posX, posY);
            veloc = new PointF(0, 0);

            force = force_;
            friction = friction_;
            radius = radius_;
            colForce = colForce_;
            mass = mass_;

            rT = rT_;
            gT = gT_;
            bT = bT_;

            aimlessness = aimlessness_;
            freForce = freForce_;

            health = maxHealth = maxHealth_;
            defence = defence_;

            dirX = accel.X < 0 ? -1 : 1;
            dirY = 1;
            dead = false;

            initStaticSprites();
            initSprite();
        }

        private void initStaticSprites()
        {
            if (spriteStatic != null)
                return;

            spriteStatic = new Bitmap(Application.StartupPath + "/bird_sprite.png");

            soulSpriteStatic = (Bitmap)spriteStatic.Clone();
            for (int i = 0; i < soulSpriteStatic.Width; i++)
                for (int j = 0; j < soulSpriteStatic.Height; j++)
                {
                    Color color = soulSpriteStatic.GetPixel(i, j);
                    int RR = color.R;
                    int GG = color.G;
                    int BB = color.B;
                    int AA = Math.Min((int)color.A, 120);
                    color = Color.FromArgb(AA, RR, GG, BB);
                    soulSpriteStatic.SetPixel(i, j, color);
                }
        }

        private void reColorSprite()
        {
            for(int i=0; i<sprite.Width; i++)
                for(int j=0; j<sprite.Height; j++)
                {
                    Color color = sprite.GetPixel(i, j);
                    int AA = color.A;
                    int RR = (int)(color.R + (255 - color.R) * rT);
                    int GG = (int)(color.G + (255 - color.G) * gT);
                    int BB = (int)(color.B + (255 - color.B) * bT);
                    color = Color.FromArgb(AA, RR, GG, BB);
                    sprite.SetPixel(i, j, color);

                    AA = Math.Min(AA, 120);
                    color = Color.FromArgb(AA, RR, GG, BB);
                    soulSprite.SetPixel(i, j, color);
                }
        }

        private void initSprite()
        {
            sprite = (Bitmap)spriteStatic.Clone();
            soulSprite = (Bitmap)soulSpriteStatic.Clone();
            if(reColoring)
                reColorSprite();
        }


        static private double vectorLength(PointF X)
        {
            return Math.Sqrt(X.X * X.X + X.Y * X.Y);
        }

        PointF getNewAccel(PointF destPos)
        {
            if (dead)
            {
                PointF res = new PointF(colAccel.X, colAccel.Y + GRAVITATIONAL_ACCELERATION);
                colAccel = new PointF(0, 0);
                return res;
            }

            if (freeFlying)
            {
                float sgn = accel.X < 0 ? -1 : 1;
                if (curPos.X > FREEXMAX)
                    sgn = -1;
                if (curPos.X < FREEXMIN)
                    sgn = 1;

                PointF res = new PointF((float)rd.NextDouble(), (float)rd.NextDouble());
                double lf = vectorLength(res);

                res.X = (float)(res.X * freForce / (lf * mass));
                res.Y = (float)(res.Y * freForce / (lf * mass));

                res.X *= sgn * (rd.NextDouble() < aimlessness ? -1 : 1);
                res.Y *= rd.Next(2) * 2 - 1;

                res.X += colAccel.X;
                res.Y += colAccel.Y;
                colAccel = new PointF(0, 0);

                return res;
            }

            PointF delta = new PointF(destPos.X - curPos.X, destPos.Y - curPos.Y);

            double l = vectorLength(delta);

            if (l == 5)
            {
                return new Point(0, 0);
            }
            // F = ma
            delta.X = (float)(delta.X * force / (l * mass));
            delta.Y = (float)(delta.Y * force / (l * mass));

            delta.X += colAccel.X;
            delta.Y += colAccel.Y;
            colAccel = new PointF(0, 0);

            return delta;
        }

        public void moveInFrame(PointF destPos)
        {
            // dirty code
            if (health < 0)
                die();

            accel = getNewAccel(destPos);

            // the bird should stop
            float la = (float)vectorLength(accel);
            float lv = (float)vectorLength(veloc);
            if (la == 0 && lv <= 5)
            {
                veloc = new PointF(0, 0);
                accel = new PointF(0, 0);
            }

            veloc.X += accel.X;
            veloc.Y += accel.Y;

            veloc.X *= (1 - friction);
            veloc.Y *= (1 - friction);

            curPos.X += veloc.X;
            curPos.Y += veloc.Y;

            if(dead)
                soulMoveInFrame();

            if(!dead)
                curRenderFrame = (curRenderFrame + 1) % 6;
        }

        private void soulMoveInFrame()
        {
            soulVeloc.X += soulAccel.X;
            soulVeloc.Y += soulAccel.Y;

            soulPos.X += soulVeloc.X;
            soulPos.Y += soulVeloc.Y;

            soulRenderFrame = (soulRenderFrame + 1) % 6;
        }

        public void bringPos(float X, float Y)
        {
            curPos.X += X;
            curPos.Y += Y;

            soulPos.X += X;
            soulPos.Y += Y;
        }

        public void render(Graphics g)
        {
            Rectangle destRect = new Rectangle(sprite.Width * curRenderFrame / 6, 0, sprite.Width / 6, sprite.Height);

            if (!dead)
            {
                dirX = accel.X < 0 ? -1 : 1;
                dirY = 1;
            }
            else
                dirY = -1;
            
            if (dirX != spriteDirX)
            {
                sprite.RotateFlip(RotateFlipType.RotateNoneFlipX);
                spriteDirX *= -1;
            }
            if (dirY != spriteDirY)
            {
                sprite.RotateFlip(RotateFlipType.RotateNoneFlipY);
                spriteDirY *= -1;
            }

            PointF curPosRender = new PointF(curPos.X - 80, curPos.Y - 80);
            g.DrawImage(sprite, curPosRender.X, curPosRender.Y, destRect, GraphicsUnit.Pixel);

            if (dead)
                renderSoul(g);

            displayHealth(g);
            /*Font drawFont = new Font("Arial", 50, FontStyle.Bold);
            SolidBrush drawBrush = new SolidBrush(Color.HotPink);
            g.DrawString(health.ToString(), drawFont, drawBrush, curPos);*/
        }

        private void renderSoul(Graphics g)
        {
            Rectangle destRect = new Rectangle(sprite.Width * soulRenderFrame / 6, 0, sprite.Width / 6, sprite.Height);
            PointF soulPosRender = new PointF(soulPos.X - 80, soulPos.Y - 80);
            g.DrawImage(soulSprite, soulPosRender.X, soulPosRender.Y, destRect, GraphicsUnit.Pixel);
        }

        private void displayHealth(Graphics g)
        {
            if (dead)
                return;
            if (!combat)
                return;

            float barLength = 80;
            float barHeight = 8;

            Pen pen = new Pen(health>1000? Color.LightGreen:Color.OrangeRed, 2);
            SolidBrush brush = new SolidBrush(health > 1000 ? Color.LightGreen : Color.OrangeRed);

            float HPLength = barLength * health / maxHealth;
            Rectangle rect = new Rectangle((int)curPos.X - 40, (int)curPos.Y + 50, (int)HPLength, (int)barHeight);
            g.FillRectangle(brush, rect);
            rect = new Rectangle((int)curPos.X - 40, (int)curPos.Y + 50, (int)barLength, (int)barHeight);
            g.DrawRectangle(pen, rect);
        }

        public void shoot(float posX, float posY)
        {
            PointF dis = new PointF(curPos.X - posX, curPos.Y - posY);

            double l = vectorLength(dis);
            if (l <= radius)
                die();
        }

        public bool disappeared(float top, float bottom)
        {
            return (dead) && (soulPos.Y < top) && (curPos.Y > bottom);
        }

        public void die()
        {
            if (dead)
                return;

            numberOfBirds--;

            dead = true;
            colForce = 0;

            soulPos = curPos;
            soulVeloc = veloc;
            soulAccel = new PointF(0, -0.5f);

            if (soulVeloc.X < 0)
                soulSprite.RotateFlip(RotateFlipType.RotateNoneFlipX);
        }

        public void regainHealth(float maxHealthRegain, PointF healthPos)
        {
            if(!combat)
            {
                health = maxHealth;
                return;
            }

            float l = (float)vectorLength(new PointF(healthPos.X - curPos.X, healthPos.Y - curPos.Y)) / 500;

            health += Math.Max( maxHealthRegain - l , - maxHealthRegain);
            health = Math.Min(health, maxHealth);
        }
        
        /// //////////////////////////////////////////////////////////////////////
        static public void collision(Bird bird1, Bird bird2)
        {
            PointF dis = new PointF(bird1.curPos.X - bird2.curPos.X, bird1.curPos.Y - bird2.curPos.Y);

            if (vectorLength(dis) <= bird1.radius + bird2.radius)
            {
                double l = vectorLength(dis);

                PointF temp = dis;
                float colforce = bird1.colForce + bird2.colForce;

                temp.X = (float)(dis.X * colforce / (l * bird1.mass));
                temp.Y = (float)(dis.Y * colforce / (l * bird1.mass));
                bird1.colAccel.X += temp.X;
                bird1.colAccel.Y += temp.Y;

                temp.X = (float)(dis.X * colforce / (l * bird2.mass));
                temp.Y = (float)(dis.Y * colforce / (l * bird2.mass));
                bird2.colAccel.X -= temp.X;
                bird2.colAccel.Y -= temp.Y;

                if (combat)
                {
                    damage(bird1, bird2);
                }
            }
        }

        static void damage(Bird bird1, Bird bird2)
        {
            if (bird1.dead || bird2.dead)
                return;

            bird1.health = bird1.health - bird2.colForce + bird1.defence;
            bird2.health = bird2.health - bird1.colForce + bird2.defence;

            if(bird1.health < 0)
            {
                bird1.die();
                if (bird2.health > 0)
                    bird2.health = Math.Min(bird2.health + WIN_REWARD, bird2.maxHealth);
            }

            if(bird2.health < 0)
            {
                bird2.die();
                if (bird1.health > 0)
                    bird1.health = Math.Min(bird1.health + WIN_REWARD, bird1.maxHealth);
            }
        }

    }
}
