using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using CheckerFootball.Models;

[System.Serializable]
public class MTileSelected : IMessage
{
    public Vector2 coord;
}

public class MPlacePiece : IMessage
{
    public Vector2 tileKey;   
    public TeamType team;
}

public class MRemovePiece : IMessage
{
    public PieceModel piece;
}

public class MTileDictionary : IMessage
{
    public Dictionary<Vector2, Tile> tileDictionary;
}
