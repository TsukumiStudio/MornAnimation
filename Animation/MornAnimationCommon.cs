using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace MornLib
{
    /// <summary>
    /// MornAnimationSettingsを1つ持ち、Show/Hideでアニメーションする。
    /// Show時の目標値（ShowAlpha, ShowPosition等）はこちらが保持する。
    /// </summary>
    public sealed class MornAnimationCommon : MornAnimationBase
    {
        private enum FadeTarget
        {
            CanvasGroup,
            Image,
        }

        [SerializeField] private MornAnimationSettings _settings;
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private FadeTarget _fadeTarget;
        [SerializeField, ShowIf(nameof(IsCanvasGroup))] private CanvasGroup _canvasGroup;
        [SerializeField, ShowIf(nameof(IsImage))] private Image _image;

        [Header("Show State")]
        [SerializeField] private float _showAlpha = 1f;
        [SerializeField] private Vector3 _showPosition;
        [SerializeField] private Vector3 _showScale = Vector3.one;
        [SerializeField] private Vector3 _showRotation;

        private CancellationTokenSource _cts;

        private bool IsCanvasGroup => _fadeTarget == FadeTarget.CanvasGroup;
        private bool IsImage => _fadeTarget == FadeTarget.Image;

        /// <summary>現在の座標・Scale・Rotation・Alphaを ShowState に取り込む。</summary>
        [Button("現在の状態をShowStateに設定")]
        public void CaptureCurrentAsShowState()
        {
            if (_rectTransform != null)
            {
                _showPosition = _rectTransform.anchoredPosition;
                _showScale = _rectTransform.localScale;
                _showRotation = _rectTransform.localEulerAngles;
            }
            _showAlpha = GetCurrentAlpha();
            MornAnimationUtil.SetDirty(this);
        }

        public override UniTask ShowAsync(CancellationToken ct = default)
        {
            if (_settings == null) return UniTask.CompletedTask;
            _cts?.Cancel();
            _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            return PlayAsync(_settings, true, _cts.Token);
        }

        public override UniTask HideAsync(CancellationToken ct = default)
        {
            if (_settings == null) return UniTask.CompletedTask;
            _cts?.Cancel();
            _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            return PlayAsync(_settings, false, _cts.Token);
        }

        [Button]
        public override void DebugInitialize()
        {
            if (_settings == null) return;
            ApplyImmediate(_settings, false);
            MornAnimationUtil.SetDirty(this);
        }

        private float GetCurrentAlpha()
        {
            return _fadeTarget switch
            {
                FadeTarget.CanvasGroup when _canvasGroup != null => _canvasGroup.alpha,
                FadeTarget.Image when _image != null => _image.color.a,
                _ => 1f,
            };
        }

        private void SetAlpha(float alpha)
        {
            switch (_fadeTarget)
            {
                case FadeTarget.CanvasGroup when _canvasGroup != null:
                    _canvasGroup.alpha = alpha;
                    break;
                case FadeTarget.Image when _image != null:
                    var c = _image.color;
                    c.a = alpha;
                    _image.color = c;
                    break;
            }
        }

        private async UniTask PlayAsync(MornAnimationSettings s, bool toShow, CancellationToken token)
        {
            var time = s.TimeSettings != null ? s.TimeSettings : MornAnimationGlobal.I.TimeSettings;
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
                tasks.Add(FadeAsync(s, toShow, duration, easeType, token));
            }
            if (s.MoveEnabled)
            {
                tasks.Add(TransformAsync(
                    toShow, duration, easeType, token,
                    _showPosition, s.HidePositionOffset,
                    s.HasSpawnPositionOffset, s.SpawnPositionOffset,
                    () => _rectTransform.anchoredPosition,
                    v => _rectTransform.anchoredPosition = v));
            }
            if (s.ScaleEnabled)
            {
                tasks.Add(TransformAsync(
                    toShow, duration, easeType, token,
                    _showScale, s.HideScaleOffset,
                    false, Vector3.zero,
                    () => _rectTransform.localScale,
                    v => _rectTransform.localScale = v));
            }
            if (s.RotateEnabled)
            {
                tasks.Add(TransformAsync(
                    toShow, duration, easeType, token,
                    _showRotation, s.HideRotateOffset,
                    false, Vector3.zero,
                    () => _rectTransform.localEulerAngles,
                    v => _rectTransform.localEulerAngles = v));
            }

            await UniTask.WhenAll(tasks);
        }

        private async UniTask FadeAsync(MornAnimationSettings s, bool toShow, float duration, MornEaseType easeType, CancellationToken token)
        {
            var startAlpha = GetCurrentAlpha();
            var endAlpha = toShow ? _showAlpha : s.HideAlpha;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                token.ThrowIfCancellationRequested();
                elapsed += MornAnimationUtil.GetDeltaTime();
                var t = Mathf.Clamp01(elapsed / duration).Ease(easeType);
                SetAlpha(Mathf.LerpUnclamped(startAlpha, endAlpha, t));
                await MornAnimationUtil.WaitNextFrame(token);
            }
            SetAlpha(endAlpha);
        }

        private async UniTask TransformAsync(
            bool toShow, float duration, MornEaseType easeType, CancellationToken token,
            Vector3 showValue, Vector3 hideOffset,
            bool hasSpawnOffset, Vector3 spawnOffset,
            System.Func<Vector3> getter, System.Action<Vector3> setter)
        {
            if (_rectTransform == null) return;
            var hideValue = showValue + hideOffset;
            var spawnValue = showValue + spawnOffset;
            var startValue = toShow && hasSpawnOffset ? spawnValue : getter();
            var endValue = toShow ? showValue : hideValue;
            if (toShow && hasSpawnOffset) setter(startValue);
            var elapsed = 0f;
            while (elapsed < duration)
            {
                token.ThrowIfCancellationRequested();
                elapsed += MornAnimationUtil.GetDeltaTime();
                var t = Mathf.Clamp01(elapsed / duration).Ease(easeType);
                setter(Vector3.LerpUnclamped(startValue, endValue, t));
                await MornAnimationUtil.WaitNextFrame(token);
            }
            setter(endValue);
        }

        private void ApplyImmediate(MornAnimationSettings s, bool show)
        {
            if (s.FadeEnabled)
            {
                SetAlpha(show ? _showAlpha : s.HideAlpha);
            }
            if (s.MoveEnabled && _rectTransform != null)
            {
                _rectTransform.anchoredPosition = show ? _showPosition : _showPosition + s.HidePositionOffset;
            }
            if (s.ScaleEnabled && _rectTransform != null)
            {
                _rectTransform.localScale = show ? _showScale : _showScale + s.HideScaleOffset;
            }
            if (s.RotateEnabled && _rectTransform != null)
            {
                _rectTransform.localEulerAngles = show ? _showRotation : _showRotation + s.HideRotateOffset;
            }
        }
    }
}
