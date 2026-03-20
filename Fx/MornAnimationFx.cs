using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MornLib
{
	/// <summary>
	/// エフェクトアニメーション。Show/Hideではなく、Play で1回再生して元に戻る。
	/// Shake系のエフェクトに対応。
	/// </summary>
	public sealed class MornAnimationFx : MonoBehaviour
	{
		private enum MoveTarget
		{
			RectTransform,
			Transform,
		}

		[SerializeField] private MornAnimationFxSettings _settings;
		[SerializeField] private MoveTarget _moveTarget;
		[SerializeField, ShowIf(nameof(IsMoveRectTransform))] private RectTransform _rectTransform;

		private CancellationTokenSource _cts;

		private bool IsMoveRectTransform => _moveTarget == MoveTarget.RectTransform;

		private void Reset()
		{
			var rt = GetComponent<RectTransform>();
			if (rt != null)
			{
				_moveTarget = MoveTarget.RectTransform;
				_rectTransform = rt;
			}
			else
			{
				_moveTarget = MoveTarget.Transform;
				_rectTransform = null;
			}
		}

		public void Play()
		{
			PlayAsync().Forget();
		}

		/// <summary>エフェクト再生。内部でdestroyTokenとリンクするため、呼び出し側でdestroyTokenを渡す必要はない。</summary>
		public async UniTask PlayAsync(CancellationToken ct = default)
		{
			if (_settings == null) return;
			_cts?.Cancel();
			_cts = CancellationTokenSource.CreateLinkedTokenSource(ct, destroyCancellationToken);
			ct = _cts.Token;

			var duration = _settings.Duration;
			var vibration = _settings.Vibration;

			// 元の値を保存
			var originalPosition = GetPosition();
			var originalRotation = transform.localEulerAngles;
			var originalScale = transform.localScale;

			try
			{
				var elapsed = 0f;
				var vibrationInterval = vibration > 0f ? 1f / vibration : 0f;
				var nextVibrationTime = 0f;
				var currentShakePos = Vector3.zero;
				var currentShakeRot = Vector3.zero;
				var currentShakeScale = Vector3.zero;

				while (elapsed < duration)
				{
					ct.ThrowIfCancellationRequested();
					if (this == null) return;
					elapsed += MornAnimationUtil.GetDeltaTime();
					var t = Mathf.Clamp01(elapsed / duration);
					var decay = 1f - t;

					// vibrationタイミングで新しいランダム値を生成
					if (elapsed >= nextVibrationTime)
					{
						if (_settings.ShakePositionEnabled)
							currentShakePos = RandomShake(_settings.ShakePositionIntensity);
						if (_settings.ShakeRotationEnabled)
							currentShakeRot = RandomShake(_settings.ShakeRotationIntensity);
						if (_settings.ShakeScaleEnabled)
							currentShakeScale = RandomShake(_settings.ShakeScaleIntensity);
						nextVibrationTime = vibrationInterval > 0f ? elapsed + vibrationInterval : float.MaxValue;
					}

					// Shake: ランダム揺れ × 減衰
					var posOffset = Vector3.zero;
					var rotOffset = Vector3.zero;
					var scaleOffset = Vector3.zero;

					if (_settings.ShakePositionEnabled)
						posOffset += currentShakePos * decay;
					if (_settings.ShakeRotationEnabled)
						rotOffset += currentShakeRot * decay;
					if (_settings.ShakeScaleEnabled)
						scaleOffset += currentShakeScale * decay;

					// Punch: sin波で「0→ピーク→0」 × 減衰
					var punch = Mathf.Sin(t * Mathf.PI) * (1f - t);
					if (_settings.PunchPositionEnabled)
						posOffset += _settings.PunchPositionIntensity * punch;
					if (_settings.PunchRotationEnabled)
						rotOffset += _settings.PunchRotationIntensity * punch;
					if (_settings.PunchScaleEnabled)
						scaleOffset += _settings.PunchScaleIntensity * punch;

					SetPosition(originalPosition + posOffset);
					transform.localEulerAngles = originalRotation + rotOffset;
					transform.localScale = originalScale + scaleOffset;

					await MornAnimationUtil.WaitNextFrame(ct);
				}
			}
			finally
			{
				// 元の値に復元
				if (this != null)
				{
					SetPosition(originalPosition);
					transform.localEulerAngles = originalRotation;
					transform.localScale = originalScale;
				}
			}
		}

		private static Vector3 RandomShake(Vector3 intensity)
		{
			return new Vector3(
				Random.Range(-intensity.x, intensity.x),
				Random.Range(-intensity.y, intensity.y),
				Random.Range(-intensity.z, intensity.z)
			);
		}

		private Vector3 GetPosition()
		{
			return _moveTarget switch
			{
				MoveTarget.RectTransform => (Vector3)_rectTransform.anchoredPosition,
				MoveTarget.Transform => transform.localPosition,
				_ => Vector3.zero,
			};
		}

		private void SetPosition(Vector3 value)
		{
			switch (_moveTarget)
			{
				case MoveTarget.RectTransform:
					_rectTransform.anchoredPosition = value;
					break;
				case MoveTarget.Transform:
					transform.localPosition = value;
					break;
			}
		}

		[Button]
		public async UniTask DebugPlay()
		{
			await PlayAsync();
			MornAnimationUtil.SetDirty(this);
		}
	}
}
