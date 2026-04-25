using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MornLib
{
    /// <summary>
    /// Fade対象 (CanvasGroup / Image / TMP_Text / SpriteRenderer) を保持する共通モジュール。
    /// </summary>
    [Serializable]
    public sealed class MornAnimationFadeTargetModule
    {
        public enum TargetType
        {
            CanvasGroup,
            Image,
            TMP_Text,
            SpriteRenderer,
        }

        [SerializeField] private TargetType _type;
        [SerializeField, ShowIf(nameof(IsCanvasGroup))] private CanvasGroup _canvasGroup;
        [SerializeField, ShowIf(nameof(IsImage))] private Image _image;
        [SerializeField, ShowIf(nameof(IsTMPText))] private TMP_Text _tmpText;
        [SerializeField, ShowIf(nameof(IsSpriteRenderer))] private SpriteRenderer _spriteRenderer;

        public bool IsCanvasGroup => _type == TargetType.CanvasGroup;
        public bool IsImage => _type == TargetType.Image;
        public bool IsTMPText => _type == TargetType.TMP_Text;
        public bool IsSpriteRenderer => _type == TargetType.SpriteRenderer;
        public TargetType Type => _type;
        public CanvasGroup CanvasGroup => _canvasGroup;

#if UNITY_EDITOR
        /// <summary>MonoBehaviour.Reset() からのみ呼ぶこと (Editor専用)。</summary>
        public void ResetFromGameObject(GameObject go)
        {
            var cg = go.GetComponent<CanvasGroup>();
            var img = go.GetComponent<Image>();
            var tmp = go.GetComponent<TMP_Text>();
            var sr = go.GetComponent<SpriteRenderer>();
            if (cg != null)
            {
                _type = TargetType.CanvasGroup;
                _canvasGroup = cg;
            }
            else if (img != null)
            {
                _type = TargetType.Image;
                _image = img;
            }
            else if (tmp != null)
            {
                _type = TargetType.TMP_Text;
                _tmpText = tmp;
            }
            else if (sr != null)
            {
                _type = TargetType.SpriteRenderer;
                _spriteRenderer = sr;
            }
        }
#endif

        public float GetAlpha()
        {
            return _type switch
            {
                TargetType.CanvasGroup when _canvasGroup != null => _canvasGroup.alpha,
                TargetType.Image when _image != null => _image.color.a,
                TargetType.TMP_Text when _tmpText != null => _tmpText.color.a,
                TargetType.SpriteRenderer when _spriteRenderer != null => _spriteRenderer.color.a,
                _ => 1f,
            };
        }

        public void SetAlpha(float alpha)
        {
            switch (_type)
            {
                case TargetType.CanvasGroup when _canvasGroup != null:
                    _canvasGroup.alpha = alpha;
                    break;
                case TargetType.Image when _image != null:
                    var c = _image.color;
                    c.a = alpha;
                    _image.color = c;
                    break;
                case TargetType.TMP_Text when _tmpText != null:
                    var tc = _tmpText.color;
                    tc.a = alpha;
                    _tmpText.color = tc;
                    break;
                case TargetType.SpriteRenderer when _spriteRenderer != null:
                    var sc = _spriteRenderer.color;
                    sc.a = alpha;
                    _spriteRenderer.color = sc;
                    break;
            }
        }
    }
}
