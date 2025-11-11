using System.Runtime.InteropServices;
using UnityEngine;

public class JSFunctCalls : MonoBehaviour
{
  [DllImport("__Internal")] private static extern void CreateInvisibleInput();
  [DllImport("__Internal")] private static extern void FocusInvisibleInput();
  [DllImport("__Internal")] private static extern void BlurInvisibleInput();
  [DllImport("__Internal")] private static extern void SendLogToReactNative(string message);

  [DllImport("__Internal")] private static extern void SendPostMessage(string message);
  // [SerializeField] private SocketIOManager socket;
  [SerializeField] private ChatManager chatManager;

  void Awake()
  {
#if UNITY_WEBGL && !UNITY_EDITOR
    CreateInvisibleInput();
#endif
  }

  internal void OpenKeyboard()
  {
#if UNITY_WEBGL && !UNITY_EDITOR
    FocusInvisibleInput();
#endif
  }

  internal void CloseKeyboard()
  {
#if UNITY_WEBGL && !UNITY_EDITOR
    BlurInvisibleInput();
#endif
  }

  // Called from JS
  internal void OnKeyboardInput(string text)
  {
    Debug.Log($"Keyboard Input: {text}");
    if (chatManager && chatManager.inputField)
      chatManager.inputField.text = text;
  }

  // Called from JS when Enter is pressed
  internal void OnKeyboardSubmit(string message)
  {
    Debug.Log($"Keyboard Submit: {message}");
    if (chatManager && !string.IsNullOrWhiteSpace(message))
    {
      chatManager.OnKeyboardSubmit(message);
    }
  }

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
