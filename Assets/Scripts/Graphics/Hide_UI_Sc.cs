using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hide_UI_Sc : MonoBehaviour
{
    public GameObject uiToHide;

    public Sprite shownImage;
    public Sprite hiddenImage;

    public void toggleUI()
    {
        if(uiToHide.activeSelf)
        {
            uiToHide.SetActive(false);
            GetComponent<Image>().sprite = hiddenImage;
        } else
        {
            uiToHide.SetActive(true);
            GetComponent<Image>().sprite = shownImage;
        }
    }
}
