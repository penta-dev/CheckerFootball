using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialBall : MonoBehaviour
{
    public Image ball;
    private Image spawnedBall;

    // Start is called before the first frame update
    void Start()
    {
        SpawnGrid.TileHeldEvent += CheckQuarterbackHeld;
        spawnedBall = Instantiate(ball, Vector2.zero, Quaternion.identity, GameObject.Find("BallHolder").transform);
    }

    private void OnDestroy()
    {
        SpawnGrid.TileHeldEvent -= CheckQuarterbackHeld;
    }

    private void DisplayBall(Tile tile)
    {
        QBStar.HideStar();
        spawnedBall.gameObject.SetActive(true);
        spawnedBall.GetComponent<CircleCollider2D>().enabled = true;
        spawnedBall.transform.position = tile.image.transform.position;
    }

    private void CheckQuarterbackHeld(Tile tile)
    {      
        tile.currentPiece.image.GetComponent<CircleCollider2D>().enabled = false;
        //currentQB = tile.currentPiece;
        DisplayBall(tile);
        StartCoroutine(HandlePass());        
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

        Debug.Log(lastPos);
        Debug.Log(endPos);

        Vector2 passDirection = endPos - lastPos;
        
        float positivepassDirectionY = passDirection.y;
        if (positivepassDirectionY < 0)
        {
            positivepassDirectionY *= -1;
        }
        if (positivepassDirectionY <= passDirection.x * 0.1f)
        {
            passDirection.y = 0;
        }       

        if (Vector2.Distance(startPos, endPos) < 100 || passDirection == Vector2.zero)
        {
            HideBall();

            yield break;
        }
        
        float passForce = Mathf.Abs(Vector2.Distance(lastPos, endPos));
        passForce = Mathf.Clamp(passForce * 50, 500, 3500);
        spawnedBall.GetComponent<BallCollider>().Pass(passDirection.normalized, passForce);
    }

    private void HideBall()
    {
        QBStar.ShowStar();
        spawnedBall.gameObject.SetActive(false);
    }
}
