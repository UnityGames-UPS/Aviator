using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Globalization;
using System.Numerics;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

public class UIManager : MonoBehaviour
{
  public static UIManager Instance { get; private set; }

  [SerializeField] private CurveManager curveAnimator;
  [SerializeField] private SocketIOManager socket;
  [SerializeField] private AnalyticsUIManager analyticsUIManager;
  [SerializeField] private RoundAnalyticsManager roundAnalyticsManager;
  [SerializeField] private PrevRoundManager prevRoundManager;
  [SerializeField] private BetHistoryManager betHistoryManager;
  [SerializeField] private AudioManager Audio;
  [SerializeField] private ProvablyFairSettingsManager provablyFairSettingsManager;

  [SerializeField] private PopupManager popupManager;
  private bool leftRequestInProgress = false;
  private bool rightRequestInProgress = false;

  [SerializeField] private Image profilePicImage;
  [SerializeField] private Sprite[] profilePicSprites;
  [SerializeField] private List<Button> avatarButtons;
  [SerializeField] private int avatarPanelIndex = 4;
  private readonly List<Sprite> avatarSprites = new List<Sprite>();

  [Header("Multiplier Objects and Values")]
  [SerializeField] private TMP_Text multiplierText;
  [SerializeField] private TMP_Text flewAwayText;
  [SerializeField] private Image blurImage;
  [SerializeField] private float displayedMult;
  [SerializeField] private float targetMult;
  [SerializeField] private Color blueColor;
  [SerializeField] private Color purpleColor;
  [SerializeField] private Color pinkColor;
  [SerializeField] private GameObject loadingBar;
  [SerializeField] private GameObject aviatorLogo;
  [SerializeField] private GameObject spribeLogo;
  [SerializeField] private ProceduralImage loadingBarFillerImage;
  [Header("Multiplier Smoothing")]
  [SerializeField] private float extrapolationMaxSeconds = 0.8f;
  [SerializeField] private float extrapolationMaxDelta = 0.5f;
  [SerializeField] private float minTickInterval = 0.05f;
  private float lastTickTime;
  private float lastTickInterval;
  private float lastMultVelocity;
  private float extrapolationElapsed;

  [Header("Other Options Menu")]
  [SerializeField] private GameObject OtherOptionsMenu;
  [SerializeField] private Button OtherOptionsMenuButton;
  [SerializeField] private Button CloseOptionsMenuButton1;
  [SerializeField] private Button HomeButton;
  [SerializeField] private Button SoundToggleButton;
  [SerializeField] private Button MusicToggleButton;
  [SerializeField] private Button AnimationToggleButton;
  [SerializeField] private GameObject OtherOptionsPanelParent;
  [SerializeField] private TMP_Text BalanceText;
  [SerializeField] private TMP_Text PlayerNameText;
  [SerializeField] private int provablyFairPanelIndex = -1;
  [SerializeField] private Button provablyFairOpenButton;
  [SerializeField] private Button provablyFairCloseButtonPortrait;
  [SerializeField] private Button provablyFairCloseButtonLandscape;
  //0: Bet History
  //1: Game Limits 
  //2: How To Play
  //3: Game Rules
  //4: Change Avatar 
  [SerializeField] private Button[] OtherOptionsButtons;
  [SerializeField] private GameObject[] OtherOptionsPanels;
  [SerializeField] private Button[] OtherOptionCloseButtons;
  [SerializeField] private Button CloseOtherOptionButton;
  [SerializeField] private Button AvatarPanelBottomCloseButton;
  [SerializeField] private GameObject BetHistoryLoader;
  [SerializeField] private TMP_Text MinBetText;
  [SerializeField] private TMP_Text MaxBetText;
  [SerializeField] private TMP_Text MaxCashoutText;
  [Header("Popups")]
  [SerializeField] private GameObject blocker;
  [SerializeField] private GameObject lowBalancePopupGO;
  [SerializeField] private GameObject ReconnectionPopupGO;
  [SerializeField] private GameObject DisconnectionPopupGO;
  [SerializeField] private Button lowBalanceCloseButton;
  [SerializeField] private Button OnDiscQuitButton;

  [Header("Provably Fair Popup")]
  [SerializeField] private GameObject provablyFairPopupGO;
  [SerializeField] private Button provablyFairPopupCloseButton;
  [SerializeField] private Button provablyFairPopupBackgroundButton;
  [SerializeField] private Button provablyFairInfoOpenButton;
  [SerializeField] private TMP_Text provablyFairRoundIdText;
  [SerializeField] private TMP_Text provablyFairMultiplierText;
  [SerializeField] private TMP_Text provablyFairCrashPointText;
  [SerializeField] private TMP_Text provablyFairTimestampText;
  [SerializeField] private TMP_Text provablyFairServerSeedText;
  [SerializeField] private TMP_Text provablyFairServerHashFullText;
  [SerializeField] private TMP_Text provablyFairServerHashHexText;
  [SerializeField] private TMP_Text provablyFairServerHashDecimalText;
  [SerializeField] private TMP_Text[] provablyFairUserIdTexts;
  [SerializeField] private TMP_Text[] provablyFairClientSeedTexts;
  [SerializeField] private Image[] provablyFairProfileImages;

  [Header("Provably Fair Info Popup")]
  [SerializeField] private GameObject provablyFairInfoPopupGO;
  [SerializeField] private Button provablyFairInfoCloseButton;
  [SerializeField] private Button provablyFairInfoBackgroundButton;

  [Header("Local Variables to keep track")]
  [SerializeField] private bool SoundToggle = true;
  [SerializeField] private bool MusicToggle = true;
  [SerializeField] private bool AnimationToggle = true;

  [Header("Left Bet UI")]
  [SerializeField] private Button LeftCashoutButton;
  [SerializeField] private GameObject LeftBlocker;
  [SerializeField] private Button[] LLeftRightBetChangeButtons; //0: Decrease Bet, 1: Increase Bet
  //0: Bet Button
  //1: Auto Bet Button
  [SerializeField] private Button[] LeftTopBarButtons;
  [SerializeField] private GameObject LeftAutoBetPanel;
  [SerializeField] private Button LeftAutoBetToggleButton;
  [SerializeField] private Button LeftAutoCashOutToggleButton;
  [SerializeField] private TMP_InputField LeftAutoCashoutInputField;
  [SerializeField] private TMP_Text LeftBetText;
  [SerializeField] private List<Button> LeftStaticBetButtons;
  [SerializeField] private Button LeftBetButton;
  [SerializeField] private Button LeftCancelBetButton;

  [Header("Right Bet UI")]
  [SerializeField] private Button RightCashoutButton;
  [SerializeField] private GameObject RightBlocker;
  [SerializeField] private Button[] RLeftRightBetChangeButtons; //0: Decrease Bet, 1: Increase Bet
  //0: Bet Button
  //1: Auto Bet Button
  [SerializeField] private Button[] RightTopBarButtons;
  [SerializeField] private GameObject RightAutoBetPanel;
  [SerializeField] private Button RightAutoBetToggleButton;
  [SerializeField] private Button RightAutoCashOutToggleButton;
  [SerializeField] private TMP_InputField RightAutoCashoutInputField;
  [SerializeField] private TMP_Text RightBetText;
  [SerializeField] private List<Button> RightStaticBetButtons;
  [SerializeField] private Button RightBetButton;
  [SerializeField] private Button RightCancelBetButton;

  [Header("Local variable to keep track")]
  [SerializeField] private bool LeftAutoToggle;
  [SerializeField] private bool LeftAutoCashOutToggle;
  [SerializeField] private bool RightAutoToggle;
  [SerializeField] private bool RightAutoCashOutToggle;
  [SerializeField] internal int LeftBetCounter;
  [SerializeField] internal int RightBetCounter;
  [SerializeField] private float minAutoCashoutMultiplier = 1.01f;
  [SerializeField] private float maxAutoCashoutMultiplier = 100f;
  [SerializeField] private float leftAutoCashoutValue = 1.01f;
  [SerializeField] private float rightAutoCashoutValue = 1.01f;
  [SerializeField] private int leftAutoBetLockedIndex = -1;
  [SerializeField] private int rightAutoBetLockedIndex = -1;
  private Coroutine autoBetCoroutine;

  [Header("Info UI")]
  //0: All bets panel
  //1: Previous bets panel
  //2: Top bets panel
  //3. Loading panel
  [SerializeField] private GameObject[] InfoUIPanels;
  [SerializeField] private Button[] InfoUIButtons;
  [SerializeField] private ScrollViewportRaycastGate[] infoScrollGates;
  //0: Player Panel
  //1: Date&Time Panel
  [SerializeField] private GameObject[] TopBetPanels;

  //0: x Button
  //1: Win Button
  //2: Rounds Button
  [SerializeField] private Button[] TopBetFilterButtons;

  //0: Day Button
  //1: Month Button
  //2: Year Button
  [SerializeField] private Button[] TopBetTimeButtons;

  [Header("Local variable to keep track")]
  [SerializeField] private int currentTopBetFilterIndex;
  [SerializeField] private int currentTopBetTimeIndex;
  [SerializeField] private string clientSeed;
  [SerializeField] private string clientSeedRandom;
  [SerializeField] private string clientSeedManual;
  [SerializeField] private string serverSeed;
  [SerializeField] private string roundIdentifier = "";

  private bool blueColTime = false;
  private bool purpleColTime = false;
  private bool pinkColTime = false;
  internal bool isUserExit = false;
  private Tween multColorTween;
  private Tween multColorTween2;
  private Tween blurTween;
  [SerializeField] internal BetData leftBetData;
  [SerializeField] internal BetData rightBetData;
  [SerializeField] private GridUIController gridController;
  private const int provablyFairMaxUsers = 3;

  private class ProvablyFairPopupPayload
  {
    public string roundId;
    public float multiplier;
    public string timestamp;
    public string serverSeed;
    public string serverHash;
    public readonly List<string> userIds = new();
    public readonly List<string> clientSeeds = new();
  }

  private void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }
    Instance = this;

    leftBetData = null;
    rightBetData = null;

    SetupAvatarButtons();
    lowBalanceCloseButton.onClick.AddListener(() => ClosePopup(lowBalancePopupGO));
    if (provablyFairPopupCloseButton != null)
      provablyFairPopupCloseButton.onClick.AddListener(CloseProvablyFairPopup);
    if (provablyFairPopupBackgroundButton != null)
      provablyFairPopupBackgroundButton.onClick.AddListener(CloseProvablyFairPopup);
    if (provablyFairInfoOpenButton != null)
      provablyFairInfoOpenButton.onClick.AddListener(OpenProvablyFairInfoPopup);
    if (provablyFairPopupGO != null)
      provablyFairPopupGO.SetActive(false);
    if (provablyFairInfoCloseButton != null)
      provablyFairInfoCloseButton.onClick.AddListener(CloseProvablyFairInfoPopup);
    if (provablyFairInfoBackgroundButton != null)
      provablyFairInfoBackgroundButton.onClick.AddListener(CloseProvablyFairInfoPopup);
    if (provablyFairInfoPopupGO != null)
      provablyFairInfoPopupGO.SetActive(false);

    LeftBetButton.onClick.AddListener(() => StartCoroutine(OnBet(true)));
    RightBetButton.onClick.AddListener(() => StartCoroutine(OnBet(false)));

    LeftCashoutButton.onClick.AddListener(() => StartCoroutine(OnCashout(true)));
    RightCashoutButton.onClick.AddListener(() => StartCoroutine(OnCashout(false)));

    LeftCancelBetButton.onClick.AddListener(() => StartCoroutine(OnCancel(true)));
    RightCancelBetButton.onClick.AddListener(() => StartCoroutine(OnCancel(false)));

    LLeftRightBetChangeButtons[0].onClick.AddListener(() => ChangeBet(false, true));
    LLeftRightBetChangeButtons[1].onClick.AddListener(() => ChangeBet(true, true));
    RLeftRightBetChangeButtons[0].onClick.AddListener(() => ChangeBet(false, false));
    RLeftRightBetChangeButtons[1].onClick.AddListener(() => ChangeBet(true, false));

    OtherOptionsMenuButton.onClick.AddListener(() => OtherOptionsMenu.SetActive(true));
    CloseOptionsMenuButton1.onClick.AddListener(() => OtherOptionsMenu.SetActive(false));
    HomeButton.onClick.AddListener(() => { isUserExit = true; socket.CloseGame(); });
    OnDiscQuitButton.onClick.AddListener(() => { isUserExit = true; socket.CloseGame(); });

    CloseOtherOptionButton.onClick.AddListener(() => CloseAllOtherOptionsMenu());
    if (provablyFairOpenButton != null)
      provablyFairOpenButton.onClick.AddListener(() =>
      {
        if (IsOtherOptionIndexValid(provablyFairPanelIndex))
          StartCoroutine(OtherOptionButtonClicked(provablyFairPanelIndex));
      });
    if (provablyFairCloseButtonPortrait != null)
      provablyFairCloseButtonPortrait.onClick.AddListener(() =>
      {
        if (IsOtherOptionIndexValid(provablyFairPanelIndex))
          CloseOtherOptionMenu(provablyFairPanelIndex);
      });
    if (provablyFairCloseButtonLandscape != null)
      provablyFairCloseButtonLandscape.onClick.AddListener(() =>
      {
        if (IsOtherOptionIndexValid(provablyFairPanelIndex))
          CloseOtherOptionMenu(provablyFairPanelIndex);
      });

    CloseOptionsMenuButton1.onClick.Invoke(); //Close Other Options Menu By Default

    SoundToggleButton.onClick.AddListener(() =>
    {
      SoundToggle = !SoundToggle;
      Audio.ToggleSoundsAudio(SoundToggle);
      ToggleButtonClicked(SoundToggleButton);
    });
    MusicToggleButton.onClick.AddListener(() =>
    {
      MusicToggle = !MusicToggle;
      Audio.ToggleBGAudio(MusicToggle);
      ToggleButtonClicked(MusicToggleButton);
    });
    AnimationToggleButton.onClick.AddListener(() =>
    {
      AnimationToggle = !AnimationToggle;
      curveAnimator.AnimationToggle(AnimationToggle);
      ToggleButtonClicked(AnimationToggleButton);
    });

    for (int i = 0; i < OtherOptionsButtons.Length; i++)
    {
      int index = i;
      OtherOptionsButtons[i].onClick.AddListener(() => StartCoroutine(OtherOptionButtonClicked(index)));
    }
    for (int i = 0; i < OtherOptionCloseButtons.Length; i++)
    {
      int index = i;
      OtherOptionCloseButtons[i].onClick.AddListener(() => CloseOtherOptionMenu(index));
    }
    if (AvatarPanelBottomCloseButton != null)
      AvatarPanelBottomCloseButton.onClick.AddListener(CloseAvatarPanelIfOpen);

    InfoUIButtons[0].onClick.AddListener(() => StartCoroutine(ShowInfoUI(0)));
    InfoUIButtons[1].onClick.AddListener(() => StartCoroutine(ShowInfoUI(1)));
    InfoUIButtons[2].onClick.AddListener(() => StartCoroutine(ShowInfoUI(2)));


    TopBetFilterButtons[0].onClick.AddListener(() => TopBetsButtonClicked(0, true));
    TopBetFilterButtons[1].onClick.AddListener(() => TopBetsButtonClicked(1, true));
    TopBetFilterButtons[2].onClick.AddListener(() => TopBetsButtonClicked(2, true));

    TopBetTimeButtons[0].onClick.AddListener(() => TopBetsButtonClicked(0, false));
    TopBetTimeButtons[1].onClick.AddListener(() => TopBetsButtonClicked(1, false));
    TopBetTimeButtons[2].onClick.AddListener(() => TopBetsButtonClicked(2, false));

    TopBetsButtonClicked(0, true, false);
    TopBetsButtonClicked(0, false, false);

    LeftTopBarButtons[0].onClick.AddListener(() => BetTopBarButtonClicked(0, true));
    LeftTopBarButtons[1].onClick.AddListener(() => BetTopBarButtonClicked(1, true));
    RightTopBarButtons[0].onClick.AddListener(() => BetTopBarButtonClicked(0, false));
    RightTopBarButtons[1].onClick.AddListener(() => BetTopBarButtonClicked(1, false));

    LeftTopBarButtons[0].onClick.Invoke(); //Default to Bet Button
    RightTopBarButtons[0].onClick.Invoke(); //Default to Bet Button

    LeftAutoBetToggleButton.onClick.AddListener(() =>
    {
      LeftAutoToggle = !LeftAutoToggle;
      ToggleButtonClicked(LeftAutoBetToggleButton);
      HandleAutoBetToggle(true);
    });
    RightAutoBetToggleButton.onClick.AddListener(() =>
    {
      RightAutoToggle = !RightAutoToggle;
      ToggleButtonClicked(RightAutoBetToggleButton);
      HandleAutoBetToggle(false);
    });

    LeftAutoCashOutToggleButton.onClick.AddListener(() =>
    {
      LeftAutoCashOutToggle = !LeftAutoCashOutToggle;
      ToggleButtonClicked(LeftAutoCashOutToggleButton);
      UpdateAutoCashoutInputInteractivity(true);
      UpdateTopBarInteractivityForAutoCashout(true);
    });
    RightAutoCashOutToggleButton.onClick.AddListener(() =>
    {
      RightAutoCashOutToggle = !RightAutoCashOutToggle;
      ToggleButtonClicked(RightAutoCashOutToggleButton);
      UpdateAutoCashoutInputInteractivity(false);
      UpdateTopBarInteractivityForAutoCashout(false);
    });

    SetupAutoCashoutInputs();
    UpdateTopBarInteractivityForAutoCashout(true);
    UpdateTopBarInteractivityForAutoCashout(false);

    clientSeedRandom = ClientSeedGenerator();
    clientSeedManual = ClientSeedGenerator();
    clientSeed = clientSeedRandom;
    serverSeed = Guid.NewGuid().ToString();
    if (provablyFairSettingsManager != null)
      provablyFairSettingsManager.Initialize(clientSeedRandom, clientSeedManual, serverSeed, useManual: false);
  }

  void Start()
  {
    InfoUIButtons[0].onClick.Invoke(); //Default to All Bets
  }

  private void Update()
  {
    if (curveAnimator == null || !curveAnimator.Flying)
      return;

    if (DOTween.IsTweening("multTween"))
      return;

    float timeSinceTick = Time.time - lastTickTime;
    if (timeSinceTick < lastTickInterval || lastMultVelocity <= 0f)
      return;

    if (extrapolationElapsed >= extrapolationMaxSeconds)
      return;

    extrapolationElapsed = Mathf.Min(extrapolationMaxSeconds, extrapolationElapsed + Time.deltaTime);
    float maxMult = targetMult + Mathf.Min(extrapolationMaxDelta, lastMultVelocity * extrapolationMaxSeconds);
    displayedMult = Mathf.Min(maxMult, displayedMult + lastMultVelocity * Time.deltaTime);
    UpdateMultiplierDisplay(displayedMult);
  }

  private void SetupAvatarButtons()
  {
    avatarSprites.Clear();

    if (avatarButtons != null && avatarButtons.Count > 0)
    {
      foreach (var button in avatarButtons)
      {
        if (button == null)
          continue;

        Image img = button.GetComponent<Image>();
        if (img == null)
          img = button.GetComponentInChildren<Image>();

        if (img == null || img.sprite == null)
          continue;

        Sprite sprite = img.sprite;
        avatarSprites.Add(sprite);
        button.onClick.AddListener(() => OnAvatarButtonClicked(sprite));
      }
    }

    if (avatarSprites.Count > 0)
    {
      profilePicImage.sprite = avatarSprites[UnityEngine.Random.Range(0, avatarSprites.Count)];
      return;
    }

    if (profilePicSprites != null && profilePicSprites.Length > 0)
      profilePicImage.sprite = profilePicSprites[UnityEngine.Random.Range(0, profilePicSprites.Length)];
  }

  private void SetProfilePicture(Sprite sprite)
  {
    if (sprite == null || profilePicImage == null)
      return;
    profilePicImage.sprite = sprite;
  }

  internal List<Sprite> GetProfileSprites()
  {
    if (avatarSprites.Count > 0)
      return avatarSprites;
    return new List<Sprite>(profilePicSprites);
  }

  internal Sprite GetRandomProfileSprite()
  {
    List<Sprite> sprites = GetProfileSprites();
    if (sprites == null || sprites.Count == 0)
      return null;
    return sprites[UnityEngine.Random.Range(0, sprites.Count)];
  }

  private void OnAvatarButtonClicked(Sprite sprite)
  {
    SetProfilePicture(sprite);
    CloseAvatarPanelIfOpen();
  }

  private void CloseAvatarPanelIfOpen()
  {
    if (OtherOptionsPanels == null)
      return;

    if (avatarPanelIndex < 0 || avatarPanelIndex >= OtherOptionsPanels.Length)
      return;

    CloseOtherOptionMenu(avatarPanelIndex);
  }

  internal void SetInit(List<float> bets, float bal, string username)
  {
    if (bets.Count <= 0)
    {
      Debug.LogError("Bets list is empty");
      return;
    }

    var staticBets = GetFourDistributedBetValuesWithIndices(bets);
    if (staticBets.Count <= 0)
    {
      Debug.LogError("Static Bet list is empty");
      return;
    }

    for (int i = 0; i < 4; i++)
    {
      int indexcopy = i;
      LeftStaticBetButtons[indexcopy].GetComponentInChildren<TMP_Text>().text = staticBets[indexcopy].value.ToString();
      LeftStaticBetButtons[indexcopy].onClick.AddListener(() => ChangeBet(staticBets[indexcopy].index, true));
      RightStaticBetButtons[indexcopy].GetComponentInChildren<TMP_Text>().text = staticBets[indexcopy].value.ToString();
      RightStaticBetButtons[indexcopy].onClick.AddListener(() => ChangeBet(staticBets[indexcopy].index, false));
    }

    LeftBetCounter = staticBets[0].index;
    LeftBetText.text = staticBets[0].value.ToString("N2");
    LeftBetButton.transform.GetChild(1).GetComponent<TMP_Text>().text = "Bet\n" + staticBets[0].value.ToString("N2") + " PKR";
    RightBetCounter = staticBets[0].index;
    RightBetText.text = staticBets[0].value.ToString("N2");
    RightBetButton.transform.GetChild(1).GetComponent<TMP_Text>().text = "Bet\n" + staticBets[0].value.ToString("N2") + " PKR";
    BalanceText.text = bal.ToString("N2");
    MinBetText.text = bets[0].ToString("N2");
    MaxBetText.text = bets[^1].ToString("N2");
    MaxCashoutText.text = (bets[^1] * socket.MaxMult).ToString("N2");
    maxAutoCashoutMultiplier = Mathf.Max(minAutoCashoutMultiplier, socket.MaxMult);
    leftAutoCashoutValue = ClampAutoCashoutValue(leftAutoCashoutValue);
    rightAutoCashoutValue = ClampAutoCashoutValue(rightAutoCashoutValue);
    RefreshAutoCashoutInputText(true);
    RefreshAutoCashoutInputText(false);
    if (username.Length > 0 && username.Length > 2)
    {
      PlayerNameText.text = username[0] + "****" + username[^1];
    }
    else
    {
      PlayerNameText.text = "Demo User";
    }
  }

  IEnumerator OnCancel(bool isLeft)
  {
    if (IsRequestInProgress(isLeft)) yield break;
    SetRequestInProgress(isLeft, true);

    if (isLeft && LeftAutoToggle)
    {
      LeftAutoToggle = false;
      leftAutoBetLockedIndex = -1;
      ToggleButtonClicked(LeftAutoBetToggleButton);
    }
    else if (!isLeft && RightAutoToggle)
    {
      RightAutoToggle = false;
      rightAutoBetLockedIndex = -1;
      ToggleButtonClicked(RightAutoBetToggleButton);
    }

    Audio.PlayButtonAudio();
    CancelData data;
    if (isLeft)
    {
      LeftBlocker.SetActive(true);
      data = new CancelData
      {
        type = "CANCEL_BET",
        payload = new CancelPayload
        {
          betIndex = LeftBetCounter,
          betId = leftBetData.payload.betId
        }
      };
    }
    else
    {
      RightBlocker.SetActive(true);
      data = new CancelData
      {
        type = "CANCEL_BET",
        payload = new CancelPayload
        {
          betIndex = RightBetCounter,
          betId = rightBetData.payload.betId
        }
      };
    }

    socket.CancelBet(data, isLeft);
    yield return new WaitUntil(() => (isLeft ? socket.leftAck.Key : socket.rightAck.Key) == true);

    AckData ackData = JsonUtility.FromJson<AckData>(isLeft ? socket.leftAck.Value : socket.rightAck.Value);

    if (!ackData.success)
    {
      popupManager.ShowPopup(false, ackData.payload.message);
    }
    else
    {
      if (isLeft)
      {
        ToggleBetButtons(state: true, isLeft: true);
        leftBetData = null;
        SetBalance(ackData.player.balance);
        LeftCancelBetButton.gameObject.SetActive(false);
        LeftCashoutButton.gameObject.SetActive(false);
        LeftBetButton.gameObject.SetActive(true);
      }
      else
      {
        ToggleBetButtons(state: true, isLeft: false);
        rightBetData = null;
        SetBalance(ackData.player.balance);
        RightCancelBetButton.gameObject.SetActive(false);
        RightCashoutButton.gameObject.SetActive(false);
        RightBetButton.gameObject.SetActive(true);
      }
    }

    yield return new WaitForSeconds(0.5f); // Delay before turning off blocker

    if (isLeft)
    {
      LeftBlocker.SetActive(false);
    }
    else
    {
      RightBlocker.SetActive(false);
    }

    SetRequestInProgress(isLeft, false);
  }

  IEnumerator OnCashout(bool isLeft)
  {
    if (IsRequestInProgress(isLeft)) yield break;
    SetRequestInProgress(isLeft, true);

    Audio.PlayButtonAudio();
    Debug.Log("OnCashout at: " + displayedMult);
    CashoutData data;
    if (isLeft)
    {
      LeftBlocker.SetActive(true);
      data = new CashoutData
      {
        type = "CASHOUT",
        payload = new CashoutPayload
        {
          betIndex = LeftBetCounter,
          betId = leftBetData.payload.betId
        }
      };
    }
    else
    {
      RightBlocker.SetActive(true);
      data = new CashoutData
      {
        type = "CASHOUT",
        payload = new CashoutPayload
        {
          betIndex = RightBetCounter,
          betId = rightBetData.payload.betId
        }
      };
    }

    socket.CashoutBet(data, isLeft);
    yield return new WaitUntil(() => (isLeft ? socket.leftAck.Key : socket.rightAck.Key) == true);

    AckData ackData = JsonUtility.FromJson<AckData>(isLeft ? socket.leftAck.Value : socket.rightAck.Value);

    if (!ackData.success)
    {
      popupManager.ShowPopup(false, ackData.payload.message);
    }
    else
    {
      Audio.PlayWinAudio();
      float winnings = ackData.payload.winAmount;
      popupManager.ShowPopup(true, "You won " + winnings.ToString("N2") + "!");

      if (isLeft)
      {
        leftBetData = null;
        ToggleBetButtons(state: true, isLeft: true);
        SetBalance(ackData.player.balance);
        LeftBetButton.gameObject.SetActive(true);
        LeftCashoutButton.gameObject.SetActive(false);
        LeftCancelBetButton.gameObject.SetActive(false);
      }
      else
      {
        rightBetData = null;
        ToggleBetButtons(state: true, isLeft: false);
        SetBalance(ackData.player.balance);
        RightBetButton.gameObject.SetActive(true);
        RightCashoutButton.gameObject.SetActive(false);
        RightCancelBetButton.gameObject.SetActive(false);
      }
    }

    yield return new WaitForSeconds(0.5f); // Delay before turning off blocker

    if (isLeft)
    {
      LeftBlocker.SetActive(false);
    }
    else
    {
      RightBlocker.SetActive(false);
    }

    SetRequestInProgress(isLeft, false);

    bool shouldAutoRebet = isLeft ? LeftAutoToggle : RightAutoToggle;
    if (shouldAutoRebet)
    {
      StartCoroutine(OnBet(isLeft));
    }
  }

  IEnumerator OnBet(bool isLeft)
  {
    if (isLeft)
    {
      LeftBetButton.gameObject.SetActive(false);
    }
    else
    {
      RightBetButton.gameObject.SetActive(false);
    }
    if (IsRequestInProgress(isLeft)) yield break;
    SetRequestInProgress(isLeft, true);

    Audio.PlayButtonAudio();
    BetData data;
    if (isLeft)
    {
      if (!CompareBalance(socket.bets[LeftBetCounter]))
      {
        SetRequestInProgress(isLeft, false); // Release throttling if balance is low
        yield break;
      }
      LeftBlocker.SetActive(true);
      data = new BetData
      {
        type = "BET",
        payload = new BetAmountData
        {
          betIndex = LeftBetCounter,
          clientSeed = this.clientSeed,
          betId = ""
        }
      };
    }
    else
    {
      if (!CompareBalance(socket.bets[RightBetCounter])) // Corrected to RightBetCounter
      {
        SetRequestInProgress(isLeft, false); // Release throttling if balance is low
        yield break;
      }
      RightBlocker.SetActive(true);
      data = new BetData
      {
        type = "BET",
        payload = new BetAmountData
        {
          betIndex = RightBetCounter,
          clientSeed = this.clientSeed,
          betId = ""
        }
      };
    }

    socket.PlaceBet(data, isLeft);
    yield return new WaitUntil(() => (isLeft ? socket.leftAck.Key : socket.rightAck.Key) == true);

    AckData ackData = JsonUtility.FromJson<AckData>(isLeft ? socket.leftAck.Value : socket.rightAck.Value);

    if (!ackData.success)
    {
      popupManager.ShowPopup(false, ackData.payload.message);
    }
    else
    {
      if (isLeft)
      {
        leftBetData = data;
        if (!ackData.payload.isUserInQueue)
        {
          LeftCancelBetButton.transform.GetChild(1).gameObject.SetActive(false);
          LeftCancelBetButton.transform.GetChild(2).gameObject.SetActive(true);
          LeftCancelBetButton.gameObject.SetActive(true);
        }
        else if (ackData.payload.isUserInQueue)
        {
          LeftCancelBetButton.transform.GetChild(1).gameObject.SetActive(true);
          LeftCancelBetButton.transform.GetChild(2).gameObject.SetActive(false);
          LeftCancelBetButton.gameObject.SetActive(true);
        }
      }
      else
      {
        rightBetData = data;
        if (!ackData.payload.isUserInQueue)
        {
          RightCancelBetButton.transform.GetChild(1).gameObject.SetActive(false);
          RightCancelBetButton.transform.GetChild(2).gameObject.SetActive(true);
          RightCancelBetButton.gameObject.SetActive(true);
        }
        else if (ackData.payload.isUserInQueue)
        {
          RightCancelBetButton.transform.GetChild(1).gameObject.SetActive(true);
          RightCancelBetButton.transform.GetChild(2).gameObject.SetActive(false);
          RightCancelBetButton.gameObject.SetActive(true);
        }
      }
      ToggleBetButtons(state: false, isLeft: isLeft); // ToggleBetButtons should be called here
      SetBalance(ackData.player.balance);
      if (isLeft)
      {
        leftBetData.payload.betId = ackData.payload.betId;
        leftBetData.serverHash = roundIdentifier;
      }
      else
      {
        rightBetData.payload.betId = ackData.payload.betId;
        rightBetData.serverHash = roundIdentifier;
      }
    }

    yield return new WaitForSeconds(0.5f); // Delay before turning off blocker

    if (isLeft)
    {
      LeftBlocker.SetActive(false);
    }
    else
    {
      RightBlocker.SetActive(false);
    }

    SetRequestInProgress(isLeft, false);
  }

  bool CompareBalance(float bet)
  {
    if (socket.balance < bet)
    {
      OpenPopup(lowBalancePopupGO);
      return false;
    }

    return true;
  }

  void ChangeBet(bool IncDec, bool isLeft)
  {
    if (isLeft)
    {
      if (IncDec)
      {
        if (LeftBetCounter + 1 >= socket.bets.Count)
        {
          LeftBetCounter = 0;
        }
        else
        {
          LeftBetCounter++;
        }
      }
      else
      {
        if (LeftBetCounter - 1 < 0)
        {
          LeftBetCounter = socket.bets.Count - 1;
        }
        else
        {
          LeftBetCounter--;
        }
      }
      LeftBetText.text = socket.bets[LeftBetCounter].ToString("N2");
      LeftBetButton.transform.GetChild(1).GetComponent<TMP_Text>().text = "Bet\n" + socket.bets[LeftBetCounter].ToString("N2") + " PKR";
    }
    else
    {
      if (IncDec)
      {
        if (RightBetCounter + 1 >= socket.bets.Count)
        {
          RightBetCounter = 0;
        }
        else
        {
          RightBetCounter++;
        }
      }
      else
      {
        if (RightBetCounter - 1 < 0)
        {
          RightBetCounter = socket.bets.Count - 1;
        }
        else
        {
          RightBetCounter--;
        }
      }
      RightBetText.text = socket.bets[RightBetCounter].ToString("N2");
      RightBetButton.transform.GetChild(1).GetComponent<TMP_Text>().text = "Bet\n" + socket.bets[RightBetCounter].ToString("N2") + " PKR";
    }
  }

  void ChangeBet(int index, bool isLeft)
  {
    if (isLeft)
    {
      LeftBetCounter = index;
      float bet = socket.bets[LeftBetCounter];
      LeftBetText.text = bet.ToString("N2");
      LeftBetButton.transform.GetChild(1).GetComponent<TMP_Text>().text = "Bet\n" + bet.ToString("N2") + " PKR";
      // Debug.Log(index + " " + bet);
    }
    else
    {
      RightBetCounter = index;
      float bet = socket.bets[RightBetCounter];
      RightBetText.text = bet.ToString("N2");
      RightBetButton.transform.GetChild(1).GetComponent<TMP_Text>().text = "Bet\n" + bet.ToString("N2") + " PKR";
    }
  }

  internal void OnTickerStart()
  {
    Audio.PlayTakeOffAudio();
    if (flewAwayText.gameObject.activeInHierarchy)
    {
      flewAwayText.gameObject.SetActive(false);
    }
    blurImage.enabled = true;
    blurImage.color = new Color(blueColor.r, blueColor.g, blueColor.b, 0f);
    curveAnimator.StartFlyingAnimation();
    multColorTween?.Kill();
    multiplierText.DOFade(1f, 0.3f).SetEase(Ease.OutSine);

    if (LeftCancelBetButton.gameObject.activeInHierarchy)
    {
      if (leftBetData.serverHash != roundIdentifier)
      {
        Debug.LogError("roundID not similar left bet data: " + leftBetData.serverHash + " roundIdentifier: " + roundIdentifier);
      }
      LeftCashoutButton.transform.GetChild(1).GetComponent<TMP_Text>().text = "Cashout\n" + (displayedMult * socket.bets[LeftBetCounter]).ToString("N2");
      LeftCashoutButton.gameObject.SetActive(true);
      LeftCancelBetButton.gameObject.SetActive(false);
      LeftBetButton.gameObject.SetActive(false);
    }
    if (RightCancelBetButton.gameObject.activeInHierarchy)
    {
      if (rightBetData.serverHash != roundIdentifier)
      {
        Debug.LogError("roundID not similar right bet data: " + leftBetData.serverHash + " roundIdentifier: " + roundIdentifier);
      }
      RightCashoutButton.transform.GetChild(1).GetComponent<TMP_Text>().text = "Cashout\n" + (displayedMult * socket.bets[RightBetCounter]).ToString("N2");
      RightCashoutButton.gameObject.SetActive(true);
      RightCancelBetButton.gameObject.SetActive(false);
      RightBetButton.gameObject.SetActive(false);
    }
  }

  internal void OnCrash(float crashMult, float CrashDuration)
  {
    LeftBlocker.SetActive(true);
    RightBlocker.SetActive(true);
    Audio.PlayCrashAudio();
    // Debug.Log("OnCrash");
    if (LeftCashoutButton.gameObject.activeInHierarchy)
    {
      leftBetData = null;

      ToggleBetButtons(true, true);
      LeftBetButton.gameObject.SetActive(true);

      LeftCancelBetButton.gameObject.SetActive(false);
      LeftCashoutButton.gameObject.SetActive(false);
    }
    if (RightCashoutButton.gameObject.activeInHierarchy)
    {
      rightBetData = null;

      ToggleBetButtons(true, false);
      RightBetButton.gameObject.SetActive(true);

      RightCancelBetButton.gameObject.SetActive(false);
      RightCashoutButton.gameObject.SetActive(false);
    }

    curveAnimator.OnCrash(crashMult);

    blueColTime = false;
    purpleColTime = false;
    pinkColTime = false;
    multColorTween?.Kill();
    DOTween.Kill("multTween");
    lastMultVelocity = 0f;
    extrapolationElapsed = 0f;

    displayedMult = crashMult;
    multiplierText.color = Color.red;
    multiplierText.text = crashMult.ToString("N2") + "x";

    flewAwayText.color = new Color(flewAwayText.color.r, flewAwayText.color.g, flewAwayText.color.b, 1f);
    flewAwayText.gameObject.SetActive(true);

    flewAwayText.DOFade(0, CrashDuration / 3).SetDelay(CrashDuration / 2).OnComplete(() => { flewAwayText.gameObject.SetActive(false); });
    multiplierText.DOFade(0, CrashDuration / 3).SetDelay(CrashDuration / 2);

    blurImage.DOKill();
    blurImage.color = new Color(blurImage.color.r, blurImage.color.g, blurImage.color.b, 0f);
    blurImage.enabled = false;
  }

  internal void OnRoundStart(float roundDuration, RoundStartData roundStartData)
  {
    LeftBlocker.SetActive(false);
    RightBlocker.SetActive(false);
    curveAnimator.ResetVisual();
    displayedMult = 1;
    targetMult = 1;
    lastMultVelocity = 0f;
    extrapolationElapsed = 0f;

    roundIdentifier = roundStartData.serverHash;

    foreach (Participant participant in roundStartData.participants)
    {
      if (participant.betId == leftBetData?.payload?.betId)
      {
        leftBetData.serverHash = roundIdentifier;
        if (LeftCancelBetButton.gameObject.activeInHierarchy && LeftCancelBetButton.transform.GetChild(0).gameObject.activeInHierarchy)
        {
          LeftCancelBetButton.transform.GetChild(1).gameObject.SetActive(false);
          LeftCancelBetButton.transform.GetChild(2).gameObject.SetActive(true);
        }
      }
      else if (participant.betId == rightBetData?.payload?.betId)
      {
        rightBetData.serverHash = roundIdentifier;
        if (RightCancelBetButton.gameObject.activeInHierarchy && RightCancelBetButton.transform.GetChild(0).gameObject.activeInHierarchy)
        {
          RightCancelBetButton.transform.GetChild(1).gameObject.SetActive(false);
          RightCancelBetButton.transform.GetChild(2).gameObject.SetActive(true);
        }
      }
    }

    flewAwayText.gameObject.SetActive(false);
    multiplierText.text = "1.00x";
    multiplierText.color = new Color(Color.white.r, Color.white.g, Color.white.b, 0f);
    blurImage.color = new Color(blueColor.r, blueColor.g, blueColor.b, 0f);

    float startDelay = roundDuration * 0.90f; // when 10% time remains
    float tweenDuration = roundDuration * 0.10f * 0.98f; // 98% of the last quarter

    loadingBar.SetActive(true);
    aviatorLogo.SetActive(true);
    spribeLogo.SetActive(true);
    loadingBarFillerImage.fillAmount = 1f;

    loadingBarFillerImage.DOFillAmount(0f, startDelay)
      .SetEase(Ease.Linear)
      .SetId("RoundLoadingTween")
      .OnComplete(() =>
      {
        aviatorLogo.SetActive(false);
        spribeLogo.SetActive(false);
        loadingBar.SetActive(false); // hide when done (optional)
      });

    multColorTween = multiplierText.DOFade(1f, tweenDuration)
        .SetDelay(startDelay);

    if (autoBetCoroutine != null)
    {
      StopCoroutine(autoBetCoroutine);
    }
    autoBetCoroutine = StartCoroutine(TryAutoBetForRoundStart());

    // Debug.Log($"🎬 OnRoundStart - delay={startDelay:N2}s, duration={tweenDuration:N2}s");
  }

  internal void OnMultiplierUpdate(float newMult, float tick)
  {
    float startVal = displayedMult;
    targetMult = newMult;
    lastTickInterval = Mathf.Max(minTickInterval, tick);
    lastTickTime = Time.time;
    lastMultVelocity = (newMult - startVal) / lastTickInterval;
    extrapolationElapsed = 0f;

    DOTween.Kill("multTween");

    if (!curveAnimator.Flying)
    {
      curveAnimator.StartFlyingAnimation();
    }
    curveAnimator.NotifyMultiplier(newMult);

    DOTween.To(() => startVal, v =>
    {
      displayedMult = v;
      UpdateMultiplierDisplay(v);
    }, newMult, tick)
    .SetId("multTween")
    .SetEase(Ease.Linear);
  }

  private void UpdateMultiplierDisplay(float mult)
  {
    curveAnimator.NotifyMultiplier(mult);
    multiplierText.text = mult.ToString("N2") + "x";

    if (LeftCashoutButton.gameObject.activeInHierarchy)
    {
      LeftCashoutButton.transform.GetChild(1).GetComponent<TMP_Text>().text = "Cashout\n" + (displayedMult * socket.bets[LeftBetCounter]).ToString("N2") + " PKR";
    }
    if (RightCashoutButton.gameObject.activeInHierarchy)
    {
      RightCashoutButton.transform.GetChild(1).GetComponent<TMP_Text>().text = "Cashout\n" + (displayedMult * socket.bets[RightBetCounter]).ToString("N2") + " PKR";
    }

    if (multiplierText.color.a <= 0.3f && multColorTween2 == null)
    {
      // Debug.Log("mult text white");
      multColorTween?.Kill();
      multColorTween2?.Kill();
      multColorTween2 = multiplierText.DOColor(Color.white, 0.3f).SetEase(Ease.OutSine);
    }

    if (mult <= 2f && !blueColTime)
    {
      // Debug.Log("blur color blue");
      blueColTime = true;
      blurTween?.Kill();
      blurTween = blurImage.DOColor(blueColor, 0.3f).SetEase(Ease.InSine);
    }
    else if (mult > 2f && mult < 10f && !purpleColTime)
    {
      // Debug.Log("blur color purple, mult: " + mult);
      purpleColTime = true;
      blurTween?.Kill();
      blurTween = blurImage.DOColor(purpleColor, 0.3f).SetEase(Ease.InSine);
    }
    else if (mult >= 10f && !pinkColTime)
    {
      // Debug.Log("blur color pink");
      pinkColTime = true;
      blurTween?.Kill();
      blurTween = blurImage.DOColor(pinkColor, 0.3f).SetEase(Ease.InSine);
    }

    TryAutoCashout(true);
    TryAutoCashout(false);
  }

  private IEnumerator TryAutoBetForRoundStart()
  {
    bool startedLeftBet = false;
    bool startedRightBet = false;

    if (LeftAutoToggle && !HasBetInProgress(true))
    {
      if (leftAutoBetLockedIndex >= 0 && leftAutoBetLockedIndex < socket.bets.Count)
      {
        ChangeBet(leftAutoBetLockedIndex, true);
      }
      startedLeftBet = true;
    }

    if (RightAutoToggle && !HasBetInProgress(false))
    {
      if (rightAutoBetLockedIndex >= 0 && rightAutoBetLockedIndex < socket.bets.Count)
      {
        ChangeBet(rightAutoBetLockedIndex, false);
      }
      startedRightBet = true;
    }

    if (startedLeftBet)
    {
      StartCoroutine(OnBet(true));
    }

    if (startedRightBet)
    {
      StartCoroutine(OnBet(false));
    }

    autoBetCoroutine = null;
    yield break;
  }

  private void HandleAutoBetToggle(bool isLeft)
  {
    if (isLeft)
    {
      if (LeftAutoToggle)
      {
        leftAutoBetLockedIndex = LeftBetCounter;
        ToggleBetButtons(false, true);
        if (!HasBetInProgress(true) && !IsRequestInProgress(true))
        {
          StartCoroutine(OnBet(true));
        }
      }
      else
      {
        leftAutoBetLockedIndex = -1;
        if (LeftCancelBetButton.gameObject.activeInHierarchy && !IsRequestInProgress(true))
        {
          StartCoroutine(OnCancel(true));
        }
        else if (!HasBetInProgress(true))
        {
          ToggleBetButtons(true, true);
          LeftCancelBetButton.gameObject.SetActive(false);
          LeftBetButton.gameObject.SetActive(true);
        }
      }
      return;
    }

    if (RightAutoToggle)
    {
      rightAutoBetLockedIndex = RightBetCounter;
      ToggleBetButtons(false, false);
      if (!HasBetInProgress(false) && !IsRequestInProgress(false))
      {
        StartCoroutine(OnBet(false));
      }
    }
    else
    {
      rightAutoBetLockedIndex = -1;
      if (RightCancelBetButton.gameObject.activeInHierarchy && !IsRequestInProgress(false))
      {
        StartCoroutine(OnCancel(false));
      }
      else if (!HasBetInProgress(false))
      {
        ToggleBetButtons(true, false);
        RightCancelBetButton.gameObject.SetActive(false);
        RightBetButton.gameObject.SetActive(true);
      }
    }

  }

  private void TryAutoCashout(bool isLeft)
  {
    if (isLeft)
    {
      if (IsRequestInProgress(true))
      {
        return;
      }

      if (!LeftAutoCashOutToggle || leftBetData == null || !LeftCashoutButton.gameObject.activeInHierarchy)
      {
        return;
      }

      if (displayedMult >= leftAutoCashoutValue)
      {
        StartCoroutine(OnCashout(true));
      }
      return;
    }

    if (IsRequestInProgress(false))
    {
      return;
    }

    if (!RightAutoCashOutToggle || rightBetData == null || !RightCashoutButton.gameObject.activeInHierarchy)
    {
      return;
    }

    if (displayedMult >= rightAutoCashoutValue)
    {
      StartCoroutine(OnCashout(false));
    }
  }

  private bool HasBetInProgress(bool isLeft)
  {
    if (isLeft)
    {
      bool hasBetData = leftBetData != null && leftBetData.payload != null && !string.IsNullOrEmpty(leftBetData.payload.betId);
      return hasBetData || LeftCancelBetButton.gameObject.activeInHierarchy || LeftCashoutButton.gameObject.activeInHierarchy;
    }

    bool rightHasBetData = rightBetData != null && rightBetData.payload != null && !string.IsNullOrEmpty(rightBetData.payload.betId);
    return rightHasBetData || RightCancelBetButton.gameObject.activeInHierarchy || RightCashoutButton.gameObject.activeInHierarchy;
  }

  private bool IsRequestInProgress(bool isLeft)
  {
    return isLeft ? leftRequestInProgress : rightRequestInProgress;
  }

  private void SetRequestInProgress(bool isLeft, bool state)
  {
    if (isLeft)
    {
      leftRequestInProgress = state;
      return;
    }

    rightRequestInProgress = state;
  }

  IEnumerator OtherOptionButtonClicked(int index)
  {
    if (!IsOtherOptionIndexValid(index))
      yield break;

    OtherOptionsMenu.SetActive(false);
    foreach (GameObject gameObject in OtherOptionsPanels)
    {
      gameObject.SetActive(false);
    }

    OtherOptionsPanelParent.SetActive(true);
    OtherOptionsPanels[index].SetActive(true);

    if (index == 0)
    {
      BetHistoryLoader.SetActive(true);
      socket.OnRequestBetHistory();
      yield return new WaitUntil(() => socket.BetHistAck);
      betHistoryManager.PopulateBetHistory();
      RefreshInfoScrollGates();
      BetHistoryLoader.SetActive(false);
    }
  }

  void CloseOtherOptionMenu(int index)
  {
    if (!IsOtherOptionIndexValid(index))
      return;

    OtherOptionsPanels[index].SetActive(false);
    OtherOptionsPanelParent.SetActive(false);
  }

  void CloseAllOtherOptionsMenu()
  {
    foreach (var panel in OtherOptionsPanels)
    {
      panel.SetActive(false);
    }
    OtherOptionsPanelParent.SetActive(false);
  }

  private bool IsOtherOptionIndexValid(int index)
  {
    if (OtherOptionsPanels == null)
      return false;
    return index >= 0 && index < OtherOptionsPanels.Length;
  }

  private void SetupAutoCashoutInputs()
  {
    leftAutoCashoutValue = ClampAutoCashoutValue(leftAutoCashoutValue);
    rightAutoCashoutValue = ClampAutoCashoutValue(rightAutoCashoutValue);

    if (LeftAutoCashoutInputField != null)
    {
      LeftAutoCashoutInputField.onEndEdit.AddListener(_ => OnAutoCashoutInputSubmitted(true));
      RefreshAutoCashoutInputText(true);
    }

    if (RightAutoCashoutInputField != null)
    {
      RightAutoCashoutInputField.onEndEdit.AddListener(_ => OnAutoCashoutInputSubmitted(false));
      RefreshAutoCashoutInputText(false);
    }

    UpdateAutoCashoutInputInteractivity(true);
    UpdateAutoCashoutInputInteractivity(false);
  }

  private void OnAutoCashoutInputSubmitted(bool isLeft)
  {
    if (isLeft)
    {
      leftAutoCashoutValue = ParseAndClampAutoCashout(LeftAutoCashoutInputField, leftAutoCashoutValue);
      RefreshAutoCashoutInputText(true);
      return;
    }

    rightAutoCashoutValue = ParseAndClampAutoCashout(RightAutoCashoutInputField, rightAutoCashoutValue);
    RefreshAutoCashoutInputText(false);
  }

  private void UpdateAutoCashoutInputInteractivity(bool isLeft)
  {
    if (isLeft)
    {
      if (LeftAutoCashoutInputField != null)
      {
        LeftAutoCashoutInputField.interactable = LeftAutoCashOutToggle;
      }
      return;
    }

    if (RightAutoCashoutInputField != null)
    {
      RightAutoCashoutInputField.interactable = RightAutoCashOutToggle;
    }
  }

  private void UpdateTopBarInteractivityForAutoCashout(bool isLeft)
  {
    Button[] topBarButtons = isLeft ? LeftTopBarButtons : RightTopBarButtons;
    bool autoCashoutEnabled = isLeft ? LeftAutoCashOutToggle : RightAutoCashOutToggle;
    bool autoPanelActive = isLeft ? LeftAutoBetPanel.activeInHierarchy : RightAutoBetPanel.activeInHierarchy;

    if (autoCashoutEnabled)
    {
      topBarButtons[0].interactable = false;
      topBarButtons[1].interactable = false;
      return;
    }

    if (autoPanelActive)
    {
      topBarButtons[0].interactable = true;
      topBarButtons[1].interactable = false;
    }
    else
    {
      topBarButtons[0].interactable = false;
      topBarButtons[1].interactable = true;
    }
  }

  private float ParseAndClampAutoCashout(TMP_InputField inputField, float fallbackValue)
  {
    if (inputField == null)
    {
      return ClampAutoCashoutValue(fallbackValue);
    }

    string input = inputField.text?.Trim();
    if (string.IsNullOrWhiteSpace(input))
    {
      return ClampAutoCashoutValue(fallbackValue);
    }

    if (!float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedValue) &&
        !float.TryParse(input, out parsedValue))
    {
      return ClampAutoCashoutValue(fallbackValue);
    }

    return ClampAutoCashoutValue(parsedValue);
  }

  private float ClampAutoCashoutValue(float value)
  {
    return Mathf.Clamp(value, minAutoCashoutMultiplier, maxAutoCashoutMultiplier);
  }

  private void RefreshAutoCashoutInputText(bool isLeft)
  {
    if (isLeft)
    {
      if (LeftAutoCashoutInputField != null)
      {
        LeftAutoCashoutInputField.text = leftAutoCashoutValue.ToString("N2");
      }
      return;
    }

    if (RightAutoCashoutInputField != null)
    {
      RightAutoCashoutInputField.text = rightAutoCashoutValue.ToString("N2");
    }
  }

  void ToggleButtonClicked(Button button)
  {
    button.interactable = false;
    RectTransform KnobRect = button.transform.GetChild(0).GetComponent<RectTransform>();
    KnobRect.DOAnchorPosX(-KnobRect.anchoredPosition.x, 0.2f).
    OnComplete(() => button.interactable = true);
  }

  void BetTopBarButtonClicked(int index, bool isLeft)
  {
    ButtonAnimation(index, isLeft ? LeftTopBarButtons : RightTopBarButtons);
    if (index == 0)
    {
      if (isLeft)
      {
        LeftAutoBetPanel?.SetActive(false);
        gridController.CheckAndFixLeftAutoBet(false);
      }
      else
      {
        RightAutoBetPanel?.SetActive(false);
        gridController.CheckAndFixRightAutoBet(false);
      }
    }
    else
    {
      if (isLeft)
      {
        LeftAutoBetPanel?.SetActive(true);
        gridController.CheckAndFixLeftAutoBet(true);
      }
      else
      {
        RightAutoBetPanel?.SetActive(true);
        gridController.CheckAndFixRightAutoBet(true);
      }
    }

    UpdateTopBarInteractivityForAutoCashout(isLeft);
  }

  void TopBetsButtonClicked(int index, bool isFilter, bool reqData = true)
  {
    ButtonAnimation(index, isFilter ? TopBetFilterButtons : TopBetTimeButtons);

    currentTopBetFilterIndex = isFilter ? index : currentTopBetFilterIndex;
    currentTopBetTimeIndex = isFilter ? currentTopBetTimeIndex : index;

    ShowTopBetsUI();
    if (reqData)
      StartCoroutine(ShowInfoUI(2));
  }

  void ShowTopBetsUI()
  {
    if (currentTopBetFilterIndex == 2)
    {
      if (!TopBetPanels[1].activeSelf)
      {
        TopBetPanels[1].SetActive(true);
        TopBetPanels[0].SetActive(false);
      }
    }
    else
    {
      if (!TopBetPanels[0].activeSelf)
      {
        TopBetPanels[0].SetActive(true);
        TopBetPanels[1].SetActive(false);
      }
    }
  }

  IEnumerator ShowInfoUI(int index)
  {
    ButtonAnimation(index, InfoUIButtons);
    foreach (GameObject p in InfoUIPanels)
    {
      p.SetActive(false);
    }

    if (index == 1)
    {
      InfoUIPanels[^1].SetActive(true);
      socket.SendPreviousRoundReq();
      yield return new WaitUntil(() => socket.PrevRoundAck);
      prevRoundManager.PopulatePreviousRounds();
      InfoUIPanels[^1].SetActive(false);
    }
    if (index == 2)
    {
      InfoUIPanels[^1].SetActive(true);
      socket.RequestRecordsData(currentTopBetTimeIndex, currentTopBetFilterIndex);
      yield return new WaitUntil(() => socket.ReceivedRecordAck);
      if (currentTopBetFilterIndex == 2)
      {
        roundAnalyticsManager.PopulateRoundAnalytics();
      }
      else
      {
        analyticsUIManager.PopulateAnalyticsUI();
      }
      InfoUIPanels[^1].SetActive(false);
    }

    RefreshInfoScrollGates();
    InfoUIPanels[index].SetActive(true);
  }

  private void RefreshInfoScrollGates()
  {
    if (infoScrollGates == null) return;
    foreach (var gate in infoScrollGates)
    {
      if (gate != null)
        StartCoroutine(gate.Refresh());
    }
  }

  void ButtonAnimation(int index, Button[] buttonArray)
  {
    foreach (Button button in buttonArray)
    {
      button.interactable = false;
      Image buttonImage = button.GetComponent<Image>();
      buttonImage.DOFade(0, 0.5f);
    }

    Image selectedButtonImage = buttonArray[index].GetComponent<Image>();
    selectedButtonImage.DOFade(1, 0.5f);

    foreach (Button button in buttonArray)
    {
      if (button != buttonArray[index])
      {
        button.interactable = true;
      }
    }
  }

  void ToggleBetButtons(bool state, bool isLeft)
  {
    bool shouldEnable = state && !(isLeft ? LeftAutoToggle : RightAutoToggle);

    if (isLeft)
    {
      foreach (Button btn in LLeftRightBetChangeButtons)
      {
        btn.interactable = shouldEnable;
      }
      foreach (Button btn in LeftStaticBetButtons)
      {
        btn.interactable = shouldEnable;
      }
    }
    else
    {
      foreach (Button btn in RLeftRightBetChangeButtons)
      {
        btn.interactable = shouldEnable;
      }
      foreach (Button btn in RightStaticBetButtons)
      {
        btn.interactable = shouldEnable;
      }
    }
  }

  void SetBalance(float bal)
  {
    DOTween.Kill("balanceTween");
    socket.balance = bal;
    // parse the current value (fallback to 0 if empty)
    float current = 0f;
    float.TryParse(BalanceText.text, out current);

    // animate from current → bal over 0.3s
    DOTween.To(() => current, x =>
    {
      current = x;
      BalanceText.text = current.ToString("N2"); // update text each frame
    },
    bal, 0.3f)
    .SetEase(Ease.OutQuad)
    .SetId("balanceTween");
  }


  private List<(int index, float value)> GetFourDistributedBetValuesWithIndices(List<float> allBets)
  {
    List<(int index, float value)> selected = new List<(int index, float value)>();

    if (allBets == null || allBets.Count == 0)
      return selected;

    // If too few bets, just take the first up to 4
    if (allBets.Count < 5)
    {
      for (int i = 0; i < Mathf.Min(4, allBets.Count); i++)
        selected.Add((i, allBets[i]));
      return selected;
    }

    int divisions = 5;
    int total = allBets.Count;

    // Select 4 roughly evenly spaced indices
    for (int i = 1; i < divisions; i++)
    {
      int index = Mathf.Clamp(Mathf.RoundToInt((float)i / divisions * total), 0, total - 1);
      selected.Add((index, allBets[index]));
    }

    // Remove duplicates (by index)
    selected = selected.GroupBy(x => x.index).Select(x => x.First()).OrderBy(x => x.index).ToList();

    // If less than 4 unique, fill missing from start
    while (selected.Count < 4 && selected.Count < allBets.Count)
    {
      int nextIndex = selected.Count;
      selected.Add((nextIndex, allBets[nextIndex]));
    }

    return selected;
  }

  string ClientSeedGenerator()
  {
    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    char[] seedChars = new char[16];

    for (int i = 0; i < seedChars.Length; i++)
    {
      int idx = RandomNumberGenerator.GetInt32(chars.Length);
      seedChars[i] = chars[idx];
    }

    string clientSeed = new string(seedChars);
    Debug.Log("Generated Client Seed: " + clientSeed);
    return clientSeed;
  }

  internal void SetActiveClientSeed(string seed)
  {
    if (string.IsNullOrEmpty(seed))
      return;
    clientSeed = seed;
  }

  internal void UpdateServerSeed(string seed)
  {
    if (string.IsNullOrEmpty(seed))
      return;
    serverSeed = seed;
    if (provablyFairSettingsManager != null)
      provablyFairSettingsManager.UpdateServerSeed(serverSeed);
  }

  internal void OpenProvablyFairPopupFromAnalytics(AnalyticsRecord record)
  {
    if (record == null)
      return;

    RoundDetails roundDetails = record.round_details;
    if (roundDetails == null)
      Debug.LogWarning("ProvablyFair popup: analytics `round_details` missing.");

    ProvablyFairPopupPayload payload = new()
    {
      roundId = record.round_id ?? "",
      multiplier = record.round_details.crashPoint > 0 ? record.round_details.crashPoint : record.multiplier,
      timestamp = record.created_at ?? "",
      serverSeed = roundDetails != null ? roundDetails.server_seed ?? "" : "",
      serverHash = roundDetails != null ? roundDetails.hash ?? "" : ""
    };

    if (roundDetails != null && roundDetails.user_ids != null)
    {
      for (int i = 0; i < roundDetails.user_ids.Count && i < provablyFairMaxUsers; i++)
      {
        payload.userIds.Add(roundDetails.user_ids[i] ?? "");
      }
    }

    if (payload.userIds.Count == 0 && !string.IsNullOrWhiteSpace(record.user_id))
      payload.userIds.Add(record.user_id);

    if (roundDetails != null && roundDetails.client_seeds != null)
    {
      for (int i = 0; i < roundDetails.client_seeds.Count && i < provablyFairMaxUsers; i++)
      {
        payload.clientSeeds.Add(roundDetails.client_seeds[i] ?? "");
      }
    }

    if (roundDetails != null && roundDetails.usedClientSeedRecords != null && roundDetails.usedClientSeedRecords.Count > 0)
    {
      payload.userIds.Clear();
      payload.clientSeeds.Clear();
      for (int i = 0; i < roundDetails.usedClientSeedRecords.Count && i < provablyFairMaxUsers; i++)
      {
        UsedClientSeedRecord seedRecord = roundDetails.usedClientSeedRecords[i];
        payload.userIds.Add(seedRecord != null ? seedRecord.userId ?? "" : "");
        payload.clientSeeds.Add(seedRecord != null ? seedRecord.seed ?? "" : "");
      }
    }

    WarnProvablyFairMissingFields(payload, "analytics");
    OpenProvablyFairPopup(payload);
  }

  internal void OpenProvablyFairPopupFromCrashHistory(CrashHistoryRoundData roundData)
  {
    if (roundData == null)
      return;

    ProvablyFairPopupPayload payload = new()
    {
      roundId = roundData.roundId ?? "",
      multiplier = roundData.crashPoint,
      timestamp = roundData.createdAt ?? "",
      serverSeed = roundData.serverSeed ?? "",
      serverHash = !string.IsNullOrWhiteSpace(roundData.combinedHash) ? roundData.combinedHash : (roundData.hash ?? "")
    };

    if (roundData.userIds != null)
    {
      for (int i = 0; i < roundData.userIds.Count && i < provablyFairMaxUsers; i++)
      {
        payload.userIds.Add(roundData.userIds[i] ?? "");
      }
    }

    if (roundData.clientSeeds != null)
    {
      for (int i = 0; i < roundData.clientSeeds.Count && i < provablyFairMaxUsers; i++)
      {
        payload.clientSeeds.Add(roundData.clientSeeds[i] ?? "");
      }
    }

    WarnProvablyFairMissingFields(payload, "crash history");
    OpenProvablyFairPopup(payload);
  }

  private void OpenProvablyFairPopup(ProvablyFairPopupPayload payload)
  {
    if (provablyFairPopupGO == null || payload == null)
      return;

    string roundId = payload.roundId ?? "";
    roundId = roundId.Trim();
    if (roundId.Length > 8)
      roundId = roundId.Substring(0, 8);

    if (provablyFairRoundIdText != null)
      provablyFairRoundIdText.text = "ROUND " + roundId;

    string multText = payload.multiplier > 0 ? payload.multiplier.ToString("N2") + "x" : "";
    if (provablyFairMultiplierText != null)
      provablyFairMultiplierText.text = multText;
    if (provablyFairCrashPointText != null)
      provablyFairCrashPointText.text = multText;

    if (provablyFairTimestampText != null)
      provablyFairTimestampText.text = FormatProvablyFairTimestamp(payload.timestamp);
    if (provablyFairServerSeedText != null)
      provablyFairServerSeedText.text = payload.serverSeed ?? "";

    string hashFull = payload.serverHash ?? "";
    if (string.IsNullOrWhiteSpace(hashFull) && !string.IsNullOrWhiteSpace(payload.serverSeed) && payload.clientSeeds.Count > 0)
      hashFull = ComputeSha512Hex(payload.serverSeed, payload.clientSeeds);

    string hashHex = GetNormalizedHashHex(hashFull);
    string hashHexPrefix = hashHex.Length > 13 ? hashHex.Substring(0, 13) : hashHex;
    if (provablyFairServerHashFullText != null)
      provablyFairServerHashFullText.text = hashHex;
    if (provablyFairServerHashHexText != null)
      provablyFairServerHashHexText.text = hashHexPrefix;
    if (provablyFairServerHashDecimalText != null)
      provablyFairServerHashDecimalText.text = ConvertHashHexToDecimal(hashHexPrefix);

    int rowCount = Mathf.Max(
      provablyFairUserIdTexts != null ? provablyFairUserIdTexts.Length : 0,
      provablyFairClientSeedTexts != null ? provablyFairClientSeedTexts.Length : 0);
    rowCount = Mathf.Max(rowCount, provablyFairProfileImages != null ? provablyFairProfileImages.Length : 0);
    rowCount = Mathf.Max(rowCount, provablyFairMaxUsers);

    for (int i = 0; i < rowCount; i++)
    {
      string userId = i < payload.userIds.Count ? payload.userIds[i] : "";
      string clientSeedValue = i < payload.clientSeeds.Count ? payload.clientSeeds[i] : "";
      bool hasData = !string.IsNullOrWhiteSpace(userId) && !string.IsNullOrWhiteSpace(clientSeedValue);

      if (provablyFairUserIdTexts != null && i < provablyFairUserIdTexts.Length && provablyFairUserIdTexts[i] != null)
        provablyFairUserIdTexts[i].text = hasData ? MaskProvablyFairUserId(userId) : "";

      if (provablyFairClientSeedTexts != null && i < provablyFairClientSeedTexts.Length && provablyFairClientSeedTexts[i] != null)
        provablyFairClientSeedTexts[i].text = hasData ? clientSeedValue : "";

      if (provablyFairProfileImages != null && i < provablyFairProfileImages.Length && provablyFairProfileImages[i] != null)
      {
        Image img = provablyFairProfileImages[i];
        if (!hasData)
        {
          img.sprite = null;
          img.enabled = false;
        }
        else
        {
          Sprite sprite = GetRandomProfileSprite();
          if (sprite == null && profilePicSprites != null && profilePicSprites.Length > 0)
            sprite = profilePicSprites[UnityEngine.Random.Range(0, profilePicSprites.Length)];
          img.sprite = sprite;
          img.enabled = true;
        }
      }
    }

    provablyFairPopupGO.SetActive(true);
  }

  private void CloseProvablyFairPopup()
  {
    if (provablyFairPopupGO != null)
      provablyFairPopupGO.SetActive(false);
  }

  internal void OpenProvablyFairInfoPopup()
  {
    if (provablyFairInfoPopupGO != null)
      provablyFairInfoPopupGO.SetActive(true);
  }

  private void CloseProvablyFairInfoPopup()
  {
    if (provablyFairInfoPopupGO != null)
      provablyFairInfoPopupGO.SetActive(false);
  }

  private string FormatProvablyFairTimestamp(string timestamp)
  {
    if (string.IsNullOrWhiteSpace(timestamp))
      return "";

    if (DateTime.TryParse(timestamp, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var utcTime))
      return utcTime.ToLocalTime().ToString("HH:mm:ss");

    if (DateTime.TryParse(timestamp, out var localTime))
      return localTime.ToString("HH:mm:ss");

    return "";
  }

  private string GetNormalizedHashHex(string rawHash)
  {
    if (string.IsNullOrWhiteSpace(rawHash))
      return "";

    string hash = rawHash.Trim();
    if (hash.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
      hash = hash.Substring(2);

    char[] chars = hash.Where(Uri.IsHexDigit).ToArray();
    return new string(chars);
  }

  private string ConvertHashHexToDecimal(string hashHex)
  {
    if (string.IsNullOrWhiteSpace(hashHex))
      return "";

    try
    {
      BigInteger value = BigInteger.Parse("0" + hashHex, NumberStyles.HexNumber);
      return value.ToString();
    }
    catch
    {
      return "";
    }
  }

  private string ComputeSha512Hex(string serverSeedValue, List<string> clientSeeds)
  {
    if (string.IsNullOrWhiteSpace(serverSeedValue) || clientSeeds == null || clientSeeds.Count == 0)
      return "";

    string combined = serverSeedValue + string.Concat(clientSeeds);
    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(combined);
    using (var sha = SHA512.Create())
    {
      byte[] hash = sha.ComputeHash(bytes);
      return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
  }

  private string MaskProvablyFairUserId(string userId)
  {
    if (string.IsNullOrWhiteSpace(userId))
      return "";

    string value = userId.Trim();
    if (value.Length <= 2)
      return value;

    return $"{value[0]}***{value[^1]}";
  }

  private void WarnProvablyFairMissingFields(ProvablyFairPopupPayload payload, string source)
  {
    List<string> missing = new();

    if (string.IsNullOrWhiteSpace(payload.roundId))
      missing.Add("roundId");
    if (payload.multiplier <= 0f)
      missing.Add("multiplier");
    if (string.IsNullOrWhiteSpace(payload.timestamp))
      missing.Add("timestamp");
    if (string.IsNullOrWhiteSpace(payload.serverSeed))
      missing.Add("serverSeed");
    if (string.IsNullOrWhiteSpace(payload.serverHash))
      missing.Add("hash");
    if (payload.userIds.Count == 0)
      missing.Add("userIds");
    if (payload.clientSeeds.Count == 0)
      missing.Add("clientSeeds");

    if (missing.Count > 0)
      Debug.LogWarning($"ProvablyFair popup ({source}): missing backend data -> {string.Join(", ", missing)}");
  }

  internal void CheckAndClosePopups()
  {
    if (ReconnectionPopupGO.activeInHierarchy)
    {
      ClosePopup(ReconnectionPopupGO);
    }
    else if (DisconnectionPopupGO.activeInHierarchy)
    {
      ClosePopup(DisconnectionPopupGO);
    }
  }

  internal void ReconnectionPopup()
  {
    if (!isUserExit)
      OpenPopup(ReconnectionPopupGO);
  }

  internal void DisconnectionPopup()
  {
    if (!isUserExit)
      OpenPopup(DisconnectionPopupGO);
  }

  void ClosePopup(GameObject popup)
  {
    blocker.SetActive(false);
    popup.SetActive(false);
  }

  void OpenPopup(GameObject popup)
  {
    blocker.SetActive(true);
    popup.SetActive(true);
  }

  internal void ResetGame()
  {
    CloseProvablyFairPopup();

    // Reset Multiplier
    multiplierText.text = "1.00x";
    multiplierText.color = Color.white;
    flewAwayText.gameObject.SetActive(false);
    blurImage.enabled = false;

    // Reset Loading Bar
    loadingBar.SetActive(false);
    loadingBarFillerImage.fillAmount = 0f;

    // Reset Bet Buttons
    ToggleBetButtons(true, true);
    LeftBetButton.gameObject.SetActive(true);
    LeftCancelBetButton.gameObject.SetActive(false);
    LeftCashoutButton.gameObject.SetActive(false);
    LeftBlocker.SetActive(false);

    ToggleBetButtons(true, false);
    RightBetButton.gameObject.SetActive(true);
    RightCancelBetButton.gameObject.SetActive(false);
    RightCashoutButton.gameObject.SetActive(false);
    RightBlocker.SetActive(false);

    // Reset Bet Data
    leftBetData = null;
    rightBetData = null;
    leftRequestInProgress = false;
    rightRequestInProgress = false;

    if (autoBetCoroutine != null)
    {
      StopCoroutine(autoBetCoroutine);
      autoBetCoroutine = null;
    }

    curveAnimator.ResetVisual();

    // Kill any running tweens
    DOTween.Kill("multTween");
    DOTween.Kill("multColorTween");
    DOTween.Kill("multColorTween2");
    DOTween.Kill("blurTween");
    DOTween.Kill("RoundLoadingTween");
  }

  private void OnApplicationFocus(bool hasFocus)
  {
    if (hasFocus)
    {
      if (socket.CurrentState == SocketIOManager.AviatorState.TickerStart)
      {
        blurImage.enabled = true;
        UpdateMultiplierDisplay(displayedMult);
      }
    }
  }
}
