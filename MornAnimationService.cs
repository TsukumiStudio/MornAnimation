using UnityEngine;

namespace MornLib
{
	[AddComponentMenu("")]
	internal class MornAnimationService : MornGlobalMonoBase<MornAnimationService>
	{
		protected override string ModuleName => "MornAnimationService";
		private AudioSource _seSource;

		protected override void OnInitialized()
		{
			_seSource = gameObject.AddComponent<AudioSource>();
			_seSource.playOnAwake = false;
			_seSource.loop = false;
			_seSource.outputAudioMixerGroup = MornAnimationGlobal.I.SeMixerGroup;
		}

		public void PlayOneShot(AudioClip clip)
		{
			if (_seSource != null && clip != null && Application.isFocused)
			{
				_seSource.MornPlayOneShot(clip);
			}
		}
	}
}