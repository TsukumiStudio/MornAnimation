using System;
using UnityEngine;

namespace MornLib
{
	[Serializable]
	internal sealed class MornAnimationMoveUGUIModule : MornAnimationTransformModuleBase
	{
		[SerializeField, EnableIf(nameof(IsCustom))]
		private RectTransform _target;

		protected override void AutoBind(MornAnimationBase parent)
		{
			_target = parent.GetComponent<RectTransform>();
		}

		protected override Vector3 Get()
		{
			return _target.anchoredPosition;
		}

		protected override void Set(Vector3 target)
		{
			_target.anchoredPosition = target;
		}

		protected override Vector3 Lerp(Vector3 start, Vector3 end, float t)
		{
			return Vector3.LerpUnclamped(start, end, t);
		}
	}
}