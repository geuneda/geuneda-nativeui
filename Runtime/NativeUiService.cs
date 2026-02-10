using System;
using UnityEngine;

// ReSharper disable once CheckNamespace

namespace Geuneda.NativeUi
{
	public enum AlertButtonStyle
	{
		Default,
		Positive,
		Negative
	}

	public struct AlertButton
	{
		public string Text;
		public AlertButtonStyle Style;
		public Action Callback;
	}
	
	/// <summary>
	/// 네이티브 UI 화면을 호출하는 기능을 제공하는 서비스
	/// </summary>
	public static class NativeUiService
	{
		/// <summary>
		/// 주어진 <paramref name="title"/>, <paramref name="message"/>와
		/// 왼쪽에서 오른쪽 순서로 정렬된 <paramref name="buttons"/>를 사용하여 네이티브 OS 알림 팝업을 표시합니다.
		/// iOS 기기에서는 <paramref name="isAlertSheet"/> 값에 따라 알림 시트 형태로 표시할 수 있습니다.
		/// </summary>
		/// <exception cref="SystemException">
		/// 현재 플랫폼이 iOS 또는 Android가 아닌 경우 발생합니다.
		/// </exception>
		public static void ShowAlertPopUp(bool isAlertSheet, string title, string message, params AlertButton[] buttons)
		{
#if UNITY_EDITOR
			Debug.Log($"Show Alert Pop Up is not available in the editor and was triggered with: {title} - {message}");
#elif UNITY_IOS
			_currentButtons = buttons ?? throw new ArgumentException("The buttons count must be higher than zero");

			var buttonsText = new string[buttons.Length];
			var buttonsStyle = new int[buttons.Length];

			for (var i = 0; i < buttons.Length; i++)
			{
				buttonsText[i] = buttons[i].Text;
				buttonsStyle[i] = (int) buttons[i].Style;
			}

			AlertMessage(isAlertSheet, title, message, buttonsText, buttonsStyle, buttons.Length, AlertButtonCallback);
#elif UNITY_ANDROID
			using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
			using (var unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
			using (var alertDialogBuilder = new AndroidJavaObject("android.app.AlertDialog$Builder", unityActivity))
			using (var alertDialog = alertDialogBuilder.Call<AndroidJavaObject>("create"))
			{
				alertDialog.Call("setTitle", title);
				alertDialog.Call("setMessage", message);
				
				for (var i = 0; i < buttons.Length; i++) 
				{
					alertDialog.Call("setButton", ConvertToAndroidStyle(buttons[i].Style), 
						buttons[i].Text, new AndroidButtonCallback(buttons[i].Callback));
				}
				
				alertDialog.Call("show");
			}
#else
			throw new SystemException("Show an alert Pop Up is only available for iOS and Android platforms");
#endif
		}

		/// <summary>
		/// 주어진 <paramref name="message"/>를 사용하여 네이티브 OS 토스트 메시지 팝업을 표시합니다.
		/// 이 토스트는 <paramref name="isLongDuration"/> 값에 따라 3.5초 또는 2초 동안 화면에 표시됩니다.
		/// </summary>
		/// <exception cref="SystemException">
		/// 현재 플랫폼이 iOS 또는 Android가 아닌 경우 발생합니다.
		/// </exception>
		public static void ShowToastMessage(string message, bool isLongDuration)
		{
#if UNITY_EDITOR
			Debug.Log($"Show Toast message is not available in the editor and was triggered with: {message}");
#elif UNITY_IOS
			ToastMessage(message, isLongDuration);
#elif UNITY_ANDROID
			using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
			using (var unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
			using (var toastClass = new AndroidJavaClass("android.widget.Toast"))
			{
				var duration = isLongDuration ? toastClass.GetStatic<int>("LENGTH_LONG") : toastClass.GetStatic<int>("LENGTH_SHORT");
				var toast = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity, message, duration);
				
				toast.Call("show");
				
				toast.Dispose();
			}
#else
			throw new SystemException("Show a Toast message is only available for iOS and Android platforms");
#endif
		}
		
#if UNITY_IOS
		internal delegate void AlertButtonDelegate(string buttonText);
		
		[System.Runtime.InteropServices.DllImport("__Internal")] 
		private static extern void AlertMessage(bool isSheet, string title, string message, string[] buttonsText, 
			int[] buttonsStyle, int buttonsLength, AlertButtonDelegate alertButtonCallback);
		
		[System.Runtime.InteropServices.DllImport("__Internal")] 
		private static extern void ToastMessage(string message, bool isLongDuration);

		[AOT.MonoPInvokeCallback(typeof(AlertButtonDelegate))]
		private static void AlertButtonCallback(string buttonText)
		{
			if (_currentButtons == null)
			{
				return;
			}

			foreach (var button in _currentButtons)
			{
				if (button.Text == buttonText)
				{
					button.Callback?.Invoke();
					break;
				}
			}
		}

		private static AlertButton[] _currentButtons;
#elif UNITY_ANDROID
		private class AndroidButtonCallback : AndroidJavaProxy
		{
			private readonly Action _callback;
			
			public AndroidButtonCallback(Action callback) : base("android.content.DialogInterface$OnClickListener")
			{
				_callback = callback;
			}

			// ReSharper disable once InconsistentNaming
			public void onClick(AndroidJavaObject dialog, int which)
			{
				dialog.Call("dismiss");

				_callback();
			}
		}

		private static int ConvertToAndroidStyle(AlertButtonStyle style)
		{
			switch (style)
			{
				case AlertButtonStyle.Default:
					return -3;
				case AlertButtonStyle.Positive:
					return -1;
				case AlertButtonStyle.Negative:
					return -2;
				default:
					throw new ArgumentOutOfRangeException(nameof(style), style, "Wrong given style");
			}
		}
#endif
	}
}