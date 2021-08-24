using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandardFreeplay : IMovementRestriction
{

    public bool CanMove(Vector2 currentPos, Vector2 toPos)
    {
        for(int x= -1; x <= 1; x++)
        {
            for(int y= -1; y <= 1; y++)
            {                
                if (toPos == new Vector2(currentPos.x + x, currentPos.y + y)) return true;
            }
        }

        return false;
    }
}
