using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMovementRestriction
{
    bool CanMove(Vector2 currentPos, Vector2 toPos);
}
