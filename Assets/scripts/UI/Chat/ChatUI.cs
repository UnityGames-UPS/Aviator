using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatUI : GenericObjectPool<ChatView>
{
  [SerializeField] private SocketIOManager socketIOManager;
  [SerializeField] private ScrollRect ScrollRect;
  [SerializeField] private TMP_InputField inputField;
  [SerializeField] private Button sendButton;
  private Queue<ChatView> activeMessages = new();

  protected override void Awake()
  {
    base.Awake();
    sendButton.onClick.AddListener(() => SendChatMessage());
    inputField.onSubmit.AddListener((s) => SendChatMessage());
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
  }

  private IEnumerator ScrollToBottomNextFrame()
  {
    yield return null;
    Canvas.ForceUpdateCanvases();

    if (ScrollRect.verticalNormalizedPosition <= 0.05f)
      ScrollRect.verticalNormalizedPosition = 0f;
  }

  private void SendChatMessage()
  {
    string msg = inputField.text.Trim();

    if (string.IsNullOrEmpty(msg))
    {
      return;
    }

    inputField.text = "";

    if (msg.Contains("Char Limit Exceeded"))
    {
      return;
    }

    if (msg.Length > socketIOManager.chatCharCap)
    {
      inputField.text = "<color=red>Char Limit Exceeded!!!</color>";
      return;
    }
    
    socketIOManager.SendChatMessage(msg);
  }
}
