using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

public class ProvablyFairSettingsManager : MonoBehaviour
{
  [Header("Layout Switch")]
  [SerializeField] private RectTransform widthSource;
  [SerializeField] private float minWidthForLandscape = 800f;
  [SerializeField] private GameObject portraitPanel;
  [SerializeField] private GameObject landscapePanel;

  [Header("Portrait UI")]
  [SerializeField] private Toggle portraitRandomToggle;
  [SerializeField] private Toggle portraitManualToggle;
  [SerializeField] private GameObject portraitRandomBlocker;
  [SerializeField] private GameObject portraitManualBlocker;
  [SerializeField] private TMP_Text portraitClientSeedRandomText;
  [SerializeField] private TMP_Text portraitClientSeedManualText;
  [SerializeField] private TMP_Text portraitServerSeedText;

  [Header("Landscape UI")]
  [SerializeField] private Toggle landscapeRandomToggle;
  [SerializeField] private Toggle landscapeManualToggle;
  [SerializeField] private GameObject landscapeRandomBlocker;
  [SerializeField] private GameObject landscapeManualBlocker;
  [SerializeField] private TMP_Text landscapeClientSeedRandomText;
  [SerializeField] private TMP_Text landscapeClientSeedManualText;
  [SerializeField] private TMP_Text landscapeServerSeedText;

  [Header("References")]
  [SerializeField] private UIManager uiManager;

  [Header("Provably Fair Info Popup")]
  [SerializeField] private Button provablyFairInfoOpenButtonPortrait;
  [SerializeField] private Button provablyFairInfoOpenButtonLandscape;

  [Header("Manual Seed Popup")]
  [SerializeField] private GameObject manualSeedPanel;
  [SerializeField] private TMP_InputField manualSeedInputField;
  [SerializeField] private Button manualSeedOpenButtonPortrait;
  [SerializeField] private Button manualSeedOpenButtonLandscape;
  [SerializeField] private Button manualSeedRandomButton;
  [SerializeField] private Button manualSeedSaveButton;
  [SerializeField] private Button manualSeedCancelButton;
  [SerializeField] private Button manualSeedCloseButton;

  [Header("Copy Buttons")]
  [SerializeField] private Button portraitCopyRandomButton;
  [SerializeField] private Button portraitCopyManualButton;
  [SerializeField] private Button landscapeCopyRandomButton;
  [SerializeField] private Button landscapeCopyManualButton;

  [Header("Copied Popup")]
  [SerializeField] private CanvasGroup copiedPopupCanvasGroup;
  [SerializeField] private RectTransform copiedPopupRect;
  [SerializeField] private float copiedPopupRise1 = 20f;
  [SerializeField] private float copiedPopupRise2 = 20f;
  [SerializeField] private float copiedPopupFadeInDuration = 0.2f;
  [SerializeField] private float copiedPopupHoldSeconds = 1f;
  [SerializeField] private float copiedPopupFadeOutDuration = 0.2f;

  private float lastWidth = -1f;
  private bool lastLandscape;
  private bool isSyncing;
  private ClientSeedMode currentMode = ClientSeedMode.Random;
  private string clientSeedRandom = "";
  private string clientSeedManual = "";
  private string serverSeed = "";
  private Vector2 copiedPopupStartPos;
  private Sequence copiedPopupSequence;

#if UNITY_WEBGL && !UNITY_EDITOR
  [DllImport("__Internal")] private static extern void CopyTextToClipboard(string text);
#endif

  private enum ClientSeedMode
  {
    Random,
    Manual
  }

  private void Awake()
  {
    BindToggles();
    BindProvablyFairInfoPopup();
    BindManualSeedPopup();
    BindCopyButtons();
    SetupCopiedPopup();
    UpdateOrientation(force: true);
  }

  private void LateUpdate()
  {
    UpdateOrientation(force: false);
  }

private void BindToggles()
  {
    if (portraitRandomToggle != null)
      portraitRandomToggle.onValueChanged.AddListener(isOn => OnToggleChanged(ClientSeedMode.Random, isOn));
    if (portraitManualToggle != null)
      portraitManualToggle.onValueChanged.AddListener(isOn => OnToggleChanged(ClientSeedMode.Manual, isOn));
    if (landscapeRandomToggle != null)
      landscapeRandomToggle.onValueChanged.AddListener(isOn => OnToggleChanged(ClientSeedMode.Random, isOn));
    if (landscapeManualToggle != null)
      landscapeManualToggle.onValueChanged.AddListener(isOn => OnToggleChanged(ClientSeedMode.Manual, isOn));
}

  private void BindProvablyFairInfoPopup()
  {
    if (provablyFairInfoOpenButtonPortrait != null)
      provablyFairInfoOpenButtonPortrait.onClick.AddListener(OpenProvablyFairInfoPopup);
    if (provablyFairInfoOpenButtonLandscape != null)
      provablyFairInfoOpenButtonLandscape.onClick.AddListener(OpenProvablyFairInfoPopup);
  }

  private void BindManualSeedPopup()
{
  if (manualSeedOpenButtonPortrait != null)
    manualSeedOpenButtonPortrait.onClick.AddListener(OpenManualSeedPopup);
  if (manualSeedOpenButtonLandscape != null)
    manualSeedOpenButtonLandscape.onClick.AddListener(OpenManualSeedPopup);
  if (manualSeedRandomButton != null)
    manualSeedRandomButton.onClick.AddListener(GenerateRandomManualSeed);
  if (manualSeedSaveButton != null)
    manualSeedSaveButton.onClick.AddListener(SaveManualSeed);
  if (manualSeedCancelButton != null)
    manualSeedCancelButton.onClick.AddListener(CloseManualSeedPopup);
  if (manualSeedCloseButton != null)
    manualSeedCloseButton.onClick.AddListener(CloseManualSeedPopup);
  if (manualSeedInputField != null)
    manualSeedInputField.onValueChanged.AddListener(_ => RefreshManualSeedSaveState());

    RefreshManualSeedSaveState();
  }

  private void OpenProvablyFairInfoPopup()
  {
    if (uiManager != null)
      uiManager.OpenProvablyFairInfoPopup();
  }

  private void BindCopyButtons()
  {
    if (portraitCopyRandomButton != null)
      portraitCopyRandomButton.onClick.AddListener(() => CopySeed(clientSeedRandom));
    if (portraitCopyManualButton != null)
      portraitCopyManualButton.onClick.AddListener(() => CopySeed(clientSeedManual));
    if (landscapeCopyRandomButton != null)
      landscapeCopyRandomButton.onClick.AddListener(() => CopySeed(clientSeedRandom));
    if (landscapeCopyManualButton != null)
      landscapeCopyManualButton.onClick.AddListener(() => CopySeed(clientSeedManual));
  }

  private void SetupCopiedPopup()
  {
    if (copiedPopupCanvasGroup == null || copiedPopupRect == null)
      return;
    copiedPopupCanvasGroup.alpha = 0f;
    copiedPopupCanvasGroup.interactable = false;
    copiedPopupCanvasGroup.blocksRaycasts = false;
    copiedPopupStartPos = copiedPopupRect.anchoredPosition;
  }

  internal void Initialize(string randomSeed, string manualSeed, string initialServerSeed, bool useManual)
  {
    clientSeedRandom = randomSeed ?? "";
    clientSeedManual = manualSeed ?? "";
    serverSeed = initialServerSeed ?? "";

    ApplySeedTexts();
  SetMode(useManual ? ClientSeedMode.Manual : ClientSeedMode.Random, notifyUIManager: true);
}

  internal void UpdateServerSeed(string newServerSeed)
  {
    serverSeed = newServerSeed ?? "";
    ApplyServerSeedText();
  }

  private void OnToggleChanged(ClientSeedMode mode, bool isOn)
  {
    if (!isOn || isSyncing)
      return;

    SetMode(mode, notifyUIManager: true);
  }

  private void SetMode(ClientSeedMode mode, bool notifyUIManager)
  {
    currentMode = mode;

    isSyncing = true;
    bool isRandom = mode == ClientSeedMode.Random;
    if (portraitRandomToggle != null)
      portraitRandomToggle.isOn = isRandom;
    if (portraitManualToggle != null)
      portraitManualToggle.isOn = !isRandom;
    if (landscapeRandomToggle != null)
      landscapeRandomToggle.isOn = isRandom;
    if (landscapeManualToggle != null)
      landscapeManualToggle.isOn = !isRandom;
    isSyncing = false;

    if (portraitRandomBlocker != null)
      portraitRandomBlocker.SetActive(!isRandom);
    if (portraitManualBlocker != null)
      portraitManualBlocker.SetActive(isRandom);
    if (landscapeRandomBlocker != null)
      landscapeRandomBlocker.SetActive(!isRandom);
    if (landscapeManualBlocker != null)
      landscapeManualBlocker.SetActive(isRandom);

    if (notifyUIManager && uiManager != null)
      uiManager.SetActiveClientSeed(isRandom ? clientSeedRandom : clientSeedManual);
  }

  private void ApplySeedTexts()
  {
    if (portraitClientSeedRandomText != null)
      portraitClientSeedRandomText.text = clientSeedRandom;
    if (portraitClientSeedManualText != null)
      portraitClientSeedManualText.text = clientSeedManual;
    if (landscapeClientSeedRandomText != null)
      landscapeClientSeedRandomText.text = clientSeedRandom;
    if (landscapeClientSeedManualText != null)
      landscapeClientSeedManualText.text = clientSeedManual;
    ApplyServerSeedText();
  }

  private void ApplyServerSeedText()
  {
    if (portraitServerSeedText != null)
      portraitServerSeedText.text = serverSeed;
    if (landscapeServerSeedText != null)
      landscapeServerSeedText.text = serverSeed;
  }

  private void CopySeed(string seed)
  {
    if (string.IsNullOrEmpty(seed))
      return;
#if UNITY_WEBGL && !UNITY_EDITOR
    CopyTextToClipboard(seed);
#else
    GUIUtility.systemCopyBuffer = seed;
    OnCopySuccess();
#endif
  }

  public void OnCopySuccess()
  {
    if (copiedPopupCanvasGroup == null || copiedPopupRect == null)
      return;

    copiedPopupRect.anchoredPosition = copiedPopupStartPos;
    copiedPopupCanvasGroup.alpha = 0f;
    if (copiedPopupSequence != null && copiedPopupSequence.IsActive())
      copiedPopupSequence.Kill();

    Vector2 start = copiedPopupStartPos;
    Vector2 mid = copiedPopupStartPos + Vector2.up * copiedPopupRise1;
    Vector2 end = mid + Vector2.up * copiedPopupRise2;

    copiedPopupSequence = DOTween.Sequence()
      .SetUpdate(true)
      .Append(copiedPopupRect.DOAnchorPos(mid, copiedPopupFadeInDuration))
      .Join(copiedPopupCanvasGroup.DOFade(1f, copiedPopupFadeInDuration))
      .AppendInterval(copiedPopupHoldSeconds)
      .Append(copiedPopupRect.DOAnchorPos(end, copiedPopupFadeOutDuration))
      .Join(copiedPopupCanvasGroup.DOFade(0f, copiedPopupFadeOutDuration));
  }

private void OpenManualSeedPopup()
{
  if (manualSeedPanel != null)
    manualSeedPanel.SetActive(true);

  if (manualSeedInputField != null)
    manualSeedInputField.text = clientSeedManual;

  RefreshManualSeedSaveState();
}

private void CloseManualSeedPopup()
{
  if (manualSeedPanel != null)
    manualSeedPanel.SetActive(false);
}

private void GenerateRandomManualSeed()
{
  if (manualSeedInputField == null)
    return;
  manualSeedInputField.text = GenerateSeedString();
  RefreshManualSeedSaveState();
}

private void SaveManualSeed()
{
  if (manualSeedInputField == null)
    return;

  string newSeed = manualSeedInputField.text.Trim();
  if (newSeed.Length < 5)
  {
    RefreshManualSeedSaveState();
    return;
  }

  clientSeedManual = newSeed;
  ApplySeedTexts();

  if (currentMode == ClientSeedMode.Manual && uiManager != null)
    uiManager.SetActiveClientSeed(clientSeedManual);

  CloseManualSeedPopup();
}

private void RefreshManualSeedSaveState()
{
  if (manualSeedSaveButton == null || manualSeedInputField == null)
    return;
  manualSeedSaveButton.interactable = manualSeedInputField.text.Trim().Length >= 5;
}

private string GenerateSeedString()
{
  const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
  char[] seedChars = new char[16];

  for (int i = 0; i < seedChars.Length; i++)
  {
    int idx = Random.Range(0, chars.Length);
    seedChars[i] = chars[idx];
  }

  return new string(seedChars);
}

private void UpdateOrientation(bool force)
  {
    if (widthSource == null || portraitPanel == null || landscapePanel == null)
      return;

    float width = widthSource.rect.width;
    if (!force && Mathf.Abs(width - lastWidth) < 0.5f)
      return;

    bool isLandscape = width >= minWidthForLandscape;
    if (!force && isLandscape == lastLandscape)
    {
      lastWidth = width;
      return;
    }

    lastWidth = width;
    lastLandscape = isLandscape;

    landscapePanel.SetActive(isLandscape);
    portraitPanel.SetActive(!isLandscape);
  }
}
