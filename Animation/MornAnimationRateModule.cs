using System;
using UnityEngine;

namespace MornLib
{
    /// <summary>
    /// 0~1 の rate を Curve または Ease で評価する共通モジュール。
    /// </summary>
    [Serializable]
    public sealed class MornAnimationRateModule
    {
        public enum RateType
        {
            Curve,
            Ease,
        }

        [SerializeField] private RateType _type;
        [SerializeField, ShowIf(nameof(IsCurve))] private AnimationCurve _curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField, ShowIf(nameof(IsEase))] private MornEaseType _easeType = MornEaseType.Linear;
        [SerializeField, ShowIf(nameof(IsEase))] private bool _easeOneMinus;

        public bool IsCurve => _type == RateType.Curve;
        public bool IsEase => _type == RateType.Ease;
        public RateType Type => _type;

        /// <summary>rate (0~1) を評価する。</summary>
        public float Evaluate(float rate)
        {
            return _type switch
            {
                RateType.Curve => _curve.Evaluate(rate),
                RateType.Ease => _easeOneMinus
                    ? 1f - Mathf.Clamp01(rate).Ease(_easeType)
                    : Mathf.Clamp01(rate).Ease(_easeType),
                _ => rate,
            };
        }
    }
}
