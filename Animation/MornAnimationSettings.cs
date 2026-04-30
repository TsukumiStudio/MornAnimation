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
        [SerializeField, Label("TimeSettings (Override)")]
        private MornAnimationTimeSettings _timeSettings;

        [Header("Fade")]
        [SerializeField] private bool _fadeEnabled;

        [Header("Move")]
        [SerializeField] private bool _moveEnabled;
        [SerializeField, ShowIf(nameof(_moveEnabled))] private Vector3 _hidePositionOffset;
        [SerializeField, ShowIf(nameof(_moveEnabled))] private bool _spawnPositionOffsetEnabled;
        [SerializeField, ShowIf(nameof(_spawnPositionOffsetEnabled))] private Vector3 _spawnPositionOffset;

        [Header("Scale")]
        [SerializeField] private bool _scaleEnabled;
        [SerializeField, ShowIf(nameof(_scaleEnabled))] private Vector3 _hideScaleMultiply = Vector3.one;
        [SerializeField, ShowIf(nameof(_scaleEnabled))] private bool _spawnScaleMultiplyEnabled;
        [SerializeField, ShowIf(nameof(_spawnScaleMultiplyEnabled))] private Vector3 _spawnScaleMultiply = Vector3.one;

        [Header("Rotate")]
        [SerializeField] private bool _rotateEnabled;
        [SerializeField, ShowIf(nameof(_rotateEnabled))] private Vector3 _hideRotateOffset;
        [SerializeField, ShowIf(nameof(_rotateEnabled))] private bool _spawnRotateOffsetEnabled;
        [SerializeField, ShowIf(nameof(_spawnRotateOffsetEnabled))] private Vector3 _spawnRotateOffset;

        public MornAnimationTimeSettings TimeSettings => _timeSettings != null ? _timeSettings : MornAnimationGlobal.I.TimeSettings;
        public bool FadeEnabled => _fadeEnabled;
        public bool MoveEnabled => _moveEnabled;
        public Vector3 HidePositionOffset => _hidePositionOffset;
        public bool SpawnPositionOffsetEnabled => _spawnPositionOffsetEnabled;
        public Vector3 SpawnPositionOffset => _spawnPositionOffset;
        public bool ScaleEnabled => _scaleEnabled;
        public Vector3 HideScaleMultiply => _hideScaleMultiply;
        public bool SpawnScaleMultiplyEnabled => _spawnScaleMultiplyEnabled;
        public Vector3 SpawnScaleMultiply => _spawnScaleMultiply;
        public bool RotateEnabled => _rotateEnabled;
        public Vector3 HideRotateOffset => _hideRotateOffset;
        public bool SpawnRotateOffsetEnabled => _spawnRotateOffsetEnabled;
        public Vector3 SpawnRotateOffset => _spawnRotateOffset;
    }
}
