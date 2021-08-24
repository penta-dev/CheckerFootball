using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//yards are measured 10-90 even though the yardlines on the field are 10-50 for both teams (mirrored)
//this is to make it easier to reason in the code about where a tackle is taking place without having to do
//lots of checks about which team is being tackled etc
public class YardLogic
{

    public Vector2 currentScrimmageLine;    
        
    public int RowToIndex(int iRow, TeamType team)
    {
        if (team == TeamType.TeamOne) return iRow;
        else return 14 - (iRow + 1);
    }
    
}
