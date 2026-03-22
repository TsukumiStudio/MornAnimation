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
		[SerializeField] private float _showInterval = 0.03f;
		[SerializeField] private float _hideInterval = 0.03f;
		[SerializeField] private float _showDelay;
		[SerializeField] private float _hideDelay;
		[SerializeField] private bool _hideReverse;
		[SerializeField] private bool _isWaitEach;

		public override async UniTask ShowAsync(CancellationToken ct = default)
		{
			await RunSequenceAsync(true, ct);
		}

		public override async UniTask HideAsync(CancellationToken ct = default)
		{
			await RunSequenceAsync(false, ct);
		}

		private async UniTask RunSequenceAsync(bool toShow, CancellationToken ct)
		{
			var delay = toShow ? _showDelay : _hideDelay;
			if (delay > 0f)
			{
				await MornAnimationUtil.WaitSeconds(delay, ct);
			}

			var taskList = new List<UniTask>();
			var targets = !toShow && _hideReverse ? _targets.AsReadOnly().Reverse() : _targets;
			var interval = toShow ? _showInterval : _hideInterval;

			foreach (var target in targets)
			{
				var task = toShow ? target.ShowAsync(ct) : target.HideAsync(ct);
				taskList.Add(task);

				if (_isWaitEach)
				{
					await UniTask.WhenAll(taskList);
					taskList.Clear();
				}

				if (interval > 0f)
				{
					await MornAnimationUtil.WaitSeconds(interval, ct);
				}
			}

			await UniTask.WhenAll(taskList);
		}

		/// <summary>除外判定用にtargetsを返す。</summary>
		internal IEnumerable<MornAnimationBase> GetTargetsForExclusion() => _targets;

		[Button("子孫オブジェクトから自動取得")]
		public void CollectFromChildren()
		{
			MornAnimationUtil.RecordUndo(this, "CollectFromChildren");
			_targets = MornAnimationUtil.CollectChildAnimations(this);
			MornAnimationUtil.SetDirty(this);
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
