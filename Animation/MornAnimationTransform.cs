using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MornLib
{
	internal sealed class MornAnimationTransform : MornAnimationTargetBase
	{
		[SerializeField] private bool _moveEnabled;
		[SerializeField, ShowIf(nameof(_moveEnabled))]
		private MornAnimationMoveModule _moveModule = new();
		[SerializeField] private bool _moveUGUIEnabled;
		[SerializeField, ShowIf(nameof(_moveUGUIEnabled))]
		private MornAnimationMoveUGUIModule _moveUGUIModule = new();
		[SerializeField] private bool _rotateEnabled;
		[SerializeField, ShowIf(nameof(_rotateEnabled))]
		private MornAnimationRotateModule _rotateModule = new();
		[SerializeField] private bool _scaleEnabled;
		[SerializeField, ShowIf(nameof(_scaleEnabled))]
		private MornAnimationScaleModule _scaleModule = new();
		private CancellationTokenSource _cts;
		private List<MornAnimationModuleBase> _modules;

		protected override void ResetCachedModules()
		{
			_modules = null;
		}

		protected override List<MornAnimationModuleBase> GetModules()
		{
			if (_modules != null) return _modules;
			_modules = new List<MornAnimationModuleBase>();
			if (_moveEnabled && _moveModule != null) _modules.Add(_moveModule);
			if (_moveUGUIEnabled && _moveUGUIModule != null) _modules.Add(_moveUGUIModule);
			if (_rotateEnabled && _rotateModule != null) _modules.Add(_rotateModule);
			if (_scaleEnabled && _scaleModule != null) _modules.Add(_scaleModule);
			return _modules;
		}
	}
}