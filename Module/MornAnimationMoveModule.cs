using System;
using UnityEngine;

namespace MornLib
{
	[Serializable]
	internal sealed class MornAnimationMoveModule : MornAnimationTransformModuleBase
	{
		[SerializeField, EnableIf(nameof(IsCustom))] private Transform _target;
		[SerializeField] private bool _isWorldPosition;

		protected override void AutoBind(MornAnimationBase parent)
		{
			_target = parent.GetComponent<Transform>();
		}

		protected override Vector3 Get()
		{
			return _isWorldPosition ? _target.position : _target.localPosition;
		}

		protected override void Set(Vector3 target)
		{
			if (_isWorldPosition) _target.position = target;
			else _target.localPosition = target;
		}

		protected override Vector3 Lerp(Vector3 start, Vector3 end, float t)
		{
			return Vector3.LerpUnclamped(start, end, t);
		}
	}
}