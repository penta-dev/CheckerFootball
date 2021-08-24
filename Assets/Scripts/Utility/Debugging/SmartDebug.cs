using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

//TODO: Should this be a static class or a singletone perhaps?
public class SmartDebug : MonoBehaviour
{

    private static TextMeshProUGUI debugText;

    void Start()
    {
        debugText = GameObject.Find("DebugText").GetComponent<TextMeshProUGUI>();
        SceneManager.sceneLoaded += OnLevelLoad;       
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnLevelLoad;
    }

    private void OnLevelLoad(Scene scene, LoadSceneMode mode)
    {
        if (GameObject.Find("DebugText"))
        {
            debugText = GameObject.Find("DebugText").GetComponent<TextMeshProUGUI>();
        } else
        {
            Debug.LogError("No Debug text set on this scene");
        }
    }

    public static void Log(string message)
    {
        if(Application.platform == RuntimePlatform.WindowsPlayer)
        {
            debugText.text = message;
        }
        else
        {            
            //Debug.Log(message);            
        }
    }



}
