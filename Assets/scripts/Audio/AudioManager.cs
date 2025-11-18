using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AudioManager : MonoBehaviour
{
  [SerializeField] private AudioSource BGAudioSource;
  [SerializeField] private AudioSource ButtonAudioSource;
  [SerializeField] private AudioSource CrashAudioSource;
  [SerializeField] private AudioSource TakeOffAudioSource;
  [SerializeField] private AudioSource WinAudioSource;
  private float bgAudioVol = 0;
  private float buttonAudioVol = 0;
  private float crashNDtakeoffAudioVol = 0;

  private const float fadeDuration = 0.2f;

  void Awake()
  {
    bgAudioVol = BGAudioSource.volume;
    buttonAudioVol = ButtonAudioSource.volume;
    crashNDtakeoffAudioVol = CrashAudioSource.volume;
  }

  internal void ToggleBGAudio(bool toggle)
  {
    if (BGAudioSource == null) return;

    BGAudioSource.DOKill(); // stop previous tweens

    if (toggle)
    {
      // Fade in
      if (!BGAudioSource.isPlaying)
        BGAudioSource.Play();

      BGAudioSource.DOFade(bgAudioVol, fadeDuration).OnComplete(() =>
      {
        BGAudioSource.mute = false;
      });
    }
    else
    {
      // Fade out
      BGAudioSource.DOFade(0f, fadeDuration).OnComplete(() =>
      {
        BGAudioSource.Pause();
      });
    }
  }

  internal void ToggleSoundsAudio(bool toggle)
  {
    FadeAudio(ButtonAudioSource, toggle);
    FadeAudio(CrashAudioSource, toggle);
    FadeAudio(TakeOffAudioSource, toggle);
    FadeAudio(WinAudioSource, toggle);
  }

  private void FadeAudio(AudioSource source, bool enable)
  {
    if (source == null) return;

    source.DOKill();

    if (enable)
    {
      source.mute = false;
      if (source == ButtonAudioSource)
      {
        source.DOFade(buttonAudioVol, fadeDuration);
      }
      else
      {
        source.DOFade(crashNDtakeoffAudioVol, fadeDuration);
      }
    }
    else
    {
      source.DOFade(0f, fadeDuration).OnComplete(() =>
      {
        source.mute = true;
      });
    }
  }

  internal void PlayButtonAudio()
  {
    if (!ButtonAudioSource.mute)
      ButtonAudioSource.Play();
  }

  internal void PlayTakeOffAudio()
  {
    if (!TakeOffAudioSource.mute)
      TakeOffAudioSource.Play();
  }

  internal void PlayCrashAudio()
  {
    if (!CrashAudioSource.mute)
      CrashAudioSource.Play();
  }

  internal void PlayWinAudio()
  {
    if(!WinAudioSource.mute)
      WinAudioSource.Play();
  }
}
