using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CheckerFootball.Models;

public class Tile 
{
    public bool IsTaken { get { return currentPiece != null; } }

    public Vector2 coord;
    public Vector2 tilePosition;
    public Image image;
    public PieceModel currentPiece;
    public Color baseColor = Color.white;
    public bool looseBallTile;

    public void SetColour(Color color)
    {
        image.color = color;
        baseColor = color;
    }

    public void SetHighlightColor(Color color)
    {
        image.color = color;
        //image.transform.localScale *= 1.05f;
    }

    public void ResetColor()
    {
        image.color = baseColor;
        //image.transform.localScale /= 1.05f;
    }
}
