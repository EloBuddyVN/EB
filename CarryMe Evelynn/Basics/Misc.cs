using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using SharpDX;

namespace CarryMe_Evelynn.Basics
{
	static class Misc
	{
		public static bool IsWithinDistance(this Obj_AI_Base unit, float min, float max)
		{
			var distance = ObjectManager.Player.Distance(unit);
			return min <= distance && distance <= max;
		}
		public static Vector2 V2E(this Vector3 from, Vector3 direction, float distance)
		{
			return from.To2D() + distance * Vector3.Normalize(direction - from).To2D();
		}
		public static Vector3 V3E(this Vector3 from, Vector3 direction, float distance)
		{
			return from + distance * Vector3.Normalize(direction - from);
		}
	}

}
