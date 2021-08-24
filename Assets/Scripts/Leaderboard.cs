using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelBusters.EssentialKit;
using VoxelBusters.CoreLibrary;

public class Leaderboard : MonoBehaviour
{

    ILocalPlayer localPlayer;
    public string leaderboardID;
    ILeaderboard leaderboard;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this);
        if (GameServices.IsAvailable()) GameServices.Authenticate();

    }

    private void OnEnable()
    {
        GameServices.OnAuthStatusChange += OnAuthStatusChange;
    }

    private void OnDisable()
    {
        GameServices.OnAuthStatusChange -= OnAuthStatusChange;

    }

    private void OnAuthStatusChange(GameServicesAuthStatusChangeResult result, Error error)
    {
        if (error == null)
        {
            Debug.Log("Received auth status change event");
            Debug.Log("Auth status: " + result.AuthStatus);
            Debug.Log("Local player: " + result.LocalPlayer);
            if (result.AuthStatus == LocalPlayerAuthStatus.Authenticated)
            {
                localPlayer = result.LocalPlayer;
                leaderboard = GameServices.CreateLeaderboard(leaderboardID);
                Debug.Log("Authenticated");
            }
        }
        else
        {
            Debug.LogError("Failed login with error : " + error);
        }
    }

    public void PushHighScore(int score)
    {
        leaderboard.LoadPlayerCenteredScores((result, error) => {
            if (error == null)
            {
                int currentScore = (int)leaderboard.LocalPlayerScore.Value;
                int newScore = currentScore + score;                

                GameServices.ReportScore(leaderboardID, newScore, (error) =>
                {
                    if (error == null)
                    {
                        Debug.Log("Request to submit score finished successfully.");
                    }
                    else
                    {
                        Debug.Log("Request to submit score failed with error: " + error.Description);
                    }
                });
            }
            else
            {
                Debug.LogError("Failed loading top scores with error : " + error.Description);
            }
        });


    }

    public void ShowLeaderboard()
    {       
        GameServices.ShowLeaderboard(leaderboardID, LeaderboardTimeScope.AllTime, callback: (result, error) =>
        {
            if (error != null) Debug.Log(error.Description);
            Debug.Log("Leaderboard UI closed");
        });
    }
    

    // Update is called once per frame
    void Update()
    {

    }
}
