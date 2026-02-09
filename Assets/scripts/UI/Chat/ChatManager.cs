using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using AdvancedInputFieldPlugin;

public class ChatManager : GenericObjectPool<ChatView>
{
  [SerializeField] private SocketIOManager socketIOManager;
  [SerializeField] private ScrollRect ScrollRect;
  [SerializeField] internal AdvancedInputField inputField;
  [SerializeField] internal TMP_Text inputText;
  [SerializeField] private Button sendButton;
  [SerializeField] private Button openChatButton;
  [SerializeField] private Button closeChatButton;
  [SerializeField] private Button OpenEmojiButton;
  [SerializeField] private GameObject EmojiWindowObject;
  private Queue<ChatView> activeMessages = new();

  [Header("Chat Layout")]
  [SerializeField] private RectTransform chatRect;
  [SerializeField] private RectTransform widthSource;
  [SerializeField] private RectTransform layoutGroupParent;
  [SerializeField] private RectTransform overlayParent;
  [SerializeField] private float minWidthSourceWidth = 600f;
  private float lastWidth = -1f;
  private Vector2 defaultSizeDelta;
  private Vector2 defaultAnchorMin;
  private Vector2 defaultAnchorMax;
  private Vector2 defaultAnchoredPosition;
  private Vector2 defaultPivot;
  private Vector2 defaultOffsetMin;
  private Vector2 defaultOffsetMax;

  protected override void Awake()
  {
    base.Awake();
    sendButton.onClick.AddListener(() => StartCoroutine(SendChatMessage(inputField.Text)));
    if (openChatButton != null)
      openChatButton.onClick.AddListener(OpenChat);
    if (closeChatButton != null)
      closeChatButton.onClick.AddListener(CloseChat);

    if (chatRect != null)
    {
      defaultSizeDelta = chatRect.sizeDelta;
      defaultAnchorMin = chatRect.anchorMin;
      defaultAnchorMax = chatRect.anchorMax;
      defaultAnchoredPosition = chatRect.anchoredPosition;
      defaultPivot = chatRect.pivot;
      defaultOffsetMin = chatRect.offsetMin;
      defaultOffsetMax = chatRect.offsetMax;
    }
    inputField.OnEndEdit.AddListener(OnEndEdit);
    inputField.OnValueChanged.AddListener((s) => OnValueChange());

    NativeKeyboardManager.Initialize(); // make sure instance exists
    DOVirtual.DelayedCall(0.5f, () =>
    {
      inputField.ManualSelect();
      inputField.ManualDeselect();
    });
    NativeKeyboardManager.LastSelectedInputField = inputField;
  }

  void LateUpdate()
  {
    UpdateChatLayout();
  }

  void UpdateChatLayout()
  {
    if (widthSource == null || chatRect == null || layoutGroupParent == null || overlayParent == null)
      return;

    float width = widthSource.rect.width;
    if (Mathf.Abs(width - lastWidth) < 0.5f)
      return;

    lastWidth = width;
    bool isNarrow = width < minWidthSourceWidth;
    if (isNarrow)
    {
      if (chatRect.parent != overlayParent)
        chatRect.SetParent(overlayParent, false);
      chatRect.SetAsLastSibling();
      // Stretch to overlay parent to avoid drift/offset when resizing.
      chatRect.anchorMin = Vector2.zero;
      chatRect.anchorMax = Vector2.one;
      chatRect.pivot = new Vector2(0.5f, 0.5f);
      chatRect.anchoredPosition = Vector2.zero;
      chatRect.offsetMin = Vector2.zero;
      chatRect.offsetMax = Vector2.zero;
    }
    else
    {
      if (chatRect.parent != layoutGroupParent)
        chatRect.SetParent(layoutGroupParent, false);
      chatRect.SetAsLastSibling();
      chatRect.anchorMin = defaultAnchorMin;
      chatRect.anchorMax = defaultAnchorMax;
      chatRect.pivot = defaultPivot;
      chatRect.anchoredPosition = defaultAnchoredPosition;
      chatRect.offsetMin = defaultOffsetMin;
      chatRect.offsetMax = defaultOffsetMax;
      chatRect.sizeDelta = defaultSizeDelta;
    }
  }

  void OpenChat()
  {
    UpdateChatLayout();
    if (chatRect != null)
      chatRect.gameObject.SetActive(true);
    openChatButton.gameObject.SetActive(false);
  }

  void CloseChat()
  {
    if (chatRect != null)
      chatRect.gameObject.SetActive(false);
    openChatButton.gameObject.SetActive(true);
  }

  internal void InitChat(List<string> userIds, List<string> messages)
  {
    for (int i = userIds.Count - 1; i >= 0; i--)
    {
      AddMessage(userIds[i], messages[i]);
    }
    StartCoroutine(ScrollToBottomNextFrame());
  }

  internal void OnChatResult(string userIds, string message)
  {
    AddMessage(userIds, message);
    StartCoroutine(ScrollToBottomNextFrame());
  }

  void AddMessage(string userId, string message)
  {
    if (base.ItemsInUse.Count >= socketIOManager.chatMessagesCap)
    {
      var old = activeMessages.Dequeue();
      base.ReturnToPool(old);
    }

    var item = base.GetFromPool();
    item.SetMessage(userId, message, socketIOManager.userId);
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
  void OnEndEdit(string text, EndEditReason reason)
  {
    if (reason == EndEditReason.KEYBOARD_DONE)
    {
      string msg = text.Replace("\n", "").Replace("\r", "").Trim();
      StartCoroutine(SendChatMessage(msg));
    }
    else if (reason == EndEditReason.KEYBOARD_CANCEL)
    {
      inputField.Text = "";
    }
  }

  void OnValueChange()
  {
    if (inputText.color == Color.red && !inputField.Text.Contains("Char Limit Exceeded"))
    {
      setColor(Color.white);
    }
  }

  void ToggleEmojiWindow(bool isActive)
  {
    EmojiWindowObject.SetActive(isActive);
  }

  internal IEnumerator SendChatMessage(string msg)
  {
    ToggleUI(false);
    inputField.Text = "";

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
      setColor(Color.red);
      inputField.Text = "Char Limit Exceeded!!!";
      ToggleUI(true);
      yield break;
    }

    socketIOManager.SendChatMessage(msg);

    yield return new WaitForSeconds(1f);

    ToggleUI(true);
  }

  void ToggleUI(bool toggle)
  {
    inputField.interactable = toggle;
    if (toggle)
      inputField.ManualSelect();
    else
      inputField.ManualDeselect();
    sendButton.interactable = toggle;
  }

  void setColor(Color input)
  {
    inputText.color = input;
  }
}
