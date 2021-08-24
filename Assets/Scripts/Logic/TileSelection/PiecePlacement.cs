using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using interfaces;
using UnityEngine.UI;
using TMPro;

public class PiecePlacement : MonoBehaviour, ITileSelection
{

    public Color blackTeam;
    public Color redTeam;

    public Sprite blackPiece;
    public Sprite blackPieceHead;
    public Sprite redPiece;
    public Sprite redPieceHead;

    private PieceFactory pieceFactory;
    private GameLogic logic;
    private PhotonView view;

    private Vector2 pieceScale;
    private YardLogic yardLogic = new YardLogic();

    public static IPiecePlacement currentPlacementRestriction = null;

    private KickoffPlacement kickoffAttacking = new KickoffPlacement()
    {
        RequiredPieces = 11,
        restrictions = new Dictionary<int, RestrictionCounter>()
        {
            {1, new RestrictionCounter(2) },
            {3, new RestrictionCounter(3) },
            {5, new RestrictionCounter(6) },
        }
        
    };
    private KickoffPlacement kickoffDefending = new KickoffPlacement()
    {
        RequiredPieces = 12,
        restrictions = new Dictionary<int, RestrictionCounter>()
        {
            {5, new RestrictionCounter(8) },
            {4, new RestrictionCounter(4) }
        }
    };
    private FreePlacement freeAttacking = new FreePlacement()
    {
        RequiredPieces = 11,
        scrimmageLine = 5,
        restrictions = new Dictionary<int, RestrictionCounter>()
        {
            { 0, new RestrictionCounter(7) },
            { 1, new RestrictionCounter(-1) }
        }
    };
    private FreePlacement freeDefending = new FreePlacement()
    {
        RequiredPieces = 12,
        scrimmageLine = 5,
        restrictions = new Dictionary<int, RestrictionCounter>()
        {
            { 0, new RestrictionCounter(6) },
            { 1, new RestrictionCounter(-1) },
            { 2, new RestrictionCounter(-1) },
            { 3, new RestrictionCounter(-1) },
            { 4, new RestrictionCounter(-1) },
            { 5, new RestrictionCounter(-1) },
            { 6, new RestrictionCounter(-1) },
            { 7, new RestrictionCounter(-1) },
            { 8, new RestrictionCounter(-1) },
            { 9, new RestrictionCounter(-1) },
            { 10, new RestrictionCounter(-1) },
            { 11, new RestrictionCounter(-1) }
        }
    };       

    private void Awake()
    {       
        pieceFactory = FindObjectOfType<PieceFactory>();
        logic = GetComponent<GameLogic>();
        view = GetComponent<PhotonView>();
        
    }

    private void Start()
    {
        SetKickoffRestriction();
        //PieceMovement.QBTackledEvent += SetFreeplayRestriction;
        GameLogic.ResetToKickoffEvent += ResetToKickoff;
        //GameLogic.ResetToNewDownEvent += SetFreeplayRestriction;
    }

    private void OnDestroy()
    {
        GameLogic.ResetToKickoffEvent -= ResetToKickoff;
    }

    private void SetKickoffRestriction()
    {
        if (TurnLogic.teamState == TeamState.Attacking) SetRestriction(OfflineChange.KickoffAttacking);
        else SetRestriction(OfflineChange.KickoffDefending);
        
    }

    [PunRPC]
    private void SetFreeplayRestriction()
    {        
        if (TurnLogic.teamState == TeamState.Attacking) SetRestriction(OfflineChange.FreeplayAttacking);
        else SetRestriction(OfflineChange.FreeplayDefending);                 
    }

    public void HighlightRestrictedRows()
    {
        SpawnGrid.HighlightRestrictedRows(currentPlacementRestriction.AllRows, TurnLogic.myTeam);
    }

    public void SetRestriction(OfflineChange change)
    {
        switch (change)
        {
            case OfflineChange.KickoffAttacking:
                currentPlacementRestriction = kickoffAttacking;
                break;
            case OfflineChange.KickoffDefending:
                currentPlacementRestriction = kickoffDefending;
                break;
            case OfflineChange.FreeplayAttacking:
                currentPlacementRestriction = freeAttacking;
                freeAttacking.SetScrimmageLine(PieceMovement.scrimmageLine);                
                SmartDebug.Log($"ScrimLine is {freeAttacking.scrimmageLine} and {TurnLogic.myTeam} and {TurnLogic.currentTurn} and {TurnLogic.teamState}");
                break;
            case OfflineChange.FreeplayDefending:
                currentPlacementRestriction = freeDefending;
                freeDefending.SetScrimmageLine(PieceMovement.scrimmageLine);                
                SmartDebug.Log($"ScrimLine is {freeDefending.scrimmageLine} and {TurnLogic.myTeam} and {TurnLogic.currentTurn} and {TurnLogic.teamState}");
                break;
        }

        currentPlacementRestriction.Reset();
        HighlightRestrictedRows();
    }    

    public void CheckTile(Vector2 tileCoord)
    {
        var tile = SpawnGrid.tiles[tileCoord];

        int row;
        if (!GameLogic.inFreePlay) row = yardLogic.RowToIndex((int)tileCoord.y, TurnLogic.myTeam);
        else row = (int)tileCoord.y;

        //Must select Quarterback before progressing
        if (currentPlacementRestriction.FinishedPlacing && TurnLogic.teamState == TeamState.Attacking)
        {
            var validRow = currentPlacementRestriction.QuarterBackRow();
            if (currentPlacementRestriction == freeDefending)
            {
                if (tile.coord.y != currentPlacementRestriction.ScrimmageLinePlz_Func())
                {
                    view.RPC("NSetQuarterback", RpcTarget.All, tile.currentPiece.coordinate);
                    logic.PlacedAllPieces();
                }
            } else
            if(tile.IsTaken && tile.currentPiece.IsMyPiece && (tile.coord.y == validRow))
            {                
                view.RPC("NSetQuarterback", RpcTarget.All, tile.currentPiece.coordinate);
                logic.PlacedAllPieces();
            }
            return;
        }

        if (tile.IsTaken)
        {
            if (tile.currentPiece.IsMyPiece)
            {
                view.RPC("NRemovePiece", RpcTarget.All, tileCoord);
                currentPlacementRestriction.PieceRemoved(row);
            }
            else SmartDebug.Log("can't remove opponent's piece");
        }
        else if(currentPlacementRestriction.CanPlacePiece(row))
        {
            view.RPC("NPlacePiece", RpcTarget.All, tileCoord, TurnLogic.myTeam);
            if (currentPlacementRestriction.FinishedPlacing)
            {                
                //Need an extra step of selecting QB on attacking team
                if (TurnLogic.teamState == TeamState.Attacking)
                {
                    SmartDebug.Log("Select Quarterback");
                }
                else
                {
                    logic.PlacedAllPieces();
                }
                SpawnGrid.UnhilightAllTiles();              
            }
        }                               
    }

    [PunRPC]
    public void NPlacePiece(Vector2 v, TeamType type)
    {
        SmartDebug.Log(v.ToString() + type.ToString());       

        var tile = SpawnGrid.tiles[v];
        pieceScale = tile.image.rectTransform.sizeDelta * 0.9f; //This should probably only be set once

        PlacePiece(v, type);
    }

    [PunRPC]
    public void NRemovePiece(Vector2 v)
    {
        SmartDebug.Log($"Removing Piece at {v}");        
        RemovePiece(v);
    }    

    [PunRPC]
    public void NSetQuarterback(Vector2 key)
    {
        SpawnGrid.tiles[key].currentPiece.SetAsQuarterback(false);
        BallLogic.currentQB = SpawnGrid.tiles[key].currentPiece;
    }

    private void PlacePiece(Vector2 tileKey, TeamType team)
    {
        var piece = pieceFactory.GetPiece(team);
        piece.coordinate = tileKey;
        piece.image.transform.localPosition = SpawnGrid.tiles[tileKey].tilePosition; 
        piece.image.rectTransform.sizeDelta = pieceScale;
        piece.image.GetComponent<CircleCollider2D>().radius = pieceScale.x / 2;
        piece.image.transform.GetChild(0).GetChild(0).GetComponent<Image>().rectTransform.sizeDelta = piece.image.rectTransform.sizeDelta;
        piece.image.transform.Find("DebugText").GetComponent<TextMeshProUGUI>().fontSize = 30 * 0.01f * piece.image.rectTransform.sizeDelta.x;

        if (GameLogic.gameType == GameType.Offline)
        {
            if (piece.team == TeamType.TeamOne)
            {
                piece.image.transform.GetChild(0).GetChild(0).localScale = new Vector3(piece.image.transform.GetChild(0).GetChild(0).localScale.x, piece.image.transform.GetChild(0).GetChild(0).localScale.x * -1, piece.image.transform.GetChild(0).GetChild(0).localScale.x);
            }
            else
            {
                piece.image.transform.GetChild(0).GetChild(0).localScale = new Vector3(piece.image.transform.GetChild(0).GetChild(0).localScale.x, piece.image.transform.GetChild(0).GetChild(0).localScale.x * 1, piece.image.transform.GetChild(0).GetChild(0).localScale.x);
            }
        } else
        {
            if (piece.IsMyPiece)
            {
                piece.image.transform.GetChild(0).GetChild(0).localScale = new Vector3(piece.image.transform.GetChild(0).GetChild(0).localScale.x, piece.image.transform.GetChild(0).GetChild(0).localScale.x * -1, piece.image.transform.GetChild(0).GetChild(0).localScale.x);
            }
            else
            {
                piece.image.transform.GetChild(0).GetChild(0).localScale = new Vector3(piece.image.transform.GetChild(0).GetChild(0).localScale.x, piece.image.transform.GetChild(0).GetChild(0).localScale.x * 1, piece.image.transform.GetChild(0).GetChild(0).localScale.x);
            }
        }

        if (piece.team == TeamType.TeamOne)
        {
            piece.image.sprite = blackPiece;
            piece.image.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = blackPieceHead;
        } else
        {
            piece.image.sprite = redPiece;
            piece.image.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = redPieceHead;
        }

        piece.SetImageName(tileKey);

        SpawnGrid.tiles[tileKey].currentPiece = piece;
    }

    public void RemovePiece(Vector2 v)
    {
        SmartDebug.Log($"Removing piece {v}");
        pieceFactory.ReturnPiece(SpawnGrid.tiles[v].currentPiece);
        SpawnGrid.tiles[v].currentPiece = null;
    }

    private void ResetToKickoff()
    {
        AILogic.timer = Time.time;
        SetKickoffRestriction();        
    }

}

public enum OfflineChange
{
    KickoffAttacking,
    KickoffDefending,
    FreeplayAttacking,
    FreeplayDefending
}

