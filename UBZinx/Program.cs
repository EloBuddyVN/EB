using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Constants;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;

namespace UBZinx
{
    internal class Program
    {
        public static Spell.Active Q = new Spell.Active(SpellSlot.Q);

        public static Spell.Skillshot W = new Spell.Skillshot(SpellSlot.W, 1500, SkillShotType.Linear, 600, 3300, 100)
        {
            MinimumHitChance = HitChance.High,
            AllowedCollisionCount = 0
        };

        public static Spell.Skillshot E = new Spell.Skillshot(SpellSlot.E, 900, SkillShotType.Circular, 1200, 1750, 1);
        public static Spell.Skillshot R = new Spell.Skillshot(SpellSlot.R, 3000, SkillShotType.Linear, 600, 1700, 140);

        public static Menu Menu, ComboMenu, HarassMenu, FarmMenu, MiscMenu, KillStealMenu, DrawMenu;

        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.Hero != Champion.Jinx) return;

            Menu = MainMenu.AddMenu("UB Zinx", "UBZinx");
            Menu.AddGroupLabel("UB Zinx");
            Menu.AddLabel("Made by UB");
            Menu.AddLabel("Dattenosa");
            Menu.AddSeparator();

            ComboMenu = Menu.AddSubMenu("Combo", "ComboZinx");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("useQCombo", new CheckBox("Dùng Q"));
            ComboMenu.Add("useQSplash", new CheckBox("Đổi Q trong combo"));
            ComboMenu.Add("useWCombo", new CheckBox("Dùng W"));
            ComboMenu.Add("useECombo", new CheckBox("Dùng E"));
            ComboMenu.Add("useRCombo", new CheckBox("Dùng R"));

            HarassMenu = Menu.AddSubMenu("Harass", "HarassZinx");
            HarassMenu.AddGroupLabel("Cài đặt Cấu máu");
            HarassMenu.Add("useQHarass", new CheckBox("Dùng Q"));
            HarassMenu.Add("useWHarass", new CheckBox("Dùng W"));

            FarmMenu = Menu.AddSubMenu("Farm", "FarmZinx");
            FarmMenu.AddGroupLabel("Farm Settings");
            FarmMenu.AddLabel("LaneClear");
            FarmMenu.Add("useQFarm", new CheckBox("Use Q"));
            FarmMenu.Add("disableRocketsWC", new CheckBox("Không dùng tên lửa (Chỉ dùng súng nhỏ)", false));
            FarmMenu.AddLabel("Last Hit");
            FarmMenu.Add("disableRocketsLH", new CheckBox("Không dùng tên lửa"));

            MiscMenu = Menu.AddSubMenu("Misc", "MiscMenuZinx");
            MiscMenu.AddGroupLabel("Misc Settings");
            MiscMenu.Add("gapcloser", new CheckBox("Dùng E để chống Gapcloser"));
            MiscMenu.Add("interruptor", new CheckBox("Interruptor E"));
            MiscMenu.Add("CCE", new CheckBox("E trên CC của đồng minh"));

            KillStealMenu = Menu.AddSubMenu("KillSteal", "KillStealZinx");
            KillStealMenu.AddGroupLabel("KillSteal");
            KillStealMenu.Add("useQKS", new CheckBox("Dùng Rocket để KS"));
            KillStealMenu.Add("useWKS", new CheckBox("Dùng W để KS"));
            KillStealMenu.Add("useRKS", new CheckBox("Dùng R để KS"));


            DrawMenu = Menu.AddSubMenu("Drawing Settings");
            DrawMenu.AddGroupLabel("Drawing Settings");
            DrawMenu.Add("drawRange", new CheckBox("Draw Tầm tấn công của súng khác", false));
            DrawMenu.Add("drawW", new CheckBox("Draw W", false));
            DrawMenu.Add("drawE", new CheckBox("Draw E", false));

            Game.OnTick += Game_OnTick;
            Gapcloser.OnGapcloser += Events.Gapcloser_OnGapCloser;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (DrawMenu["drawRange"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(Color.HotPink, !Events.FishBonesActive ? Events.FishBonesBonus + Events.MinigunRange() - Player.Instance.BoundingRadius / 2 : Events.MinigunRange() - Player.Instance.BoundingRadius / 2, Player.Instance.Position);
            }
            if (DrawMenu["drawW"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(W.IsReady() ? Color.Cyan : Color.DarkRed, W.Range, Player.Instance.Position);
            }
            if (DrawMenu["drawE"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(E.IsReady() ? Color.Yellow : Color.DarkRed, E.Range, Player.Instance.Position);
            }
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender,
            Interrupter.InterruptableSpellEventArgs e)
        {
            if (MiscMenu["interruptor"].Cast<CheckBox>().CurrentValue && sender.IsEnemy &&
                e.DangerLevel == DangerLevel.High && sender.IsValidTarget(900))
            {
                E.Cast(sender);
            }
        }

        private static void Game_OnTick(EventArgs args)
        {
            Orbwalker.ForcedTarget = null;

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) Combo();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass)) Harass();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear)) WaveClear();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit)) LastHit();

            if (MiscMenu["CCE"].Cast<CheckBox>().CurrentValue)
            {
                foreach (var enemy in EntityManager.Heroes.Enemies)
                {
                    if (enemy.Distance(Player.Instance) <= E.Range &&
                        (enemy.HasBuffOfType(BuffType.Stun)
                         || enemy.HasBuffOfType(BuffType.Knockup)
                         || enemy.HasBuffOfType(BuffType.Snare)
                         || enemy.HasBuffOfType(BuffType.Suppression)))
                    {
                        E.Cast(enemy);
                    }
                }
            }
        }

        public static void LastHit()
        {
            if (FarmMenu["disableRocketsLH"].Cast<CheckBox>().CurrentValue && Events.FishBonesActive)
            {
                Q.Cast();
            }
        }

        public static void WaveClear()
        {
            if (Orbwalker.IsAutoAttacking) return;
            if (FarmMenu["useQFarm"].Cast<CheckBox>().CurrentValue)
            {
                var unit =
                    EntityManager.MinionsAndMonsters.GetLaneMinions()
                        .Where(
                            a =>
                                a.IsValidTarget(Events.MinigunRange(a) + Events.FishBonesBonus) &&
                                a.Health < Player.Instance.GetAutoAttackDamage(a) * 1.1)
                        .FirstOrDefault(minion => EntityManager.MinionsAndMonsters.EnemyMinions.Count(
                            a => a.Distance(minion) < 150 && a.Health < Player.Instance.GetAutoAttackDamage(a) * 1.1) > 1);

                if (unit != null)
                {
                    if (!Events.FishBonesActive)
                    {
                        Q.Cast();
                    }
                    Orbwalker.ForcedTarget = unit;
                    return;
                }

                if (Events.FishBonesActive)
                {
                    Q.Cast();
                }
            }
            else if (FarmMenu["disableRocketsLH"].Cast<CheckBox>().CurrentValue && Events.FishBonesActive)
            {
                Q.Cast();
            }
        }

        public static void Harass()
        {
            var targetW = TargetSelector.SelectedTarget != null &&
                         TargetSelector.SelectedTarget.Distance(Player.Instance) < W.Range
                ? TargetSelector.SelectedTarget
                : TargetSelector.GetTarget(W.Range, DamageType.Physical);

            var target = TargetSelector.SelectedTarget != null &&
                         TargetSelector.SelectedTarget.Distance(Player.Instance) < (!Events.FishBonesActive ? Player.Instance.GetAutoAttackRange() + Events.FishBonesBonus : Player.Instance.GetAutoAttackRange()) + 300
                ? TargetSelector.SelectedTarget
                : TargetSelector.GetTarget((!Events.FishBonesActive ? Player.Instance.GetAutoAttackRange() + Events.FishBonesBonus : Player.Instance.GetAutoAttackRange()) + 300, DamageType.Physical);

            Orbwalker.ForcedTarget = null;

            if (Orbwalker.IsAutoAttacking) return;

            if (targetW != null)
            {
                // W out of range
                if (HarassMenu["useWHarass"].Cast<CheckBox>().CurrentValue && W.IsReady() &&
                    target.Distance(Player.Instance) > Player.Instance.GetAutoAttackRange(targetW) &&
                    targetW.IsValidTarget(W.Range))
                {
                    W.Cast(targetW);
                }
            }

            if (target != null)
            {

                if (HarassMenu["useQHarass"].Cast<CheckBox>().CurrentValue)
                {
                    // Aoe Logic
                    foreach (
                        var enemy in
                            EntityManager.Heroes.Enemies.Where(
                                a => a.IsValidTarget(Events.MinigunRange(a) + Events.FishBonesBonus))
                                .OrderBy(TargetSelector.GetPriority))
                    {
                        if (enemy.CountEnemiesInRange(150) > 1 &&
                            (enemy.NetworkId == target.NetworkId || enemy.Distance(target) < 150))
                        {
                            if (!Events.FishBonesActive)
                            {
                                Q.Cast();
                            }
                            Orbwalker.ForcedTarget = enemy;
                            return;
                        }
                    }

                    // Regular Q Logic
                    if (Events.FishBonesActive)
                    {
                        if (target.Distance(Player.Instance) <= Player.Instance.GetAutoAttackRange(target) - Events.FishBonesBonus)
                        {
                            Q.Cast();
                        }
                    }
                    else
                    {
                        if (target.Distance(Player.Instance) > Player.Instance.GetAutoAttackRange(target))
                        {
                            Q.Cast();
                        }
                    }
                }
            }
        }

        public static void Combo()
        {

            var targetW = TargetSelector.SelectedTarget != null &&
                         TargetSelector.SelectedTarget.Distance(Player.Instance) < W.Range
                ? TargetSelector.SelectedTarget
                : TargetSelector.GetTarget(W.Range, DamageType.Physical);


            var target = TargetSelector.SelectedTarget != null &&
                         TargetSelector.SelectedTarget.Distance(Player.Instance) < (!Events.FishBonesActive ? Player.Instance.GetAutoAttackRange() + Events.FishBonesBonus : Player.Instance.GetAutoAttackRange()) + 300
                ? TargetSelector.SelectedTarget
                : TargetSelector.GetTarget((!Events.FishBonesActive ? Player.Instance.GetAutoAttackRange() + Events.FishBonesBonus : Player.Instance.GetAutoAttackRange()) + 300, DamageType.Physical);
            var rtarget = TargetSelector.GetTarget(3000, DamageType.Physical);

            Orbwalker.ForcedTarget = null;

            if (Orbwalker.IsAutoAttacking) return;

            // E on immobile

            if (target != null && ComboMenu["useECombo"].Cast<CheckBox>().CurrentValue && (target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Stun)))
            {
                E.Cast(target);
            }

            if (rtarget != null)
            {
                // W/R KS
                var wPred = W.GetPrediction(rtarget);

                if (KillStealMenu["useWKS"].Cast<CheckBox>().CurrentValue &&
                    wPred.HitChance >= HitChance.Medium && W.IsReady() && rtarget.IsValidTarget(W.Range) &&
                    Damages.WDamage(target) >= rtarget.Health)
                {
                    W.Cast(rtarget);
                }
                else if (KillStealMenu["useRKS"].Cast<CheckBox>().CurrentValue && rtarget != null &&
                         rtarget.Distance(Player.Instance) > Events.MinigunRange(target) + Events.FishBonesBonus &&
                         rtarget.IsKillableByR())
                {
                    R.Cast(rtarget);
                }
            }

            // W out of range
            if (targetW != null && ComboMenu["useWCombo"].Cast<CheckBox>().CurrentValue && W.IsReady() && targetW.Distance(Player.Instance) > Player.Instance.GetAutoAttackRange(targetW) &&
                targetW.IsValidTarget(W.Range))
            {
                W.Cast(targetW);
            }

            if (target != null && ComboMenu["useQSplash"].Cast<CheckBox>().CurrentValue)
            {
                // Aoe Logic
                foreach (var enemy in EntityManager.Heroes.Enemies.Where(
                    a => a.IsValidTarget(Events.MinigunRange(a) + Events.FishBonesBonus))
                    .OrderBy(TargetSelector.GetPriority).Where(enemy => enemy.CountEnemiesInRange(150) > 1 && (enemy.NetworkId == target.NetworkId || enemy.Distance(target) < 150)))
                {
                    if (!Events.FishBonesActive)
                    {
                        Q.Cast();
                    }
                    Orbwalker.ForcedTarget = enemy;
                    return;
                }
            }

            // Regular Q Logic
            if (target != null && ComboMenu["useQCombo"].Cast<CheckBox>().CurrentValue && Events.FishBonesActive)
            {
                if (target.Distance(Player.Instance) <= Player.Instance.GetAutoAttackRange(target) - Events.FishBonesBonus)
                {
                    Q.Cast();
                }
            }
            else if (target != null && ComboMenu["useQCombo"].Cast<CheckBox>().CurrentValue)
            {
                if (target.Distance(Player.Instance) > Player.Instance.GetAutoAttackRange(target))
                {
                    Q.Cast();
                }
            }
        }
    }
}
