using System;
using UnityEngine;

namespace MornLib
{
	[Serializable]
	internal sealed class MornAnimationRotateModule : MornAnimationTransformModuleBase
	{
		[SerializeField, EnableIf(nameof(IsCustom))] private Transform _target;
		[SerializeField] private bool _isWorldRotation;

		protected override void AutoBind(MornAnimationBase parent)
		{
			_target = parent.GetComponent<Transform>();
		}

		protected override Vector3 Get()
		{
			return _isWorldRotation ? _target.eulerAngles : _target.localEulerAngles;
		}

		protected override void Set(Vector3 target)
		{
			if (_isWorldRotation) _target.eulerAngles = target;
			else _target.localEulerAngles = target;
		}

		protected override Vector3 Lerp(Vector3 start, Vector3 end, float t)
		{
			return Quaternion.LerpUnclamped(Quaternion.Euler(start), Quaternion.Euler(end), t).eulerAngles;
		}
	}
}