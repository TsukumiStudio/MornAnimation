using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace MornLib
{
	[Serializable]
	internal sealed class MornAnimationTextModule : MornAnimationModuleBase
	{
		private enum ReferenceType
		{
			Custom,
			Auto,
		}

		[SerializeField] private ReferenceType _referenceType;
		[SerializeField, EnableIf(nameof(IsCustom))] private TMP_Text _targetText;
		[SerializeField, TextArea(3, 10)] private string _text;
		[SerializeField] private float _overrideSecondsPerChar = -1f;
		[SerializeField] private AudioClip _overrideCharSound;
		private bool IsCustom => _referenceType == ReferenceType.Custom;
		private CancellationTokenSource _cts;
		private float SecondsPerChar => _overrideSecondsPerChar >= 0f ? _overrideSecondsPerChar : MornAnimationGlobal.I.SecondsPerChar;
		private AudioClip CharSound => _overrideCharSound != null ? _overrideCharSound : MornAnimationGlobal.I.CharSound;

		public override void OnAwake(MornAnimationBase parent)
		{
			if (_referenceType == ReferenceType.Auto)
			{
				_targetText = parent.GetComponent<TMP_Text>();
			}

			if (_targetText != null)
			{
				_targetText.text = _text;
				_targetText.maxVisibleCharacters = 0;
			}
		}

		public override void OnValidate(MornAnimationBase parent)
		{
			if (_referenceType == ReferenceType.Auto)
			{
				_targetText = parent.GetComponent<TMP_Text>();
			}
		}

		public override void OnShowImmediate()
		{
			if (_targetText != null)
			{
				_targetText.text = _text;
				_targetText.maxVisibleCharacters = _targetText.textInfo.characterCount;
			}
		}

		public override void OnHideImmediate()
		{
			if (_targetText != null)
			{
				_targetText.maxVisibleCharacters = 0;
			}
		}

		public override async UniTask ShowAsync(CancellationToken ct = default)
		{
			if (_targetText == null) return;

			_cts?.Cancel();
			_cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
			var token = _cts.Token;

			var delay = Time != null ? Time.ShowDelay : 0f;
			if (delay > 0f)
			{
				await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);
			}

			_targetText.text = _text;
			_targetText.ForceMeshUpdate();
			var totalChars = _targetText.textInfo.characterCount;
			_targetText.maxVisibleCharacters = 0;

			var charSound = CharSound;
			var secondsPerChar = SecondsPerChar;
			var global = MornAnimationGlobal.I;
			var silentWaitMultiplier = global.SilentCharWaitMultiplier;

			for (var i = 1; i <= totalChars; i++)
			{
				token.ThrowIfCancellationRequested();
				_targetText.maxVisibleCharacters = i;

				// 現在表示された文字を取得 (0ベースなのでi-1)
				var currentChar = _targetText.textInfo.characterInfo[i - 1].character;
				var isSilent = global.IsSilentChar(currentChar);

				if (charSound != null && !isSilent)
				{
					MornAnimationService.I.PlayOneShot(charSound);
				}

				if (secondsPerChar > 0f)
				{
					var waitTime = isSilent ? secondsPerChar * silentWaitMultiplier : secondsPerChar;
					await UniTask.Delay(TimeSpan.FromSeconds(waitTime), cancellationToken: token);
				}
			}
		}

		public override async UniTask HideAsync(CancellationToken ct = default)
		{
			_cts?.Cancel();
			if (_targetText != null)
			{
				_targetText.maxVisibleCharacters = 0;
			}

			await UniTask.CompletedTask;
		}
	}
}