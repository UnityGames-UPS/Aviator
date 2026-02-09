using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AdvancedInputFieldPlugin.InputFieldDropdown;

public class FTDStateDropdownItem : MonoBehaviour
{
    DropdownItem thisItem;

    private void OnEnable()
    {
        StartCoroutine(SetItemColor());
    }

    IEnumerator SetItemColor()
    {
        float timeout = 5;
        while (thisItem == null && timeout > 0)
        {
            thisItem = this.GetComponent<DropdownItem>();
            timeout -= Time.deltaTime;
            yield return null;
        }

        if(thisItem != null)
        {
            if (thisItem.disabledText)
            {
                thisItem.text.color = Color.black;
                thisItem.image.enabled = true;
                thisItem.image.color = new Color(0, 0, 0, 0.2f);
            }
            else
            {
                thisItem.text.color = Color.black;
            }
        }
        else
        {
            Debug.Log("DropdownItem is null");
        }
    }
}