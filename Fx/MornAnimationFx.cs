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

		public async UniTask PlayAsync(CancellationToken ct = default)
		{
			if (_settings == null) return;
			_cts?.Cancel();
			_cts = CancellationTokenSource.CreateLinkedTokenSource(ct, destroyCancellationToken);
			ct = _cts.Token;

			var duration = _settings.Duration;
			var easeType = _settings.EaseType;

			// 元の値を保存
			var originalPosition = GetPosition();
			var originalRotation = transform.localEulerAngles;
			var originalScale = transform.localScale;

			try
			{
				var elapsed = 0f;
				while (elapsed < duration)
				{
					ct.ThrowIfCancellationRequested();
					if (this == null) return;
					elapsed += MornAnimationUtil.GetDeltaTime();
					var t = Mathf.Clamp01(elapsed / duration);

					// 減衰係数（1→0）
					var decay = (1f - t).Ease(easeType);

					if (_settings.ShakePositionEnabled)
					{
						var shake = RandomShake(_settings.ShakePositionIntensity) * decay;
						SetPosition(originalPosition + shake);
					}

					if (_settings.ShakeRotationEnabled)
					{
						var shake = RandomShake(_settings.ShakeRotationIntensity) * decay;
						transform.localEulerAngles = originalRotation + shake;
					}

					if (_settings.ShakeScaleEnabled)
					{
						var shake = RandomShake(_settings.ShakeScaleIntensity) * decay;
						transform.localScale = originalScale + shake;
					}

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
