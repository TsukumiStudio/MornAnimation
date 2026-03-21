using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
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
            TMP_Text,
        }

        private enum MoveTarget
        {
            RectTransform,
            Transform,
        }

        [SerializeField] private MornAnimationSettings _settings;
        [SerializeField] private MoveTarget _moveTarget;
        [SerializeField, ShowIf(nameof(IsMoveRectTransform))] private RectTransform _rectTransform;
        [SerializeField, ShowIf(nameof(IsMoveRectTransform))] private bool _autoPivotCenter = true;
        [SerializeField] private FadeTarget _fadeTarget;
        [SerializeField, ShowIf(nameof(IsCanvasGroup))] private CanvasGroup _canvasGroup;
        [SerializeField, ShowIf(nameof(IsImage))] private Image _image;
        [SerializeField, ShowIf(nameof(IsTMPText))] private TMP_Text _tmpText;

        [Header("Show State")]
        [SerializeField] private float _showAlpha = 1f;
        [SerializeField] private Vector3 _showPosition;
        [SerializeField] private Vector3 _showScale = Vector3.one;
        [SerializeField] private Vector3 _showRotation;

        private CancellationTokenSource _cts;

        private bool IsMoveRectTransform => _moveTarget == MoveTarget.RectTransform;
        private bool IsCanvasGroup => _fadeTarget == FadeTarget.CanvasGroup;
        private bool IsImage => _fadeTarget == FadeTarget.Image;
        private bool IsTMPText => _fadeTarget == FadeTarget.TMP_Text;

        private void Awake()
        {
            if (_autoPivotCenter && _moveTarget == MoveTarget.RectTransform && _rectTransform != null)
            {
                var oldPivot = _rectTransform.pivot;
                var newPivot = new Vector2(0.5f, 0.5f);
                var delta = newPivot - oldPivot;
                var size = _rectTransform.rect.size;
                var offset = new Vector2(delta.x * size.x, delta.y * size.y);

                _rectTransform.pivot = newPivot;
                _rectTransform.anchoredPosition += offset;
                _showPosition += (Vector3)offset;
            }
            if (_settings != null) ApplyImmediate(_settings, false);
        }

        private void Reset()
        {
            // MoveTarget: RectTransform優先、なければTransform
            var rt = GetComponent<RectTransform>();
            if (rt != null)
            {
                _moveTarget = MoveTarget.RectTransform;
                _rectTransform = rt;
                _showPosition = rt.anchoredPosition;
                _showScale = rt.localScale;
                _showRotation = rt.localEulerAngles;
            }
            else
            {
                _moveTarget = MoveTarget.Transform;
                _rectTransform = null;
                _showPosition = transform.localPosition;
                _showScale = transform.localScale;
                _showRotation = transform.localEulerAngles;
            }

            // FadeTarget
            var cg = GetComponent<CanvasGroup>();
            var img = GetComponent<Image>();
            var tmp = GetComponent<TMP_Text>();
            if (cg != null)
            {
                _fadeTarget = FadeTarget.CanvasGroup;
                _canvasGroup = cg;
            }
            else if (img != null)
            {
                _fadeTarget = FadeTarget.Image;
                _image = img;
            }
            else if (tmp != null)
            {
                _fadeTarget = FadeTarget.TMP_Text;
                _tmpText = tmp;
            }
        }

        /// <summary>現在の座標・Scale・Rotation・Alphaを ShowState に取り込む。</summary>
        [Button("現在の状態をShowStateに設定")]
        public void CaptureCurrentAsShowState()
        {
            _showPosition = GetPosition();
            _showScale = transform.localScale;
            _showRotation = transform.localEulerAngles;
            _showAlpha = GetCurrentAlpha();
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

        private float GetCurrentAlpha()
        {
            return _fadeTarget switch
            {
                FadeTarget.CanvasGroup when _canvasGroup != null => _canvasGroup.alpha,
                FadeTarget.Image when _image != null => _image.color.a,
                FadeTarget.TMP_Text when _tmpText != null => _tmpText.color.a,
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
                case FadeTarget.TMP_Text when _tmpText != null:
                    var tc = _tmpText.color;
                    tc.a = alpha;
                    _tmpText.color = tc;
                    break;
            }
        }

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
                    _showPosition, s.HidePositionOffset,
                    s.SpawnPositionOffsetEnabled, s.SpawnPositionOffset,
                    () => GetPosition(),
                    v => SetPosition(v)));
            }
            if (s.ScaleEnabled)
            {
                tasks.Add(TransformAsync(
                    toShow, duration, easeType, token,
                    _showScale, s.HideScaleOffset,
                    false, Vector3.zero,
                    () => transform.localScale,
                    v => transform.localScale = v));
            }
            if (s.RotateEnabled)
            {
                tasks.Add(TransformAsync(
                    toShow, duration, easeType, token,
                    _showRotation, s.HideRotateOffset,
                    false, Vector3.zero,
                    () => transform.localEulerAngles,
                    v => transform.localEulerAngles = v));
            }

            await UniTask.WhenAll(tasks);
        }

        private async UniTask FadeAsync(bool toShow, float duration, MornEaseType easeType, CancellationToken token)
        {
            var startAlpha = GetCurrentAlpha();
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
                    SetAlpha(Mathf.LerpUnclamped(startAlpha, endAlpha, t));
                    await MornAnimationUtil.WaitNextFrame(token);
                }
            }
            finally
            {
                if (this != null) SetAlpha(endAlpha);
            }
        }

        private async UniTask TransformAsync(
            bool toShow, float duration, MornEaseType easeType, CancellationToken token,
            Vector3 showValue, Vector3 hideOffset,
            bool hasSpawnOffset, Vector3 spawnOffset,
            System.Func<Vector3> getter, System.Action<Vector3> setter)
        {
            var hideValue = showValue + hideOffset;
            var spawnValue = showValue + spawnOffset;
            var startValue = toShow && hasSpawnOffset ? spawnValue : getter();
            var endValue = toShow ? showValue : hideValue;
            if (toShow && hasSpawnOffset) setter(startValue);
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

        private void SetCanvasGroupActive(bool active)
        {
            if (!IsCanvasGroup) return;
            MornAnimationUtil.SetCanvasGroupActive(_canvasGroup, active);
        }

        private void ApplyImmediate(MornAnimationSettings s, bool show)
        {
            if (s.FadeEnabled)
            {
                SetAlpha(show ? _showAlpha : 0f);
            }
            if (s.MoveEnabled)
            {
                SetPosition(show ? _showPosition : _showPosition + s.HidePositionOffset);
            }
            if (s.ScaleEnabled)
            {
                transform.localScale = show ? _showScale : _showScale + s.HideScaleOffset;
            }
            if (s.RotateEnabled)
            {
                transform.localEulerAngles = show ? _showRotation : _showRotation + s.HideRotateOffset;
            }
            SetCanvasGroupActive(show);
        }
    }
}
