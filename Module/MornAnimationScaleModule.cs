using System;
using UnityEngine;

namespace MornLib
{
	[Serializable]
	internal sealed class MornAnimationScaleModule : MornAnimationTransformModuleBase
	{
		[SerializeField, EnableIf(nameof(IsCustom))] private Transform _target;

		protected override void AutoBind(MornAnimationBase parent)
		{
			_target = parent.GetComponent<Transform>();
		}

		protected override Vector3 Get()
		{
			return _target.localScale;
		}

		protected override void Set(Vector3 target)
		{
			_target.localScale = target;
		}

		protected override Vector3 Lerp(Vector3 start, Vector3 end, float t)
		{
			return Vector3.LerpUnclamped(start, end, t);
		}
	}
}