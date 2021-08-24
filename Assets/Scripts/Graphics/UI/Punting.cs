using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Punting : MonoBehaviour
{
    public Transform puntMenu;

    public static event Action PuntEvent;
    public static event Action NoPuntEvent;

    void Start()
    {
        GameLogic.ShowPuntEvent += ShowPunt;
    }

    private void OnDestroy()
    {
        GameLogic.ShowPuntEvent -= ShowPunt;
    }

    private void ShowPunt()
    {
        Debug.Log("Punt Choice");
        GameLogic.inActivePlay = false;
        puntMenu.gameObject.SetActive(true);
    }

    public void ChosePunt()
    {
        PuntEvent?.Invoke();
        puntMenu.gameObject.SetActive(false);
    }
   

    public void NoPunt()
    {
        NoPuntEvent?.Invoke();
        puntMenu.gameObject.SetActive(false);
    } 

   
}
