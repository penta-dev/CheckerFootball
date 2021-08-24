using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandardKickOff : IMovementRestriction
{    

    public bool CanMove(Vector2 currentPos, Vector2 toPos)
    {
        var myTeam = TurnLogic.myTeam;
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (toPos == new Vector2(currentPos.x + x, currentPos.y + y))
                {
                    if (myTeam == TeamType.TeamOne && currentPos.y < toPos.y) return true;
                    if (myTeam == TeamType.TeamTwo && currentPos.y > toPos.y) return true;
                }                

            }
        }

        return false;
    }
}
