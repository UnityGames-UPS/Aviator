using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

internal class PopupManager : MonoBehaviour
{
  [SerializeField] private GameObject popupObject;
  [SerializeField] private TMP_Text popupText;
  [SerializeField] private Image statusImage;
  [SerializeField] private Sprite[] statusImages;
  [SerializeField] private Color greenColor;
  private static readonly string[] DummyPopupMessages =
  {
    "Test popup: Connection restored.",
    "Test popup: Bet placed.",
    "Test popup: Cashout failed.",
    "Test popup: Balance updated.",
    "Test popup: You won 12.34!",
    "Test popup: Try again."
  };
  private ProceduralImage PopupImage;
  private CanvasGroup popupCanvasGroup;
  private RectTransform popupRectTransform;
  private float initYposi;
  [SerializeField] private float animationDuration = 0.5f;

  private void Awake()
  {
    // Hide the popup initially
    if (popupObject != null)
    {
      popupObject.SetActive(false);

      popupRectTransform = popupObject.GetComponent<RectTransform>();
      initYposi = popupRectTransform.localPosition.y;

      PopupImage = popupObject.GetComponent<ProceduralImage>();
      popupCanvasGroup = popupObject.GetComponent<CanvasGroup>();
      popupCanvasGroup.alpha = 0;
    }
  }

#if UNITY_EDITOR
  // private void Update()
  // {
  //   if (Input.GetKeyDown(KeyCode.Space))
  //   {
  //     ShowRandomDummyPopup();
  //   }
  // }

  // private void ShowRandomDummyPopup()
  // {
  //   bool status = Random.value > 0.5f;
  //   string message = DummyPopupMessages[Random.Range(0, DummyPopupMessages.Length)];
  //   ShowPopup(status, message);
  // }
#endif

  internal void ShowPopup(bool status, string message)
  {
    if (popupObject == null) return;

    PopupImage.color = status ? greenColor : Color.black;
    statusImage.sprite = statusImages[status ? 0 : 1];
    popupText.text = message;

    AnimatePopup();
  }

  private void AnimatePopup()
  {
    if (popupObject.activeSelf)
    {
      popupCanvasGroup.DOKill();
      popupRectTransform.DOKill();
      popupRectTransform.localPosition = new(popupRectTransform.localPosition.x, initYposi, popupRectTransform.localPosition.z);
      popupObject.SetActive(false);
      popupCanvasGroup.alpha=0;
    }
    popupObject.SetActive(true);
    popupCanvasGroup.DOFade(1, animationDuration);
    popupRectTransform.DOLocalMoveY(initYposi - 100f, animationDuration).SetEase(Ease.OutBack)
    .OnComplete(() =>
    {
      popupCanvasGroup.DOFade(0, animationDuration/2).SetDelay(animationDuration + animationDuration/2);
      popupRectTransform.DOLocalMoveY(popupRectTransform.localPosition.y - 100f, animationDuration).SetEase(Ease.InBack).SetDelay(animationDuration);
    });
  }
}
