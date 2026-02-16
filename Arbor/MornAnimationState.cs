#if USE_ARBOR
using System.Collections.Generic;
using Arbor;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MornLib
{
	internal sealed class MornAnimationState : StateBehaviour
	{
		[SerializeField] private List<MornAnimationEntry> _targets;
		[SerializeField] private StateLink _onComplete;
		[SerializeField] private bool _isExecuteAsIsolated;

		public override async void OnStateBegin()
		{
			var ct = _isExecuteAsIsolated ? destroyCancellationToken : CancellationTokenOnEnd;
			try
			{
				var tasks = new List<UniTask>();
				foreach (var target in _targets)
				{
					tasks.Add(target.ExecuteAsync(ct));
				}

				await UniTask.WhenAll(tasks);
				Transition(_onComplete);
			}
			catch (System.OperationCanceledException)
			{
			}
		}
	}
}
#endif