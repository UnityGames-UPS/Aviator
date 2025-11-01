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
  [SerializeField] private CurveAnimator curveAnimator;
  [SerializeField] private SocketIOManager socket;

  [Header("Multiplier Objects and Values")]
  [SerializeField] private TMP_Text multiplierText;
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
  //0: Bet History
  //1: Game Limits 
  //2: How To Play
  //3: Game Rules
  [SerializeField] private Button[] OtherOptionsButtons;
  [SerializeField] private GameObject[] OtherOptionsPanels;
  [SerializeField] private Button[] OtherOptionCloseButtons;

  [Header("Popups")]
  [SerializeField] private GameObject blocker;
  [SerializeField] private GameObject lowBalancePopup;
  [SerializeField] private Button lowBalanceCloseButton;

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
  private Tween multColorTween;
  private Tween multColorTween2;
  private Tween blurTween;
  [SerializeField] internal BetData leftBetData;
  [SerializeField] internal BetData rightBetData;

  private void Awake()
  {
    lowBalanceCloseButton.onClick.AddListener(() => ClosePopup(lowBalancePopup));

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
    HomeButton.onClick.AddListener(() => socket.CloseGame());

    CloseOptionsMenuButton1.onClick.Invoke(); //Close Other Options Menu By Default

    SoundToggleButton.onClick.AddListener(() =>
    {
      SoundToggle = !SoundToggle;
      ToggleButtonClicked(SoundToggleButton);
    });
    MusicToggleButton.onClick.AddListener(() =>
    {
      MusicToggle = !MusicToggle;
      ToggleButtonClicked(MusicToggleButton);
    });
    AnimationToggleButton.onClick.AddListener(() =>
    {
      AnimationToggle = !AnimationToggle;
      ToggleButtonClicked(AnimationToggleButton);
    });

    for (int i = 0; i < OtherOptionsButtons.Length; i++)
    {
      int index = i;
      OtherOptionsButtons[i].onClick.AddListener(() => OtherOptionButtonClicked(index));
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

  internal void SetInit(List<float> bets, float bal)
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
      LeftStaticBetButtons[indexcopy].GetComponentInChildren<TMP_Text>().text = staticBets[indexcopy].value.ToString("F2");
      LeftStaticBetButtons[indexcopy].onClick.AddListener(() => ChangeBet(staticBets[indexcopy].index, true));
      RightStaticBetButtons[indexcopy].GetComponentInChildren<TMP_Text>().text = staticBets[indexcopy].value.ToString("F2");
      RightStaticBetButtons[indexcopy].onClick.AddListener(() => ChangeBet(staticBets[indexcopy].index, false));
    }

    LeftBetCounter = staticBets[0].index;
    LeftBetText.text = staticBets[0].value.ToString("F2");
    LeftBetButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Bet\n" + staticBets[0].value.ToString("F2");
    RightBetCounter = staticBets[0].index;
    RightBetText.text = staticBets[0].value.ToString("F2");
    RightBetButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Bet\n" + staticBets[0].value.ToString("F2");
    BalanceText.text = bal.ToString("F2");
  }

  IEnumerator OnCancel(bool isLeft)
  {
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
    // Debug.Log("Acknowledgement processed for " + (isLeft ? "Left" : "Right") + " Cancel");

    if (isLeft)
    {
      AckData ackData = JsonUtility.FromJson<AckData>(socket.leftAck.Value);
      if (!ackData.success)
      {
        Debug.LogError("Left Cancel failed: " + ackData.payload.message);
        yield break;
      }
      ToggleBetButtons(state: true, isLeft: true);
      leftBetData = null;
      SetBalance(ackData.player.balance);
      LeftCancelBetButton.gameObject.SetActive(false);
      LeftCashoutButton.gameObject.SetActive(false);
      LeftBetButton.gameObject.SetActive(true);
      LeftBlocker.SetActive(false);
    }
    else
    {
      AckData ackData = JsonUtility.FromJson<AckData>(socket.rightAck.Value);
      if (!ackData.success)
      {
        Debug.LogError("Right Cancel failed: " + ackData.payload.message);
        yield break;
      }
      ToggleBetButtons(state: true, isLeft: false);
      rightBetData = null;
      SetBalance(ackData.player.balance);
      RightCancelBetButton.gameObject.SetActive(false);
      RightCashoutButton.gameObject.SetActive(false);
      RightBetButton.gameObject.SetActive(true);
      RightBlocker.SetActive(false);
    }
  }

  IEnumerator OnCashout(bool isLeft)
  {
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
    // Debug.Log("Acknowledgement processed for " + (isLeft ? "Left" : "Right") + " Cashout");

    if (isLeft)
    {
      AckData ackData = JsonUtility.FromJson<AckData>(socket.leftAck.Value);
      if (!ackData.success)
      {
        Debug.LogError("Left Cashout failed: " + ackData.payload.message);
        yield break;
      }
      ToggleBetButtons(state: true, isLeft: true);
      SetBalance(ackData.player.balance);
      LeftBetButton.gameObject.SetActive(true);
      LeftCashoutButton.gameObject.SetActive(false);
      LeftCancelBetButton.gameObject.SetActive(false);
      LeftBlocker.SetActive(false);
    }
    else
    {
      AckData ackData = JsonUtility.FromJson<AckData>(socket.rightAck.Value);
      if (!ackData.success)
      {
        Debug.LogError("Right Cashout failed: " + ackData.payload.message);
        yield break;
      }
      ToggleBetButtons(state: true, isLeft: false);
      SetBalance(ackData.player.balance);
      RightBetButton.gameObject.SetActive(true);
      RightCashoutButton.gameObject.SetActive(false);
      RightCancelBetButton.gameObject.SetActive(false);
      RightBlocker.SetActive(false);
    }
  }

  IEnumerator OnBet(bool isLeft)
  {
    BetData data;
    if (isLeft)
    {
      if (!CompareBalance(socket.bets[LeftBetCounter]))
      {

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
    // Debug.Log("Acknowledgement processed for " + (isLeft ? "Left" : "Right") + " Bet");
    if (isLeft)
    {
      AckData ackData = JsonUtility.FromJson<AckData>(socket.leftAck.Value);
      if (!ackData.success)
      {
        Debug.LogError("Left Bet failed: " + ackData.payload.message);
        yield break;
      }
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
      ToggleBetButtons(state: false, isLeft: true);
      SetBalance(ackData.player.balance);
      leftBetData.payload.betId = ackData.payload.betId;
      leftBetData.serverHash = roundIdentifier;
      LeftBlocker.SetActive(false);
    }
    else
    {
      AckData ackData = JsonUtility.FromJson<AckData>(socket.rightAck.Value);
      if (!ackData.success)
      {
        Debug.LogError("Right Bet failed: " + ackData.payload.message);
        yield break;
      }
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
      ToggleBetButtons(state: false, isLeft: false);
      SetBalance(ackData.player.balance);
      rightBetData.payload.betId = ackData.payload.betId;
      rightBetData.serverHash = roundIdentifier;
      RightBlocker.SetActive(false);
    }
  }

  bool CompareBalance(float bet)
  {
    if (socket.balance < bet)
    {
      OpenPopup(lowBalancePopup);
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
      LeftBetText.text = socket.bets[LeftBetCounter].ToString("F2");
      LeftBetButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Bet\n" + socket.bets[LeftBetCounter].ToString("F2");
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
      RightBetText.text = socket.bets[RightBetCounter].ToString("F2");
      RightBetButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Bet\n" + socket.bets[RightBetCounter].ToString("F2");
    }
  }

  void ChangeBet(int index, bool isLeft)
  {
    if (isLeft)
    {
      LeftBetCounter = index;
      float bet = socket.bets[LeftBetCounter];
      LeftBetText.text = bet.ToString("F2");
      LeftBetButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Bet\n" + bet.ToString("F2");
      Debug.Log(index + " " + bet);
    }
    else
    {
      RightBetCounter = index;
      float bet = socket.bets[RightBetCounter];
      RightBetText.text = bet.ToString("F2");
      RightBetButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Bet\n" + bet.ToString("F2");
    }
  }

  internal void OnTickerStart()
  {
    multColorTween?.Kill();
    multiplierText.DOFade(1f, 0.3f).SetEase(Ease.OutSine);

    if (LeftCancelBetButton.gameObject.activeInHierarchy)
    {
      if (leftBetData.serverHash != roundIdentifier)
      {
        Debug.LogError("roundID not similar left bet data: " + leftBetData.serverHash + " roundIdentifier: " + roundIdentifier);
      }
      LeftCashoutButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Cashout\n" + (displayedMult * socket.bets[LeftBetCounter]).ToString("F2");
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
      RightCashoutButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Cashout\n" + (displayedMult * socket.bets[RightBetCounter]).ToString("F2");
      RightCashoutButton.gameObject.SetActive(true);
      RightCancelBetButton.gameObject.SetActive(false);
      RightBetButton.gameObject.SetActive(false);
    }
  }

  internal void OnCrash(float crashMult, float CrashDuration)
  {
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

    LeftBlocker.SetActive(true);
    RightBlocker.SetActive(true);

    blueColTime = false;
    purpleColTime = false;
    pinkColTime = false;
    multColorTween?.Kill();

    displayedMult = crashMult;
    multiplierText.color = Color.red;
    multiplierText.text = crashMult.ToString("F2") + "x";

    multiplierText.DOFade(0, CrashDuration / 3).SetDelay(CrashDuration / 2);
    blurTween?.Kill();
    blurImage.color = new Color(blueColor.r, blueColor.g, blueColor.b, 0f);
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

    LeftBlocker.SetActive(false);
    RightBlocker.SetActive(false);

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

    // Debug.Log($"🎬 OnRoundStart - delay={startDelay:F2}s, duration={tweenDuration:F2}s");
  }

  internal void OnMultiplierUpdate(float newMult, float tick)
  {
    curveAnimator.OnMultiplierUpdate(newMult, tick);

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
  }

  private void UpdateMultiplierDisplay(float mult)
  {
    multiplierText.text = mult.ToString("F2") + "x";

    if (LeftCashoutButton.gameObject.activeInHierarchy)
    {
      LeftCashoutButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Cashout\n" + (displayedMult * socket.bets[LeftBetCounter]).ToString("F2");
    }
    if (RightCashoutButton.gameObject.activeInHierarchy)
    {
      RightCashoutButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Cashout\n" + (displayedMult * socket.bets[RightBetCounter]).ToString("F2");
    }

    if (multiplierText.color.a <= 0.3f && multColorTween2 == null)
    {
      // Debug.Log("mult text white");
      multColorTween?.Kill();
      multColorTween2?.Kill();
      multColorTween2 = multiplierText.DOColor(Color.white, 0.3f).SetEase(Ease.OutSine);
    }

    if (mult < socket.takeoffEnd && !blueColTime)
    {
      // Debug.Log("blur color blue");
      blueColTime = true;
      blurTween?.Kill();
      blurTween = blurImage.DOColor(blueColor, 0.3f).SetEase(Ease.InSine);
    }
    else if (mult > socket.takeoffEnd && !purpleColTime)
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

  void OtherOptionButtonClicked(int index)
  {
    foreach (GameObject gameObject in OtherOptionsPanels)
    {
      gameObject.SetActive(false);
    }
    OtherOptionsPanelParent.SetActive(true);
    OtherOptionsPanels[index].SetActive(true);
  }

  void CloseOtherOptionMenu(int index)
  {
    OtherOptionsPanels[index].SetActive(false);
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

  void TopBetsButtonClicked(int index, bool isFilter)
  {
    ButtonAnimation(index, isFilter ? TopBetFilterButtons : TopBetTimeButtons);

    currentTopBetFilterIndex = isFilter ? index : currentTopBetFilterIndex;
    currentTopBetTimeIndex = isFilter ? currentTopBetTimeIndex : index;

    ShowTopBetsUI();
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
      InfoUIPanels[^1].SetActive(false);
    }
    if (index == 2)
    {
      InfoUIPanels[^1].SetActive(true);
      socket.RequestRecordsData(currentTopBetTimeIndex, currentTopBetFilterIndex);
      yield return new WaitUntil(() => socket.ReceivedRecordAck);
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
      BalanceText.text = current.ToString("0.00"); // update text each frame
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

  void ClosePopup(GameObject popup)
  {
    blocker.SetActive(false);
    popup.SetActive(false);
  }

  void OpenPopup(GameObject popup)
  {
    blocker.SetActive(false);
    popup.SetActive(false);
  }
}
