using UnityEngine;
using DG.Tweening;

public class LoadingAnimation : MonoBehaviour
{
  [SerializeField] private RectTransform targetObject;
  [SerializeField] private float rotationSpeed = 180f; // degrees per second
  private Tween rotationTween;

  private void OnEnable()
  {
    if (targetObject == null) return;

    targetObject.DOKill();

    rotationTween = targetObject
      .DORotate(new Vector3(0f, 0f, -360f), 360f / rotationSpeed, RotateMode.FastBeyond360)
      .SetEase(Ease.Linear)
      .SetLoops(-1, LoopType.Restart);
  }

  private void OnDisable()
  {
    if (rotationTween != null && rotationTween.IsActive())
      rotationTween.Kill();

    if (targetObject != null)
      targetObject.localRotation = Quaternion.identity;
  }
}
