using UnityEngine;

namespace MornLib
{
	/// <summary>
	/// エフェクトアニメーションの定義。Shake系・Punch系のパラメータを持つ。
	/// </summary>
	[CreateAssetMenu(fileName = nameof(MornAnimationFxSettings), menuName = "Morn/Animation/" + nameof(MornAnimationFxSettings))]
	public sealed class MornAnimationFxSettings : ScriptableObject
	{
		public enum CurveApplyMode
		{
			Multiply,
			Add,
			Override,
		}

		[SerializeField] private float _duration = 0.3f;

		[Header("Shake")]
		[SerializeField] private float _shakeVibration = 20f;
		[SerializeField] private bool _shakePositionEnabled;
		[SerializeField, ShowIf(nameof(_shakePositionEnabled))] private Vector3 _shakePositionIntensity = new(10f, 10f, 0f);
		[SerializeField] private bool _shakeRotationEnabled;
		[SerializeField, ShowIf(nameof(_shakeRotationEnabled))] private Vector3 _shakeRotationIntensity = new(0f, 0f, 15f);
		[SerializeField] private bool _shakeScaleEnabled;
		[SerializeField, ShowIf(nameof(_shakeScaleEnabled))] private Vector3 _shakeScaleIntensity = new(0.1f, 0.1f, 0f);

		[Header("Scale")]
		[SerializeField] private bool _scaleEnabled;
		[SerializeField, ShowIf(nameof(_scaleEnabled))] private MornAnimationRateModule _scaleRate = new();
		[SerializeField, ShowIf(nameof(_scaleEnabled))] private CurveApplyMode _scaleMode = CurveApplyMode.Multiply;

		[Header("Fade")]
		[SerializeField] private bool _fadeEnabled;
		[SerializeField, ShowIf(nameof(_fadeEnabled))] private MornAnimationRateModule _fadeRate = new();
		[SerializeField, ShowIf(nameof(_fadeEnabled))] private CurveApplyMode _fadeMode = CurveApplyMode.Multiply;

		[Header("Random Position")]
		[SerializeField] private bool _randomPositionEnabled;
		[SerializeField, ShowIf(nameof(_randomPositionEnabled))] private Vector3 _randomPositionMin;
		[SerializeField, ShowIf(nameof(_randomPositionEnabled))] private Vector3 _randomPositionMax;
		[SerializeField, ShowIf(nameof(_randomPositionEnabled))] private bool _randomPositionRelative = true;

		[Header("Random Rotation")]
		[SerializeField] private bool _randomRotationEnabled;
		[SerializeField, ShowIf(nameof(_randomRotationEnabled))] private Vector3 _randomRotationMin;
		[SerializeField, ShowIf(nameof(_randomRotationEnabled))] private Vector3 _randomRotationMax;
		[SerializeField, ShowIf(nameof(_randomRotationEnabled))] private bool _randomRotationRelative = true;

		[Header("Random Scale")]
		[SerializeField] private bool _randomScaleEnabled;
		[SerializeField, ShowIf(nameof(_randomScaleEnabled))] private Vector3 _randomScaleMin = Vector3.one;
		[SerializeField, ShowIf(nameof(_randomScaleEnabled))] private Vector3 _randomScaleMax = Vector3.one;
		[SerializeField, ShowIf(nameof(_randomScaleEnabled))] private CurveApplyMode _randomScaleMode = CurveApplyMode.Multiply;

		[Header("Punch")]
		[SerializeField] private bool _punchPositionEnabled;
		[SerializeField, ShowIf(nameof(_punchPositionEnabled))] private Vector3 _punchPositionIntensity = new(0f, 30f, 0f);
		[SerializeField] private bool _punchRotationEnabled;
		[SerializeField, ShowIf(nameof(_punchRotationEnabled))] private Vector3 _punchRotationIntensity = new(0f, 0f, 15f);
		[SerializeField] private bool _punchScaleEnabled;
		[SerializeField, ShowIf(nameof(_punchScaleEnabled))] private Vector3 _punchScaleIntensity = new(0.2f, 0.2f, 0f);

		public float Duration => _duration;
		public float ShakeVibration => _shakeVibration;
		public bool ShakePositionEnabled => _shakePositionEnabled;
		public Vector3 ShakePositionIntensity => _shakePositionIntensity;
		public bool ShakeRotationEnabled => _shakeRotationEnabled;
		public Vector3 ShakeRotationIntensity => _shakeRotationIntensity;
		public bool ShakeScaleEnabled => _shakeScaleEnabled;
		public Vector3 ShakeScaleIntensity => _shakeScaleIntensity;
		public bool ScaleEnabled => _scaleEnabled;
		public MornAnimationRateModule ScaleRate => _scaleRate;
		public CurveApplyMode ScaleMode => _scaleMode;
		public bool FadeEnabled => _fadeEnabled;
		public MornAnimationRateModule FadeRate => _fadeRate;
		public CurveApplyMode FadeMode => _fadeMode;
		public bool RandomPositionEnabled => _randomPositionEnabled;
		public Vector3 RandomPositionMin => _randomPositionMin;
		public Vector3 RandomPositionMax => _randomPositionMax;
		public bool RandomPositionRelative => _randomPositionRelative;
		public bool RandomRotationEnabled => _randomRotationEnabled;
		public Vector3 RandomRotationMin => _randomRotationMin;
		public Vector3 RandomRotationMax => _randomRotationMax;
		public bool RandomRotationRelative => _randomRotationRelative;
		public bool RandomScaleEnabled => _randomScaleEnabled;
		public Vector3 RandomScaleMin => _randomScaleMin;
		public Vector3 RandomScaleMax => _randomScaleMax;
		public CurveApplyMode RandomScaleMode => _randomScaleMode;
		public bool PunchPositionEnabled => _punchPositionEnabled;
		public Vector3 PunchPositionIntensity => _punchPositionIntensity;
		public bool PunchRotationEnabled => _punchRotationEnabled;
		public Vector3 PunchRotationIntensity => _punchRotationIntensity;
		public bool PunchScaleEnabled => _punchScaleEnabled;
		public Vector3 PunchScaleIntensity => _punchScaleIntensity;
	}
}
