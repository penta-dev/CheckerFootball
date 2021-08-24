using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenericSlidePointer : MonoBehaviour
{

    private Vector2 pointOne;
    private Vector2 pointTwo;

    public Image arrow;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
      
    }

    public void SetPoints(Vector2 one, Vector2 two, PointDirection dir, float offset)
    {
        StopAllCoroutines();

        pointOne = one;
        pointTwo = two;

        arrow.enabled = true;

        PointDirectionOffset(SpawnGrid.tiles[pointOne].image.transform.position, dir, offset);
        StartCoroutine(PointSlide(dir));        
    }

    public void HidePointer()
    {
        StopAllCoroutines();
        arrow.enabled = false;
    }

    private void PointDirectionOffset(Vector2 pos, PointDirection dir, float offset)
    {
        Debug.Log(offset);
        float tileWidth = SpawnGrid.tiles[Vector2.zero].image.rectTransform.sizeDelta.x;
        float tileHeight = SpawnGrid.tiles[Vector2.zero].image.rectTransform.sizeDelta.x;
        arrow.transform.position = pos;
        switch (dir)
        {
            case PointDirection.Up:                
                arrow.transform.localEulerAngles = new Vector3(0, 0, 270);
                if (offset != 0) arrow.transform.Translate(Vector2.down * (tileHeight * offset));               
                break;
            case PointDirection.Down:
                arrow.transform.localEulerAngles = new Vector3(0, 0, 90);
                if (offset != 0) arrow.transform.Translate(Vector2.up * (tileHeight * offset));                
                break;
            case PointDirection.Left:
                arrow.transform.localEulerAngles = Vector3.zero;
                if (offset != 0) arrow.transform.Translate(Vector2.right * (tileWidth * offset));                
                break;
            case PointDirection.Right:
                arrow.transform.localEulerAngles = new Vector3(0, 0, 180);
                if (offset != 0) arrow.transform.Translate(transform.right * (tileHeight * offset) );              
                break;
        }
    }

    IEnumerator PointSlide(PointDirection dir)
    {        
        Vector2 farPos = SpawnGrid.tiles[pointTwo].image.transform.position;
        Vector2 nearPos = SpawnGrid.tiles[pointOne].image.transform.position;
        
        switch (dir)
        {
            case PointDirection.Up:
                nearPos = new Vector2(nearPos.x, arrow.transform.position.y);
                farPos = new Vector2(farPos.x, arrow.transform.position.y);
                break;
            case PointDirection.Down:
                nearPos = new Vector2(nearPos.x, arrow.transform.position.y);
                farPos = new Vector2(farPos.x, arrow.transform.position.y);
                break;
            case PointDirection.Left:
                nearPos = new Vector2(arrow.transform.position.x, nearPos.y);
                farPos = new Vector2(arrow.transform.position.x, farPos.y);
                break;
            case PointDirection.Right:
                nearPos = new Vector2(arrow.transform.position.x, nearPos.y);
                farPos = new Vector2(arrow.transform.position.x, farPos.y);
                break;
        }
        arrow.transform.position = nearPos;
        Vector2 targetPos = farPos;
        
        while (true)
        {
            if ((Vector2)arrow.transform.position != targetPos)
            {
                arrow.transform.position = Vector2.MoveTowards(arrow.transform.position, targetPos, 300f * Time.deltaTime);
            }
            else
            {
                targetPos = targetPos == farPos ? nearPos : farPos;
            }

            yield return null;
        }
    }

    public void PointToSquare(Vector2 position, PointDirection dir, float offset)
    {
        StopAllCoroutines();
        
        PointDirectionOffset(position, dir, offset);
        arrow.enabled = true;            
        StartCoroutine(PointBounce(dir));
    }

    IEnumerator PointBounce(PointDirection dir)
    {
        Vector2 farPos = Vector2.zero;
        switch (dir)
        {
            case PointDirection.Up:
                farPos = new Vector2(arrow.transform.position.x, arrow.transform.position.y + arrow.rectTransform.sizeDelta.y / 2);
                break;
            case PointDirection.Down:
                farPos = new Vector2(arrow.transform.position.x, arrow.transform.position.y - arrow.rectTransform.sizeDelta.y / 2);
                break;
            case PointDirection.Left:
                farPos = new Vector2(arrow.transform.position.x + arrow.rectTransform.sizeDelta.x / 2, arrow.transform.position.y);
                break;
            case PointDirection.Right:
                farPos = new Vector2(arrow.transform.position.x - arrow.rectTransform.sizeDelta.x / 2, arrow.transform.position.y);
                break;
        }
        
        Vector2 nearPos = arrow.transform.position;

        arrow.transform.position = nearPos;
        Vector2 targetPos = farPos;        
        while (true)
        {
            if ((Vector2)arrow.transform.position != targetPos)
            {
                arrow.transform.position = Vector2.MoveTowards(arrow.transform.position, targetPos, 200f * Time.deltaTime);
            }
            else
            {
                targetPos = targetPos == farPos ? nearPos : farPos;
            }

            yield return null;
        }
    }
}

public enum PointDirection
{
    Up,
    Down,
    Left,
    Right
}
