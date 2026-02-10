using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CrashHistoryView : MonoBehaviour
{
  [SerializeField] private TMP_Text multiplierText;
  [SerializeField] private RectTransform rect;
  [SerializeField] private Button detailsButton;
  [SerializeField] private Color PurpleColor;
  private Vector3 originalScale;
  private CrashHistoryRoundData roundData;

  internal RectTransform Rect => rect;

  void OnValidate()
  {
    if (!multiplierText) multiplierText = GetComponent<TMP_Text>();
    if (!rect) rect = GetComponent<RectTransform>();
    if (!detailsButton) detailsButton = GetComponent<Button>();
  }

  void Awake()
  {
    if (!multiplierText) multiplierText = GetComponent<TMP_Text>();
    if (!rect) rect = GetComponent<RectTransform>();
    if (!detailsButton) detailsButton = GetComponent<Button>();
    originalScale = rect.localScale;

    if (detailsButton != null)
      detailsButton.onClick.AddListener(OpenProvablyFairPopup);
  }

  internal void SetData(CrashHistoryRoundData data, bool resetTransforms = false)
  {
    roundData = data;
    float multiplier = data != null ? data.crashPoint : 0f;
    multiplierText.text = multiplier.ToString("N2") + "x";

    // colors (optional)
    if (multiplier <= 3.8f) multiplierText.color = new(Color.blue.r, Color.blue.g, Color.blue.b, 0);
    else multiplierText.color = PurpleColor;

    if (resetTransforms)
    {
      rect.localScale = originalScale;
      multiplierText.alpha = 1f;
      // DO NOT force anchoredPosition here on init; layout will place it.
    }
  }

  private void OpenProvablyFairPopup()
  {
    if (roundData == null)
      return;

    if (UIManager.Instance != null)
      UIManager.Instance.OpenProvablyFairPopupFromCrashHistory(roundData);
  }

  internal void PrepareSpawnVisual()
  {
    rect.localScale = originalScale * 1.25f;
    multiplierText.alpha = 0f;
    rect.DOKill();
    multiplierText.DOKill();
  }

  internal Tween DOScaleToOne(float duration, Ease ease)
  {
    return rect.DOScale(originalScale, duration).SetEase(ease);
  }

  internal Tween DOFadeInText(float duration)
  {
    return multiplierText.DOFade(1f, duration).SetEase(Ease.OutSine);
  }
}
