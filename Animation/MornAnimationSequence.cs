using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MornLib
{
	internal sealed class MornAnimationSequence : MornAnimationBase
	{
		[SerializeField] private List<MornAnimationBase> _targets;
		[SerializeField] private float _showInterval;
		[SerializeField] private float _hideInterval;
		[SerializeField] private float _showDelay;
		[SerializeField] private float _hideDelay;
		[SerializeField] private bool _hideReverse;
		[SerializeField] private bool _isWaitEach;
		private CancellationTokenSource _cts;

		public override async UniTask ShowAsync(CancellationToken ct = default)
		{
			await SequenceAsync(true, ct);
		}

		public override async UniTask HideAsync(CancellationToken ct = default)
		{
			await SequenceAsync(false, ct);
		}

		private async UniTask SequenceAsync(bool toShow, CancellationToken ct = default)
		{
			_cts?.Cancel();
			_cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
			ct = _cts.Token;
			var delay = toShow ? _showDelay : _hideDelay;
			if (delay > 0f)
			{
				await MornAnimationUtil.WaitSeconds(delay, ct);
			}

			var taskList = new List<UniTask>();
			var targets = !toShow && _hideReverse ? _targets.AsReadOnly().Reverse() : _targets;
			foreach (var target in targets)
			{
				if (toShow)
				{
					taskList.Add(target.ShowAsync(ct));
					if (_isWaitEach)
					{
						await UniTask.WhenAll(taskList);
						taskList.Clear();
					}

					if (_showInterval > 0f)
					{
						await MornAnimationUtil.WaitSeconds(_showInterval, ct);
					}
				}
				else
				{
					taskList.Add(target.HideAsync(ct));

					if (_isWaitEach)
					{
						await UniTask.WhenAll(taskList);
						taskList.Clear();
					}

					if (_hideInterval > 0f)
					{
						await MornAnimationUtil.WaitSeconds(_hideInterval, ct);
					}
				}
			}

			await UniTask.WhenAll(taskList);
		}

		[Button]
		public override void DebugInitialize()
		{
			foreach (var target in _targets)
			{
				target.DebugInitialize();
			}
		}

		[Button]
		public async UniTask DebugShow()
		{
			await ShowAsync();
			MarkAllDirty();
		}

		[Button]
		public async UniTask DebugHide()
		{
			await HideAsync();
			MarkAllDirty();
		}

		private void MarkAllDirty()
		{
			foreach (var target in _targets)
			{
				MornAnimationUtil.SetDirty(target);
			}

			MornAnimationUtil.SetDirty(this);
		}
	}
}