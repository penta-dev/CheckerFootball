using System;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using CheckerFootball.Models;
using TMPro;

[RequireComponent(typeof(PhotonView))]
public class GameLogic : MonoBehaviour
{
    public TextMeshProUGUI roomNameText;
    public TextMeshProUGUI otherPlayerStatusText;

    public MessageHandler messaging;
    public static GameType gameType;

    private PhotonView view;    

    public static ITileSelection selectionState;

    public static PiecePlacement piecePlacement;
    public static PieceMovement pieceMovement;

    public static event Action EnterFreeplayEvent;   

    int teamFinishedPlacing = 0;
    public static int blockCount = 0;
    int tackleCounter;

    public static bool inFreePlay;
    public static bool inOnePointConversion;
    public static bool inTwoPointConversion;
    public static bool inActivePlay;
    public static event Action ResetToKickoffEvent;   
    public static event Action<TeamType, string> SetTeamScoreEvent;
    public static event Action NewDownEvent;   
    //public static event Action ResetToNewDownEvent;
    public static event Action ClearBoardEvent;
    public static event Action ChangedTurnEvent;
    public static event Action EnteringPlacementEvent;
    public static event Action EnteringGameplayEvent;
    public static event Action<int, int, bool> GameEndedEvent;
    public static event Action ShowPuntEvent;

    private int teamOneScore;
    private int teamTwoScore;

    public Transform tileHolderObj;
    public Transform pieceHolderObj;
    public Transform logoObj;

    public static Player[] playerList;

    bool overideWin = false;
    bool overideLose = false;

    bool kickoffPlacement;

    float selfDisconnectionTimer;
    float otherDisconnectionTimer;

    public InterstitialAdsSc interstitialAdsSc;

    float firstAdTimer;

    public TextMeshProUGUI otherPlayerDisconnectionText;
    public TextMeshProUGUI thisPlayerDisconnectionText;

    public TurnTimer turnTimer_Sc;

    #region Setup

    private void Awake()
    {
        Application.targetFrameRate = 60;

        kickoffPlacement = true;

        gameType = new GameType();
        selectionState = null;
        piecePlacement = null;
        pieceMovement = null;
        blockCount = 0;
        playerList = null;
        inFreePlay = false;
        inOnePointConversion = false;
        inTwoPointConversion = false;
        inActivePlay = false;

        playerList = PhotonNetwork.PlayerList;
        inActivePlay = false;
        if (PhotonNetwork.OfflineMode) gameType = GameType.Offline;
        Setup();
        SetTeam();   
        //Set board rotation relative to player
        if(TurnLogic.myTeam == TeamType.TeamTwo)
        {
            tileHolderObj.Rotate(0, 0, 180);
            pieceHolderObj.Rotate(0, 0, 180);
        }
        firstAdTimer = 2;
    }

    public void OnDestroy()
    {
        PieceMovement.TackledEvent -= PieceTackled;
        PieceMovement.QBTackledEvent -= QBTackled;
        SpawnGrid.TileHeldEvent -= TestHold;
        PieceModel.ClaimedBallEvent -= PickupFumbleCheck;
        ConversionMenu.OnePointConversionEvent -= StartOnePointConversion;
        ConversionMenu.TwoPointConversionEvent -= StartTwoPointConversion;
        BallLogic.ScoredOnePointConversion -= OnePointConversionSuccess;
        BallLogic.FailedOnePointConversion -= OnePointConversionFailed;
        TurnTimer.TimerRanOutEvent -= NoMoveMade;
        ScoreBoard.TimerZeroEvent -= GameEnd;
        Punting.PuntEvent -= Punt;
        Punting.NoPuntEvent -= NoPunt;
        BallLogic.OffScreenPassEvent -= OffScreenPass;

        gameType = new GameType();
        selectionState = null;
        piecePlacement = null;
        pieceMovement = null;
        blockCount = 0;
        playerList = null;
        inFreePlay = false;
        inOnePointConversion = false;
        inTwoPointConversion = false;
        inActivePlay = false;

        PhotonNetwork.Disconnect();
    }

    private void Setup()
    {
        view = PhotonView.Get(this);       
        piecePlacement = FindObjectOfType<PiecePlacement>();
        pieceMovement = GetComponent<PieceMovement>();
      
        messaging.Subscribe<MTileSelected>(TileSelected);

        selectionState = piecePlacement;

        PieceMovement.TackledEvent += PieceTackled;
        PieceMovement.QBTackledEvent += QBTackled;
        SpawnGrid.TileHeldEvent += TestHold;
        PieceModel.ClaimedBallEvent += PickupFumbleCheck;
        ConversionMenu.OnePointConversionEvent += StartOnePointConversion;
        ConversionMenu.TwoPointConversionEvent += StartTwoPointConversion;
        BallLogic.ScoredOnePointConversion += OnePointConversionSuccess;
        BallLogic.FailedOnePointConversion += OnePointConversionFailed;
        TurnTimer.TimerRanOutEvent += NoMoveMade;
        ScoreBoard.TimerZeroEvent += GameEnd;
        Punting.PuntEvent += Punt;
        Punting.NoPuntEvent += NoPunt;
        BallLogic.OffScreenPassEvent += OffScreenPass;
    }

    private void SetTeam()
    {
        if (PhotonNetwork.IsMasterClient) TurnLogic.SetMyTeam(TeamType.TeamOne);
        else TurnLogic.SetMyTeam(TeamType.TeamTwo);
    }
    
    private void Update()
    {
        firstAdTimer -= Time.deltaTime;
        if(firstAdTimer <= 0)
        {
            interstitialAdsSc.ShowInterstitialAd();
            firstAdTimer = float.PositiveInfinity;
        }

        //J- Check room still has players
        if(gameType == GameType.Online)
        {
            //J// Check for disconnection of self or others
            //Debug.Log(PhotonNetwork.GetPing());
            if (!PhotonNetwork.NetworkingClient.IsConnected || PhotonNetwork.GetPing() >= 1000)
            {
                selfDisconnectionTimer -= Time.deltaTime;
                thisPlayerDisconnectionText.text = (int)selfDisconnectionTimer + " Seconds until auto-resign";
                thisPlayerDisconnectionText.transform.parent.gameObject.SetActive(true);
                turnTimer_Sc.timerStartStamp += Time.deltaTime;
                turnTimer_Sc.gameTimerStartStamp += Time.deltaTime;
            } else
            {
                selfDisconnectionTimer = 60;
                thisPlayerDisconnectionText.text = (int)selfDisconnectionTimer + " Seconds until auto-resign";
                thisPlayerDisconnectionText.transform.parent.gameObject.SetActive(false);
            }

            if (PhotonNetwork.PlayerList.Length < 2
                || PhotonNetwork.PlayerList[0].UserId != playerList[0].UserId || PhotonNetwork.PlayerList[1].UserId != playerList[1].UserId)
            {
                otherDisconnectionTimer -= Time.deltaTime;
                otherPlayerDisconnectionText.text = (int)otherDisconnectionTimer + " Seconds";
                otherPlayerDisconnectionText.transform.parent.gameObject.SetActive(true);
                turnTimer_Sc.timerStartStamp += Time.deltaTime;
                turnTimer_Sc.gameTimerStartStamp += Time.deltaTime;
            } else
            {
                otherDisconnectionTimer = 60;
                otherPlayerDisconnectionText.text = (int)otherDisconnectionTimer + " Seconds";
                otherPlayerDisconnectionText.transform.parent.gameObject.SetActive(false);
            }

            if(selfDisconnectionTimer <= 0)
            {
                overideLose = true;
                GameEnd();
                thisPlayerDisconnectionText.transform.parent.gameObject.SetActive(false);
                otherPlayerDisconnectionText.transform.parent.gameObject.SetActive(false);
            }

            if(otherDisconnectionTimer <= 0)
            {
                overideWin = true;
                GameEnd();
                thisPlayerDisconnectionText.transform.parent.gameObject.SetActive(false);
                otherPlayerDisconnectionText.transform.parent.gameObject.SetActive(false);
            }

            //J- Update other player of this players status
            if(TurnLogic.IsMyTurn)
            {
                string statusString = "Taking their turn...";
                if(!inActivePlay)
                {
                    statusString = "Choosing what to do...";
                } else if(inOnePointConversion) {
                    statusString = "Kicking...";
                }
                view.RPC("NUpdateOtherPlayerStatusText", RpcTarget.Others, statusString);
            } else
            {
                view.RPC("NUpdateOtherPlayerStatusText", RpcTarget.Others, ""); //J- Blank == Hidden
            }
        }
    }

    #endregion

    #region Messaging

    private void TileSelected(MTileSelected msg)
    {                
        if(selectionState == null) SmartDebug.Log("No SelectionState");        
        else selectionState.CheckTile(msg.coord);
    }

    #endregion

    [PunRPC] void NUpdateOtherPlayerStatusText(string status) //J- Blank == hide
    {
        if(status != "")
        {
            otherPlayerStatusText.transform.parent.gameObject.SetActive(true);
            otherPlayerStatusText.text = status;
        } else
        {
            otherPlayerStatusText.transform.parent.gameObject.SetActive(false);
            otherPlayerStatusText.text = "";
        }
    }

    public void EnterPiecePlacement()
    {
        inActivePlay = false;
        teamFinishedPlacing = 0;
        blockCount = 0;
        selectionState = piecePlacement;
        EnteringPlacementEvent?.Invoke();
    }

    [PunRPC]
    private void ChangeTurns()
    {
        if (gameType == GameType.Online)
        {
            TurnLogic.ChangeTurn();
            SmartDebug.Log(TurnLogic.IsMyTurn.ToString());
        }
        else
        {
            OfflineChangeTurns();
        }

        ChangedTurnEvent?.Invoke();
    }

    private void PickupFumbleCheck(PieceModel piece)
    {
        if (gameType == GameType.Offline)
        {
            if(TurnLogic.teamState == TeamState.Defending) TurnLogic.ChangePossesion();
        }
        else
        {
            TurnLogic.SetPossessionOnFumbledBallPickup(piece.team);
        }
        
    }

    private void NoMoveMade()
    {
        view.RPC("ChangeTurns", RpcTarget.All);
    }

    private void OfflineChangeTurns()
    {
        TurnLogic.ChangeTurn();
        TurnLogic.ChangeTeam();
        TurnLogic.ChangePossesion();
    }

    private void TestHold(Tile tile)
    {
        Debug.Log(TurnLogic.myTeam + " - TILE HELD " + tile.coord);
    }

    public void PlacedAllPieces()
    {
        if (gameType == GameType.Online)
        {
            selectionState = null;
            if (TurnLogic.teamState == TeamState.Attacking) TurnLogic.currentTurn = TurnLogic.myTeam;
            else TurnLogic.currentTurn = TurnLogic.myTeam == TeamType.TeamOne ? TeamType.TeamTwo : TeamType.TeamOne;
            view.RPC("FinishedPlacing", RpcTarget.All);
        }
        else
        {
            Debug.Log(TurnLogic.myTeam + " - Placed all pieces"); 
            OfflineChangeTurns();
            teamFinishedPlacing++;
            if(teamFinishedPlacing == 1)
            {
                if(TurnLogic.teamState == TeamState.Attacking)
                {
                    if (inFreePlay) piecePlacement.SetRestriction(OfflineChange.FreeplayAttacking);
                    else piecePlacement.SetRestriction(OfflineChange.FreeplayAttacking);
                }
                else
                {
                    if (inFreePlay) piecePlacement.SetRestriction(OfflineChange.FreeplayDefending);
                    else piecePlacement.SetRestriction(OfflineChange.KickoffDefending);
                }
                                         
            }
            else
            {
                selectionState = pieceMovement;
                inActivePlay = true;
                EnteringGameplayEvent?.Invoke();
                Debug.Log(kickoffPlacement);
                if (kickoffPlacement)
                {
                    view.RPC("ChangeTurns", RpcTarget.All);
                }
                kickoffPlacement = false;
            }

        }

        if (inFreePlay)
        {
            Debug.Log(TurnLogic.myTeam + " - FINISHED AND GOING INTO FREEPLAY");
            EnterFreeplay();            
        }

    }

    [PunRPC]
    private void FinishedPlacing()
    {
        SmartDebug.Log(TurnLogic.myTeam + " - Placed Pieces");
        teamFinishedPlacing++;
        if(teamFinishedPlacing == 2)
        {
            inActivePlay = true;
            selectionState = pieceMovement;
            EnteringGameplayEvent?.Invoke();
            if (kickoffPlacement)
            {
                if (TurnLogic.teamState == TeamState.Defending) TurnLogic.currentTurn = TurnLogic.myTeam;
                else TurnLogic.currentTurn = TurnLogic.myTeam == TeamType.TeamOne ? TeamType.TeamTwo : TeamType.TeamOne;
            }
            kickoffPlacement = false;
        }
    }

    [PunRPC]
    private void TouchDown(TeamType team)
    {
        interstitialAdsSc.ShowInterstitialAd();
        inActivePlay = false;
        SmartDebug.Log($"{team} scored a touchdown!");

        if (inTwoPointConversion)
        {
            if (team == TeamType.TeamOne) SetTeamScoreEvent(team, (teamOneScore += 2).ToString());
            else SetTeamScoreEvent(team, (teamTwoScore += 2).ToString());

            if (gameType == GameType.Offline) ChangeTurns();

        }
        else
        {
            if (team == TeamType.TeamOne) SetTeamScoreEvent(team, (teamOneScore += 6).ToString());
            else SetTeamScoreEvent(team, (teamTwoScore += 6).ToString());
        }

        //TouchdownEvent for scoring etc
        if (inTwoPointConversion) ResetToKickoff();
        inTwoPointConversion = false;
    }

    [PunRPC]
    private void EndzoneSafety(TeamType team)
    {
        Debug.Log($"{team} scored a EZ safety!");
        
        ChangeTurns();

        if (team == TeamType.TeamOne)
        {
            SetTeamScoreEvent(team, (teamOneScore += 2).ToString());
            pieceMovement.DoSafety(5);
        }
        else
        {
            SetTeamScoreEvent(team, (teamTwoScore += 2).ToString());
            pieceMovement.DoSafety(8);
        }
        tackleCounter = 1;
    }

    [PunRPC]
    private void DeadBall(bool isSafety, int scrimmageLine)
    {
        Debug.Log(TurnLogic.myTeam + " - Dead Ball");
        if (isSafety)
        {
            tackleCounter = 1;
            pieceMovement.DoSafety(scrimmageLine);
        }
        else
        {
            ResetToKickoff();
        }
    }

    private void ResetToKickoff()
    {
        inTwoPointConversion = false;
        kickoffPlacement = true;
        tackleCounter = 0;
        inActivePlay = false;
        ChangeTurns();
        inFreePlay = false;
        TurnLogic.ChangePossesion();
        view.RPC("ChangeTurns", RpcTarget.All);
        ResetToKickoffEvent?.Invoke();
        EnterPiecePlacement();        
    }    

    private void PieceTackled()
    {
        blockCount++;
        if(blockCount == 3) EnterFreeplay();
    }

    private void QBTackled()
    {
        if(inTwoPointConversion)
        {
            inActivePlay = false;
            if (gameType == GameType.Offline) ChangeTurns();
            ResetToKickoff();
        } else
        {
            tackleCounter++;
            if (tackleCounter == 1 && TurnLogic.IsMyTurn)
            {
                Debug.Log("---J-T1");
                ShowPuntEvent?.Invoke();
            }
            if (tackleCounter == 2)
            {
                inFreePlay = true;
                Debug.Log("---J-T2");
                Debug.Log(tackleCounter);
                Debug.Log(inFreePlay);
                TurnLogic.ChangePossesion();
                tackleCounter = 0;
                NewDown();
                view.RPC("SetFreeplayRestriction", RpcTarget.All);
            }
        }
    }
    
    public void OffScreenPass()
    {
        tackleCounter++;
        if (tackleCounter == 1)
        {
            ShowPuntEvent?.Invoke();
        }
        if (tackleCounter == 2)
        {
            tackleCounter = 0;
            view.RPC("NChangePossesion", RpcTarget.All);
            view.RPC("ChangeTurns", RpcTarget.All);
            view.RPC("NewDown", RpcTarget.All);
            view.RPC("SetFreeplayRestriction", RpcTarget.All);
        }
    }

    [PunRPC]
    private void NChangePossesion()
    {
        TurnLogic.ChangePossesion();
    }

    [PunRPC]
    private void NewDown()
    {
        inActivePlay = false;
        EnterPiecePlacement();
        NewDownEvent?.Invoke();
    }

    private void Punt()
    {
        inActivePlay = false;
        Debug.Log("Punting");
        view.RPC("NPunt", RpcTarget.All);
        view.RPC("SetFreeplayRestriction", RpcTarget.All);
    }

    [PunRPC]
    private void NPunt()
    {
        inActivePlay = false;
        pieceMovement.DoPunt();
        TurnLogic.ChangePossesion();
        tackleCounter = 0;
        NewDown();        
    }

    private void NoPunt()
    {
        inActivePlay = false;
        Debug.Log("No Punt");
        view.RPC("SetFreeplayRestriction", RpcTarget.All);
        view.RPC("NewDown", RpcTarget.All);
    }

    private void EnterFreeplay()
    {
        EnterFreeplayEvent?.Invoke();
        inFreePlay = true;
        SmartDebug.Log("Moving To Freeplay");
    }    

    public void StartOnePointConversion()
    {
        interstitialAdsSc.ShowInterstitialAd();
        ClearBoardEvent?.Invoke();
        view.RPC("NWipeBoard", RpcTarget.Others);
        int y = TurnLogic.myTeam == TeamType.TeamOne ? 6 : 7;
        int x = UnityEngine.Random.Range(1, 7);
        Vector2 tileCoord = new Vector2(x, y);

        view.RPC("NPlacePiece", RpcTarget.All, tileCoord, TurnLogic.myTeam);

        view.RPC("NSetQuarterback", RpcTarget.All, tileCoord);

        selectionState = pieceMovement;
        inOnePointConversion = true;

    }

    [PunRPC]
    void NWipeBoard()
    {
        ClearBoardEvent?.Invoke();
    }

    private void OnePointConversionSuccess()
    {
        view.RPC("NOnePointConversionSuccess", RpcTarget.All, TurnLogic.myTeam);        
    }

    [PunRPC]
    void NOnePointConversionSuccess(TeamType team)
    {
        inActivePlay = false;
        if (team == TeamType.TeamOne) SetTeamScoreEvent(team, (teamOneScore += 1).ToString());
        else SetTeamScoreEvent(team, (teamTwoScore += 1).ToString());
        if (gameType == GameType.Offline) ChangeTurns();
        ResetToKickoff();
        inOnePointConversion = false;
    }


    private void OnePointConversionFailed()
    {
        view.RPC("NOnePointConversionFailed", RpcTarget.All);
        inOnePointConversion = false;
    }

    [PunRPC]
    private void NOnePointConversionFailed()
    {
        inActivePlay = false;
        if (gameType == GameType.Offline) ChangeTurns();
        ResetToKickoff();
    }
    
    public void StartTwoPointConversion()
    {
        int scrimLine = TurnLogic.myTeam == TeamType.TeamOne ? 6 : 7;

        Debug.Log("Two Point Conversion");
        view.RPC("NStartTwoPointConversion", RpcTarget.All, scrimLine);              
    }

    [PunRPC]
    private void NStartTwoPointConversion(int scrimmageLine)
    {
        interstitialAdsSc.ShowInterstitialAd();
        pieceMovement.DoStartTwoPointConversion(scrimmageLine);
        inTwoPointConversion = true;
    }

    [PunRPC]
    private void Interception()
    {
        TurnLogic.ChangePossesion();
        tackleCounter = 0;
    }

    [PunRPC]
    private void NOtherPlayerLeft()
    {
        otherDisconnectionTimer = 10; //J// Fast end "Disconnect"
    }

    [PunRPC]
    private void GameEnd()
    {
        inActivePlay = false;
        bool didWin = false;
        if (TurnLogic.myTeam == TeamType.TeamOne && teamOneScore > teamTwoScore) didWin = true;
        if (TurnLogic.myTeam == TeamType.TeamTwo && teamTwoScore > teamOneScore) didWin = true;
        if (overideWin) didWin = true; //J- Win when other leave
        if (overideLose) didWin = false; //J- Win when other leave
        GameEndedEvent?.Invoke(teamOneScore, teamTwoScore, didWin);
    }

    private void OnApplicationQuit()
    {
        view.RPC("NOtherPlayerLeft", RpcTarget.Others);
    }
}

public enum GameType
{
    Online,
    Offline,
    AI
}

