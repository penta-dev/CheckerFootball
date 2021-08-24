using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KickoffPlacement : interfaces.IPiecePlacement
{
    public int RequiredPieces { get ; set; }
    private int totalPlaced;
    public List<int> AllRows =>  new List<int>(restrictions.Keys); 
    public Dictionary<int, RestrictionCounter> restrictions { get; set; }   
    public bool FinishedPlacing { get { return totalPlaced == RequiredPieces; } }

    public bool CanPlacePiece(int iRow)
    {
        if (restrictions.ContainsKey(iRow))
        {            
            if (restrictions[iRow].current < restrictions[iRow].required)
            {
                totalPlaced++;
                restrictions[iRow].current++;                
                return true;
            }
        }
        return false;
    }

    public void PieceRemoved(int iRow)
    {
        restrictions[iRow].current--;
        totalPlaced--;
    }

    public void Reset()
    {
        var keys = new List<int>(restrictions.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            restrictions[keys[i]].current = 0;
        }
        totalPlaced = 0;
    }

    public int QuarterBackRow()
    {
        if (TurnLogic.myTeam == TeamType.TeamOne) return 1;
        else return 12;
    }

    public int ScrimmageLinePlz_Func()
    {
        //J//Not needed
        return 0;
    }
}
