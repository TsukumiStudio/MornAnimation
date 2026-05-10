using System.Collections.Generic;
using System;
#if !USE_ARBOR
using MornLib;
#else
using Arbor;
#endif
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MornLib
{
	[Serializable]
#if !USE_ARBOR
	internal sealed class MornAnimationState : MornStateBehaviour
#else
	internal sealed class MornAnimationState : StateBehaviour
#endif
	{
		[SerializeField, NoLabel] private List<MornAnimationEntry> _targets;
		[SerializeField] private StateLink _onComplete;
		[SerializeField] private bool _isExecuteAsIsolated;
		[SerializeField] private float _interval = 0.03f;
		[SerializeField] private float _delay;
		[SerializeField] private bool _isWaitEach;

		public override async void OnStateBegin()
		{
			var ct = _isExecuteAsIsolated ? destroyCancellationToken : CancellationTokenOnEnd;
			try
			{
				if (_delay > 0f)
				{
					await MornAnimationUtil.WaitSeconds(_delay, ct);
				}

				var tasks = new List<UniTask>();
				foreach (var target in _targets)
				{
					tasks.Add(target.ExecuteAsync(ct));

					if (_isWaitEach)
					{
						await UniTask.WhenAll(tasks);
						tasks.Clear();
					}

					if (_interval > 0f)
					{
						await MornAnimationUtil.WaitSeconds(_interval, ct);
					}
				}

				await UniTask.WhenAll(tasks);
				Transition(_onComplete);
			}
			catch (System.OperationCanceledException)
			{
			}
		}

		[Button("子孫オブジェクトから自動取得 (Show)")]
		public void CollectFromChildrenShow()
		{
			CollectFromChildren(true);
		}

		[Button("子孫オブジェクトから自動取得 (Hide)")]
		public void CollectFromChildrenHide()
		{
			CollectFromChildren(false);
		}

		private void CollectFromChildren(bool toShow)
		{
#if !USE_ARBOR
			var component = (Component)Owner;
			var unityObject = (UnityEngine.Object)Owner;
#else
			var component = (Component)this;
			var unityObject = (UnityEngine.Object)this;
#endif
			MornAnimationUtil.RecordUndo(unityObject, "CollectFromChildren");
			var anims = MornAnimationUtil.CollectChildAnimations(component);
			_targets = new List<MornAnimationEntry>();
			foreach (var anim in anims)
			{
				_targets.Add(MornAnimationEntry.Create(anim, toShow));
			}
			MornAnimationUtil.SetDirty(unityObject);
		}
	}
}
