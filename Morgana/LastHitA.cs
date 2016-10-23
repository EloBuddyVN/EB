using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu.Values;

namespace Morgana
{
    internal class LastHitA
    {
        public enum AttackSpell
        {
            Q
        };

        public static AIHeroClient Morgana
        {
            get { return ObjectManager.Player; }
        }

        public static float Qcalc(Obj_AI_Base target)
        {
            return Morgana.CalculateDamageOnUnit(target, DamageType.Magical,
                (new float[] {0, 80, 135, 190, 245, 300}[Program.Q.Level] + (0.90f*Morgana.FlatMagicDamageMod)));
        }

        public static Obj_AI_Base MinionLh(GameObjectType type, AttackSpell spell)
        {
            return
                ObjectManager.Get<Obj_AI_Base>()
                    .OrderBy(a => a.Health)
                    .FirstOrDefault(
                        a =>
                            a.IsEnemy && a.Type == type && a.Distance(Morgana) <= Program.Q.Range && !a.IsDead &&
                            !a.IsInvulnerable && a.IsValidTarget(Program.Q.Range) && a.Health <= Qcalc(a));
        }

        public static void LastHitB()
        {
            var qcheck = Program.LastHit["LHQ"].Cast<CheckBox>().CurrentValue;
            var qready = Program.Q.IsReady();
            if (!qcheck || !qready) return;
            {
                var minion = (Obj_AI_Minion) MinionLh(GameObjectType.obj_AI_Minion, AttackSpell.Q);
                if (minion != null)
                    if (Program.Q.MinimumHitChance >= HitChance.Low)
                    {
                        Program.Q.Cast(minion.ServerPosition);
                    }
            }
        }
    }
}