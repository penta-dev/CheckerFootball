using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using System;

public class TurnTimer : MonoBehaviour
{

    public TextMeshProUGUI text;
    public TextMeshProUGUI gameTimerText;
    public Image timerTurnDIsplay;

    public static event Action TimerRanOutEvent;

    public static bool timerOn;

    public double timerStartStamp;

    public double gameTimerStartStamp;
    
    private PhotonView view;

    private void Awake()
    {
        timerOn = false;
        timerStartStamp = Time.time;
        if(PhotonNetwork.OfflineMode)
        {
            gameTimerStartStamp = Time.time;
        } else
        {
            SetGameTimer(Photon.Pun.PhotonNetwork.Time);
        }
    }

    private void OnDestroy()
    {
        GameLogic.ChangedTurnEvent -= ResetTimer;
        GameLogic.EnteringGameplayEvent -= ResetTimer;
        GameLogic.EnteringPlacementEvent -= NoTimer;

        timerOn = false;
        timerStartStamp = Time.time;
    }

    void Start()
    {
        GameLogic.ChangedTurnEvent += ResetTimer;
        GameLogic.EnteringGameplayEvent += ResetTimer;
        GameLogic.EnteringPlacementEvent += NoTimer;
    }

    private void ResetTimer()
    {
        SetTimer(Photon.Pun.PhotonNetwork.Time);
    }

    [PunRPC]
    void SetTimer(double serverTime)
    {
        timerOn = true;
        timerStartStamp = serverTime;
        if (Photon.Pun.PhotonNetwork.IsMasterClient)
        {
            //Set other client
            view = PhotonView.Get(this);
            view.RPC("SetTimer", RpcTarget.Others, timerStartStamp);
        }
    }

    [PunRPC]
    void SetGameTimer(double serverTime)
    {
        gameTimerStartStamp = serverTime;
        if (Photon.Pun.PhotonNetwork.IsMasterClient)
        {
            //Set other client
            view = PhotonView.Get(this);
            view.RPC("SetGameTimer", RpcTarget.Others, gameTimerStartStamp);
        }
    }

    private void NoTimer()
    {
        timerOn = false;
        text.text = "-";
        timerTurnDIsplay.color = Color.gray;
    }

    void Update()
    {
        float gameTimer = 0;

        if (GameLogic.gameType == GameType.Offline)
        {
            gameTimer = (60 * 30) - (float)(Time.time - gameTimerStartStamp);
        } else
        {
            gameTimer = (60 * 30) - (float)(Photon.Pun.PhotonNetwork.Time - gameTimerStartStamp);
        }

        if (gameTimer <= 0)
        {
            Debug.Log("GAME OVER DUE TO TIMER");
            Debug.Log(GameLogic.gameType == GameType.Offline);
            ScoreBoard.GameTimerZero();
        }

        string minutes = Mathf.Floor(gameTimer / 60).ToString("00");
        string seconds = (gameTimer % 60).ToString("00");
        
        gameTimerText.text = string.Format("{0}:{1}", minutes, seconds);

        //Cancel timer if not in play requiring timer / And wipe text
        if (!GameLogic.inActivePlay || !timerOn)
        {
            NoTimer();
            return;
        }

        if (TurnLogic.currentTurn == TeamType.TeamOne) timerTurnDIsplay.color = Color.black;
        else timerTurnDIsplay.color = Color.red;

        //J- Timer is hard coded to 20Sec but can be changed
        double timerCurrent;

        if (GameLogic.gameType == GameType.Offline)
        {
            timerCurrent = 20 - (Photon.Pun.PhotonNetwork.Time - timerStartStamp);
        }
        else
        {
            timerCurrent = 20 - (Photon.Pun.PhotonNetwork.Time - timerStartStamp);
        }

        if (timerCurrent >= 0)
        {
            text.text = (Mathf.CeilToInt((float)timerCurrent)).ToString();
        } else
        {
            if (TurnLogic.IsMyTurn) TimerRanOutEvent?.Invoke();
        }
    }
}
