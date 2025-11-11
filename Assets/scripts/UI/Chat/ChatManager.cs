using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : GenericObjectPool<ChatView>
{
  [SerializeField] private SocketIOManager socketIOManager;
  [SerializeField] private ScrollRect ScrollRect;
  [SerializeField] internal TMP_InputField inputField;
  [SerializeField] private Button sendButton;
  [SerializeField] private JSFunctCalls jsBridge;
  private Queue<ChatView> activeMessages = new();

  protected override void Awake()
  {
    base.Awake();
    sendButton.onClick.AddListener(() => StartCoroutine(SendChatMessage()));

// #if UNITY_WEBGL && !UNITY_EDITOR
//     inputField.onSelect.AddListener(_ => jsBridge.OpenKeyboard());
// #else
    inputField.onEndEdit.AddListener((s) => OnEndEdit());
// #endif
  }

  internal void OnKeyboardSubmit(string message)
  {
    if (string.IsNullOrEmpty(message))
      return;

    inputField.text = message;
    StartCoroutine(SendChatMessage());
  }

  internal void InitChat(List<string> usernames, List<string> messages)
  {
    for (int i = usernames.Count - 1; i >= 0; i--)
    {
      AddMessage(usernames[i], messages[i]);
    }
    StartCoroutine(ScrollToBottomNextFrame());
  }

  internal void OnChatResult(string username, string message)
  {
    AddMessage(username, message);
    StartCoroutine(ScrollToBottomNextFrame());
  }

  void AddMessage(string username, string message)
  {
    if (base.ItemsInUse.Count >= socketIOManager.chatMessagesCap)
    {
      var old = activeMessages.Dequeue();
      base.ReturnToPool(old);
    }

    var item = base.GetFromPool();
    item.SetMessage(username, message);
    item.transform.SetAsLastSibling();
    activeMessages.Enqueue(item);
    item.transform.localScale = Vector3.one * 0.95f;
    item.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
  }

  private IEnumerator ScrollToBottomNextFrame()
  {
    yield return null;
    Canvas.ForceUpdateCanvases();

    if (ScrollRect.verticalNormalizedPosition <= 0.05f)
      ScrollRect.verticalNormalizedPosition = 0f;
  }

  void OnEndEdit()
  {
    bool enterPressed = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
    bool escapePressed = Input.GetKeyDown(KeyCode.Escape);
    if (enterPressed)
    {
      StartCoroutine(SendChatMessage());
    }
    else if (escapePressed)
    {
      DOVirtual.DelayedCall(0.001f, () =>
      {
        inputField.text = "";
      });
    }
  }

  private IEnumerator SendChatMessage()
  {
    ToggleUI(false);
    string msg = inputField.text.Trim();

    if (string.IsNullOrEmpty(msg))
    {
      ToggleUI(true);
      yield break;
    }

    if (msg.Contains("Char Limit Exceeded"))
    {
      ToggleUI(true);
      yield break;
    }

    if (msg.Length > socketIOManager.chatCharCap)
    {
      inputField.text = "<color=red>Char Limit Exceeded!!!</color>";
      ToggleUI(true);
      yield break;
    }

    inputField.text = "";
    socketIOManager.SendChatMessage(msg);

    yield return new WaitForSeconds(1f);

    ToggleUI(true);
  }

  void ToggleUI(bool toggle)
  {
    inputField.interactable = toggle;
    if (toggle)
    {
      inputField.ActivateInputField();
    }
    else
    {
      inputField.DeactivateInputField();
    }
    sendButton.interactable = toggle;
  }

}
