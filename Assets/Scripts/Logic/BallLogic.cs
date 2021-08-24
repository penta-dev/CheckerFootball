using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CheckerFootball.Models;
using Photon.Pun;
using System;


public class BallLogic : MonoBehaviour
{
    public Image ball;
    private Image spawnedBall;

    public static PieceModel currentQB;

    private bool forwardPassMadeThisDown;

    public static event Action ScoredOnePointConversion;
    public static event Action FailedOnePointConversion;
    public static event Action PassingStartedEvent;
    public static event Action OffScreenPassEvent;
    public static event Action NOffScreenPassEvent;

    Vector3 ballLastPos;
    bool ballLastState;

    private void Awake()
    {
        currentQB = null;
    }

    private void OnDestroy()
    {
        SpawnGrid.TileHeldEvent -= CheckQuarterbackHeld;
        BallCollider.BallCollidedEvent -= BallCollidedWithPlayer;
        BallCollider.BallFumbledEvent -= BallFumbled;
        PieceModel.ClaimedBallEvent -= HideBall;
        GameLogic.NewDownEvent -= Reset;

        currentQB = null;
    }

    void Start()
    {
        SpawnGrid.TileHeldEvent += CheckQuarterbackHeld;
        BallCollider.BallCollidedEvent += BallCollidedWithPlayer;
        BallCollider.BallFumbledEvent += BallFumbled;
        PieceModel.ClaimedBallEvent += HideBall;
        GameLogic.NewDownEvent += Reset;
         
        spawnedBall = Instantiate(ball, Vector2.zero, Quaternion.identity, GameObject.Find("BallHolder").transform);
        ballLastPos = spawnedBall.transform.position;
        ballLastState = spawnedBall.gameObject.activeSelf;

        if (Application.platform == RuntimePlatform.WindowsPlayer) spawnedBall.rectTransform.sizeDelta /= 3;

        HideBall(null);
    }

    private void Update()
    {
        if(GameLogic.inActivePlay || GameLogic.inOnePointConversion)
        {
            if ((!GameLogic.inActivePlay || ballLastState != spawnedBall.gameObject.activeSelf || ballLastPos != spawnedBall.transform.position) 
                && (TurnLogic.IsMyTurn || GameLogic.inOnePointConversion))
            {
                DoNUpdateBallPos(true); //J- This one to set False
            }
        }
        ballLastPos = spawnedBall.transform.position;
        ballLastState = spawnedBall.gameObject.activeSelf;
    }

    void DoNUpdateBallPos(bool snapTo)
    {
        Vector3 scaledDownPos = spawnedBall.transform.localPosition;
        scaledDownPos.x /= SpawnGrid.tileSize.x;
        scaledDownPos.y /= SpawnGrid.tileSize.y;
        PhotonView.Get(this).RPC("NUpdateBallPos", RpcTarget.Others, scaledDownPos, spawnedBall.gameObject.activeSelf, snapTo);
    }

    private void Reset()
    {
        forwardPassMadeThisDown = false;
    }

    private void DisplayBall(Tile tile)
    {
        QBStar.HideStar();
        spawnedBall.gameObject.SetActive(true);
        spawnedBall.GetComponent<CircleCollider2D>().enabled = true;
        spawnedBall.transform.position = tile.image.transform.position;
    }

    private void HideBall(PieceModel model)
    {
        QBStar.ShowStar();
        spawnedBall.gameObject.SetActive(false);
    }

    private void CheckQuarterbackHeld(Tile tile)
    {
        if (TurnLogic.currentTurn != TurnLogic.myTeam || !tile.currentPiece.IsMyPiece) return;
        if (!tile.IsTaken || !tile.currentPiece.IsQuarterBack) return;
        if (!GameLogic.inFreePlay && !GameLogic.inOnePointConversion) return;
        tile.currentPiece.image.GetComponent<CircleCollider2D>().enabled = false;
        currentQB = tile.currentPiece;
        DisplayBall(tile);
        StartCoroutine(HandlePass());
        PassingStartedEvent?.Invoke();
    }        

    [PunRPC]
    private void NetworkedBallMovement(Vector2 startKey, Vector2 endKey, BallAction actionType)
    {
        StartCoroutine(SendBallToLocation(startKey, endKey, actionType));
    }

    private IEnumerator SendBallToLocation(Vector2 startKey, Vector2 endKey, BallAction actionType)
    {
        Tile startTile = SpawnGrid.tiles[startKey];
        Vector2 position = SpawnGrid.tiles[endKey].image.transform.position;

        GameLogic.pieceMovement.ShowLastMove(SpawnGrid.tiles[startKey], SpawnGrid.tiles[endKey]);

        /*
        DisplayBall(startTile);
        while ((Vector2)spawnedBall.transform.position != position)
        {
            spawnedBall.GetComponent<Rigidbody2D>().MovePosition(position);
            spawnedBall.GetComponent<CircleCollider2D>().enabled = false;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        */

        spawnedBall.transform.position = position;
        spawnedBall.GetComponent<CircleCollider2D>().enabled = false;

        if (actionType == BallAction.Fumbled)
        {
            SpawnGrid.tiles[endKey].looseBallTile = true;
            SpawnGrid.tiles[startKey].currentPiece.DemoteQuarterback();
            spawnedBall.GetComponent<Image>().raycastTarget = false;
            HideBall(null);
        }
        else if(actionType == BallAction.Safety)
        {            
            HideBall(null);
        }
        else if(actionType == BallAction.OnePoint)
        {
            HideBall(null);
        }
        else
        {
            SpawnGrid.tiles[startKey].currentPiece.DemoteQuarterback();
            SpawnGrid.tiles[endKey].currentPiece.SetAsQuarterback(true);
            HideBall(null);
            spawnedBall.GetComponent<CircleCollider2D>().enabled = false;
        }

        yield return null;
    }

    private void BallCollidedWithPlayer(Vector2 key)
    {
        Tile tile = SpawnGrid.tiles[key];
        
        HideBall(null);
        //Stop collision with current QB if throw not strong enough
        if (tile.currentPiece.IsQuarterBack) return;

        if (tile.currentPiece.IsMyPiece)
        {
            //TODO
            Debug.Log("Passed");
        }
        else
        {
            //TODO
            PhotonView.Get(this).RPC("Interception", RpcTarget.All);
            Debug.Log("Intercepted");
        }

        GameLogic.pieceMovement.ShowLastMove(SpawnGrid.tiles[currentQB.coordinate], tile);

        if (WasPassForward(currentQB.coordinate, tile.coord)) forwardPassMadeThisDown = true;
        
        PhotonView.Get(this).RPC("NetworkedBallMovement", RpcTarget.Others, currentQB.coordinate, tile.coord, BallAction.Passed);
        PhotonView.Get(this).RPC("ChangeTurns", RpcTarget.All);

        currentQB.DemoteQuarterback();
        currentQB.image.GetComponent<CircleCollider2D>().enabled = true;
        tile.currentPiece.SetAsQuarterback(true);
        currentQB = tile.currentPiece;
        PhotonView.Get(this).RPC("NBallPassUpdate", RpcTarget.Others, key);
    }    

    [PunRPC]
    void NBallPassUpdate(Vector2 key)
    {
        Tile tile = SpawnGrid.tiles[key];
        GameLogic.pieceMovement.ShowLastMove(SpawnGrid.tiles[currentQB.coordinate], tile);
        currentQB.DemoteQuarterback();
        currentQB.image.GetComponent<CircleCollider2D>().enabled = true;
        tile.currentPiece.SetAsQuarterback(true);
        currentQB = tile.currentPiece;
    }

    private void BallFumbled()
    {
        Tile closestTile = SpawnGrid.GetTileClosestTo(spawnedBall.transform.position);
        if(closestTile == null && !GameLogic.inOnePointConversion)
        {
            Debug.Log("Offscreen Pass");
            OffScreenPassEvent?.Invoke();
            PhotonView.Get(this).RPC("NOffScreenPass", RpcTarget.Others);
            return;
        }

        if (GameLogic.inOnePointConversion)
        {
            Debug.Log("1px check");
            OnePointConversionCheck(closestTile);
        }
        else
        {
            Debug.Log("Fumblecheck");
            FumbleCheck(closestTile);
        }
        DoNUpdateBallPos(true);
    }

    [PunRPC]
    private void NOffScreenPass()
    {
        NOffScreenPassEvent?.Invoke();
        GameLogic.inActivePlay = false;
    }

    private void FumbleCheck(Tile closestTile)
    {
        bool wasForwardPass = WasPassForward(currentQB.coordinate, closestTile.coord); //J- Maybe issue
        spawnedBall.transform.position = closestTile.image.transform.position;

        GameLogic.pieceMovement.ShowLastMove(SpawnGrid.tiles[currentQB.coordinate], SpawnGrid.tiles[closestTile.coord]);

        if (wasForwardPass)
        {
            PhotonView.Get(this).RPC("DeadBall", RpcTarget.All, currentQB.IsInEndZone, (int)closestTile.coord.y);
            HideBall(null);
            DoNUpdateBallPos(true);
            PhotonView.Get(this).RPC("NetworkedBallMovement", RpcTarget.Others, currentQB.coordinate, closestTile.coord, BallAction.Safety);
        }
        else
        {
            currentQB.DemoteQuarterback();
            closestTile.looseBallTile = true;
            forwardPassMadeThisDown = true;
            DoNUpdateBallPos(true);
            PhotonView.Get(this).RPC("NetworkedBallMovement", RpcTarget.Others, currentQB.coordinate, closestTile.coord, BallAction.Fumbled);
            Debug.Log("Loose Ball");
        }

        
        PhotonView.Get(this).RPC("ChangeTurns", RpcTarget.All);

        currentQB.image.GetComponent<CircleCollider2D>().enabled = true;
    }

    private void OnePointConversionCheck(Tile closestTile)
    {
        Debug.Log("Checking One Point Conversion");
        bool success = false;
        
        if(closestTile != null)
        {
            if (TurnLogic.myTeam == TeamType.TeamOne) success = closestTile.coord.y == 12 || closestTile.coord.y == 13;
            else success = closestTile.coord.y == 0 || closestTile.coord.y == 1;

            //off the map
            if (success && Vector2.Distance(spawnedBall.transform.position, closestTile.image.transform.position) > closestTile.image.rectTransform.sizeDelta.y * 1.1f)
            {
                Debug.Log("FAILED");
                success = false;
            }
        }

        if (success) ScoredOnePointConversion?.Invoke();
        else FailedOnePointConversion?.Invoke();
        HideBall(null);
    }


    private bool WasPassForward(Vector2 startPos, Vector2 endPos)
    {
        return TurnLogic.myTeam == TeamType.TeamOne ? endPos.y > startPos.y : endPos.y < startPos.y; //J- Fixed for rotated board

    }

    IEnumerator HandlePass()
    {
        Vector2 startPos = Input.mousePosition;
        Vector2 lastPos = Vector2.zero;
        Vector2 endPos = Vector2.zero;

        while (true)
        {
            lastPos = endPos;
            endPos = Input.mousePosition;
            if (Input.GetMouseButtonUp(0))
            {
                break;
            }

            yield return null;
        }

        Vector2 passDirection = endPos - lastPos;
        float positivepassDirectionY = passDirection.y;
        if(positivepassDirectionY < 0)
        {
            positivepassDirectionY *= -1;
        }
        if (positivepassDirectionY <= passDirection.x * 0.1f)
        {
            passDirection.y = 0;
        }

        if (!GameLogic.inOnePointConversion && (forwardPassMadeThisDown && passDirection.y > 0))
        {
            Debug.Log("Forward pass not allowed");
            HideBall(null);
            yield break;
        }

        if (Vector2.Distance(startPos, endPos) < 100)
        {
            HideBall(null);

            yield break;
        }

        float passForce = Mathf.Abs(Vector2.Distance(lastPos, endPos));
        passForce = Mathf.Clamp(passForce * 50, 500, 3500);
        spawnedBall.GetComponent<BallCollider>().Pass(passDirection.normalized, passForce);        
    }

    [PunRPC]
    private void NUpdateBallPos(Vector3 ballPos, bool visible, bool snapTo)
    {
        spawnedBall.gameObject.SetActive(visible);
        spawnedBall.GetComponent<CircleCollider2D>().enabled = false;
        Vector3 targetPos = new Vector3(ballPos.x * SpawnGrid.tileSize.x, ballPos.y * SpawnGrid.tileSize.y, spawnedBall.transform.position.z);

        if (snapTo)
        {
            spawnedBall.transform.localPosition = targetPos;
        } else
        {
            spawnedBall.transform.localPosition = Vector3.Lerp(spawnedBall.transform.localPosition, targetPos, Time.deltaTime * 40);
        }
        
        //J- Account for rotation
        var capturedBallPos = spawnedBall.transform.position;
        var origRot = spawnedBall.transform.parent.rotation;
        var tempRot = spawnedBall.transform.parent.rotation;
        tempRot.z = 180;
        spawnedBall.transform.parent.rotation = tempRot;
        spawnedBall.transform.position = capturedBallPos;
        spawnedBall.transform.parent.rotation = origRot;

        ballLastPos = spawnedBall.transform.position;
        ballLastState = spawnedBall.gameObject.activeSelf;
    }
}

public enum BallAction
{
    Passed,
    Fumbled,
    Safety,
    OnePoint
}
