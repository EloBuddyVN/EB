using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarryMe_Evelynn.Basics;
using EloBuddy;
using EloBuddy.SDK;
using System.Drawing;

namespace CarryMe_Evelynn
{
	class Evelynn
	{
		internal static MenuBuilder Config;
		public static int NotifieDelay;

		public static Spell.Active Q = new Spell.Active(SpellSlot.Q, 500);
		public static Spell.Active W = new Spell.Active(SpellSlot.W,700);
		public static Spell.Targeted E = new Spell.Targeted(SpellSlot.E, 225);
		internal static void Load()
		{
			Config = Evelynn_Menu.Load();

			Game.OnUpdate += OnUpdate;
			Game.OnTick += OnTick;
			Drawing.OnDraw += OnDraw;
		}
		private static void OnDraw(EventArgs args)
		{
			if (Config.IsChecked(MenuBuilder.MenuNames.Drawing, "draw.Q.range"))
			{
				var circle = new Geometry.Polygon.Circle(ObjectManager.Player.Position, Q.Range, 50);
				circle.Draw(Q.IsReady() ? Color.Green : Color.Brown);
			}	

			if (Config.IsChecked(MenuBuilder.MenuNames.Drawing, "draw.Q.prediction"))
			{
				var target = ObjectManager.Get<Obj_AI_Base>()
						.Where(u => u.IsValidTarget(Q.Range) && !u.IsDead && !u.IsAlly && u.Type == GameObjectType.AIHeroClient)
						.OrderBy(u => u.Distance(ObjectManager.Player))
						.FirstOrDefault();
				if (target != null)
				{
					var rect = new Geometry.Polygon.Rectangle(ObjectManager.Player.Position, ObjectManager.Player.Position.V3E(target.Position, Q.Range), 50);
					rect.Draw(Q.IsReady() ? Color.Blue : Color.Brown);
				}
			}
		}
		private static void OnUpdate(EventArgs args)
		{
			if (Config.IsChecked(MenuBuilder.MenuNames.Anti_FPS_Drop, "antiFPS.useOnTick"))
				return;
			OnLogicRun();
		}
		private static void OnTick(EventArgs args)
		{
			if (!Config.IsChecked(MenuBuilder.MenuNames.Anti_FPS_Drop, "antiFPS.useOnTick"))
				return;
			if (NotifieDelay + 15000 < Core.GameTickCount && Game.FPS > 100)
			{
				Chat.Print("CarryMe Evelynn: Please use framelock 60 fps");
				Chat.Print("CarryMe Evelynn: You find it in League Settings.");
				NotifieDelay = Core.GameTickCount;
			}
			OnLogicRun();
		}

		private static void OnLogicRun()
		{
			
			if (ObjectManager.Player.IsDead)
				return;

			Orbwalker.DisableAttacking = Config.IsChecked(Orbwalker.ActiveModes.Combo, "use.blockAA") && E.IsReady();

			if (Q.IsReady() &&
			    (Config.IsChecked(Orbwalker.ActiveModes.Combo, "use.Q") ||
			     Config.IsChecked(Orbwalker.ActiveModes.Harass, "use.Q")))
			{
				var Nearsttarget = ObjectManager.Get<Obj_AI_Base>()
						.Where(u => u.IsValidTarget(Q.Range) && !u.IsDead && !u.IsAlly &&  u.Type == GameObjectType.AIHeroClient)
						.OrderBy(u => u.Distance(ObjectManager.Player))
						.FirstOrDefault();
				if (Nearsttarget != null)
					Q.Cast();
			}
			if (W.IsReady() &&
				(Config.IsChecked(Orbwalker.ActiveModes.Combo, "use.W") ||
				 Config.IsChecked(Orbwalker.ActiveModes.Harass, "use.W")))
			{
				var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
				if (target != null && !ObjectManager.Player.IsInAutoAttackRange(target))
				{
					W.Cast();
				}
			}
			if (E.IsReady() &&
				(Config.IsChecked(Orbwalker.ActiveModes.Combo, "use.E") ||
				 Config.IsChecked(Orbwalker.ActiveModes.Harass, "use.E")))
			{
				var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
				if (target != null)
				{
					E.Cast(target);
				}
			}
			if (Q.IsReady() &&
			    (Config.IsChecked(Orbwalker.ActiveModes.LaneClear, "use.Q") ||
			     Config.IsChecked(Orbwalker.ActiveModes.JungleClear, "use.Q")))
			{
				Q.Cast();
			}
			if (E.IsReady() && Config.IsChecked(Orbwalker.ActiveModes.LaneClear, "use.E"))
			{
				var target =
					EntityManager.MinionsAndMonsters.Minions.Where(u => u.IsValidTarget(E.Range) && !u.IsDead && !u.IsAlly)
						.OrderBy(u => u.Health)
						.Reverse()
						.FirstOrDefault();
				if (target != null)
				E.Cast(target);
			}
			if (E.IsReady() && Config.IsChecked(Orbwalker.ActiveModes.JungleClear, "use.E"))
			{
				var target =
					EntityManager.MinionsAndMonsters.Monsters.Where(u => u.IsValidTarget(E.Range) && !u.IsDead && !u.IsAlly)
						.OrderBy(u => u.Health)
						.Reverse()
						.FirstOrDefault();
				if (target != null)
				E.Cast(target);
			}
		}
	}
}
