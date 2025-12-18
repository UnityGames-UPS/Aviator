using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCountManager : MonoBehaviour
{
  [SerializeField] private TMP_Text playerCountText;
  [SerializeField] private Image[] profilePicImages;
  [SerializeField] private Sprite[] availableProfilePics;

  private int playerCount;

  internal void UpdatePlayerCount(int count)
  {
    playerCount = count;
    playerCountText.text = playerCount.ToString();
    UpdateProfilePictures();
  }

  private void UpdateProfilePictures()
  {
    // Create a list of available sprites to choose from
    List<Sprite> availableSprites = new List<Sprite>(availableProfilePics);

    // Hide all images first
    foreach (var image in profilePicImages)
    {
      image.transform.parent.gameObject.SetActive(false);
    }

    // Determine how many images to show (max 3)
    int imagesToShow = Mathf.Min(playerCount, profilePicImages.Length);
    imagesToShow = Mathf.Min(imagesToShow, availableProfilePics.Length);

    // Show and randomize the required number of images
    for (int i = 0; i < imagesToShow; i++)
    {
      int randomIndex = Random.Range(0, availableSprites.Count);
      profilePicImages[i].sprite = availableSprites[randomIndex];
      profilePicImages[i].transform.parent.gameObject.SetActive(true);

      // Remove the chosen sprite to avoid duplicates in one update
      availableSprites.RemoveAt(randomIndex);
    }
  }
}
