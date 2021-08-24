using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class SpawnGrid : MonoBehaviour
{
    public Color fieldBlue;
    public Color fieldGreen;
    public Color fieldWhite;
    public Color highlight;

    public MessageHandler messaging;
    public Image tile;
    public TextMeshProUGUI yardMark;
    public Transform yardMarkHolder;
    public Canvas canvas;

    private static YardLogic yardLogic = new YardLogic();

    public static Vector2 tileSize;

    Transform tileHolder;

    public static Dictionary<Vector2, Tile> tiles = new Dictionary<Vector2, Tile>();

    private static List<Tile> highlightedTiles = new List<Tile>();

    public static event Action<Tile> TileHeldEvent;



    void Awake()
    {
        yardLogic = new YardLogic();
        tileSize = new Vector2();
        tiles = new Dictionary<Vector2, Tile>();
        highlightedTiles = new List<Tile>();

        tileHolder = GameObject.Find("TileHolder").transform;

        if (Application.platform == RuntimePlatform.WindowsPlayer) tileSize = new Vector2(Screen.height / 14, Screen.height / 14);
        else tileSize = new Vector2(Screen.width / 8, Screen.height / 14);

        CreateGrid();

        //GameLogic.ResetToNewDownEvent += Reset;
        PieceMovement.QBTackledEvent += Reset;
        GameLogic.ResetToKickoffEvent += Reset;
        GameLogic.ClearBoardEvent += Reset;
        PieceMovement.twoPCEvent += Reset;
        BallLogic.OffScreenPassEvent += Reset;
        BallLogic.NOffScreenPassEvent += Reset;
    }

    private void OnDestroy()
    {
        PieceMovement.QBTackledEvent -= Reset;
        GameLogic.ResetToKickoffEvent -= Reset;
        GameLogic.ClearBoardEvent -= Reset;
        PieceMovement.twoPCEvent -= Reset;
        BallLogic.OffScreenPassEvent -= Reset;
        BallLogic.NOffScreenPassEvent -= Reset;

        yardLogic = new YardLogic();
        tileSize = new Vector2();
        tiles = new Dictionary<Vector2, Tile>();
        highlightedTiles = new List<Tile>();
    }

    public void CreateGrid()
    {
        
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 14; y++)
            {
                var spawned = Instantiate(tile, Vector3.zero, Quaternion.identity, tileHolder);
                spawned.rectTransform.sizeDelta = tileSize;
                
                PositionTile(spawned, x, y);
                

                Vector2 tileCoord = new Vector2(x, y);

                Tile newTile = new Tile()
                {
                    coord = tileCoord,
                    tilePosition = spawned.transform.localPosition,
                    image = spawned,                    
                };

                newTile.image.transform.localScale /= 1.05f;

                tiles.Add(tileCoord, newTile);

                AssignTilePressAction(spawned, x, y);
                ColourTile(tileCoord, newTile);
                SpawnYardMark(tileCoord, newTile);
            }
        }
              
    }
    
    private void AssignTilePressAction(Image tile, int x, int y)
    {
        Vector2 tileCoord = new Vector2(x, y);
        var message = new MTileSelected{ coord = tileCoord };

        tile.GetComponent<Button>().onClick.AddListener( ()=> messaging.SendMessage(message) );

        var entry = new EventTrigger.Entry() { eventID = EventTriggerType.PointerDown };
        entry.callback.AddListener((data) => StartCoroutine(CheckTileHold(tileCoord)) );

        tile.GetComponent<EventTrigger>().triggers.Add(entry);
    }    

    private IEnumerator CheckTileHold(Vector2 tileCoord)
    {
        float time = 0;
        while(Input.GetMouseButton(0))
        {
            time += Time.deltaTime;
            if(time >= 0.75f)
            {
                TileHeldEvent?.Invoke(tiles[tileCoord]);
                break;
            }
            yield return null;
        }
    }

    private void PositionTile(Image tile, int x, int y)
    {
        Vector2 tileDimensions = tile.rectTransform.sizeDelta;
        Vector2 startPoint = new Vector2((0 - Screen.width / 2) + tileDimensions.x / 2, (0 - Screen.height / 2) + tileDimensions.y / 2);
        tile.transform.localPosition = new Vector2(startPoint.x + tileDimensions.x * x, startPoint.y + tileDimensions.y * y);               
    }

    public static void HighlightTileInRow(int iRow)
    {
        tiles[(new Vector2(0, iRow))].SetHighlightColor(Color.blue);
        
    }

    public static void HighlightRestrictedRows(List<int> rows, TeamType team)
    {
        foreach (int i in rows)
        {            
            if (!GameLogic.inFreePlay) HighlightRow(yardLogic.RowToIndex(i, team));
            else HighlightRow(i);   
        }
    }

    public static void HighlightRow(int iRow)
    {
        if(iRow > 0 && iRow <= 13)
        {
            for (int i = 0; i < 8; i++)
            {
                var v = new Vector2(i, iRow);
                tiles[v].SetHighlightColor(new Color(0.9f, 0.5f, 0.1f, 0.65f));
                highlightedTiles.Add(tiles[v]);
            }
        }
    }

    public static void UnhilightAllTiles()
    {
        for(int i=0; i<highlightedTiles.Count; i++)
        {
            highlightedTiles[i].ResetColor();
        }
        highlightedTiles.Clear();
    }

    private void Reset()
    {
        var ts = new List<Tile>(tiles.Values);
        foreach(var t in ts)
        {
            t.currentPiece = null;
        }
        UnhilightAllTiles();
    }

    private void ColourTile(Vector2 coord, Tile tile)
    {
        int y = (int)coord.y;
        int x = (int)coord.x;

        if (y > 1 && y < 12) tile.SetColour(fieldGreen);
        
        if (y <= 1)
        {
            if(y == 0 && x % 2 == 0) tile.SetColour(fieldBlue);
            if(y == 1 && x % 2 != 0) tile.SetColour(fieldBlue);
        }
        
        if (y >= 12)
        {
            if (y == 12 && x % 2 != 0) tile.SetColour(fieldBlue);
            if (y == 13 && x % 2 == 0) tile.SetColour(fieldBlue);
        }

    }

    private void SpawnYardMark(Vector2 coord, Tile tile)
    {
        float scaler = Screen.width / 1080f; 

        if (coord.x != 1 && coord.x != 6) return;
        if (coord.y < 1 || coord.y > 11) return;

        var mark = Instantiate(yardMark, tile.image.transform.position, Quaternion.identity, yardMarkHolder);

        mark.fontSize *= scaler;

        if (coord.y > 1 && coord.y < 11)
        {
            if (coord.y > 6) mark.text = $"{ 100 - (coord.y - 1) * 10}";
            else mark.text = $"{(coord.y - 1) * 10}";
        }

        Vector2 offsetDir = coord.x == 1 ? Vector2.left : Vector2.right;
        Vector3 offsetRot = coord.x == 1 ? new Vector3(0,0, -90) : new Vector3(0, 0, 90);

        mark.transform.Translate(Vector2.up * tile.image.rectTransform.sizeDelta.y / 2);
        mark.transform.Translate((offsetDir * (tile.image.rectTransform.sizeDelta.y / 2f - mark.rectTransform.sizeDelta.y / 1.5f)) / scaler);
        mark.transform.localEulerAngles = offsetRot;
        
    }

    public static Tile GetTileClosestTo(Vector3 location)
    {
        List<Vector2> keys = new List<Vector2>(tiles.Keys);
        Vector2 closest = Vector2.zero;
        float distance = float.PositiveInfinity;
        for (int i = 0; i < keys.Count; i++)
        {
            var key = keys[i];
            var checkDist = Vector3.Distance(tiles[keys[i]].image.transform.position, location);
            if (checkDist < distance)
            {
                distance = checkDist;
                closest = key;
            }
        }
        Debug.Log("--1");
        var tileSpacing = Vector3.Distance(tiles[keys[0]].image.transform.position, tiles[keys[1]].image.transform.position);
        if(distance > tileSpacing && !GameLogic.inOnePointConversion)
        {
            Debug.Log("--2");
            return null;
        }
        Debug.Log("--3");

        return tiles[closest];
    }
}
