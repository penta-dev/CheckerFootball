using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using CheckerFootball.Models;
using Random = UnityEngine.Random;

public class AILogic : MonoBehaviour
{

    public static bool aiOn;

    int pieceChoiceRandomier;

    public static double timer;

    private void Awake()
    {
        aiOn = false;
        timer = 999d;
    }

    private void OnDestroy()
    {
        aiOn = false;
        timer = 999d;
    }

    private void Start()
    {
        timer = Time.time;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.I))
        {
            if(aiOn)
            {
                aiOn = false;
            } else
            {
                aiOn = true;
            }
        }

        if(aiOn && GameLogic.selectionState == GameLogic.piecePlacement && (Time.time - timer >= 1f))
        {
            timer = Time.time;
            int cnt_1 = 0;
            while (cnt_1 <= 10 && GameLogic.selectionState == GameLogic.piecePlacement)
            {
                cnt_1++;
                foreach (KeyValuePair<Vector2, Tile> _tile in SpawnGrid.tiles)
                {
                    if ((GameLogic.selectionState == GameLogic.piecePlacement) && Random.Range(0, 10) >= 5 && (_tile.Value.currentPiece == null || PiecePlacement.currentPlacementRestriction.FinishedPlacing))
                    {
                        GameLogic.selectionState.CheckTile(_tile.Value.coord);
                    }
                }
            }
        }

        //J- Run Check
        if (!aiOn || !TurnLogic.IsMyTurn || !GameLogic.inActivePlay || (Time.time - timer <= 1f)) return;

        timer = Time.time;
        var currentTile = new KeyValuePair<Vector2, Tile>();
        float bestDistance = 99999;
        foreach(KeyValuePair<Vector2, Tile> _tile in SpawnGrid.tiles)
        {
            if(_tile.Value.currentPiece != null)
            {
                var piece = _tile.Value.currentPiece;
                if ((piece.IsQuarterBack && GameLogic.blockCount >= 3) || !piece.IsQuarterBack)
                {
                    if (piece.team == TurnLogic.myTeam)
                    {
                        var targetCoord = BallLogic.currentQB.coordinate;
                        float distanceToQB = Vector2.Distance(piece.coordinate, targetCoord);
                        if(distanceToQB > 2 || TurnLogic.teamState == TeamState.Attacking)
                        {
                            targetCoord.y += 1;
                            distanceToQB = Vector2.Distance(piece.coordinate, targetCoord);
                        }
                        if(piece.IsInBlock || TurnLogic.teamState == TeamState.Attacking)
                        {
                            distanceToQB *= 10; //J- Non Blocking piece to have priority
                        }
                        if(distanceToQB < bestDistance || Random.Range(0, pieceChoiceRandomier) >= 45)
                        {
                            if((piece.IsQuarterBack && Random.Range(0, 10) >= 9) || !piece.IsQuarterBack)
                            {
                                bestDistance = distanceToQB;
                                currentTile = _tile;
                            }
                        }
                    }
                }
            }
        }

        int xOffset = 0;
        if (BallLogic.currentQB.coordinate.x > currentTile.Value.currentPiece.coordinate.x)
        {
            xOffset = 1;
        } else 
        if (BallLogic.currentQB.coordinate.x < currentTile.Value.currentPiece.coordinate.x)
        {
            xOffset = -1;
        }
        int yOffset = 0;
        if (BallLogic.currentQB.coordinate.y > currentTile.Value.currentPiece.coordinate.y)
        {
            yOffset = 1;
        }
        else
        if (BallLogic.currentQB.coordinate.y < currentTile.Value.currentPiece.coordinate.y)
        {
            yOffset = -1;
        }

        if(currentTile.Value.currentPiece.IsQuarterBack)
        {
            xOffset = 0;
            yOffset = 10;
        }

        PieceMovement referenceToMovement = FindObjectOfType<PieceMovement>();
        referenceToMovement.currentSelectedPiece = currentTile.Value.currentPiece;
        int xOffset_2 = xOffset;
        int yOffset_2 = yOffset;
        int cnt = 0;
        while (GameLogic.inActivePlay && TurnLogic.IsMyTurn && cnt < 50) //J- Very Hack !!!!
        {
            if (xOffset_2 != 0 || yOffset_2 != 0)
            {
                var coordClamped = currentTile.Value.coord + new Vector2(xOffset_2, yOffset_2);
                coordClamped.x = Mathf.Clamp(coordClamped.x, 0, 7);
                coordClamped.y = Mathf.Clamp(coordClamped.y, 1, 12);
                referenceToMovement.CheckTile(coordClamped);
                pieceChoiceRandomier++;
            }
            xOffset_2 = Mathf.Clamp(Random.Range(Random.Range(-1, 2), xOffset_2 * 10), -1, 1); //J- Direction Priority
            yOffset_2 = Mathf.Clamp(Random.Range(Random.Range(-1, 2), yOffset_2 * 10), -1, 1);
            
            cnt++;
        }
        if(!TurnLogic.IsMyTurn)
        {
            pieceChoiceRandomier = 0;
        }
    }
}
