using UnityEngine.UI;

namespace MornLib
{
	internal static class MornAnimationUtil
	{
		public static float GetAlpha(this Image target)
		{
			return target.color.a;
		}

		public static void SetAlpha(this Image target, float alpha)
		{
			var color = target.color;
			color.a = alpha;
			target.color = color;
		}
	}
}