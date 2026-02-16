using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace MornLib
{
	internal static class MornAnimationUtil
	{
#if UNITY_EDITOR
		private static bool _initialized;
		private static float _lastEditorTime;
		private static float _editorDeltaTime;

		[UnityEditor.InitializeOnLoadMethod]
		private static void Initialize()
		{
			if (_initialized) return;
			_initialized = true;
			_lastEditorTime = (float)UnityEditor.EditorApplication.timeSinceStartup;
			UnityEditor.EditorApplication.update += OnEditorUpdate;
		}

		private static void OnEditorUpdate()
		{
			var currentTime = (float)UnityEditor.EditorApplication.timeSinceStartup;
			_editorDeltaTime = Mathf.Clamp(currentTime - _lastEditorTime, 0f, 0.1f);
			_lastEditorTime = currentTime;
		}
#endif

		public static float GetAlpha(this Image target)
		{
			return target.color.a;
		}

		public static void SetAlpha(this Image target, float alpha)
		{
			var color = target.color;
			color.a = alpha;
			target.color = color;
		}

		/// <summary>
		/// Editor/Runtime両方で動作するデルタタイムを取得
		/// </summary>
		public static float GetDeltaTime()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				return _editorDeltaTime;
			}
#endif
			return Time.deltaTime;
		}

		/// <summary>
		/// Editor/Runtime両方で動作する次フレーム待機
		/// </summary>
		public static async UniTask WaitNextFrame(CancellationToken ct)
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				var tcs = new UniTaskCompletionSource();
				void OnUpdate()
				{
					UnityEditor.EditorApplication.update -= OnUpdate;
					if (ct.IsCancellationRequested)
					{
						tcs.TrySetCanceled(ct);
					}
					else
					{
						tcs.TrySetResult();
					}
				}
				UnityEditor.EditorApplication.update += OnUpdate;
				UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
				UnityEditor.SceneView.RepaintAll();
				await tcs.Task;
				return;
			}
#endif
			await UniTask.Yield(ct);
		}

		/// <summary>
		/// Editor/Runtime両方で動作する秒数待機
		/// </summary>
		public static async UniTask WaitSeconds(float seconds, CancellationToken ct)
		{
			if (seconds <= 0f) return;
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				var startTime = (float)UnityEditor.EditorApplication.timeSinceStartup;
				while ((float)UnityEditor.EditorApplication.timeSinceStartup - startTime < seconds)
				{
					ct.ThrowIfCancellationRequested();
					await WaitNextFrame(ct);
				}
				return;
			}
#endif
			await UniTask.Delay(TimeSpan.FromSeconds(seconds), cancellationToken: ct);
		}

		/// <summary>
		/// Editor上でオブジェクトをDirtyとしてマーク（Prefab変更の保存用）
		/// </summary>
		public static void SetDirty(UnityEngine.Object target)
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				UnityEditor.EditorUtility.SetDirty(target);
			}
#endif
		}
	}
}