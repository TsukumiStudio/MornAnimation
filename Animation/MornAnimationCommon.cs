using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MornLib
{
    /// <summary>
    /// MornAnimationSettingsを1つ持ち、Show/Hideでアニメーションする。
    /// Show時の目標値（ShowAlpha, ShowPosition等）はこちらが保持する。
    /// </summary>
    public sealed class MornAnimationCommon : MornAnimationBase
    {
        [SerializeField] private MornAnimationSettings _settings;
        [SerializeField] private MornAnimationMoveTargetModule _moveTarget = new();
        [SerializeField] private MornAnimationFadeTargetModule _fadeTarget = new();

        [Header("Show State")]
        [SerializeField] private float _showAlpha = 1f;
        [SerializeField] private Vector3 _showPosition;
        [SerializeField] private Vector3 _showScale = Vector3.one;
        [SerializeField] private Vector3 _showRotation;

        private CancellationTokenSource _cts;

        /// <summary>現在の座標・Scale・Rotation・Alphaを ShowState に取り込む。</summary>
        [Button("現在の状態をShowStateに設定")]
        public void CaptureCurrentAsShowState()
        {
            MornAnimationUtil.RecordUndo(this, "CaptureCurrentAsShowState");
            _showPosition = _moveTarget.GetPosition(transform);
            _showScale = transform.localScale;
            _showRotation = transform.localEulerAngles;
            _showAlpha = _fadeTarget.GetAlpha();
            MornAnimationUtil.SetDirty(this);
        }

        public override async UniTask ShowAsync(CancellationToken ct = default)
        {
            if (_settings == null) return;
            _cts?.Cancel();
            _cts = CancellationTokenSource.CreateLinkedTokenSource(ct, destroyCancellationToken);
            await PlayAsync(_settings, true, _cts.Token);
            SetCanvasGroupActive(true);
        }

        public override UniTask HideAsync(CancellationToken ct = default)
        {
            if (_settings == null) return UniTask.CompletedTask;
            SetCanvasGroupActive(false);
            _cts?.Cancel();
            _cts = CancellationTokenSource.CreateLinkedTokenSource(ct, destroyCancellationToken);
            return PlayAsync(_settings, false, _cts.Token);
        }

        [Button]
        public override void DebugInitialize()
        {
            if (_settings == null) return;
            ApplyImmediate(_settings, false);
            MornAnimationUtil.SetDirty(this);
        }

        [Button]
        public async UniTask DebugShow()
        {
            await ShowAsync();
            MornAnimationUtil.SetDirty(this);
        }

        [Button]
        public async UniTask DebugHide()
        {
            await HideAsync();
            MornAnimationUtil.SetDirty(this);
        }

        private void Awake()
        {
            _showPosition += _moveTarget.ApplyAutoPivotCenter();
            if (_settings != null) ApplyImmediate(_settings, false);
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _moveTarget = new MornAnimationMoveTargetModule();
            _moveTarget.ResetFromGameObject(gameObject);
            _showPosition = _moveTarget.GetPosition(transform);
            _showScale = transform.localScale;
            _showRotation = transform.localEulerAngles;

            _fadeTarget = new MornAnimationFadeTargetModule();
            _fadeTarget.ResetFromGameObject(gameObject);
        }
#endif

        private async UniTask PlayAsync(MornAnimationSettings s, bool toShow, CancellationToken token)
        {
            var time = s.TimeSettings;
            var duration = toShow ? time.ShowDuration : time.HideDuration;
            var delay = toShow ? time.ShowDelay : time.HideDelay;
            var easeType = toShow ? time.ShowEaseType : time.HideEaseType;

            if (delay > 0f)
            {
                await MornAnimationUtil.WaitSeconds(delay, token);
            }

            var tasks = new List<UniTask>();

            if (s.FadeEnabled)
            {
                tasks.Add(FadeAsync(toShow, duration, easeType, token));
            }
            if (s.MoveEnabled)
            {
                tasks.Add(TransformAsync(
                    toShow, duration, easeType, token,
                    _showPosition, _showPosition + s.HidePositionOffset,
                    s.SpawnPositionOffsetEnabled, _showPosition + s.SpawnPositionOffset,
                    () => _moveTarget.GetPosition(transform),
                    v => _moveTarget.SetPosition(transform, v)));
            }
            if (s.ScaleEnabled)
            {
                tasks.Add(TransformAsync(
                    toShow, duration, easeType, token,
                    _showScale, Vector3.Scale(_showScale, s.HideScaleMultiply),
                    s.SpawnScaleMultiplyEnabled, Vector3.Scale(_showScale, s.SpawnScaleMultiply),
                    () => transform.localScale,
                    v => transform.localScale = v));
            }
            if (s.RotateEnabled)
            {
                tasks.Add(TransformAsync(
                    toShow, duration, easeType, token,
                    _showRotation, _showRotation + s.HideRotateOffset,
                    s.SpawnRotateOffsetEnabled, _showRotation + s.SpawnRotateOffset,
                    () => transform.localEulerAngles,
                    v => transform.localEulerAngles = v));
            }

            await UniTask.WhenAll(tasks);
        }

        private async UniTask FadeAsync(bool toShow, float duration, MornEaseType easeType, CancellationToken token)
        {
            var startAlpha = _fadeTarget.GetAlpha();
            var endAlpha = toShow ? _showAlpha : 0f;
            try
            {
                var elapsed = 0f;
                while (elapsed < duration)
                {
                    token.ThrowIfCancellationRequested();
                    if (this == null) return;
                    elapsed += MornAnimationUtil.GetDeltaTime();
                    var t = Mathf.Clamp01(elapsed / duration).Ease(easeType);
                    _fadeTarget.SetAlpha(Mathf.LerpUnclamped(startAlpha, endAlpha, t));
                    await MornAnimationUtil.WaitNextFrame(token);
                }
            }
            finally
            {
                if (this != null) _fadeTarget.SetAlpha(endAlpha);
            }
        }

        private async UniTask TransformAsync(
            bool toShow, float duration, MornEaseType easeType, CancellationToken token,
            Vector3 showValue, Vector3 hideValue,
            bool hasSpawnValue, Vector3 spawnValue,
            System.Func<Vector3> getter, System.Action<Vector3> setter)
        {
            var startValue = toShow && hasSpawnValue ? spawnValue : getter();
            var endValue = toShow ? showValue : hideValue;
            if (toShow && hasSpawnValue) setter(startValue);
            try
            {
                var elapsed = 0f;
                while (elapsed < duration)
                {
                    token.ThrowIfCancellationRequested();
                    if (this == null) return;
                    elapsed += MornAnimationUtil.GetDeltaTime();
                    var t = Mathf.Clamp01(elapsed / duration).Ease(easeType);
                    setter(Vector3.LerpUnclamped(startValue, endValue, t));
                    await MornAnimationUtil.WaitNextFrame(token);
                }
            }
            finally
            {
                if (this != null) setter(endValue);
            }
        }

        private void SetCanvasGroupActive(bool active)
        {
            if (!_fadeTarget.IsCanvasGroup) return;
            MornAnimationUtil.SetCanvasGroupActive(_fadeTarget.CanvasGroup, active);
        }

        private void ApplyImmediate(MornAnimationSettings s, bool show)
        {
            if (s.FadeEnabled)
            {
                _fadeTarget.SetAlpha(show ? _showAlpha : 0f);
            }
            if (s.MoveEnabled)
            {
                _moveTarget.SetPosition(transform, show ? _showPosition : _showPosition + s.HidePositionOffset);
            }
            if (s.ScaleEnabled)
            {
                transform.localScale = show ? _showScale : Vector3.Scale(_showScale, s.HideScaleMultiply);
            }
            if (s.RotateEnabled)
            {
                transform.localEulerAngles = show ? _showRotation : _showRotation + s.HideRotateOffset;
            }
            SetCanvasGroupActive(show);
        }
    }
}
