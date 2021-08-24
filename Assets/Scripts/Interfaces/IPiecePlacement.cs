using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace interfaces
{   
    public interface IPiecePlacement
    {
        int RequiredPieces { get; set; }
        bool CanPlacePiece(int iRow);
        void PieceRemoved(int iRow);
        int QuarterBackRow();
        int ScrimmageLinePlz_Func();
        Dictionary<int, RestrictionCounter> restrictions { get; set; }
        List<int> AllRows { get; }
        bool FinishedPlacing { get; }
        void Reset();
        
    }
}
