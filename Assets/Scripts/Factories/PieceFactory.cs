using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CheckerFootball.Models;

public class PieceFactory : MonoBehaviour
{
    public Image rawPiece;
    public Transform pieceHolder;
    public MessageHandler messaging;

    private List<PieceModel> unusedPieces = new List<PieceModel>();
    private List<PieceModel> usedPieces = new List<PieceModel>();

    private void Start()
    {
        SpawnPieces();

        //Piece movement states are reset when they are requested via GetPiece so there is no need for an EnterKickoff
        //GameLogic.ResetToNewDownEvent += ReturnAllPieces;
        GameLogic.EnterFreeplayEvent += UpdatePiecesToFreeplay;
        PieceMovement.QBTackledEvent += ReturnAllPieces;
        GameLogic.ResetToKickoffEvent += ReturnAllPieces;
        GameLogic.ClearBoardEvent += ReturnAllPieces;
        PieceMovement.twoPCEvent += ReturnAllPieces;
        BallLogic.OffScreenPassEvent += ReturnAllPieces;
        BallLogic.NOffScreenPassEvent += ReturnAllPieces;
    }

    private void OnDestroy()
    {
        GameLogic.EnterFreeplayEvent -= UpdatePiecesToFreeplay;
        PieceMovement.QBTackledEvent -= ReturnAllPieces;
        GameLogic.ResetToKickoffEvent -= ReturnAllPieces;
        GameLogic.ClearBoardEvent -= ReturnAllPieces;
        PieceMovement.twoPCEvent -= ReturnAllPieces;
        BallLogic.OffScreenPassEvent -= ReturnAllPieces;
        BallLogic.NOffScreenPassEvent -= ReturnAllPieces;
    }

    private void SpawnPieces()
    {
        for(int i=0; i<50; i++)
        {
            var spawned = Instantiate(rawPiece, Vector3.one * 10000, Quaternion.identity, transform);
            PieceModel piece = new PieceModel(i, spawned);
            
            unusedPieces.Add(piece);
        }
    }

    public PieceModel GetPiece(TeamType teamType)
    {
        var toReturn = unusedPieces[unusedPieces.Count - 1];
        unusedPieces.Remove(toReturn);
        usedPieces.Add(toReturn);
        toReturn.team = teamType;

        toReturn.movementType = new StandardKickOff();

        //if (teamType == TeamType.TeamOne) toReturn.image.color = Color.black;
        //else toReturn.image.color = Color.red;
        return toReturn;
    }

    
    public void ReturnPiece(PieceModel piece)
    {
        unusedPieces.Add(piece);
        usedPieces.Remove(piece);
        piece.image.transform.localPosition = Vector3.one * 10000;
        piece.Reset();
    }    

    private void ReturnAllPieces()
    {
        for(int i=usedPieces.Count-1; i>=0 ; i--)
        {
            ReturnPiece(usedPieces[i]);
        }
    }
    
    private void UpdatePiecesToFreeplay()
    {
        foreach(PieceModel piece in usedPieces)
        {
            if (piece.IsQuarterBack) piece.movementType = new QuarterBack();
            else piece.movementType = new StandardFreeplay();
        }
    }
    public void TutorialReturnPieces()
    {
        ReturnAllPieces();
    }

}
