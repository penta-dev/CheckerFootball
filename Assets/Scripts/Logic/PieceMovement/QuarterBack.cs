using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuarterBack : IMovementRestriction
{

    private IMovementRestriction standard = new StandardFreeplay();

    public bool jumped;

    public bool CanMove(Vector2 currentPos, Vector2 toPos)
    {
        bool isStandard = standard.CanMove(currentPos, toPos);
        //QB can't make normal move after jumping own player
        if (isStandard) return !jumped;

        bool jumping = Jumping(currentPos, toPos);        
        if (jumping)
        {
            jumped = jumping;
            return true;
        }

        return false;
    }

    private bool Jumping(Vector2 currentPos, Vector2 toPos)
    {
        if (SpawnGrid.tiles[currentPos].currentPiece.IsInBlock) return false;
        for(int x=-2; x <= 2; x+= 2)
        {
            for(int y=-2; y<= 2; y+= 2)
            {
                var key = new Vector2(currentPos.x + x, currentPos.y + y);
                var qb = SpawnGrid.tiles[currentPos].currentPiece;
                if (!SpawnGrid.tiles.ContainsKey(key)) continue;
                var tile = SpawnGrid.tiles[key];
                var offset = GetOffset(currentPos, tile.coord);                

                if (tile.coord == toPos && SpawnGrid.tiles[offset].IsTaken && SpawnGrid.tiles[offset].currentPiece.team == TurnLogic.currentTurn && !qb.IsInBlock) return true;
            }
        }
        return false;
    }

    Vector2 GetOffset(Vector2 current, Vector2 to)
    {
        float x = 0;
        float y = 0;

        if (to.x > current.x) x = to.x - 1;
        if (to.x < current.x) x = to.x + 1;
        if (to.x == current.x) x = to.x;

        if (to.y > current.y) y = to.y - 1;
        if (to.y < current.y) y = to.y + 1;
        if (to.y == current.y) y = to.y ;

        return new Vector2(x, y);
    }

}
