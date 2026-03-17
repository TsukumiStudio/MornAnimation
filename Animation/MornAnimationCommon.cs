using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace MornLib
{
    /// <summary>
    /// MornAnimationSettingsをリストで持ち、Show/Hideでまとめてアニメーションする。
    /// 自分自身のGameObjectのコンポーネント（CanvasGroup, RectTransform等）を対象とする。
    /// </summary>
    public sealed class MornAnimationCommon : MornAnimationBase
    {
        [SerializeField] private List<MornAnimationSettings> _settings = new();
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _rectTransform;

        private CancellationTokenSource _cts;

        public override UniTask ShowAsync(CancellationToken ct = default)
        {
            _cts?.Cancel();
            _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            var token = _cts.Token;
            var tasks = new List<UniTask>();
            foreach (var s in _settings)
            {
                if (s == null) continue;
                tasks.Add(PlayAsync(s, true, token));
            }
            return UniTask.WhenAll(tasks);
        }

        public override UniTask HideAsync(CancellationToken ct = default)
        {
            _cts?.Cancel();
            _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            var token = _cts.Token;
            var tasks = new List<UniTask>();
            foreach (var s in _settings)
            {
                if (s == null) continue;
                tasks.Add(PlayAsync(s, false, token));
            }
            return UniTask.WhenAll(tasks);
        }

        [Button]
        public override void DebugInitialize()
        {
            foreach (var s in _settings)
            {
                if (s == null) continue;
                ApplyImmediate(s, false);
            }
            MornAnimationUtil.SetDirty(this);
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

            switch (s.Type)
            {
                case MornAnimationSettings.MotionType.Fade:
                    await FadeAsync(s, toShow, duration, easeType, token);
                    break;
                case MornAnimationSettings.MotionType.Move:
                    await TransformAsync(s, toShow, duration, easeType, token,
                        () => _rectTransform.anchoredPosition,
                        v => _rectTransform.anchoredPosition = v);
                    break;
                case MornAnimationSettings.MotionType.Scale:
                    await TransformAsync(s, toShow, duration, easeType, token,
                        () => _rectTransform.localScale,
                        v => _rectTransform.localScale = v);
                    break;
                case MornAnimationSettings.MotionType.Rotate:
                    await TransformAsync(s, toShow, duration, easeType, token,
                        () => _rectTransform.localEulerAngles,
                        v => _rectTransform.localEulerAngles = v);
                    break;
            }
        }

        private async UniTask FadeAsync(MornAnimationSettings s, bool toShow, float duration, MornEaseType easeType, CancellationToken token)
        {
            if (_canvasGroup == null) return;
            var startAlpha = _canvasGroup.alpha;
            var endAlpha = toShow ? s.ShowAlpha : s.HideAlpha;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                token.ThrowIfCancellationRequested();
                elapsed += MornAnimationUtil.GetDeltaTime();
                var t = Mathf.Clamp01(elapsed / duration).Ease(easeType);
                _canvasGroup.alpha = Mathf.LerpUnclamped(startAlpha, endAlpha, t);
                await MornAnimationUtil.WaitNextFrame(token);
            }
            _canvasGroup.alpha = endAlpha;
        }

        private async UniTask TransformAsync(MornAnimationSettings s, bool toShow, float duration, MornEaseType easeType, CancellationToken token,
            System.Func<Vector3> getter, System.Action<Vector3> setter)
        {
            if (_rectTransform == null) return;
            var startValue = toShow && s.HasSpawnOffset ? s.SpawnValue : getter();
            var endValue = toShow ? s.ShowValue : s.HideValue;
            if (toShow && s.HasSpawnOffset) setter(startValue);
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
            switch (s.Type)
            {
                case MornAnimationSettings.MotionType.Fade:
                    if (_canvasGroup != null) _canvasGroup.alpha = show ? s.ShowAlpha : s.HideAlpha;
                    break;
                case MornAnimationSettings.MotionType.Move:
                    if (_rectTransform != null) _rectTransform.anchoredPosition = show ? s.ShowValue : s.HideValue;
                    break;
                case MornAnimationSettings.MotionType.Scale:
                    if (_rectTransform != null) _rectTransform.localScale = show ? s.ShowValue : s.HideValue;
                    break;
                case MornAnimationSettings.MotionType.Rotate:
                    if (_rectTransform != null) _rectTransform.localEulerAngles = show ? s.ShowValue : s.HideValue;
                    break;
            }
        }
    }
}
