using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarryMe_Evelynn.Basics;
using EloBuddy.SDK;

namespace CarryMe_Evelynn
{
	class Evelynn_Menu
	{
		internal static MenuBuilder Load()
		{
			var menuBuilder = new MenuBuilder();
			menuBuilder.AddMenu();
			menuBuilder.AddMenu(Orbwalker.ActiveModes.Combo);
			menuBuilder.AddLabel(Orbwalker.ActiveModes.Combo, "Basic Rules", true);
			menuBuilder.AddCheckBox(Orbwalker.ActiveModes.Combo, "Use Q", "use.Q", true);
			menuBuilder.AddCheckBox(Orbwalker.ActiveModes.Combo, "Use W", "use.W", true);
			menuBuilder.AddCheckBox(Orbwalker.ActiveModes.Combo, "Use E", "use.E", true);
			menuBuilder.AddCheckBox(Orbwalker.ActiveModes.Combo, "Block AA while E Ready", "use.blockAA", true);

			menuBuilder.AddMenu(Orbwalker.ActiveModes.Harass);
			menuBuilder.AddLabel(Orbwalker.ActiveModes.Harass, "Basic Rules", true);
			menuBuilder.AddCheckBox(Orbwalker.ActiveModes.Harass, "Use Q", "use.Q", true);
			menuBuilder.AddCheckBox(Orbwalker.ActiveModes.Harass, "Use W", "use.W", true);
			menuBuilder.AddCheckBox(Orbwalker.ActiveModes.Harass, "Use E", "use.E", true);

			menuBuilder.AddMenu(Orbwalker.ActiveModes.LaneClear);
			menuBuilder.AddLabel(Orbwalker.ActiveModes.LaneClear, "Basic Rules", true);
			menuBuilder.AddCheckBox(Orbwalker.ActiveModes.LaneClear, "Use Q", "use.Q", true);
			menuBuilder.AddCheckBox(Orbwalker.ActiveModes.LaneClear, "Use E", "use.E", true);

			menuBuilder.AddMenu(Orbwalker.ActiveModes.JungleClear);
			menuBuilder.AddLabel(Orbwalker.ActiveModes.JungleClear, "Basic Rules", true);
			menuBuilder.AddCheckBox(Orbwalker.ActiveModes.JungleClear, "Use Q", "use.Q", true);
			menuBuilder.AddCheckBox(Orbwalker.ActiveModes.JungleClear, "Use E", "use.E", true);

			menuBuilder.AddMenu(MenuBuilder.MenuNames.Drawing);
			menuBuilder.AddCheckBox(MenuBuilder.MenuNames.Drawing, "Draw Q Range", "draw.Q.range", true);
			menuBuilder.AddCheckBox(MenuBuilder.MenuNames.Drawing, "Draw Q Prediction", "draw.Q.prediction", true);
			menuBuilder.AddCheckBox(MenuBuilder.MenuNames.Drawing, "Draw E Range", "draw.E.range", true);
			menuBuilder.AddCheckBox(MenuBuilder.MenuNames.Drawing, "Draw R Range", "draw.R.range", true);

			menuBuilder.AddMenu(MenuBuilder.MenuNames.Anti_FPS_Drop);
			menuBuilder.AddLabel(MenuBuilder.MenuNames.Anti_FPS_Drop, "just use OnTickMode if you have FPS Problems ( it reduce the needed ressources but also the performance )");
			menuBuilder.AddCheckBox(MenuBuilder.MenuNames.Anti_FPS_Drop, "Use OnTickMode", "antiFPS.useOnTick", false);

			return menuBuilder;
		}

	}
}
