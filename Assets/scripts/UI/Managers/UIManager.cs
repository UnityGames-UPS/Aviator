using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
  [SerializeField] private CurveManager curveAnimator;
  [SerializeField] private SocketIOManager socket;
  [SerializeField] private AnalyticsUIManager analyticsUIManager;
  [SerializeField] private RoundAnalyticsManager roundAnalyticsManager;
  [SerializeField] private PrevRoundManager prevRoundManager;
  [SerializeField] private BetHistoryManager betHistoryManager;
  [SerializeField] private AudioManager Audio;

  [SerializeField] private PopupManager popupManager;
  private bool requestInProgress = false;

  [SerializeField] private Image profilePicImage;
  [SerializeField] private Sprite[] profilePicSprites;

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
  [SerializeField] private Image loadingBarFillerImage;

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
  //0: Bet History
  //1: Game Limits 
  //2: How To Play
  //3: Game Rules
  [SerializeField] private Button[] OtherOptionsButtons;
  [SerializeField] private GameObject[] OtherOptionsPanels;
  [SerializeField] private Button[] OtherOptionCloseButtons;
  [SerializeField] private Button CloseOtherOptionButton;
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

  [Header("Info UI")]
  //0: All bets panel
  //1: Previous bets panel
  //2: Top bets panel
  //3. Loading panel
  [SerializeField] private GameObject[] InfoUIPanels;
  [SerializeField] private Button[] InfoUIButtons;
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

  private void Awake()
  {
    profilePicImage.sprite = profilePicSprites[Random.Range(0, profilePicSprites.Length)];
    lowBalanceCloseButton.onClick.AddListener(() => ClosePopup(lowBalancePopupGO));

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

    InfoUIButtons[0].onClick.AddListener(() => StartCoroutine(ShowInfoUI(0)));
    InfoUIButtons[1].onClick.AddListener(() => StartCoroutine(ShowInfoUI(1)));
    InfoUIButtons[2].onClick.AddListener(() => StartCoroutine(ShowInfoUI(2)));

    InfoUIButtons[0].onClick.Invoke(); //Default to All Bets

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
    });
    RightAutoBetToggleButton.onClick.AddListener(() =>
    {
      RightAutoToggle = !RightAutoToggle;
      ToggleButtonClicked(RightAutoBetToggleButton);
    });

    LeftAutoCashOutToggleButton.onClick.AddListener(() =>
    {
      LeftAutoCashOutToggle = !LeftAutoCashOutToggle;
      ToggleButtonClicked(LeftAutoCashOutToggleButton);
    });
    RightAutoCashOutToggleButton.onClick.AddListener(() =>
    {
      RightAutoCashOutToggle = !RightAutoCashOutToggle;
      ToggleButtonClicked(RightAutoCashOutToggleButton);
    });

    clientSeed = ClientSeedGenerator();
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
      LeftStaticBetButtons[indexcopy].GetComponentInChildren<TMP_Text>().text = staticBets[indexcopy].value.ToString("N2");
      LeftStaticBetButtons[indexcopy].onClick.AddListener(() => ChangeBet(staticBets[indexcopy].index, true));
      RightStaticBetButtons[indexcopy].GetComponentInChildren<TMP_Text>().text = staticBets[indexcopy].value.ToString("N2");
      RightStaticBetButtons[indexcopy].onClick.AddListener(() => ChangeBet(staticBets[indexcopy].index, false));
    }

    LeftBetCounter = staticBets[0].index;
    LeftBetText.text = staticBets[0].value.ToString("N2");
    LeftBetButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Bet\n" + staticBets[0].value.ToString("N2");
    RightBetCounter = staticBets[0].index;
    RightBetText.text = staticBets[0].value.ToString("N2");
    RightBetButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Bet\n" + staticBets[0].value.ToString("N2");
    BalanceText.text = bal.ToString("N2");
    MinBetText.text = bets[0].ToString("N2");
    MaxBetText.text = bets[^1].ToString("N2");
    MaxCashoutText.text = (bets[^1] * socket.MaxMult).ToString("N2");
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
    if (requestInProgress) yield break;
    requestInProgress = true;

    Audio.PlayButtonAudio();
    CancelData data;
    if (isLeft)
    {
      socket.leftAck = new KeyValuePair<bool, string>(false, "wait");
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
      socket.rightAck = new KeyValuePair<bool, string>(false, "wait");
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

    socket.CancelBet(data);
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

    requestInProgress = false;
  }

  IEnumerator OnCashout(bool isLeft)
  {
    if (requestInProgress) yield break;
    requestInProgress = true;

    Audio.PlayButtonAudio();
    Debug.Log("OnCashout at: " + displayedMult);
    CashoutData data;
    if (isLeft)
    {
      socket.leftAck = new KeyValuePair<bool, string>(false, "wait");
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
      socket.rightAck = new KeyValuePair<bool, string>(false, "wait");
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

    socket.CashoutBet(data);
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
        ToggleBetButtons(state: true, isLeft: true);
        SetBalance(ackData.player.balance);
        LeftBetButton.gameObject.SetActive(true);
        LeftCashoutButton.gameObject.SetActive(false);
        LeftCancelBetButton.gameObject.SetActive(false);
      }
      else
      {
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

    requestInProgress = false;
  }

  IEnumerator OnBet(bool isLeft)
  {
    if (requestInProgress) yield break;
    requestInProgress = true;

    Audio.PlayButtonAudio();
    BetData data;
    if (isLeft)
    {
      if (!CompareBalance(socket.bets[LeftBetCounter]))
      {
        requestInProgress = false; // Release throttling if balance is low
        yield break;
      }
      socket.leftAck = new KeyValuePair<bool, string>(false, "wait");
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
        requestInProgress = false; // Release throttling if balance is low
        yield break;
      }
      socket.rightAck = new KeyValuePair<bool, string>(false, "wait");
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

    socket.PlaceBet(data);
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
          LeftCancelBetButton.transform.GetChild(0).gameObject.SetActive(false);
          LeftCancelBetButton.transform.GetChild(1).gameObject.SetActive(true);
          LeftCancelBetButton.gameObject.SetActive(true);
        }
        else if (ackData.payload.isUserInQueue)
        {
          LeftCancelBetButton.transform.GetChild(0).gameObject.SetActive(true);
          LeftCancelBetButton.transform.GetChild(1).gameObject.SetActive(false);
          LeftCancelBetButton.gameObject.SetActive(true);
        }
      }
      else
      {
        rightBetData = data;
        if (!ackData.payload.isUserInQueue)
        {
          RightCancelBetButton.transform.GetChild(0).gameObject.SetActive(false);
          RightCancelBetButton.transform.GetChild(1).gameObject.SetActive(true);
          RightCancelBetButton.gameObject.SetActive(true);
        }
        else if (ackData.payload.isUserInQueue)
        {
          RightCancelBetButton.transform.GetChild(0).gameObject.SetActive(true);
          RightCancelBetButton.transform.GetChild(1).gameObject.SetActive(false);
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

    requestInProgress = false;
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
      LeftBetButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Bet\n" + socket.bets[LeftBetCounter].ToString("N2");
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
      RightBetButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Bet\n" + socket.bets[RightBetCounter].ToString("N2");
    }
  }

  void ChangeBet(int index, bool isLeft)
  {
    if (isLeft)
    {
      LeftBetCounter = index;
      float bet = socket.bets[LeftBetCounter];
      LeftBetText.text = bet.ToString("N2");
      LeftBetButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Bet\n" + bet.ToString("N2");
      Debug.Log(index + " " + bet);
    }
    else
    {
      RightBetCounter = index;
      float bet = socket.bets[RightBetCounter];
      RightBetText.text = bet.ToString("N2");
      RightBetButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Bet\n" + bet.ToString("N2");
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
      LeftCashoutButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Cashout\n" + (displayedMult * socket.bets[LeftBetCounter]).ToString("N2");
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
      RightCashoutButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Cashout\n" + (displayedMult * socket.bets[RightBetCounter]).ToString("N2");
      RightCashoutButton.gameObject.SetActive(true);
      RightCancelBetButton.gameObject.SetActive(false);
      RightBetButton.gameObject.SetActive(false);
    }
  }

  internal void OnCrash(float crashMult, float CrashDuration)
  {
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
    curveAnimator.OnCrash();

    blueColTime = false;
    purpleColTime = false;
    pinkColTime = false;
    multColorTween?.Kill();
    DOTween.Kill("multTween");

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
    curveAnimator.ResetVisual();
    displayedMult = 1;

    roundIdentifier = roundStartData.serverHash;

    foreach (Participant participant in roundStartData.participants)
    {
      if (participant.betId == leftBetData?.payload?.betId)
      {
        leftBetData.serverHash = roundIdentifier;
        if (LeftCancelBetButton.gameObject.activeInHierarchy && LeftCancelBetButton.transform.GetChild(0).gameObject.activeInHierarchy)
        {
          LeftCancelBetButton.transform.GetChild(0).gameObject.SetActive(false);
          LeftCancelBetButton.transform.GetChild(1).gameObject.SetActive(true);
        }
      }
      else if (participant.betId == rightBetData?.payload?.betId)
      {
        rightBetData.serverHash = roundIdentifier;
        if (RightCancelBetButton.gameObject.activeInHierarchy && RightCancelBetButton.transform.GetChild(0).gameObject.activeInHierarchy)
        {
          RightCancelBetButton.transform.GetChild(0).gameObject.SetActive(false);
          RightCancelBetButton.transform.GetChild(1).gameObject.SetActive(true);
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
    loadingBarFillerImage.fillAmount = 1f;

    loadingBarFillerImage.DOFillAmount(0f, startDelay)
      .SetEase(Ease.Linear)
      .SetId("RoundLoadingTween")
      .OnComplete(() =>
      {
        loadingBar.SetActive(false); // hide when done (optional)
      });

    multColorTween = multiplierText.DOFade(1f, tweenDuration)
        .SetDelay(startDelay);

    // Debug.Log($"🎬 OnRoundStart - delay={startDelay:N2}s, duration={tweenDuration:N2}s");
  }

  internal void OnMultiplierUpdate(float newMult, float tick)
  {
    float startVal = displayedMult;
    targetMult = newMult;

    DOTween.Kill("multTween");

    DOTween.To(() => startVal, v =>
    {
      displayedMult = v;
      UpdateMultiplierDisplay(v);
    }, newMult, tick)
    .SetId("multTween")
    .SetEase(Ease.Linear);

    if (!curveAnimator.Flying)
    {
      curveAnimator.StartFlyingAnimation();
    }
  }

  private void UpdateMultiplierDisplay(float mult)
  {
    multiplierText.text = mult.ToString("N2") + "x";

    if (LeftCashoutButton.gameObject.activeInHierarchy)
    {
      LeftCashoutButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Cashout\n" + (displayedMult * socket.bets[LeftBetCounter]).ToString("N2");
    }
    if (RightCashoutButton.gameObject.activeInHierarchy)
    {
      RightCashoutButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Cashout\n" + (displayedMult * socket.bets[RightBetCounter]).ToString("N2");
    }

    if (multiplierText.color.a <= 0.3f && multColorTween2 == null)
    {
      // Debug.Log("mult text white");
      multColorTween?.Kill();
      multColorTween2?.Kill();
      multColorTween2 = multiplierText.DOColor(Color.white, 0.3f).SetEase(Ease.OutSine);
    }

    if (mult <= 3.8f && !blueColTime)
    {
      // Debug.Log("blur color blue");
      blueColTime = true;
      blurTween?.Kill();
      blurTween = blurImage.DOColor(blueColor, 0.3f).SetEase(Ease.InSine);
    }
    else if (mult > 3.8f && !purpleColTime)
    {
      // Debug.Log("blur color purple, mult: " + mult);
      purpleColTime = true;
      blurTween?.Kill();
      blurTween = blurImage.DOColor(purpleColor, 0.3f).SetEase(Ease.InSine);
    }
    else if (mult > 10 && !pinkColTime)
    {
      // Debug.Log("blur color pink");
      pinkColTime = true;
      blurTween?.Kill();
      blurTween = blurImage.DOColor(pinkColor, 0.3f).SetEase(Ease.InSine);
    }
  }

  IEnumerator OtherOptionButtonClicked(int index)
  {
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
      BetHistoryLoader.SetActive(false);
    }
  }

  void CloseOtherOptionMenu(int index)
  {
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

  void ToggleButtonClicked(Button button)
  {
    button.interactable = false;
    RectTransform KnobRect = button.transform.GetChild(0).GetComponent<RectTransform>();
    KnobRect.DOLocalMoveX(-KnobRect.localPosition.x, 0.2f).
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
      }
      else
      {
        RightAutoBetPanel?.SetActive(false);
      }
    }
    else
    {
      if (isLeft)
      {
        LeftAutoBetPanel?.SetActive(true);
      }
      else
      {
        RightAutoBetPanel?.SetActive(true);
      }
    }
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

    InfoUIPanels[index].SetActive(true);
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
    if (isLeft)
    {
      foreach (Button btn in LLeftRightBetChangeButtons)
      {
        btn.interactable = state;
      }
      foreach (Button btn in LeftStaticBetButtons)
      {
        btn.interactable = state;
      }
    }
    else
    {
      foreach (Button btn in RLeftRightBetChangeButtons)
      {
        btn.interactable = state;
      }
      foreach (Button btn in RightStaticBetButtons)
      {
        btn.interactable = state;
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
    if (isUserExit)
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
