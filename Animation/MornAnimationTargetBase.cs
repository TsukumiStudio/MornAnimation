using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace MornLib
{
	public abstract class MornAnimationTargetBase : MornAnimationBase
	{
		private CancellationTokenSource _cts;
		protected abstract List<MornAnimationModuleBase> GetModules();

		private void Awake()
		{
			foreach (var module in GetModules())
			{
				module.OnAwake(this);
			}
		}

		private void OnValidate()
		{
			foreach (var module in GetModules())
			{
				module.OnValidate(this);
			}
		}

		public override UniTask ShowAsync(CancellationToken ct = default)
		{
			_cts?.Cancel();
			_cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
			ct = _cts.Token;
			var tasks = new List<UniTask>();
			foreach (var module in GetModules())
			{
				tasks.Add(module.ShowAsync(ct));
			}

			return UniTask.WhenAll(tasks);
		}

		public override UniTask HideAsync(CancellationToken ct = default)
		{
			_cts?.Cancel();
			_cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
			ct = _cts.Token;
			var tasks = new List<UniTask>();
			foreach (var module in GetModules())
			{
				tasks.Add(module.HideAsync(ct));
			}

			return UniTask.WhenAll(tasks);
		}

		[Button]
		public override void DebugInitialize()
		{
			foreach (var module in GetModules())
			{
				module.OnInitialize();
			}

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
	}
}