using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArrowPointer : MonoBehaviour
{
    
    private Image image;
    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PointToSquare(Vector2 position)
    {
        StopAllCoroutines();

        Show();
        transform.position = position;
        transform.Translate(Vector2.right * image.rectTransform.sizeDelta.x);
        StartCoroutine(PointBounce());
    }

    IEnumerator PointBounce()
    {
        Vector2 farPos = new Vector2(transform.position.x + image.rectTransform.sizeDelta.x / 2, transform.position.y);
        Vector2 nearPos = transform.position;

        Vector2 targetPos = farPos;
        while (true)
        {
            if((Vector2)transform.position != targetPos)
            {
                transform.position = Vector2.MoveTowards(transform.position, targetPos, 0.8f);
            }
            else
            {
                targetPos = targetPos == farPos ? nearPos : farPos;
            }

            yield return null;
        }
    }

    public void Hide() => image.enabled = false;
    public void Show() => image.enabled = true;
}
