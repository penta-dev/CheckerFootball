using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CheckerFootball.Models;
using Photon.Pun;

public class PieceMovement : MonoBehaviour, ITileSelection
{

    private PieceFactory pieceFactory;

    public PieceModel currentSelectedPiece;
    private PhotonView view;

    public static event Action TackledEvent;
    public static event Action QBTackledEvent;
    public static event Action TouchDownEvent;
    public static event Action MadeMoveEvent;
    public static event Action twoPCEvent;

    public static int scrimmageLine;

    public Tile lastTileFrom;
    public Tile lastTileTo;
    public Color lastTileFromColor;
    public Color lastTileToColor;

    private void Awake()
    {
        scrimmageLine = 7;
    }

    void OnDestroy()
    {
        BallLogic.PassingStartedEvent -= DeselectPiece;
        TurnTimer.TimerRanOutEvent -= DeselectPiece;

        scrimmageLine = 7;
    }

    private void Start()
    {
        pieceFactory = FindObjectOfType<PieceFactory>();
        view = GetComponent<PhotonView>();
        BallLogic.PassingStartedEvent += DeselectPiece;
        TurnTimer.TimerRanOutEvent += DeselectPiece;
    }

    private void Update()
    {
        if(!GameLogic.inActivePlay)
        {
            HideLastMove();
        }
    }

    public void CheckTile(Vector2 tileCoord)
    {
        //J- Null reference safety net
        if (SpawnGrid.tiles[tileCoord] == null) return;

        //J- Added check to make sure no selection can be made if the game is not in play
        if (!TurnLogic.IsMyTurn || !GameLogic.inActivePlay) return;
        var selectedTile = SpawnGrid.tiles[tileCoord];

        bool hasSelectedPiece = currentSelectedPiece != null;
        bool tileHasPiece = selectedTile.currentPiece != null;
        bool pieceIsMine = tileHasPiece && selectedTile.currentPiece.IsMyPiece;
        bool pieceCanTackle = currentSelectedPiece != null && !currentSelectedPiece.IsInBlock;

        //J- Debug line to track selection (mainly for AI)
        if(AILogic.aiOn && tileHasPiece && hasSelectedPiece)
            Debug.DrawLine(currentSelectedPiece.image.transform.position, selectedTile.image.transform.position, Color.green, 5f);

        //J- Stop selection if AI is just checking if it can move there
        if (pieceIsMine && !AILogic.aiOn)
        {
            HandleSelection(selectedTile);
            return;
        }
        if (hasSelectedPiece && !tileHasPiece)
        {
            HandleMovement(selectedTile);
            return;
        }
        if (hasSelectedPiece && !pieceIsMine && pieceCanTackle)
        {
            HandleTackling(selectedTile);
            return;
        }
    }

    private void HandleSelection(Tile tile)
    {
        if (currentSelectedPiece == null)
        {
            SelectPiece(tile.currentPiece);
            return;
        }
        if (tile.currentPiece == currentSelectedPiece)
        {
            ManualDeselect();
            return;
        }

        //Changing selection
        //This if is so that you can't change selection after QB has jumped
        if (currentSelectedPiece.IsQuarterBack)
        {
            QuarterBack qb = currentSelectedPiece.movementType as QuarterBack;
            if (qb.jumped)
            {
                return;
            }
        }
        DeselectPiece();
        SelectPiece(tile.currentPiece);

    }

    private void SelectPiece(PieceModel piece)
    {        
        //Dissalow selecting QB before freeplay
        if (piece.movementType.GetType() == typeof(QuarterBackStandard)) return;
        
        currentSelectedPiece = piece;
        piece.SelectEffect();
    }


    //This is so that you can end your turn by deselecting QB after jumping v
    private void ManualDeselect()
    {
        Debug.Log("Manual Deselect");
        if (currentSelectedPiece.IsQuarterBack)
        {
            Debug.Log("Quarterback");
            QuarterBack qb = currentSelectedPiece.movementType as QuarterBack;
            if (qb.jumped)
            {
                Debug.Log("DESELECTING JUMPED QB");
                qb.jumped = false;
                view.RPC("ChangeTurns", RpcTarget.All);
            }
        }

        currentSelectedPiece.DeselectEffect();
        currentSelectedPiece = null;
    }

    private void DeselectPiece()
    {
        if (currentSelectedPiece == null) return;

        currentSelectedPiece.DeselectEffect();
        currentSelectedPiece = null;
    }

    private void HandleMovement(Tile tile)
    {
        if (!currentSelectedPiece.movementType.CanMove(currentSelectedPiece.coordinate, tile.coord)) return;

        //This if handles the QB being able to jump multiple of his own pieces
        if (currentSelectedPiece.IsQuarterBack)
        {
            if (CheckTouchdown((int)tile.coord.y))
            {
                view.RPC("MovePiece", RpcTarget.All, currentSelectedPiece.coordinate, tile.coord);
                DeselectPiece();
                if (!GameLogic.inTwoPointConversion) TouchDownEvent?.Invoke();
                view.RPC("TouchDown", RpcTarget.All, TurnLogic.currentTurn);               
                return;
            }

            QuarterBack qb = currentSelectedPiece.movementType as QuarterBack;
            var reselect = currentSelectedPiece;
            if (qb.jumped)
            {
                view.RPC("MovePiece", RpcTarget.All, currentSelectedPiece.coordinate, tile.coord);
                SelectPiece(reselect);                               
                return;
            }
        }               

        view.RPC("MovePiece", RpcTarget.All, currentSelectedPiece.coordinate, tile.coord);
        view.RPC("ChangeTurns", RpcTarget.All);

        MadeMoveEvent?.Invoke();
    }

    private bool CheckTouchdown(int row)
    {
        bool teamOneTD = row == 12 || row == 13;
        bool teamTwoTD = row == 0 || row == 1;
        if (TurnLogic.myTeam == TeamType.TeamOne && teamOneTD) return true;
        if (TurnLogic.myTeam == TeamType.TeamTwo && teamTwoTD) return true;
        return false;
    }

    private void HandleTackling(Tile toBlock)
    {
        //J// Use "Touchdown check to see if it is a Saftey in other endzone
        if (CheckTouchdown((int)toBlock.coord.y))
        {
            if (!currentSelectedPiece.movementType.CanMove(currentSelectedPiece.coordinate, toBlock.coord)) return;
            view.RPC("EndzoneSafety", RpcTarget.All, TurnLogic.myTeam);
        } else
        {
            if (!currentSelectedPiece.movementType.CanMove(currentSelectedPiece.coordinate, toBlock.coord)) return;
            view.RPC("TacklePiece", RpcTarget.All, currentSelectedPiece.coordinate, toBlock.coord);
            PhotonView.Get(this).RPC("ChangeTurns", RpcTarget.All);
        }
    }


    [PunRPC]
    private void MovePiece(Vector2 pieceCoord, Vector2 tileCoord)
    {
        Tile pieceTile = SpawnGrid.tiles[pieceCoord];
        Tile moveTile = SpawnGrid.tiles[tileCoord];

        if(GameLogic.inActivePlay)
        {
            ShowLastMove(pieceTile, moveTile);
            pieceTile.currentPiece.MovePiece(moveTile);
            pieceTile.currentPiece = null;
        }

        DeselectPiece();
    }

    public void ShowLastMove(Tile tile1, Tile tile2)
    {
        if (lastTileFrom != null)
        {
            lastTileFrom.SetColour(lastTileFromColor);
            lastTileTo.SetColour(lastTileToColor);
        }
        lastTileFrom = tile1;
        lastTileTo = tile2;
        lastTileFromColor = tile1.baseColor;
        lastTileToColor = tile2.baseColor;
        tile1.SetColour(new Color(1, 0, 0, 0.25f));
        tile2.SetColour(new Color(1, 0, 0, 0.5f));
    }

    public void HideLastMove()
    {
        if (lastTileFrom != null)
        {
            if(lastTileFrom.image.color == new Color(1, 0, 0, 0.25f))
                lastTileFrom.SetColour(lastTileFromColor);
            
            if(lastTileTo.image.color == new Color(1, 0, 0, 0.5f))
                lastTileTo.SetColour(lastTileToColor);
        }
    }

    public void DoSafety(int _scrimmageLine)
    {
        scrimmageLine = _scrimmageLine;
        StartCoroutine(DelayQBEvent());
        //J//0505211924/ if(GameLogic.gameType == GameType.Offline) view.RPC("ChangeTurns", RpcTarget.All);
    }

    public void DoStartTwoPointConversion(int _scrimmageLine)
    {        
        scrimmageLine = _scrimmageLine;
        StartCoroutine(PartTwoTwoPointConversion());
    }

    IEnumerator PartTwoTwoPointConversion ()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        twoPCEvent?.Invoke();
        yield return new WaitForSecondsRealtime(0.1f);
        if (TurnLogic.teamState == TeamState.Attacking)
        {
            GameLogic.inActivePlay = false;
            view.RPC("SetFreeplayRestriction", RpcTarget.All);
            view.RPC("NewDown", RpcTarget.All);
        }
        yield return null;
    }

    public void DoPunt()
    {
        if (TurnLogic.currentTurn == TeamType.TeamOne){
            scrimmageLine += 4;
        } else
        {
            scrimmageLine -= 4;
        }
        //J- Limit scrimmage to 20Yrd lines
        scrimmageLine = Mathf.Clamp(scrimmageLine, 3, 10);
        view.RPC("ChangeTurns", RpcTarget.All);
    }

    [PunRPC]
    private void TacklePiece(Vector2 tacklerCoord, Vector2 tackleeCoord)
    {
        var tackler = SpawnGrid.tiles[tacklerCoord].currentPiece;
        var tacklee = SpawnGrid.tiles[tackleeCoord];

        ShowLastMove(SpawnGrid.tiles[tacklerCoord], SpawnGrid.tiles[tackleeCoord]);

        var blockPiece = tacklee.currentPiece;
        bool wasQB = blockPiece.IsQuarterBack;
        pieceFactory.ReturnPiece(blockPiece);
        tackler.MovePiece(tacklee);
        SpawnGrid.tiles[tacklerCoord].currentPiece = null;
        tackler.EnterBlock();
        DeselectPiece();

        if (wasQB)
        {
            scrimmageLine = (int)tacklee.coord.y;
            StartCoroutine(DelayQBEvent());
            GameLogic.inActivePlay = false;
            SmartDebug.Log("TACKLED QB");
        }
        else
        {
            TackledEvent?.Invoke();
            Debug.Log("Tackling piece");
        }

    }

    //Need to wait until the next frame for turn change to occur
    private IEnumerator DelayQBEvent()
    {
        yield return new WaitForSeconds(0.1f);
        QBTackledEvent?.Invoke();
    }

}
