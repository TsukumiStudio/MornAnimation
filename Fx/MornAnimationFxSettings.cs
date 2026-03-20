using UnityEngine;

namespace MornLib
{
	/// <summary>
	/// エフェクトアニメーションの定義。Shake系のパラメータを持つ。
	/// </summary>
	[CreateAssetMenu(fileName = nameof(MornAnimationFxSettings), menuName = "Morn/Animation/" + nameof(MornAnimationFxSettings))]
	public sealed class MornAnimationFxSettings : ScriptableObject
	{
		[SerializeField] private float _duration = 0.3f;
		[SerializeField] private MornEaseType _easeType = MornEaseType.EaseOutQuart;
		[SerializeField, Tooltip("1秒あたりの振動回数")]
		private float _vibration = 20f;

		[Header("Shake Position")]
		[SerializeField] private bool _shakePositionEnabled;
		[SerializeField, ShowIf(nameof(_shakePositionEnabled))] private Vector3 _shakePositionIntensity = new(10f, 10f, 0f);

		[Header("Shake Rotation")]
		[SerializeField] private bool _shakeRotationEnabled;
		[SerializeField, ShowIf(nameof(_shakeRotationEnabled))] private Vector3 _shakeRotationIntensity = new(0f, 0f, 15f);

		[Header("Shake Scale")]
		[SerializeField] private bool _shakeScaleEnabled;
		[SerializeField, ShowIf(nameof(_shakeScaleEnabled))] private Vector3 _shakeScaleIntensity = new(0.1f, 0.1f, 0f);

		public float Duration => _duration;
		public MornEaseType EaseType => _easeType;
		public float Vibration => _vibration;
		public bool ShakePositionEnabled => _shakePositionEnabled;
		public Vector3 ShakePositionIntensity => _shakePositionIntensity;
		public bool ShakeRotationEnabled => _shakeRotationEnabled;
		public Vector3 ShakeRotationIntensity => _shakeRotationIntensity;
		public bool ShakeScaleEnabled => _shakeScaleEnabled;
		public Vector3 ShakeScaleIntensity => _shakeScaleIntensity;
	}
}
