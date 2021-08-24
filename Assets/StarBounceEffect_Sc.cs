using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarBounceEffect_Sc : MonoBehaviour
{
    public Vector3 originalPos;

    public int dir;

    public float distanceToGo;

    public float startingScale;

    // Start is called before the first frame update
    void Start()
    {
        startingScale = transform.localScale.x;
        originalPos = transform.localPosition;
        dir = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.parent != null && transform.parent.parent != null && transform.parent.parent != null && transform.parent.parent.parent.parent != null)
        {
            distanceToGo = 20 * (transform.parent.parent.parent.parent.GetComponent<RectTransform>().sizeDelta.x / 1080);
            transform.localScale = Vector3.one * startingScale * (transform.parent.parent.parent.parent.GetComponent<RectTransform>().sizeDelta.x / 1080);
        }

        transform.localPosition = Vector3.Lerp(transform.localPosition, transform.localPosition + (Vector3.up * dir), 2.5f * Time.deltaTime * distanceToGo);

        if(transform.localPosition.y > originalPos.y + distanceToGo * 0.99f)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, originalPos.y + (distanceToGo * 0.98f), transform.localPosition.z);
            dir *= -1;
        }

        if (transform.localPosition.y < originalPos.y - distanceToGo * 0.99f)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, originalPos.y - (distanceToGo * 0.98f), transform.localPosition.z);
            dir *= -1;
        }
    }
}
