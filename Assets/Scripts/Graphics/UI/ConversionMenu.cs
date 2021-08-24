using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ConversionMenu : MonoBehaviour
{

    public GameObject menu;
    public static event Action OnePointConversionEvent;
    public static event Action TwoPointConversionEvent;

    private void Start()
    {
        PieceMovement.TouchDownEvent += ShowMenu;
    }

    private void OnDestroy()
    {
        PieceMovement.TouchDownEvent -= ShowMenu;
    }

    private void ShowMenu()
    {
        menu.gameObject.SetActive(true);
        //To make sure that the system knows it's not in active play
        GameLogic.inActivePlay = false;
    }

    public void OnePointConversionSelected()
    {
        menu.gameObject.SetActive(false);
        OnePointConversionEvent?.Invoke();
    }

    public void TwoPOintConversionSelected()
    {
        menu.gameObject.SetActive(false);
        TwoPointConversionEvent?.Invoke();
    }
}
