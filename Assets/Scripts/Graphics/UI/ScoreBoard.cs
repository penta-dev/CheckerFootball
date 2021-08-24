using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ScoreBoard : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public GameObject teamOneTurn;
    public GameObject teamTwoTurn;

    public Image possessionMarker;
    public Vector3 teamOnePossession;
    public Vector3 teamTwoPossession;

    public TextMeshProUGUI teamOneScore;
    public TextMeshProUGUI teamTwoScore;

    public Transform scoreBoard;

    private bool timerPaused = true;

    public static event Action TimerZeroEvent;

    float fTimer = 60 * 30;

    private void Start()
    {
        //J- StartCoroutine(TestTimer()); !!!!

        TurnLogic.ChangedPossessionEvent += SetPossession;
        TurnLogic.ChangedTurnEvent += SetTurn;
        TurnLogic.SetInitialValues();

        GameLogic.SetTeamScoreEvent += SetScore;

        GameLogic.EnteringGameplayEvent += UnpauseTimer;
        GameLogic.EnteringPlacementEvent += PauseTimer;
    }

    private void OnDestroy()
    {
        TurnLogic.ChangedPossessionEvent -= SetPossession;
        TurnLogic.ChangedTurnEvent -= SetTurn;
        GameLogic.SetTeamScoreEvent -= SetScore;
        GameLogic.EnteringGameplayEvent -= UnpauseTimer;
        GameLogic.EnteringPlacementEvent -= PauseTimer;
    }

    public void SetPossession()
    {                
        if(TurnLogic.teamState == TeamState.Attacking)
        {
            if (TurnLogic.myTeam == TeamType.TeamOne) possessionMarker.transform.localPosition = teamOnePossession;
            else possessionMarker.transform.localPosition = teamTwoPossession;
        }
        else
        {
            if (TurnLogic.myTeam == TeamType.TeamOne) possessionMarker.transform.localPosition = teamTwoPossession;
            else possessionMarker.transform.localPosition = teamOnePossession;
        }
    }

    public void SetTurn()
    {
        if (TurnLogic.currentTurn == TeamType.TeamOne)
        {
            teamOneTurn.gameObject.SetActive(true);
            teamTwoTurn.gameObject.SetActive(false);
        }
        else
        {
            teamOneTurn.gameObject.SetActive(false);
            teamTwoTurn.gameObject.SetActive(true);
        }
    }

    public static void GameTimerZero()
    {
        TimerZeroEvent?.Invoke();
    }

    IEnumerator TestTimer()
    {
        
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                TimerZeroEvent?.Invoke();
            }
            if (!timerPaused)
            {
                fTimer -= Time.deltaTime;
                var minutes = Mathf.FloorToInt(fTimer / 60);
                var seconds = Mathf.FloorToInt(fTimer % 60);
                timerText.text = string.Format("{00:00}:{1:00}", minutes, seconds);

                if (fTimer <= 0) TimerZeroEvent?.Invoke();
            }
            
            yield return null;
        }
    }

    private void SetScore(TeamType team, string amount)
    {
        if (team == TeamType.TeamOne) teamOneScore.text = amount;
        else teamTwoScore.text = amount;

        StartCoroutine(DisplayBoard());
    }

    private IEnumerator DisplayBoard()
    {
        Vector2 toPos = new Vector2(scoreBoard.localPosition.x, scoreBoard.localPosition.y - scoreBoard.GetComponent<Image>().rectTransform.sizeDelta.y * 1.5f);

        while ((Vector2)scoreBoard.localPosition != toPos)
        {
            scoreBoard.localPosition = Vector2.MoveTowards(scoreBoard.localPosition, toPos, 5f);
            yield return null;
        }

        yield return new WaitForSeconds(3);

        StartCoroutine(HideBoard());
    }

    private IEnumerator HideBoard()
    {
        Vector2 toPos = new Vector2(scoreBoard.localPosition.x, scoreBoard.localPosition.y + scoreBoard.GetComponent<Image>().rectTransform.sizeDelta.y * 1.5f);

        while ((Vector2)scoreBoard.localPosition != toPos)
        {
            scoreBoard.localPosition = Vector2.MoveTowards(scoreBoard.localPosition, toPos, 5f);
            yield return null;
        }
    }

    private void PauseTimer()
    {
        timerPaused = true;
    }

    private void UnpauseTimer()
    {
        Debug.Log("UNPAUSING");
        timerPaused = false;
    }


}
