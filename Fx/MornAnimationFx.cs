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
		[SerializeField] private MornAnimationFxSettings _settings;
		[SerializeField] private bool _autoStart;
		[SerializeField] private bool _autoDestroy;
		[SerializeField] private MornAnimationMoveTargetModule _moveTarget = new();
		[SerializeField] private MornAnimationFadeTargetModule _fadeTarget = new();

		[Header("Initial State")]
		[SerializeField] private float _initialAlpha = 1f;
		[SerializeField] private Vector3 _initialPosition;
		[SerializeField] private Vector3 _initialScale = Vector3.one;
		[SerializeField] private Vector3 _initialRotation;

		private CancellationTokenSource _cts;

		/// <summary>現在の座標・Scale・Rotation・Alphaを InitialState に取り込む。</summary>
		[Button("現在の状態をInitialStateに設定")]
		public void CaptureCurrentAsInitialState()
		{
			MornAnimationUtil.RecordUndo(this, "CaptureCurrentAsInitialState");
			_initialPosition = _moveTarget.GetPosition(transform);
			_initialScale = transform.localScale;
			_initialRotation = transform.localEulerAngles;
			_initialAlpha = _fadeTarget.GetAlpha();
			MornAnimationUtil.SetDirty(this);
		}

		public void Play()
		{
			PlayAsync().Forget();
		}

		[Button]
		public void DebugInitialize()
		{
			_moveTarget.SetPosition(transform, _initialPosition);
			transform.localEulerAngles = _initialRotation;
			transform.localScale = _initialScale;
			_fadeTarget.SetAlpha(_initialAlpha);
			MornAnimationUtil.SetDirty(this);
		}

		[Button]
		public async UniTask DebugPlay()
		{
			await PlayAsync();
			MornAnimationUtil.SetDirty(this);
		}

		/// <summary>エフェクト再生。内部でdestroyTokenとリンクするため、呼び出し側でdestroyTokenを渡す必要はない。</summary>
		public async UniTask PlayAsync(CancellationToken ct = default)
		{
			if (_settings == null) return;
			_cts?.Cancel();
			_cts = CancellationTokenSource.CreateLinkedTokenSource(ct, destroyCancellationToken);
			ct = _cts.Token;

			var duration = _settings.Duration;
			var vibration = _settings.ShakeVibration;

			// 初期値を基準に動作 (DebugPlayで状態が崩れないようにする)
			var originalPosition = _initialPosition;
			var originalRotation = _initialRotation;
			var originalScale = _initialScale;
			var originalAlpha = _initialAlpha;

			var vibrationInterval = vibration > 0f ? 1f / vibration : 0f;
			var elapsed = 0f;
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

				var baseScale = originalScale;
				if (_settings.ScaleEnabled)
				{
					var v = _settings.ScaleRate.Evaluate(t);
					baseScale = ApplyCurveMode(_settings.ScaleMode, originalScale, v);
				}

				_moveTarget.SetPosition(transform, originalPosition + posOffset);
				transform.localEulerAngles = originalRotation + rotOffset;
				transform.localScale = baseScale + scaleOffset;

				if (_settings.FadeEnabled)
				{
					var v = _settings.FadeRate.Evaluate(t);
					_fadeTarget.SetAlpha(ApplyCurveMode(_settings.FadeMode, originalAlpha, v));
				}

				await MornAnimationUtil.WaitNextFrame(ct);
			}

			if (_autoDestroy && Application.isPlaying && this != null)
			{
				Destroy(gameObject);
			}
		}

		private void Start()
		{
			if (_autoStart) Play();
		}

#if UNITY_EDITOR
		private void Reset()
		{
			_moveTarget = new MornAnimationMoveTargetModule();
			_moveTarget.ResetFromGameObject(gameObject);
			_fadeTarget = new MornAnimationFadeTargetModule();
			_fadeTarget.ResetFromGameObject(gameObject);
			_initialPosition = _moveTarget.GetPosition(transform);
			_initialScale = transform.localScale;
			_initialRotation = transform.localEulerAngles;
			_initialAlpha = _fadeTarget.GetAlpha();
		}
#endif

		private static Vector3 ApplyCurveMode(MornAnimationFxSettings.CurveApplyMode mode, Vector3 original, float curveValue)
		{
			return mode switch
			{
				MornAnimationFxSettings.CurveApplyMode.Multiply => original * curveValue,
				MornAnimationFxSettings.CurveApplyMode.Add => original + Vector3.one * curveValue,
				MornAnimationFxSettings.CurveApplyMode.Override => Vector3.one * curveValue,
				_ => original,
			};
		}

		private static float ApplyCurveMode(MornAnimationFxSettings.CurveApplyMode mode, float original, float curveValue)
		{
			return mode switch
			{
				MornAnimationFxSettings.CurveApplyMode.Multiply => original * curveValue,
				MornAnimationFxSettings.CurveApplyMode.Add => original + curveValue,
				MornAnimationFxSettings.CurveApplyMode.Override => curveValue,
				_ => original,
			};
		}

		private static Vector3 RandomShake(Vector3 intensity)
		{
			return new Vector3(
				Random.Range(-intensity.x, intensity.x),
				Random.Range(-intensity.y, intensity.y),
				Random.Range(-intensity.z, intensity.z)
			);
		}

	}
}
