using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public static class TurnLogic
{//J- Issue?
    public static TeamType myTeam = new TeamType();
    public static TeamType currentTurn = new TeamType();
    public static TeamState teamState = new TeamState();

    public static event Action ChangedPossessionEvent;
    public static event Action ChangedTurnEvent;

    public static bool IsMyTurn { get { return currentTurn == myTeam; } }  

    public static void SetMyTeam(TeamType type)
    {
        myTeam = type;
        if (myTeam == TeamType.TeamOne) teamState = TeamState.Attacking;
        else teamState = TeamState.Defending;
    }   

    public static void SetInitialValues()
    {
        ChangedTurnEvent?.Invoke();
        ChangedPossessionEvent?.Invoke();
    }

    public static void ChangeTurn()
    {
        if (currentTurn == TeamType.TeamOne) currentTurn = TeamType.TeamTwo;
        else currentTurn = TeamType.TeamOne;

        ChangedTurnEvent?.Invoke();
    }

    public static void ChangeTeam()
    {
        if (myTeam == TeamType.TeamOne) myTeam = TeamType.TeamTwo;
        else myTeam = TeamType.TeamOne;
    }

    public static void ChangePossesion()
    {
        teamState = teamState == TeamState.Attacking ? TeamState.Defending : TeamState.Attacking;
        ChangedPossessionEvent?.Invoke();
    }  

    public static void SetPossessionOnFumbledBallPickup(TeamType team)
    {
        if (myTeam == team) teamState = TeamState.Attacking;
        else teamState = TeamState.Defending;

        ChangedPossessionEvent?.Invoke();
    }

}

public enum TeamState
{
    Attacking,
    Defending
}
