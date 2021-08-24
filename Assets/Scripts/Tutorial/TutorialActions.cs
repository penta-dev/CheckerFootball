using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TutorialActions : MonoBehaviour
{    
    public Sprite blackPiece;
    public Sprite blackPieceHead;
    public Sprite redPiece;
    public Sprite redPieceHead;

    PieceFactory factory;
    Vector2 pieceScale;

    // Start is called before the first frame update
    void Start()
    {
        factory = FindObjectOfType<PieceFactory>();
        pieceScale = SpawnGrid.tiles[new Vector2(0, 0)].image.rectTransform.sizeDelta * 0.9f;
        
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space)) PlaceScrimmageLinePieces();
    }    

    public void PlacePieceOne()
    {
        var piece = factory.GetPiece(TeamType.TeamOne);
        piece.coordinate = new Vector2(5,5);
        piece.image.transform.localPosition = SpawnGrid.tiles[piece.coordinate].tilePosition;
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
        }
        else
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
        }
        else
        {
            piece.image.sprite = redPiece;
            piece.image.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = redPieceHead;
        }
        SpawnGrid.tiles[piece.coordinate].currentPiece = piece;
    }

    public void RemovePieceOne()
    {               
        factory.ReturnPiece(SpawnGrid.tiles[new Vector2(5,5)].currentPiece);
        SpawnGrid.tiles[new Vector2(5,5)].currentPiece = null;        
    }

    public void PlaceScrimmageLinePieces()
    {
        for(int i=0; i<8; i++)
        {                       
            SetPieceWithPosition(TeamType.TeamTwo, new Vector2(i, 7));
            SetPieceWithPosition(TeamType.TeamOne, new Vector2(i, 6));
        }

        SetPieceWithPosition(TeamType.TeamOne, new Vector2(3, 5));
        SetPieceWithPosition(TeamType.TeamOne, new Vector2(4, 5));
        SetPieceWithPosition(TeamType.TeamOne, new Vector2(5, 5));

        SetPieceWithPosition(TeamType.TeamTwo, new Vector2(3, 8));
        SetPieceWithPosition(TeamType.TeamTwo, new Vector2(4, 8));
        SetPieceWithPosition(TeamType.TeamTwo, new Vector2(5, 8));
        SetPieceWithPosition(TeamType.TeamTwo, new Vector2(6, 8));

        SetQuarterback();


    }

    public void PlaceKickoffAttackingPieces()
    {
        for(int i=0; i<6; i++)
        {
            SetPieceWithPosition(TeamType.TeamOne, new Vector2(i, 5));            
        }

        SetPieceWithPosition(TeamType.TeamOne, new Vector2(3, 3));
        SetPieceWithPosition(TeamType.TeamOne, new Vector2(4, 3));
        SetPieceWithPosition(TeamType.TeamOne, new Vector2(5, 3));

        SetPieceWithPosition(TeamType.TeamOne, new Vector2(4, 1));
        SetPieceWithPosition(TeamType.TeamOne, new Vector2(5, 1));

        SetQuarterback(new Vector2(5,1));
    } 
     
    public void RemoveScrimmagePieces()
    {
        factory.TutorialReturnPieces();
    }

    public void RemoveAllPieces()
    {
        factory.TutorialReturnPieces();
    }
   
    private void SetQuarterback(Vector2 pos)
    {
        SpawnGrid.tiles[pos].currentPiece.SetAsQuarterback(false);
    }

    public void SetQuarterback()
    {
        SpawnGrid.tiles[new Vector2(4, 5)].currentPiece.SetAsQuarterback(false);
    }

    /*
    private void SetPieceWithPosition(TeamType team, Vector2 coord)
    {
        var piece = factory.GetPiece(team);
        piece.coordinate = coord;
        piece.image.transform.localPosition = SpawnGrid.tiles[piece.coordinate].tilePosition;
        piece.image.rectTransform.sizeDelta = pieceScale;
        piece.image.GetComponent<CircleCollider2D>().radius = pieceScale.x / 2;

        if (piece.team == TeamType.TeamOne) piece.image.color = Color.black;
        else piece.image.color = Color.red;

        //piece.SetImageName(tileKey);

        SpawnGrid.tiles[piece.coordinate].currentPiece = piece;
    }
    */

    private void SetPieceWithPosition(TeamType team, Vector2 coord)
    {
        var piece = factory.GetPiece(team);
        piece.coordinate = coord;
        piece.image.transform.localPosition = SpawnGrid.tiles[piece.coordinate].tilePosition;
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
        }
        else
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
        }
        else
        {
            piece.image.sprite = redPiece;
            piece.image.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = redPieceHead;
        }
        SpawnGrid.tiles[piece.coordinate].currentPiece = piece;
    }


    public void PiecePlacementRemoveOnePiece()
    {
        factory.ReturnPiece(SpawnGrid.tiles[new Vector2(0, 5)].currentPiece);
        SpawnGrid.tiles[new Vector2(5, 1)].currentPiece.DemoteQuarterback();
    }

    public void DemoRemoveQBPiece()
    {
        factory.ReturnPiece(SpawnGrid.tiles[new Vector2(5, 1)].currentPiece);
    }

    public void DemoAddPieceBack()
    {
        SetPieceWithPosition(TeamType.TeamOne, new Vector2(0, 5));
    }

    public void DemoAddQBBack()
    {
        SetPieceWithPosition(TeamType.TeamOne, new Vector2(5, 1));
    }

    public void SetQB()
    {
        SpawnGrid.tiles[new Vector2(5, 1)].currentPiece.SetAsQuarterback(false);
    }

    public void SpawnDefendingSide()
    {
        for (int i = 0; i < 8; i++)
        {
            SetPieceWithPosition(TeamType.TeamOne, new Vector2(i, 5));
        }

        SetPieceWithPosition(TeamType.TeamOne, new Vector2(3, 4));
        SetPieceWithPosition(TeamType.TeamOne, new Vector2(4, 4));
        SetPieceWithPosition(TeamType.TeamOne, new Vector2(5, 4));
        SetPieceWithPosition(TeamType.TeamOne, new Vector2(6, 4));
    }


    public void SpawnMovementTeamOnePieces()
    {
        //PlaceKickoffAttackingPieces();
        for (int i = 1; i < 7; i++)
        {
            SetPieceWithPosition(TeamType.TeamOne, new Vector2(i, 5));
        }

        SetPieceWithPosition(TeamType.TeamOne, new Vector2(3, 3));
        SetPieceWithPosition(TeamType.TeamOne, new Vector2(4, 3));
        SetPieceWithPosition(TeamType.TeamOne, new Vector2(5, 3));

        SetPieceWithPosition(TeamType.TeamOne, new Vector2(4, 1));
        SetPieceWithPosition(TeamType.TeamOne, new Vector2(5, 1));

        SetQuarterback(new Vector2(5, 1));
    }

    public void SpawnMovementTeamTwoPieces()
    {
        for (int i = 0; i < 8; i++)
        {
            SetPieceWithPosition(TeamType.TeamTwo, new Vector2(i, 8));
        }

        SetPieceWithPosition(TeamType.TeamTwo, new Vector2(3, 9));
        SetPieceWithPosition(TeamType.TeamTwo, new Vector2(4, 9));
        SetPieceWithPosition(TeamType.TeamTwo, new Vector2(5, 9));
        SetPieceWithPosition(TeamType.TeamTwo, new Vector2(6, 9));
    }

    public void SelectPieceForMovement()
    {
        SpawnGrid.tiles[new Vector2(5, 5)].currentPiece.SelectEffect();
    }

    public void DeselectPiece()
    {
        SpawnGrid.tiles[new Vector2(5, 5)].currentPiece.DeselectEffect();
    }

    public void FirstMovePiece()
    {
        DeselectPiece();
        factory.ReturnPiece(SpawnGrid.tiles[new Vector2(5, 5)].currentPiece);
        SetPieceWithPosition(TeamType.TeamOne, new Vector2(5, 6));
    }

    public void MoveOpponentPiece()
    {
        factory.ReturnPiece(SpawnGrid.tiles[new Vector2(5, 8)].currentPiece);
        SetPieceWithPosition(TeamType.TeamTwo, new Vector2(5, 7));
    }

   public void MoveOpponentPieceTwo()
    {
        factory.ReturnPiece(SpawnGrid.tiles[new Vector2(4, 8)].currentPiece);
        SetPieceWithPosition(TeamType.TeamTwo, new Vector2(4, 7));
    }

    public void SelectPieceForBlocking()
    {
        SpawnGrid.tiles[new Vector2(5, 6)].currentPiece.SelectEffect();
    }

    public void FirstBlockDemo()
    {
        factory.ReturnPiece(SpawnGrid.tiles[new Vector2(5, 7)].currentPiece);        

        factory.ReturnPiece(SpawnGrid.tiles[new Vector2(5, 6)].currentPiece);
        SetPieceWithPosition(TeamType.TeamOne, new Vector2(5, 7));

        SpawnGrid.tiles[new Vector2(5, 7)].currentPiece.EnterBlock();
        SpawnGrid.tiles[new Vector2(5, 7)].currentPiece.DeselectEffect();
    }

    public void SetupThreeBlocks()
    {
        factory.ReturnPiece(SpawnGrid.tiles[new Vector2(4, 5)].currentPiece);
        factory.ReturnPiece(SpawnGrid.tiles[new Vector2(4, 8)].currentPiece);
        SetPieceWithPosition(TeamType.TeamTwo, new Vector2(4, 6));
        SpawnGrid.tiles[new Vector2(4, 6)].currentPiece.EnterBlock();

        factory.ReturnPiece(SpawnGrid.tiles[new Vector2(3, 5)].currentPiece);
        factory.ReturnPiece(SpawnGrid.tiles[new Vector2(3, 8)].currentPiece);
        SetPieceWithPosition(TeamType.TeamOne, new Vector2(3, 7));
        SpawnGrid.tiles[new Vector2(3, 7)].currentPiece.EnterBlock();
    }

    public void SelectQuarterback()
    {
        SpawnGrid.tiles[new Vector2(5, 1)].currentPiece.SelectEffect();
    }

    public void MoveQuarterback()
    {
        factory.ReturnPiece(SpawnGrid.tiles[new Vector2(5, 1)].currentPiece);
        SpawnGrid.tiles[new Vector2(5, 1)].currentPiece.DeselectEffect();
        SetPieceWithPosition(TeamType.TeamOne, new Vector2(5, 2));
        SpawnGrid.tiles[new Vector2(5, 2)].currentPiece.SetAsQuarterback(true);
    }

    public void MovePieceToBlockQB()
    {
        factory.ReturnPiece(SpawnGrid.tiles[new Vector2(7, 8)].currentPiece);
        SetPieceWithPosition(TeamType.TeamTwo, new Vector2(7, 7));
    }

    public void SelectQuarterbackForJump()
    {
        SpawnGrid.tiles[new Vector2(5, 2)].currentPiece.SelectEffect();
    }

    public void FirstQBJump()
    {
        factory.ReturnPiece(SpawnGrid.tiles[new Vector2(5, 2)].currentPiece);
        SetPieceWithPosition(TeamType.TeamOne, new Vector2(5, 4));
        SpawnGrid.tiles[new Vector2(5, 4)].currentPiece.SetAsQuarterback(true);
    }

    public void SecondQBJump()
    {
        factory.ReturnPiece(SpawnGrid.tiles[new Vector2(5, 4)].currentPiece);
        SetPieceWithPosition(TeamType.TeamOne, new Vector2(7, 6));
        SpawnGrid.tiles[new Vector2(7, 6)].currentPiece.SetAsQuarterback(true);
    }

    public void DeselectQB()
    {
        SpawnGrid.tiles[new Vector2(7, 6)].currentPiece.DeselectEffect();
    }
   
    public void QuarterbackTackled()
    {
        factory.ReturnPiece(SpawnGrid.tiles[new Vector2(7, 7)].currentPiece);
        factory.ReturnPiece(SpawnGrid.tiles[new Vector2(7, 6)].currentPiece);
        SetPieceWithPosition(TeamType.TeamTwo, new Vector2(7, 6));
        //SpawnGrid.tiles[new Vector2(7, 6)].currentPiece.EnterBlock();
    }

    public void PlaceScrimmageLinePiecesTwo()
    {
        for (int i = 0; i < 8; i++)
        {

            SetPieceWithPosition(TeamType.TeamOne, new Vector2(i, 5));
            SetPieceWithPosition(TeamType.TeamTwo, new Vector2(i, 6));
        }

        SetPieceWithPosition(TeamType.TeamOne, new Vector2(3, 4));
        SetPieceWithPosition(TeamType.TeamOne, new Vector2(4, 4));
        SetPieceWithPosition(TeamType.TeamOne, new Vector2(5, 4));

        SetPieceWithPosition(TeamType.TeamTwo, new Vector2(3, 7));
        SetPieceWithPosition(TeamType.TeamTwo, new Vector2(4, 7));
        SetPieceWithPosition(TeamType.TeamTwo, new Vector2(5, 7));
        SetPieceWithPosition(TeamType.TeamTwo, new Vector2(6, 7));

        SpawnGrid.tiles[new Vector2(4, 4)].currentPiece.SetAsQuarterback(false);


    }

    public void PlaceDefendingNormalDownPieces()
    {
        for(int i=0; i<6; i++)
        {
            SetPieceWithPosition(TeamType.TeamOne, new Vector2(i, 5));
        }

        SetPieceWithPosition(TeamType.TeamOne, new Vector2(3, 4));
        SetPieceWithPosition(TeamType.TeamOne, new Vector2(4, 4));
        SetPieceWithPosition(TeamType.TeamOne, new Vector2(2, 3));
        SetPieceWithPosition(TeamType.TeamOne, new Vector2(5, 3));
        SetPieceWithPosition(TeamType.TeamOne, new Vector2(3, 1));
        SetPieceWithPosition(TeamType.TeamOne, new Vector2(4, 1));
    }

    public void PlaceDefendingNowPieces()
    {
        for (int i = 0; i < 8; i++)
        {
            SetPieceWithPosition(TeamType.TeamOne, new Vector2(i, 5));
        }

        SetPieceWithPosition(TeamType.TeamOne, new Vector2(3, 4));
        SetPieceWithPosition(TeamType.TeamOne, new Vector2(4, 4));
        SetPieceWithPosition(TeamType.TeamOne, new Vector2(5, 4));
        SetPieceWithPosition(TeamType.TeamOne, new Vector2(6, 4));
    }

    public void OtherTeamAttacking()
    {
        for (int i = 0; i < 6; i++)
        {
            SetPieceWithPosition(TeamType.TeamTwo, new Vector2(i, 8));
        }

        SetPieceWithPosition(TeamType.TeamTwo, new Vector2(3, 10));
        SetPieceWithPosition(TeamType.TeamTwo, new Vector2(4, 10));
        SetPieceWithPosition(TeamType.TeamTwo, new Vector2(5, 10));

        SetPieceWithPosition(TeamType.TeamTwo, new Vector2(4, 12));
        SetPieceWithPosition(TeamType.TeamTwo, new Vector2(5, 12));

        SpawnGrid.tiles[new Vector2(5, 12)].currentPiece.SetAsQuarterback(false);
    }

    public void SpawnPassQB()
    {
        SetPieceWithPosition(TeamType.TeamOne, new Vector2(5, 5));
        SpawnGrid.tiles[new Vector2(5, 5)].currentPiece.image.GetComponent<CircleCollider2D>().enabled = false;
        SetQuarterback(new Vector2(5, 5));
        TurnLogic.currentTurn = TeamType.TeamOne;
        TurnLogic.myTeam = TeamType.TeamOne;
        GameLogic.inFreePlay = true;
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene(1);
    }

}
