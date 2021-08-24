using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CheckerFootball.Types;

namespace CheckerFootball.Models 
{ 
    public class TeamModel
    {
        public List<PieceModel> pieces;
        public TeamType team;
        public TurnType turn; 

        /*
        public TeamModel(TeamType _team)
        {
            team = _team;
            for(int i=0; i<12; i++)
            {
                pieces.Add(GeneratePiece(i));
            }
        }
        */

        /*
        private PieceModel GeneratePiece(int ID)
        {
            return new PieceModel(ID);
        }
        */
    }
}
