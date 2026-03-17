using UnityEngine;

namespace MornLib
{
    /// <summary>
    /// アニメーションモーションの定義。ScriptableObjectとして管理し、
    /// MornAnimationCommonのリストに積んで使う。
    /// Show時の目標値はCommon側が持ち、こちらはHide時のオフセットとタイミングを定義する。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(MornAnimationSettings), menuName = "Morn/Animation/" + nameof(MornAnimationSettings))]
    public sealed class MornAnimationSettings : ScriptableObject
    {
        [SerializeField, Tooltip("未設定(null)の場合はMornAnimationGlobalのTimeSettingsを使用")]
        [Label("TimeSettings (Override)")]
        private MornAnimationTimeSettings _timeSettings;

        [Header("Fade")]
        [SerializeField] private bool _fadeEnabled;

        [Header("Move")]
        [SerializeField] private bool _moveEnabled;
        [SerializeField, ShowIf(nameof(_moveEnabled))] private Vector3 _hidePositionOffset;
        [SerializeField, ShowIf(nameof(_moveEnabled))] private bool _hasSpawnPositionOffset;
        [SerializeField, ShowIf(nameof(_hasSpawnPositionOffset))] private Vector3 _spawnPositionOffset;

        [Header("Scale")]
        [SerializeField] private bool _scaleEnabled;
        [SerializeField, ShowIf(nameof(_scaleEnabled))] private Vector3 _hideScaleOffset;

        [Header("Rotate")]
        [SerializeField] private bool _rotateEnabled;
        [SerializeField, ShowIf(nameof(_rotateEnabled))] private Vector3 _hideRotateOffset;

        public MornAnimationTimeSettings TimeSettings => _timeSettings != null ? _timeSettings : MornAnimationGlobal.I.TimeSettings;
        public bool FadeEnabled => _fadeEnabled;
        public bool MoveEnabled => _moveEnabled;
        public Vector3 HidePositionOffset => _hidePositionOffset;
        public bool HasSpawnPositionOffset => _hasSpawnPositionOffset;
        public Vector3 SpawnPositionOffset => _spawnPositionOffset;
        public bool ScaleEnabled => _scaleEnabled;
        public Vector3 HideScaleOffset => _hideScaleOffset;
        public bool RotateEnabled => _rotateEnabled;
        public Vector3 HideRotateOffset => _hideRotateOffset;
    }
}
