using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace MornLib
{
	[CreateAssetMenu(fileName = nameof(MornAnimationGlobal), menuName = "Morn/" + nameof(MornAnimationGlobal))]
	internal sealed class MornAnimationGlobal : MornGlobalBase<MornAnimationGlobal>
	{
		protected override string ModuleName => "MornAnimation";
		[SerializeField] private MornAnimationTimeSettings _timeSettings;
		[SerializeField] private AudioMixerGroup _seMixerGroup;
		[Header("テキスト")]
		[SerializeField] private float _secondsPerChar = 0.05f;
		[SerializeField] private AudioClip _charSound;
		[SerializeField] private List<char> _silentChars = new() { '、', '。', ' ', '　' };
		[SerializeField] private bool _silentOnNewline = true;
		[SerializeField] private float _silentCharWaitMultiplier = 2f;
		public MornAnimationTimeSettings TimeSettings => _timeSettings;
		public AudioMixerGroup SeMixerGroup => _seMixerGroup;
		public float SecondsPerChar => _secondsPerChar;
		public AudioClip CharSound => _charSound;
		public float SilentCharWaitMultiplier => _silentCharWaitMultiplier;

		public bool IsSilentChar(char c)
		{
			if (_silentOnNewline && c == '\n') return true;
			return _silentChars.Contains(c);
		}
	}
}