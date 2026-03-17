using UnityEngine;

namespace MornLib
{
    /// <summary>
    /// アニメーションモーションの定義。ScriptableObjectとして管理し、
    /// MornAnimationCommonのリストに積んで使う。
    /// ターゲットの参照は持たず、相対的な変化量とタイミングを定義する。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(MornAnimationSettings), menuName = "Morn/" + nameof(MornAnimationSettings))]
    public sealed class MornAnimationSettings : ScriptableObject
    {
        public enum MotionType
        {
            Fade,
            Move,
            Scale,
            Rotate,
        }

        [SerializeField] private MotionType _motionType;
        [SerializeField] private MornAnimationTimeSettings _timeSettings;

        [Header("Fade (MotionType=Fade)")]
        [SerializeField] private float _showAlpha = 1f;
        [SerializeField] private float _hideAlpha;

        [Header("Transform (MotionType=Move/Scale/Rotate)")]
        [SerializeField] private Vector3 _showValue;
        [SerializeField] private Vector3 _hideOffset;
        [SerializeField] private bool _hasSpawnOffset;
        [SerializeField] private Vector3 _spawnOffset;

        public MotionType Type => _motionType;
        public MornAnimationTimeSettings TimeSettings => _timeSettings;
        public float ShowAlpha => _showAlpha;
        public float HideAlpha => _hideAlpha;
        public Vector3 ShowValue => _showValue;
        public Vector3 HideOffset => _hideOffset;
        public bool HasSpawnOffset => _hasSpawnOffset;
        public Vector3 SpawnOffset => _spawnOffset;
        public Vector3 HideValue => _showValue + _hideOffset;
        public Vector3 SpawnValue => _showValue + _spawnOffset;
    }
}
