using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MornLib
{
	internal sealed class MornAnimationFade : MornAnimationTargetBase
	{
		[SerializeField] private MornAnimationFadeModule _fadeModule = new();
		private CancellationTokenSource _cts;
		private List<MornAnimationModuleBase> _modules;

		protected override List<MornAnimationModuleBase> GetModules()
		{
			if (_modules != null) return _modules;
			_modules = new List<MornAnimationModuleBase>();
			if (_fadeModule != null) _modules.Add(_fadeModule);
			return _modules;
		}
	}
}