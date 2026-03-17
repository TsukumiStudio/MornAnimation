using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MornLib
{
	public abstract class MornAnimationBase : MonoBehaviour
	{
		public abstract UniTask ShowAsync(CancellationToken ct = default);
		public abstract UniTask HideAsync(CancellationToken ct = default);
		public abstract void DebugInitialize();

		/// <summary>UnityEvent用。引数なしでShowAsyncを実行する。</summary>
		public void Show() => ShowAsync().Forget();

		/// <summary>UnityEvent用。引数なしでHideAsyncを実行する。</summary>
		public void Hide() => HideAsync().Forget();
	}
}
