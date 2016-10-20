using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using EloBuddy;

namespace KappaBaseUlt
{
    internal static class Recallbar
    {
        public static float X2;
        public static float Y2;

        public static readonly float X = Drawing.Width * 0.425f;
        public static readonly float Y = Drawing.Height * 0.80f;

        private static readonly int Width = (int)(Drawing.Width - 2 * X);
        private static readonly int Height = 6;

        private static readonly float Scale = (float)Width / 8000;

        public static void RecallBarDraw(this Program.EnemyInfo enemy)
        {
            Rect(X + X2, Y + Y2, Width, Height, 1, Color.White);
            var c = Color.White;
            if (enemy.CountDown() >= enemy.Enemy.traveltime())
            {
                c = Color.White;
                if (enemy.Enemy.Killable())
                {
                    c = Color.Red;
                    Drawing.DrawLine((X + 450) + X2 + Scale * enemy.Enemy.traveltime() - 1, Y + Y2 + 7, (X + 450) + X2 + Scale * enemy.Enemy.traveltime(), Y + Y2 - 11, 3, c);
                }
            }
            Drawing.DrawText((X + 450) + X2 + Scale * enemy.CountDown() - 1, Y + Y2 - 30, c, "(" + (int)enemy.Enemy.HealthPercent + "%)" + enemy.Enemy.BaseSkinName);
            Drawing.DrawLine((X + 450) + X2 + Scale * enemy.CountDown() - 1, Y + Y2 + 7, (X + 450) + X2 + Scale * enemy.CountDown(), Y + Y2 - 11, 3, c);
        }

        public static void Rect(float x, float y, int width, int height, float bold, Color color)
        {
            var x2 = x + 450;
            for (var i = 0; i < height; i++)
            {
                Drawing.DrawLine(x2, y + i, x2 + width, y + i, bold, color);
            }
        }
    }
}
