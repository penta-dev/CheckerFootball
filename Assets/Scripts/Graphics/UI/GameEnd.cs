using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameEnd : MonoBehaviour
{
    public Transform gameEndBoard;
    public Transform gameEndBoardWon;
    public Transform gameEndBoardLost;
    public TextMeshProUGUI scoreOneText;
    public TextMeshProUGUI scoreTwoText;
    public TextMeshProUGUI victoryText;

    // Start is called before the first frame update
    void Start()
    {
        GameLogic.GameEndedEvent += DisplayGameOver;
    }

    private void OnDestroy()
    {
        GameLogic.GameEndedEvent -= DisplayGameOver;
    }

    private void DisplayGameOver(int scoreOne, int scoreTwo, bool didWin)
    {
        gameEndBoard.gameObject.SetActive(true);
        scoreOneText.text = scoreOne.ToString();
        scoreTwoText.text = scoreTwo.ToString();
        gameEndBoardWon.gameObject.SetActive(didWin);
        gameEndBoardLost.gameObject.SetActive(!didWin);

        //victoryText.text = didWin ? "You Win!" : "You Lose";      

        SubmitScores(scoreOne, scoreTwo);
    }

    private void SubmitScores(int scoreOne, int scoreTwo)
    {        
        if (TurnLogic.myTeam == TeamType.TeamOne)
        {
           
            GameObject.Find("Leaderboard").GetComponent<Leaderboard>().PushHighScore(scoreOne);
            
        }
        else
        {            
            GameObject.Find("Leaderboard").GetComponent<Leaderboard>().PushHighScore(scoreTwo);            
        }
    }

  
}
