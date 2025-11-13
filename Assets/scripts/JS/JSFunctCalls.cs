using System.Runtime.InteropServices;
using UnityEngine;

public class JSFunctCalls : MonoBehaviour
{
  // [DllImport("__Internal")] private static extern void CreateInvisibleInput();
  // [DllImport("__Internal")] private static extern void FocusInvisibleInput();
  // [DllImport("__Internal")] private static extern void BlurInvisibleInput();
  [DllImport("__Internal")] private static extern void SendLogToReactNative(string message);

  [DllImport("__Internal")] private static extern void SendPostMessage(string message);
  [SerializeField] private ChatManager chatManager;

//   void Awake()
//   {
// #if UNITY_WEBGL && !UNITY_EDITOR
//     CreateInvisibleInput();
// #endif
//   }

//   internal void OpenKeyboard()
//   {
//     chatManager.consoleText.text = "OpenKeyboard";
// #if UNITY_WEBGL && !UNITY_EDITOR
//     FocusInvisibleInput();
// #endif
//   }

//   internal void CloseKeyboard()
//   {
//     chatManager.consoleText.text = "CloseKeyboard";
// #if UNITY_WEBGL && !UNITY_EDITOR
//     BlurInvisibleInput();
// #endif
//   }

//   internal void OnKeyboardInput(string text)
//   {
//     chatManager.consoleText.text = text;
//     chatManager.inputField.text = text;
//     chatManager.inputField.caretPosition = text.Length;
//   }

//   internal void OnKeyboardSubmit()
//   {
//     chatManager.consoleText.text = "OnKeyBoardSubmit";
//     StartCoroutine(chatManager.SendChatMessage(chatManager.inputField.text));
//   }

  void OnEnable()
  {
#if UNITY_WEBGL && !UNITY_EDITOR
    Application.logMessageReceived += HandleLog;
#endif
  }

  void OnDisable()
  {
#if UNITY_WEBGL && !UNITY_EDITOR
    Application.logMessageReceived -= HandleLog;
#endif
  }

#if UNITY_WEBGL && !UNITY_EDITOR
  void HandleLog(string logString, string stackTrace, LogType type)
  {
    string formattedMessage = $"[{type}] {logString}";
    SendLogToReactNative(formattedMessage);
  }
#endif

  internal void SendCustomMessage(string message)
  {
#if UNITY_WEBGL && !UNITY_EDITOR
    SendPostMessage(message);
#endif
  }
}
