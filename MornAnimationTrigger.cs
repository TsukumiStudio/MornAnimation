using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MornLib
{
    /// <summary>
    /// MornAnimationBase/FxのShow/Playを自動または手動で発火するトリガー。
    /// _isAutoOnEnabled=trueの場合、OnEnableでShowやPlayを呼ぶ。
    /// 2回目の発火時に前回の再生を自動キャンセルする。
    /// </summary>
    public sealed class MornAnimationTrigger : MonoBehaviour
    {
        [SerializeField] private MornAnimationBase _animation;
        [SerializeField] private MornAnimationFx _fx;
        [SerializeField] private bool _isAutoOnEnabled = true;

        private CancellationTokenSource _cts;

        private void Reset()
        {
            _animation = GetComponent<MornAnimationBase>();
            _fx = GetComponent<MornAnimationFx>();
        }

        private void OnEnable()
        {
            if (!_isAutoOnEnabled) return;
            Fire();
        }

        public void Fire()
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var ct = _cts.Token;
            if (_animation != null)
            {
                _animation.DebugInitialize();
                _animation.ShowAsync(ct).Forget();
            }
            if (_fx != null) _fx.PlayAsync(ct).Forget();
        }
    }
}
