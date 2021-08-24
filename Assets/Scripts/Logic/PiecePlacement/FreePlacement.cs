using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*Somewhat confusing - just remember that the scrimmageline is going to be off-by-one
 * between TeamOne and TeamTwo because of the grid system, and you're subtracting the row
 * from the scrimmageLine to determine whether it's either on the scrimmageline, or immediately behind it
 * including the offset either up or down from the scrimmageLine based on TeamOne or TeamTwo 
 */
public class FreePlacement : interfaces.IPiecePlacement
{
    public int RequiredPieces { get ; set ; }
    private int piecesPlaced;
    public int scrimmageLine;

    //The keys are always 0 and 1 because those are the offsets from the scrimmageLine during freeplay
    public Dictionary<int, RestrictionCounter> restrictions { get ; set ; }
    public bool FinishedPlacing { get { return piecesPlaced == RequiredPieces; } }

    public List<int> AllRows => OffsetKeys();
    
    //This is because the key in the FreePlacement dictionary represents the offset from the scrimmageLine,
    //Not the actual row itself like in the KickoffPlacement. Potentially change this so that they both work the same.
    private List<int> OffsetKeys()
    {
        List<int> keys = new List<int>(restrictions.Keys);
        for(int i=0; i<keys.Count; i++)
        {
            if (TurnLogic.myTeam == TeamType.TeamOne) keys[i] = scrimmageLine - keys[i];
            else
            {
                Debug.Log($"ScrimmageLine is {scrimmageLine}, sending {scrimmageLine + keys[i]}" );
                keys[i] = scrimmageLine + keys[i];
            }
        }
        return keys;
    }

    public void SetScrimmageLine(int i)
    {
        if(TurnLogic.myTeam == TeamType.TeamOne)
        {
            if (TurnLogic.teamState == TeamState.Attacking) scrimmageLine = i;
            else scrimmageLine = i - 1;
        }
        else
        {
            if (TurnLogic.teamState == TeamState.Attacking) scrimmageLine = i;
            else scrimmageLine = i + 1;
        }
    }
    
    //Remember that 0 and 1 as keys relate to the 0 and 1 offset method used in the 
    //FreePlacement restrictions dictionary
    public bool CanPlacePiece(int iRow)
    {
        int totalPieces = restrictions[0].current + restrictions[1].current;
        int frontRow = restrictions[0].current;
        int requiredFrontRow = restrictions[0].required;

        if (totalPieces == RequiredPieces) return false;

        bool isFrontRow = scrimmageLine - iRow == 0;
        if (isFrontRow)
        {
            restrictions[0].current++;
            piecesPlaced++;           
            return true;
        }

        bool isBackRow = CheckIsBackRow(iRow);       
        if (isBackRow && Mathf.Abs(scrimmageLine - iRow) < restrictions.Count)
        {
            int difference = requiredFrontRow - frontRow;
            if (frontRow < requiredFrontRow && (totalPieces + difference) + 1 > RequiredPieces) return false;
            else
            {
                restrictions[1].current++;
                piecesPlaced++;
                return true;
            }
        }
        return false;
    }

    //This gets the offset from the front row (up or down depending on team)
    //And then checks if it is equal to 1, meaning it is the row directly behind the
    //line of scrimmage
    private bool CheckIsBackRow(int iRow)
    {
        int backRow;
        if (TurnLogic.myTeam == TeamType.TeamOne) backRow = scrimmageLine - iRow;
        else backRow =  iRow - scrimmageLine;
        return backRow >= 1;
    }

    public void PieceRemoved(int iRow)
    {
        int var_I = Mathf.Abs(scrimmageLine - iRow);
        if (var_I > 1)
            var_I = 1;
        restrictions[var_I].current--;
        piecesPlaced--;
    }

    public void Reset()
    {
        var keys = new List<int>(restrictions.Keys);
        for(int i=0; i<keys.Count; i++)
        {
            restrictions[keys[i]].current = 0;
        }
        piecesPlaced = 0;

    }

    public int QuarterBackRow()
    {
        if (TurnLogic.myTeam == TeamType.TeamOne) return scrimmageLine - 1;
        else return scrimmageLine + 1;
    }

    public int ScrimmageLinePlz_Func()
    {
        return scrimmageLine;
    }
}

public class RestrictionCounter
{
    public RestrictionCounter(int _required)
    {
        required = _required;
        current = 0;
    }

    public int required { get; set; }
    public int current { get; set; }
}
