using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MornLib
{
	[Serializable]
	internal abstract class MornAnimationModuleBase
	{
		[SerializeField] private MornAnimationTimeSettings _overrideTimeSettings;
		protected MornAnimationTimeSettings Time => _overrideTimeSettings != null ? _overrideTimeSettings : MornAnimationGlobal.I.TimeSettings;
		public abstract void OnAwake(MornAnimationBase parent);
		public abstract void OnValidate(MornAnimationBase parent);
		public abstract void OnShowImmediate();
		public abstract void OnHideImmediate();
		public abstract UniTask ShowAsync(CancellationToken ct = default);
		public abstract UniTask HideAsync(CancellationToken ct = default);
	}
}