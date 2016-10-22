using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy.SDK.Events;
using EloBuddy;

namespace CarryMe_Evelynn
{
	class Program
	{
		static void Main(string[] args)
		{
			Loading.OnLoadingComplete += Loading_OnLoadingComplete;
		}

		private static void Loading_OnLoadingComplete(EventArgs args)
		{
			if (ObjectManager.Player.ChampionName == "Evelynn")
				Evelynn.Load();
		}
	}
}
