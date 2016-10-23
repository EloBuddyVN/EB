using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace Morgana
{
    internal class LaneClearA
    {
        public enum AttackSpell
        {
            W
        };

        public static AIHeroClient Morgana
        {
            get { return ObjectManager.Player; }
        }

        public static Obj_AI_Base GetEnemy(float range, GameObjectType t)
        {
            switch (t)
            {
                case GameObjectType.AIHeroClient:
                    return EntityManager.Heroes.Enemies.OrderBy(a => a.Health).FirstOrDefault(
                        a => a.Distance(Player.Instance) < range && !a.IsDead && !a.IsInvulnerable);
                default:
                    return EntityManager.MinionsAndMonsters.EnemyMinions.OrderBy(a => a.Health).FirstOrDefault(
                        a => a.Distance(Player.Instance) < range && !a.IsDead && !a.IsInvulnerable);
            }
        }
        public static void LaneClear()
        {
            var wcheck = Program.LaneClear["LCW"].Cast<CheckBox>().CurrentValue;
            var wready = Program.W.IsReady();

            if (!wready || !wcheck) return;
            {
                var wenemy =
                    (Obj_AI_Minion)GetEnemy(Program.W.Range, GameObjectType.obj_AI_Minion);

                if (wenemy != null)
                { 
                        Program.W.Cast(wenemy);
                    }
            }
        }
    }
}