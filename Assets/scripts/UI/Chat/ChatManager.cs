using System.Collections;
using System.Collections.Generic;
using Best.HTTP.Cookies;
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
  [SerializeField] private Button chatToggle;
  [SerializeField] private JSFunctCalls jsBridge;
  private Queue<ChatView> activeMessages = new();

  [Header("Mobile GO Ref")]
  [SerializeField] private RectTransform topBarRect;
  [SerializeField] private RectTransform crashHistRect;
  [SerializeField] private RectTransform gamePlayRect;
  [SerializeField] private RectTransform betPanelRect;
  [SerializeField] private RectTransform chatRect;
  private bool isChatToggle = true;
  private float topBarXPosi;
  private float topBarXSD;
  private float crashHistXPosi;
  private float crashHistXSD;
  private float gamePlayXPosi;
  private float gamePlayXSD;
  private float betPanelXPosi;
  private float betPanelXSD;
  private float chatPanelXPosi;
  [SerializeField] internal TMP_Text consoleText;

  protected override void Awake()
  {
    base.Awake();
    sendButton.onClick.AddListener(() => StartCoroutine(SendChatMessage(inputField.text)));
    chatToggle.onClick.AddListener(() =>
    {
      isChatToggle = !isChatToggle;
      ToggleChat(isChatToggle);
    });

    topBarXPosi = topBarRect.localPosition.x;
    topBarXSD = topBarRect.sizeDelta.x;
    crashHistXPosi = crashHistRect.localPosition.x;
    crashHistXSD = crashHistRect.sizeDelta.x;
    gamePlayXPosi = gamePlayRect.localPosition.x;
    gamePlayXSD = gamePlayRect.sizeDelta.x;
    betPanelXPosi = betPanelRect.localPosition.x;
    betPanelXSD = betPanelRect.sizeDelta.x;
    chatPanelXPosi = chatRect.localPosition.x;

#if UNITY_WEBGL && !UNITY_EDITOR
    inputField.onSelect.AddListener(_ => jsBridge.OpenKeyboard());
    inputField.onDeselect.AddListener(_ => jsBridge.CloseKeyboard());
    inputField.onEndEdit.AddListener((s) => OnEndEdit());
#else
    inputField.onEndEdit.AddListener((s) => OnEndEdit());
#endif
  }

  void ToggleChat(bool toggle)
  {
    Debug.Log("Toggling chat");

    float duration = 0.35f;
    Ease easing = Ease.OutCubic;

    if (toggle)
    {
      chatRect.DOLocalMoveX(chatPanelXPosi, duration).SetEase(easing);
      topBarRect.DOLocalMoveX(topBarXPosi, duration).SetEase(easing);
      topBarRect.DOSizeDelta(new Vector2(topBarXSD, topBarRect.sizeDelta.y), duration).SetEase(easing);

      crashHistRect.DOLocalMoveX(crashHistXPosi, duration).SetEase(easing);
      crashHistRect.DOSizeDelta(new Vector2(crashHistXSD, crashHistRect.sizeDelta.y), duration).SetEase(easing);

      gamePlayRect.DOLocalMoveX(gamePlayXPosi, duration).SetEase(easing);
      gamePlayRect.DOSizeDelta(new Vector2(gamePlayXSD, gamePlayRect.sizeDelta.y), duration).SetEase(easing);

      betPanelRect.DOLocalMoveX(betPanelXPosi, duration).SetEase(easing);
      betPanelRect.DOSizeDelta(new Vector2(betPanelXSD, betPanelRect.sizeDelta.y), duration).SetEase(easing);
    }
    else
    {
      chatRect.DOLocalMoveX(chatPanelXPosi + (243*5), duration).SetEase(easing);
      topBarRect.DOLocalMoveX(topBarXPosi + 243f, duration).SetEase(easing);
      topBarRect.DOSizeDelta(new Vector2(2340f, topBarRect.sizeDelta.y), duration).SetEase(easing);

      crashHistRect.DOLocalMoveX(crashHistXPosi + 243f, duration).SetEase(easing);
      crashHistRect.DOSizeDelta(new Vector2(1746f, crashHistRect.sizeDelta.y), duration).SetEase(easing);

      gamePlayRect.DOLocalMoveX(gamePlayXPosi + 243f, duration).SetEase(easing);
      gamePlayRect.DOSizeDelta(new Vector2(1750f, gamePlayRect.sizeDelta.y), duration).SetEase(easing);

      betPanelRect.DOLocalMoveX(betPanelXPosi + 243f, duration).SetEase(easing);
      betPanelRect.DOSizeDelta(new Vector2(1747f, betPanelRect.sizeDelta.y), duration).SetEase(easing);
    }
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
      string msg = inputField.text = inputField.text.Replace("\n", "").Replace("\r", "").Trim();
      StartCoroutine(SendChatMessage(msg));
    }
    else if (escapePressed)
    {
      DOVirtual.DelayedCall(0.001f, () =>
      {
        inputField.text = "";
      });
    }
  }

  internal IEnumerator SendChatMessage(string msg)
  {
    ToggleUI(false);

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
    // if (toggle)
    // {
    //   inputField.ActivateInputField();
    // }
    // else
    // {
    //   inputField.DeactivateInputField();
    // }
    sendButton.interactable = toggle;
  }
}
