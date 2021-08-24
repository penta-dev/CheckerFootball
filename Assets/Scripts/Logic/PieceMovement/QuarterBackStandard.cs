using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuarterBackStandard : IMovementRestriction
{
    public bool CanMove(Vector2 currentPos, Vector2 toPos)
    {
        return false;
    }
}
