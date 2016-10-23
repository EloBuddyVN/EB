using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Constants;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;

namespace Morgana
{
    internal class Program
    {
        public const string Hero = "Morgana";
        public static Spell.Skillshot Q, W;
        public static Spell.Active R;
        public static Spell.Targeted E, Exhaust;
        public static Item Talisman, Zhonia, Randuin;
        public static int[] AbilitySequence;
        public static int QOff = 0, WOff = 0, EOff = 0, ROff = 0;

        public static Menu MorgMenu,
            ComboMenu,
            DrawMenu,
            SkinMenu,
            MiscMenu,
            QMenu,
            AutoCastMenu,
            LaneClear,
            LastHit;

        public static AIHeroClient Me = ObjectManager.Player;
        public static HitChance QHitChance;

        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoaded;
        }

        public static bool HasSpell(string s)
        {
            return Player.Spells.FirstOrDefault(o => o.SData.Name.Contains(s)) != null;
        }

        private static void OnLoaded(EventArgs args)
        {
            if (Player.Instance.ChampionName != Hero)
                return;
            Bootstrap.Init(null);
            Q = new Spell.Skillshot(SpellSlot.Q, 1175, SkillShotType.Linear, 250, 1200, 80);
            W = new Spell.Skillshot(SpellSlot.W, 900, SkillShotType.Circular, 250, 2200, 350);
            E = new Spell.Targeted(SpellSlot.E, 800);
            R = new Spell.Active(SpellSlot.R, 600);
            Exhaust = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerexhaust"), 650);
            
            Talisman = new Item((int)ItemId.Talisman_of_Ascension);
            Randuin = new Item((int)ItemId.Randuins_Omen);
            Zhonia = new Item((int)ItemId.Zhonyas_Hourglass);
            AbilitySequence = new[] { 1, 3, 2, 1, 1, 4, 1, 2, 1, 2, 4, 2, 2, 3, 3, 4, 3, 3 };

            MorgMenu = MainMenu.AddMenu("Bloodimir Morgana", "bmorgana");
            MorgMenu.AddGroupLabel("Bloodimir Morgana");
            MorgMenu.AddSeparator();
            MorgMenu.AddLabel("Bloodimir Morgana v2.1.0.0");

            ComboMenu = MorgMenu.AddSubMenu("Combo", "sbtw");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.AddSeparator();
            ComboMenu.Add("usecomboq", new CheckBox("Use Q"));
            ComboMenu.Add("usecombow", new CheckBox("Use W"));

            AutoCastMenu = MorgMenu.AddSubMenu("Auto Cast", "ac");
            AutoCastMenu.AddGroupLabel("Auto Cast");
            AutoCastMenu.AddSeparator();
            AutoCastMenu.Add("qd", new CheckBox("Auto Q Dashing"));
            AutoCastMenu.Add("qi", new CheckBox("Auto Q Immobile"));
            AutoCastMenu.Add("ar", new CheckBox("Auto R"));
            AutoCastMenu.Add("rslider", new Slider("Minimum people for Auto R", 2, 0, 5));

            QMenu = MorgMenu.AddSubMenu("Q Settings", "qsettings");
            QMenu.AddGroupLabel("Q Settings");
            QMenu.AddSeparator();
            QMenu.Add("qmin", new Slider("Min Range", 150, 0, (int)Q.Range));
            QMenu.Add("qmax", new Slider("Max Range", (int)Q.Range, 0, (int)Q.Range));
            QMenu.AddSeparator();
            foreach (var obj in ObjectManager.Get<AIHeroClient>().Where(obj => obj.Team != Me.Team))		
            {		
              QMenu.Add("bind" + obj.ChampionName.ToLower(), new CheckBox("Bind " + obj.ChampionName));		
           }
            QMenu.AddSeparator();
            QMenu.Add("mediumpred", new CheckBox("MEDIUM Bind Hitchance Prediction", false));
            QMenu.AddSeparator();
            QMenu.Add("intq", new CheckBox("Q to Interrupt"));

            SkinMenu = MorgMenu.AddSubMenu("Skin Changer", "skin");
            SkinMenu.AddGroupLabel("Choose the desired skin");

            var skinchange = SkinMenu.Add("sID", new Slider("Skin", 5, 0, 7));
            var sid = new[] { "Default", "Exiled", "Sinful Succulence", "Blade Mistress", "Blackthorn", "Ghost Bride", "Victorius", "Lunar Wraith" };
            skinchange.DisplayName = sid[skinchange.CurrentValue];
            skinchange.OnValueChange +=
                delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
                {
                    sender.DisplayName = sid[changeArgs.NewValue];
                };

            MiscMenu = MorgMenu.AddSubMenu("Misc", "misc");
            MiscMenu.AddGroupLabel("Misc");
            MiscMenu.AddSeparator();
            MiscMenu.Add("ksq", new CheckBox("KS with Q"));
            MiscMenu.Add("antigapcloser", new CheckBox("Anti Gapcloser"));
            MiscMenu.Add("talisman", new CheckBox("Use Talisman of Ascension"));
            MiscMenu.Add("randuin", new CheckBox("Use Randuin"));
            MiscMenu.Add("szhonya", new CheckBox("Smart Zhonya"));
            MiscMenu.Add("lvlup", new CheckBox("Auto Level Up Spells", false));
            MiscMenu.AddSeparator();
            MiscMenu.Add("EAllies", new CheckBox("Auto E"));
            foreach (var obj in ObjectManager.Get<AIHeroClient>().Where(obj => obj.Team == Me.Team))
            {
                MiscMenu.Add("shield" + obj.ChampionName.ToLower(), new CheckBox("Shield " + obj.ChampionName));
            }
            MiscMenu.AddSeparator();
            MiscMenu.Add("support", new CheckBox("Support Mode", false));
            MiscMenu.Add("useexhaust", new CheckBox("Use Exhaust"));
            foreach (var source in ObjectManager.Get<AIHeroClient>().Where(a => a.IsEnemy))
            {
                MiscMenu.Add(source.ChampionName + "exhaust",
                    new CheckBox("Exhaust " + source.ChampionName, false));
            }

            DrawMenu = MorgMenu.AddSubMenu("Drawings", "drawings");
            DrawMenu.AddGroupLabel("Drawings");
            DrawMenu.AddSeparator();
            DrawMenu.Add("drawq", new CheckBox("Draw Q"));
            DrawMenu.Add("draww", new CheckBox("Draw W"));
            DrawMenu.Add("drawe", new CheckBox("Draw E"));
            DrawMenu.Add("drawr", new CheckBox("Draw R"));
            DrawMenu.Add("drawaa", new CheckBox("Draw AA"));
            DrawMenu.Add("predictions", new CheckBox("Visualize Q Prediction"));

            LaneClear = MorgMenu.AddSubMenu("Lane Clear", "laneclear");
            LaneClear.AddGroupLabel("Lane Clear Settings");
            LaneClear.Add("LCW", new CheckBox("Use W"));

            LastHit = MorgMenu.AddSubMenu("Last Hit", "lasthit");
            LastHit.AddGroupLabel("Last Hit Settings");
            LastHit.Add("LHQ", new CheckBox("Use Q", false));

            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Game.OnUpdate += OnUpdate;
            Orbwalker.OnPreAttack += Orbwalker_OnPreAttack;
            Obj_AI_Base.OnProcessSpellCast += Auto_EOnProcessSpell;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            Drawing.OnDraw += delegate
            {
                if (!Me.IsDead)
                {
                    if (DrawMenu["drawr"].Cast<CheckBox>().CurrentValue && R.IsLearned)
                    {
                        Circle.Draw(Color.Red, R.Range, Player.Instance.Position);
                    }
                    if (DrawMenu["draww"].Cast<CheckBox>().CurrentValue && W.IsLearned)
                    {
                        Circle.Draw(Color.Purple, W.Range, Player.Instance.Position);
                    }
                    if (DrawMenu["drawe"].Cast<CheckBox>().CurrentValue && E.IsLearned)
                    {
                        Circle.Draw(Color.Green, E.Range, Player.Instance.Position);
                    }
                    if (DrawMenu["drawaa"].Cast<CheckBox>().CurrentValue)
                    {
                        Circle.Draw(Color.Blue, Q.Range, Player.Instance.Position);
                    }
                    var predictedPositions = new Dictionary<int, Tuple<int, PredictionResult>>();
                    var predictions = DrawMenu["predictions"].Cast<CheckBox>().CurrentValue;
                    var qRange = DrawMenu["drawq"].Cast<CheckBox>().CurrentValue;

                    foreach (
                        var enemy in
                            EntityManager.Heroes.Enemies.Where(
                                enemy => QMenu["bind" + enemy.ChampionName].Cast<CheckBox>().CurrentValue &&
                                         enemy.IsValidTarget(Q.Range + 150) &&
                                         !enemy.HasBuffOfType(BuffType.SpellShield)))
                    {
                        var predictionsq = Q.GetPrediction(enemy);
                        predictedPositions[enemy.NetworkId] = new Tuple<int, PredictionResult>(Environment.TickCount,
                            predictionsq);
                        if (qRange && Q.IsLearned)
                        {
                            Circle.Draw(Q.IsReady() ? Color.Blue : Color.Red, Q.Range,
                                Player.Instance.Position);
                        }

                        if (!predictions)
                        {
                            return;
                        }

                        foreach (var prediction in predictedPositions.ToArray())
                        {
                            if (Environment.TickCount - prediction.Value.Item1 > 2000)
                            {
                                predictedPositions.Remove(prediction.Key);
                                continue;
                            }

                            Circle.Draw(Color.Red, 75, prediction.Value.Item2.CastPosition);
                            Line.DrawLine(System.Drawing.Color.GreenYellow, Player.Instance.Position,
                                prediction.Value.Item2.CastPosition);
                            Line.DrawLine(System.Drawing.Color.CornflowerBlue,
                                EntityManager.Heroes.Enemies.Find(o => o.NetworkId == prediction.Key).Position,
                                prediction.Value.Item2.CastPosition);
                            Drawing.DrawText(prediction.Value.Item2.CastPosition.WorldToScreen() + new Vector2(0, -20),
                                System.Drawing.Color.LimeGreen,
                                string.Format("Hitchance: {0}%", Math.Ceiling(prediction.Value.Item2.HitChancePercent)),
                                10);
                        }
                    }
                    ;
                }
                ;
            };
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender,
            Interrupter.InterruptableSpellEventArgs args)
        {
            if (Q.IsReady() && sender.IsValidTarget(Q.Range) && MiscMenu["intq"].Cast<CheckBox>().CurrentValue)
                Q.Cast(sender);
        }

        private static void Auto_EOnProcessSpell(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            string[] nonskillshots =
            {
                "ShenShadowDash", "Pulverize", "GragasE", "FizzPiercingStrike", "reksaiqburrowed",
                "RunePrison", "Fling", "NocturneUnspeakableHorror", "SejuaniArcticAssault", "ShyvanaTransformCast",
                "PantheonW", "Ice Blast", "Terrify", "GalioIdolOfDurand", "GnarR", "JaxCounterStrike", "BlindMonkRKick",
                "UFSlash", "JayceThunderingBlow", "ZacR", "StaticField", "GarenQ", "TalonCutthroat", "ViR"
            };

            string[] skillShots =
            {
                "AhriSeduce", "AhriOrbofDeception", "SwainShadowGrasp", "syndrae5", "SyndraE", "TahmKenchQ", "VarusE",
                "VeigarBalefulStrike", "VeigarDarkMatter", "VeigarEventHorizon", "VelkozQ", "VelkozQSplit", "VelkozE",
                "Laser", "Vi-q", "xeratharcanopulse2", "XerathArcaneBarrage2", "XerathMageSpear",
                "xerathrmissilewrapper", "BraumQ", "RocketGrab", "JavelinToss", "BrandBlazeMissile", "Heimerdingerwm",
                "JannaQ", "JarvanIVEQ", "BandageToss", "CaitlynEntrapment", "PhosphorusBomb", "MissileBarrage2",
                "DariusAxeGrabCone", "DianaArc", "DianaArcArc", "InfectedCleaverMissileCast", "DravenDoubleShot",
                "EkkoQ", "EkkoW", "EkkoR", "EliseHumanE", "GalioResoluteSmite", "GalioRighteousGust",
                "GalioIdolOfDurand", "CurseoftheSadMummy", "FlashFrost", "EvelynnR", "QuinnQ", "yasuoq3w",
                "RengarEFinal", "ZiggsW", "ZyraGraspingRoots", "ZyraBrambleZone", "Dazzle", "FiddlesticksDarkWind",
                "FeralScream", "ZiggsW", "ViktorChaosStorm", "AlZaharCalloftheVoid",
                "RumbleCarpetBombMissile", "ThreshQ", "ThreshE", "NamiQ", "DarkBindingMissile", "OrianaDetonateCommand",
                "NautilusAnchorDrag",
                "SejuaniGlacialPrisonCast", "SonaR", "VarusR", "rivenizunablade", "EnchantedCrystalArrow", "BardR",
                "InfernalGuardian",
                "CassiopeiaPetrifyingGaze",
                "BraumRWrapper", "FizzMarinerDoomMissile", "ViktorDeathRay", "ViktorDeathRay3", "XerathMageSpear",
                "GragasR", "HecarimUlt", "LeonaSolarFlare", "LissandraR", "LuxLightBinding", "LuxMaliceCannon", "JinxW",
                "LuxLightStrikeKugel"
            };
            if (sender.Type != Me.Type || !E.IsReady() || !sender.IsEnemy ||
                !MiscMenu["EAllies"].Cast<CheckBox>().CurrentValue)
                return;
            foreach (var ally in EntityManager.Heroes.Allies.Where(x => x.IsValidTarget(E.Range)))
            {
                var detectRange = ally.ServerPosition +
                                  (args.End - ally.ServerPosition).Normalized() * ally.Distance(args.End);
                if (detectRange.Distance(ally.ServerPosition) > ally.AttackRange - ally.BoundingRadius)
                    continue;
                {
                    if (!args.SData.IsAutoAttack())
                    {
                        foreach (var item in skillShots)
                        {
                            if (args.SData.Name == item &&
                                (MiscMenu["Shield " + ally.ChampionName].Cast<CheckBox>().CurrentValue))
                            {
                                E.Cast(ally);
                            }
                        }
                    }
                    for (var i = 0; i <= 4; i++)
                    {
                        if (args.SData.Name == nonskillshots[i])
                        {
                            if (ally.Distance(args.End) < 325 &&
                                (MiscMenu["Shield " + ally.ChampionName].Cast<CheckBox>().CurrentValue))
                            {
                                E.Cast(ally);
                            }
                        }
                    }
                }
            }
        }

        private static void RanduinU()
        {
            if (Randuin.IsReady() && Randuin.IsOwned())
            {
                var randuin = MiscMenu["randuin"].Cast<CheckBox>().CurrentValue;
                if (randuin && Me.HealthPercent <= 15 && Me.CountEnemiesInRange(Randuin.Range) >= 1 ||
                    Me.CountEnemiesInRange(Randuin.Range) >= 2)
                {
                    Randuin.Cast();
                }
            }
        }

        private static void Ascension()
        {
            if (Talisman.IsReady() && Talisman.IsOwned())
            {
                var ascension = MiscMenu["talisman"].Cast<CheckBox>().CurrentValue;
                if (ascension && Me.HealthPercent <= 15 && Me.CountEnemiesInRange(800) >= 1 ||
                    Me.CountEnemiesInRange(Q.Range) >= 3)
                {
                    Talisman.Cast();
                }
            }
        }

        private static void ZhonyaU()
        {
            var zhoniaon = MiscMenu["szhonya"].Cast<CheckBox>().CurrentValue;

            if (zhoniaon && Zhonia.IsReady() && Zhonia.IsOwned())
            {
                if (Me.CountEnemiesInRange(R.Range) >= 4)
                {
                    R.Cast();
                    Zhonia.Cast();
                }
                else
                {
                    if (Me.HealthPercent <= 10 && Me.CountEnemiesInRange(E.Range) >= 1)
                    {
                        Zhonia.Cast();
                    }
                }
            }
        }

        private static void LevelUpSpells()
        {
            var qL = Me.Spellbook.GetSpell(SpellSlot.Q).Level + QOff;
            var wL = Me.Spellbook.GetSpell(SpellSlot.W).Level + WOff;
            var eL = Me.Spellbook.GetSpell(SpellSlot.E).Level + EOff;
            var rL = Me.Spellbook.GetSpell(SpellSlot.R).Level + ROff;
            if (qL + wL + eL + rL < ObjectManager.Player.Level)
            {
                int[] level = { 0, 0, 0, 0 };
                for (var i = 0; i < ObjectManager.Player.Level; i++)
                {
                    level[AbilitySequence[i] - 1] = level[AbilitySequence[i] - 1] + 1;
                }
                if (qL < level[0]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);
                if (wL < level[1]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);
                if (eL < level[2]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
                if (rL < level[3]) ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.R);
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            QHitChance = QMenu["mediumpred"].Cast<CheckBox>().CurrentValue ? HitChance.Medium : HitChance.High;
            Killsteal();
            SkinChange();
            Ascension();
            RanduinU();
            ZhonyaU();
            if (MiscMenu["lvlup"].Cast<CheckBox>().CurrentValue) LevelUpSpells();
            AutoCast(immobile: AutoCastMenu["qi"].Cast<CheckBox>().CurrentValue,
                dashing: AutoCastMenu["qd"].Cast<CheckBox>().CurrentValue);
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                    Combo(ComboMenu["usecomboq"].Cast<CheckBox>().CurrentValue);
                UseW(ComboMenu["usecombow"].Cast<CheckBox>().CurrentValue);
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) ||
                Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                LaneClearA.LaneClear();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                LastHitA.LastHitB();
            }

                if (MiscMenu["useexhaust"].Cast<CheckBox>().CurrentValue)
                    foreach (
                        var enemy in
                            ObjectManager.Get<AIHeroClient>()
                                .Where(a => a.IsEnemy && a.IsValidTarget(Exhaust.Range))
                                .Where(enemy => MiscMenu[enemy.ChampionName + "exhaust"].Cast<CheckBox>().CurrentValue))
                    {
                        if (enemy.IsFacing(Me))
                        {
                            if (!(Me.HealthPercent < 50)) continue;
                            Exhaust.Cast(enemy);
                            return;
                        }
                        if (!(enemy.HealthPercent < 50)) continue;
                        Exhaust.Cast(enemy);
                        return;
                    }
            }
        

        private static void Orbwalker_OnPreAttack(AttackableUnit target, Orbwalker.PreAttackArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) ||
                Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit) ||
                (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) ||
                 Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear)))
            {
                var t = target as Obj_AI_Minion;
                if (t != null)
                {
                    {
                        if (MiscMenu["support"].Cast<CheckBox>().CurrentValue)
                            args.Process = false;
                    }
                }
            }
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs a)
        {
            var agapcloser = MiscMenu["antigapcloser"].Cast<CheckBox>().CurrentValue;
            var antigapc = E.IsReady() && agapcloser;
            if (antigapc)
            {
                if (sender.IsMe)
                {
                    var gap = a.Sender;
                    if (gap.IsValidTarget(4000))
                    {
                        E.Cast(Me);
                    }
                }
            }
        }

        public static Obj_AI_Base GetEnemy(float range, GameObjectType t)
        {
            switch (t)
            {
                default:
                    return EntityManager.Heroes.Enemies.OrderBy(a => a.Health).FirstOrDefault(
                        a => a.Distance(Player.Instance) < range && !a.IsDead && !a.IsInvulnerable);
            }
        }

        private static void Killsteal()
        {
            if (MiscMenu["ksq"].Cast<CheckBox>().CurrentValue && Q.IsReady())
            {
                foreach (
                    var qtarget in
                        EntityManager.Heroes.Enemies.Where(
                            hero => hero.IsValidTarget(Q.Range) && !hero.IsDead && !hero.IsZombie))
                {
                    if (Me.GetSpellDamage(qtarget, SpellSlot.Q) >= qtarget.Health)
                    {
                        var poutput = Q.GetPrediction(qtarget);
                        if (poutput.HitChance >= HitChance.Medium)
                        {
                            Q.Cast(poutput.CastPosition);
                        }
                    }
                }
            }
        }

        private static void SkinChange()
        {
            var style = SkinMenu["sID"].DisplayName;
            switch (style)
            {
                case "Default":
                    Player.SetSkinId(0);
                    break;
                case "Exiled":
                    Player.SetSkinId(1);
                    break;
                case "Sinful Succulence":
                    Player.SetSkinId(2);
                    break;
                case "Blade Mistress":
                    Player.SetSkinId(3);
                    break;
                case "Blackthorn":
                    Player.SetSkinId(4);
                    break;
                case "Ghost Bride":
                    Player.SetSkinId(5);
                    break;
                case "Victorius":
                    Player.SetSkinId(6);
                    break;
                case "Lunar Wraith":
                    Player.SetSkinId(7);
                    break;
            }
        }

        private static bool Immobile(Obj_AI_Base unit)
        {
            return unit.HasBuffOfType(BuffType.Charm) || unit.HasBuffOfType(BuffType.Stun) ||
                   unit.HasBuffOfType(BuffType.Knockup) || unit.HasBuffOfType(BuffType.Snare) ||
                   unit.HasBuffOfType(BuffType.Taunt) || unit.HasBuffOfType(BuffType.Suppression);
        }

        private static void AutoCast(bool dashing, bool immobile)
        {
            if (Q.IsReady())
            {
                foreach (var itarget in EntityManager.Heroes.Enemies.Where(h => h.IsValidTarget(Q.Range)))
                {
                    if (immobile && Immobile(itarget) && Q.GetPrediction(itarget).HitChance >= QHitChance)
                    {
                        Q.Cast(itarget);
                    }

                    if (dashing && itarget.Distance(Me.ServerPosition) <= 400f &&
                        Q.GetPrediction(itarget).HitChance >= HitChance.Dashing)
                    {
                        Q.Cast(itarget);
                    }
                }
            }
            if (R.IsReady())
            {
                if (AutoCastMenu["ar"].Cast<CheckBox>().CurrentValue &&
                    Me.CountEnemiesInRange(R.Range) >= AutoCastMenu["rslider"].Cast<Slider>().CurrentValue)
                {
                    R.Cast();
                    if (E.IsReady())
                        E.Cast(Me);
                    else if (Zhonia.IsReady() && !E.IsReady() && Me.CountEnemiesInRange(425) >= 3)
                    Zhonia.Cast();

                }
            }
        }

        private static void Combo(bool useQ)
        {
            if (useQ && Q.IsReady())
            {
                var bindTarget = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
                if (bindTarget.IsValidTarget(Q.Range))
                {
                    if (Q.GetPrediction(bindTarget).HitChance >= QHitChance)
                    {
                        if (bindTarget.Distance(Me.ServerPosition) > QMenu["qmin"].Cast<Slider>().CurrentValue && bindTarget.Distance(Me.ServerPosition) < QMenu["qmax"].Cast<Slider>().CurrentValue)
                        {
                            if (QMenu["bind" + bindTarget.ChampionName].Cast<CheckBox>().CurrentValue)
                            {
                                Q.Cast(bindTarget);
                            }
                        }
                    }
                }
            }
        }

        private static void UseW(bool useW)
        {
            if (useW && W.IsReady() && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                var wenemy =
                    (AIHeroClient)GetEnemy(W.Range, GameObjectType.AIHeroClient);
                if (wenemy != null && Immobile(wenemy))
                {
                    W.Cast(wenemy);
                }
            }
        }
    }
}
