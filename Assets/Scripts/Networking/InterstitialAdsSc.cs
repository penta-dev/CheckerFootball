using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class InterstitialAdsSc : MonoBehaviour
{

    string gameId = "1234567";
    bool testMode = false;

    void Start()
    {
        if(Application.platform == RuntimePlatform.Android)
        {
            gameId = "4050121";
        } else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            gameId = "4050120";
        }

        // Initialize the Ads service:
        Advertisement.Initialize(gameId, testMode);
    }

    public void ShowInterstitialAd()
    {
        
        // Check if UnityAds ready before calling Show method:
        if (Advertisement.IsReady())
        {
            Advertisement.Show();
        }
        else
        {
            Debug.Log("Interstitial ad not ready at the moment! Please try again later!");
        }
        
    }
}
