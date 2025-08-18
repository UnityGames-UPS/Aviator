using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
  [Header("Left Bet UI")]
  //0: Bet Button
  //1: Auto Bet Button
  [SerializeField] private Button[] LeftTopBarButtons;
  [SerializeField] private GameObject LeftAutoBetPanel;
  [SerializeField] private Button LeftAutoBetToggleButton;
  [SerializeField] private Button LeftAutoCashOutToggleButton;

  [Header("Right Bet UI")]
  //0: Bet Button
  //1: Auto Bet Button
  [SerializeField] private Button[] RightTopBarButtons;
  [SerializeField] private GameObject RightAutoBetPanel;
  [SerializeField] private Button RightAutoBetToggleButton;
  [SerializeField] private Button RightAutoCashOutToggleButton;

  [Header("Local variable to keep track")]
  [SerializeField] private bool LeftAutoToggle;
  [SerializeField] private bool LeftAutoCashOutToggle;
  [SerializeField] private bool RightAutoToggle;
  [SerializeField] private bool RightAutoCashOutToggle;

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

  private void Awake()
  {
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

    TopBetFilterButtons[0].onClick.Invoke(); //Default to x Button
    TopBetTimeButtons[0].onClick.Invoke(); //Default to Day Button

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
  }

  void ToggleButtonClicked(Button button)
  {
    button.interactable = false;
    RectTransform KnobRect = button.transform.GetChild(0).GetComponent<RectTransform>();
    KnobRect.DOLocalMoveX(-KnobRect.localPosition.x, 0.2f).
    OnComplete(()=> button.interactable = true);
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

    // InfoUIPanels[^1].SetActive(true);
    // yield return new WaitForSeconds(1f); //Replace later with actual response time
    // InfoUIPanels[^1].SetActive(false);
    yield return null;

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
}
