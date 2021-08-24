using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CheckerFootball.Models;

public class QBStar : MonoBehaviour
{

    public Transform starImage;
    private static Transform star;

    private void Awake()
    {
        star = null;
    }

    private void OnDestroy()
    {
        star = null;
    }

    private void Start()
    {
        star = Instantiate(starImage); //starImage;
        star.GetComponent<Image>().rectTransform.sizeDelta = new Vector2(Screen.width / 10, Screen.width / 10);
    }

    public static void SetSize(Vector2 v)
    {
        star.transform.GetChild(0).GetComponent<Image>().rectTransform.sizeDelta = v;
    }

    public static void SetStarToQB(PieceModel qb)
    {
        star.SetParent(qb.image.transform);
        star.SetAsLastSibling();
        star.gameObject.SetActive(true);
        star.position = qb.image.transform.position;
        
    }

    public static void HideStar()
    {
        star.gameObject.SetActive(false);
    }

    public static void ShowStar()
    {
        star.gameObject.SetActive(true);
    }

}
