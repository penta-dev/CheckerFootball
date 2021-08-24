using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon;
using TMPro;

public class PhotonConnect : MonoBehaviourPunCallbacks
{

    public TextMeshProUGUI statusText;
    public TextMeshProUGUI roomNameInputText;
    public TextMeshProUGUI regionText;
    public TextMeshProUGUI errorText;

    public GameObject optionUI;

    private int loadTicker; //J- Just a counter for a UI effect

    public bool offlineMode;

    public string userRoomName;

    private bool autoMatch;

    private LoadBalancingClient loadBalancingClient;

    private int waitingSafety;


    bool hasAttemptedOptimalConnection;

    Photon.Realtime.RegionHandler handler;

    // Start is called before the first frame update
    void Start()
    {
        waitingSafety = 0;

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }

        PhotonNetwork.OfflineMode = offlineMode;

       
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            PhotonNetwork.Disconnect();
        }
    }    

    public override void OnRegionListReceived(Photon.Realtime.RegionHandler regionHandler)
    {
        base.OnRegionListReceived(regionHandler);
        handler = regionHandler;

        statusText.text = "Finding Optimal Region";
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        if(cause.ToString() != "None") 
        {
            //errorText.text = cause.ToString();
        }        
    }

    public void RandomMatchmake()
    {
        optionUI.SetActive(false); //J- Hide option buttons etc
        autoMatch = true;
        statusText.text = "Finding Match...";
        if (!PhotonNetwork.IsConnected)
        {
            ConnectToPhoton();
            statusText.text = "Connecting to server.";
            SmartDebug.Log("Connecting To Photon");
        } else
        {
            PhotonNetwork.LeaveRoom();
            statusText.text = "Finding better room...";
            SmartDebug.Log("Leaving room, then attempting to join random room");
        }
    }
    public void OfflineMatch()
    {
        offlineMode = true;
        PhotonNetwork.OfflineMode = offlineMode;
    }

    public void ManualMatchmake()
    {
        statusText.text = "Loading Match...";
        userRoomName = roomNameInputText.text;
        if (userRoomName.Length < 4 || userRoomName.Length > 12)
        {
            statusText.text = "*Room Name must be 4-12 Characters*";
            return;
        }
        optionUI.SetActive(false); //J- Hide option buttons etc
        autoMatch = false;
        if(!PhotonNetwork.IsConnected)
        {
            ConnectToPhoton();
            SmartDebug.Log("Connecting To Photon for Manual Match/Create");
        } else
        {
            PhotonNetwork.LeaveRoom();
            SmartDebug.Log("Connected to Photon, attempting to join/create custom room");
            JoinOrCreatePrivateRoom(userRoomName);
        }
    }

    private void ConnectToPhoton()
    {
        statusText.text = "Connecting to server..";
        PhotonNetwork.ConnectUsingSettings();
    }      

    public override void OnConnectedToMaster()
    {
        if (offlineMode)
        {
            if (autoMatch)
            {
                SmartDebug.Log("Connected to Photon, attempting to join random room....");
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                SmartDebug.Log("Connected to Photon, attempting to join/create custom room");
                JoinOrCreatePrivateRoom(userRoomName);
            }
            return;
        }

        base.OnConnectedToMaster();
        Debug.Log("Connected " + PhotonNetwork.CloudRegion);

        if (!hasAttemptedOptimalConnection) {

            hasAttemptedOptimalConnection = true;

            bool canConnectToUSA = false;
            bool canConnectToAsia = false;
            
            foreach (Region r in handler.EnabledRegions)
            {
                if (r.Code == "us") canConnectToUSA = true;
                if (r.Code == "asia") canConnectToAsia = true;
            }

            if (canConnectToUSA)
            {
                Debug.Log("Attempting");
                PhotonNetwork.Disconnect();
                PhotonNetwork.ConnectToRegion("us");
                return;
            }

            if (canConnectToAsia)
            {
                Debug.Log("Attempting");
                PhotonNetwork.Disconnect();
                PhotonNetwork.ConnectToRegion("asia");
                return;
            }
            
            Debug.Log("RAN");
            regionText.text = $"region: {PhotonNetwork.CloudRegion}";
        }
        else
        {
            Debug.Log("RAN");
            statusText.text = $"Connected to region {PhotonNetwork.CloudRegion}";
            regionText.text = $"region: {PhotonNetwork.CloudRegion}";
            if (autoMatch)
            {
                SmartDebug.Log("Connected to Photon, attempting to join random room....");
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                SmartDebug.Log("Connected to Photon, attempting to join/create custom room");
                JoinOrCreatePrivateRoom(userRoomName);
            }
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        statusText.text = "...";
        SmartDebug.Log("Failed to join room, creating new room");
        RoomOptions roomOptions_RO = new RoomOptions();
        roomOptions_RO.MaxPlayers = 2;
        PhotonNetwork.CreateRoom(null, roomOptions_RO);
    }

    public override void OnJoinedRoom()
    {
        statusText.text = "Found room, checking quality...";
        SmartDebug.Log("Successfully joined room");
        Debug.Log(PhotonNetwork.OfflineMode);
        StartCoroutine("WaitForOthers");
    }

    IEnumerator WaitForOthers()
    {
        statusText.text = "Waiting for players";

        waitingSafety += Random.Range(0, 3);

        if (waitingSafety >= 15 && autoMatch)
        {
            waitingSafety = 0;
            RandomMatchmake();
            StopAllCoroutines();
            yield return null;
        }

        if (offlineMode)
        {
            SceneHandler.LoadScene(SceneHandler.GameScene);
            yield return null;
        }

        //J- Status text to show user that they are getting matched
        loadTicker++;
        if(loadTicker > 3)
        {
            loadTicker = 0;
        }
        int cnt = 0;
        while(cnt < loadTicker)
        {
            statusText.text += ".";
            cnt++;
        }

        statusText.text += "\n" + "Room Name : " + PhotonNetwork.CurrentRoom.Name.Substring(0, 8);

        SmartDebug.Log($"Total Players: {PhotonNetwork.PlayerList.Length}");
        if (PhotonNetwork.PlayerList.Length > 1)
        {
            yield return new WaitForSeconds(0.5f);
            if(PhotonNetwork.PlayerList.Length > 1)
            {
                //J// Lock room to avoid others joing on player disconnect (Or player reconnecting after leaving)
                PhotonNetwork.CurrentRoom.IsOpen = false;
                SceneHandler.LoadScene(SceneHandler.GameScene);
            } else
            {
                SmartDebug.Log("Waiting for players");
                yield return new WaitForSeconds(0.5f);
                StartCoroutine("WaitForOthers");
            }
        } else
        {
            SmartDebug.Log("Waiting for players");
            yield return new WaitForSeconds(0.5f);
            StartCoroutine("WaitForOthers");
        }
        yield return null;
    }

    //J- Manual matchmaking

    public void JoinOrCreatePrivateRoom(string customRoomName)
    {
        EnterRoomParams enterRoomParams = new EnterRoomParams();
        enterRoomParams.RoomName = customRoomName;
        enterRoomParams.RoomOptions = new RoomOptions();
        enterRoomParams.RoomOptions.IsVisible = false;
        PhotonNetwork.JoinOrCreateRoom(customRoomName, enterRoomParams.RoomOptions, enterRoomParams.Lobby);
    }
}
