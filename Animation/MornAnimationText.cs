using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MornLib
{
	internal sealed class MornAnimationText : MornAnimationTargetBase
	{
		[SerializeField] private MornAnimationTextModule _textModule = new();
		private CancellationTokenSource _cts;
		private List<MornAnimationModuleBase> _modules;

		protected override List<MornAnimationModuleBase> GetModules()
		{
			if (_modules != null) return _modules;
			_modules = new List<MornAnimationModuleBase>();
			if (_textModule != null) _modules.Add(_textModule);
			return _modules;
		}
	}
}