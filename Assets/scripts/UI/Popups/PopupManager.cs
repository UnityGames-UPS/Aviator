using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
  [SerializeField] private GameObject popupObject;
  [SerializeField] private TMP_Text popupText;
  [SerializeField] private Image statusImage;
  [SerializeField] private Sprite[] statusImages;
  [SerializeField] private Color greenColor;
  private Image PopupImage;
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

      PopupImage = popupObject.GetComponent<Image>();
      popupCanvasGroup = popupObject.GetComponent<CanvasGroup>();
      popupCanvasGroup.alpha = 0;
    }
  }

  public void ShowPopup(bool status, string message)
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
