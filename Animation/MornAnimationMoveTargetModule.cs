using System;
using UnityEngine;

namespace MornLib
{
    /// <summary>
    /// Move対象の Transform / RectTransform を保持する共通モジュール。
    /// </summary>
    [Serializable]
    public sealed class MornAnimationMoveTargetModule
    {
        public enum TargetType
        {
            RectTransform,
            Transform,
        }

        [SerializeField] private TargetType _type;
        [SerializeField, ShowIf(nameof(IsRectTransform))] private RectTransform _rectTransform;
        [SerializeField, ShowIf(nameof(IsRectTransform))] private bool _autoPivotCenter = true;

        public bool IsRectTransform => _type == TargetType.RectTransform;
        public TargetType Type => _type;
        public RectTransform RectTransform => _rectTransform;

#if UNITY_EDITOR
        /// <summary>MonoBehaviour.Reset() からのみ呼ぶこと (Editor専用)。</summary>
        public void ResetFromGameObject(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>();
            if (rt != null)
            {
                _type = TargetType.RectTransform;
                _rectTransform = rt;
            }
            else
            {
                _type = TargetType.Transform;
                _rectTransform = null;
            }
        }
#endif

        public Vector3 GetPosition(Transform fallback)
        {
            return _type switch
            {
                TargetType.RectTransform => (Vector3)_rectTransform.anchoredPosition,
                TargetType.Transform => fallback.localPosition,
                _ => Vector3.zero,
            };
        }

        public void SetPosition(Transform fallback, Vector3 value)
        {
            switch (_type)
            {
                case TargetType.RectTransform:
                    _rectTransform.anchoredPosition = value;
                    break;
                case TargetType.Transform:
                    fallback.localPosition = value;
                    break;
            }
        }

        /// <summary>RectTransform の Pivot を中央に補正し、結果として生じる anchoredPosition のずれを返す。</summary>
        public Vector3 ApplyAutoPivotCenter()
        {
            if (!_autoPivotCenter || _type != TargetType.RectTransform || _rectTransform == null) return Vector3.zero;
            var oldPivot = _rectTransform.pivot;
            var newPivot = new Vector2(0.5f, 0.5f);
            var delta = newPivot - oldPivot;
            var size = _rectTransform.rect.size;
            var offset = new Vector2(delta.x * size.x, delta.y * size.y);
            _rectTransform.pivot = newPivot;
            _rectTransform.anchoredPosition += offset;
            return offset;
        }
    }
}
