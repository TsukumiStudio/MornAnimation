using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MornLib
{
	public abstract class MornAnimationBase : MonoBehaviour
	{
		[SerializeField] private bool _excludeFromAutoCollect;

		/// <summary>自動取得(CollectChildAnimations)の対象外にするか。</summary>
		public bool ExcludeFromAutoCollect => _excludeFromAutoCollect;

		public abstract UniTask ShowAsync(CancellationToken ct = default);
		public abstract UniTask HideAsync(CancellationToken ct = default);
		public abstract void DebugInitialize();

		/// <summary>UnityEvent用。引数なしでShowAsyncを実行する。</summary>
		public void Show() => ShowAsync().Forget();

		/// <summary>UnityEvent用。引数なしでHideAsyncを実行する。</summary>
		public void Hide() => HideAsync().Forget();
	}
}
