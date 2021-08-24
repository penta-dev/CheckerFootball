using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CheckerFootball.Types;
using TMPro;
using System;

namespace CheckerFootball.Models
{
    public class PieceModel
    {
        public bool IsMyPiece { get { return team == TurnLogic.myTeam; } }
        public bool IsInEndZone {  get { return InEndZone(); } }
        public bool IsInBlock;
        public bool IsQuarterBack;

        public int ID;
        public Vector2 coordinate;
        public PieceType type;
        public TeamType team;
        public Image image;
        public IMovementRestriction movementType = new StandardKickOff();

        private TextMeshProUGUI debugText;

        public static event Action<PieceModel> ClaimedBallEvent;

        public PieceModel(int _ID, Image _image)
        {
            ID = _ID;
            type = PieceType.Standard;
            image = _image;
            debugText = image.transform.Find("DebugText").GetComponent<TextMeshProUGUI>();
        }

        public void MovePiece(Tile tile)
        {
            image.transform.localPosition = tile.tilePosition;
            coordinate = tile.coord;
            tile.currentPiece = this;
            SetImageName(tile.coord);

            if (tile.looseBallTile)
            {                
                SetAsQuarterback(true);
                tile.looseBallTile = false;
                ClaimedBallEvent?.Invoke(this);                
            }
        }

        public void SetImageName(Vector2 v)
        {
            image.name = $"{(int)v.x}{(int)v.y}";
        }

        public void SelectEffect() 
        {  
            image.transform.localScale = Vector3.one * 1.1f;
            Debug.Log("Selecting");
        }

        public void DeselectEffect()
        {
            image.transform.localScale = Vector3.one;
            Debug.Log("Deselecting");
        }

        public void EnterBlock()
        {
            IsInBlock = true;
            debugText.text = "B";
        }

        public void SetAsQuarterback(bool freePlay)
        {
            IsQuarterBack = true;
            if (!freePlay) movementType = new QuarterBackStandard();
            else movementType = new QuarterBack();
            QBStar.SetStarToQB(this);
            QBStar.SetSize(image.rectTransform.sizeDelta / 1.1f);
            //debugText.text = "Q";
        }        

        public void DemoteQuarterback()
        {
            IsQuarterBack = false;
            movementType = new StandardFreeplay();
            QBStar.HideStar();
            //debugText.text = "";
        }

        private bool InEndZone()
        {
            if (team == TeamType.TeamOne) if (coordinate.y == 0 || coordinate.y == 1) return true;           
            else 
            {
                if (coordinate.y == 12 || coordinate.y == 13) return true;
            }
            return false;
        }

        public void Reset()
        {
            IsInBlock = false;
            if (IsQuarterBack) DemoteQuarterback();
            //IsQuarterBack = false;
            debugText.text = "";
        }

    }
}
