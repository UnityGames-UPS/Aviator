using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VisualScripting;

public class ButtonTextHoverAnim : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
  private TMP_Text buttonText;
  private Button button;
  private Color originalColor;
  private void OnValidate()
  {
    if(!buttonText) buttonText = GetComponentInChildren<TMP_Text>();
    if(!button) button = GetComponent<Button>();
  }

  private void Awake()
  {
    if (!buttonText) buttonText = GetComponentInChildren<TMP_Text>();
    if (!button) button = GetComponent<Button>();
    originalColor = buttonText.color;
  }

  public void OnPointerEnter(PointerEventData eventData)
  {
    if (buttonText == null) return;
    if (button.interactable == false) return;

    buttonText.DOColor(Color.red, 0.5f).SetEase(Ease.OutCubic);
  }

  public void OnPointerDown(PointerEventData eventData)
  {
    if (buttonText == null) return;
    
    buttonText.DOColor(originalColor, 0.5f).SetEase(Ease.OutCubic);
  }

  public void OnPointerExit(PointerEventData eventData)
  {
    if (buttonText == null) return;

    if (buttonText.color != originalColor)
      buttonText.DOColor(originalColor, 0.5f).SetEase(Ease.OutCubic);
  }
}
