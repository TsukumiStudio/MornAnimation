using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MornLib
{
	[Serializable]
	internal abstract class MornAnimationTransformModuleBase : MornAnimationModuleBase
	{
		protected abstract void AutoBind(MornAnimationBase parent);
		protected abstract Vector3 Get();
		protected abstract void Set(Vector3 target);
		protected abstract Vector3 Lerp(Vector3 start, Vector3 end, float t);

		private enum ReferenceType
		{
			Custom,
			Auto,
		}

		[SerializeField] private ReferenceType _referenceType;
		[SerializeField] protected bool _hasSpawnValue;
		[SerializeField, EnableIf(nameof(_hasSpawnValue))] private Vector3 _spawnValue;
		[SerializeField] private Vector3 _showValue;
		[SerializeField] private Vector3 _hideValue;
		private CancellationTokenSource _cts;
		protected bool IsCustom => _referenceType == ReferenceType.Custom;
		private bool IsAuto => _referenceType == ReferenceType.Auto;

		public override void OnAwake(MornAnimationBase parent)
		{
			if (IsAuto) AutoBind(parent);
			Set(_hasSpawnValue ? _spawnValue : _hideValue);
		}

		public override void OnValidate(MornAnimationBase parent)
		{
			if (IsAuto) AutoBind(parent);
		}

		public override void OnInitialize()
		{
			Set(_hasSpawnValue ? _spawnValue : _hideValue);
		}

		public override void OnShowImmediate()
		{
			Set(_showValue);
		}

		public override void OnHideImmediate()
		{
			Set(_hideValue);
		}

		public override async UniTask ShowAsync(CancellationToken ct = default)
		{
			if (_hasSpawnValue)
			{
				Set(_spawnValue);
			}

			await MoveAsync(true, Get(), _showValue, ct);
		}

		public override async UniTask HideAsync(CancellationToken ct = default)
		{
			await MoveAsync(false, Get(), _hideValue, ct);
		}

		private async UniTask MoveAsync(bool toShow, Vector3 startPos, Vector3 endPos, CancellationToken ct = default)
		{
			_cts?.Cancel();
			_cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
			var token = _cts.Token;
			var duration = toShow ? Time.ShowDuration : Time.HideDuration;
			var delay = toShow ? Time.ShowDelay : Time.HideDelay;
			var elapsed = 0f;
			if (delay > 0f)
			{
				await MornAnimationUtil.WaitSeconds(delay, token);
			}

			var easeType = toShow ? Time.ShowEaseType : Time.HideEaseType;
			while (elapsed < duration)
			{
				token.ThrowIfCancellationRequested();
				var deltaTime = MornAnimationUtil.GetDeltaTime();
				elapsed += deltaTime;
				var t = Mathf.Clamp01(elapsed / duration);
				var easedT = t.Ease(easeType);
				Set(Lerp(startPos, endPos, easedT));
				await MornAnimationUtil.WaitNextFrame(token);
			}

			Set(endPos);
		}
	}
}