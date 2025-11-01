using DG.Tweening;
using TMPro;
using UnityEngine;

public class CrashHistoryView : MonoBehaviour
{
  [SerializeField] private TMP_Text multiplierText;
  [SerializeField] private RectTransform rect;
  private Vector3 originalScale;

  internal RectTransform Rect => rect;

  void OnValidate()
  {
    if (!multiplierText) multiplierText = GetComponent<TMP_Text>();
    if (!rect) rect = GetComponent<RectTransform>();
  }

  void Awake()
  {
    if (!multiplierText) multiplierText = GetComponent<TMP_Text>();
    if (!rect) rect = GetComponent<RectTransform>();
    originalScale = rect.localScale;
  }

  internal void SetValue(float multiplier, bool resetTransforms = false)
  {
    multiplierText.text = multiplier.ToString("F2") + "x";

    // colors (optional)
  if (multiplier < 2f) multiplierText.color = new(Color.blue.r, Color.blue.g, Color.blue.b, 0);

    if (resetTransforms)
    {
      rect.localScale = originalScale;
      multiplierText.alpha = 1f;
      // DO NOT force anchoredPosition here on init; layout will place it.
    }
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
