using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using TMPro;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TutorialHandler : MonoBehaviour
{

    public MessageHandler messaging;
    public List<TutorialStep> steps;
    
    public Image textBack;
    public Transform textHolder;

    public ArrowPointer arrow;
    public TextMeshProUGUI tutorialText;

    TutorialStep currentStep;
    public int currentIndex = 0;

    private bool canTapToContinue;

    private void Awake()
    {
        PhotonNetwork.OfflineMode = true;
        BallCollider.BallFumbledEvent += BallStopped;
    }

    private void OnDisable()
    {
        BallCollider.BallFumbledEvent -= BallStopped;
    }

    void Start()
    {
        messaging.Subscribe<MTileSelected>(GetInput);

        ShowStepInfo(currentIndex);       
        SetupGridButtons();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && canTapToContinue)
        {
           NextStep();
        }
    }

    private void SetupGridButtons()
    {
        var tileKeys = new List<Vector2>(SpawnGrid.tiles.Keys);
        foreach(Vector2 key in tileKeys)
        {
            Tile t = SpawnGrid.tiles[key];
            AssignTilePressAction(t.image, (int)key.x, (int)key.y);
        }
    }

    private void ShowStepInfo(int step)
    {

        currentStep = steps[step];

        tutorialText.text = currentStep.stepText;        
        currentStep.StepStartEvent?.Invoke();

        var pointer = currentStep.pointer;
        if (pointer.type == PointerType.NoPointer) FindObjectOfType<GenericSlidePointer>().HidePointer();
        if (pointer.type == PointerType.Slider) FindObjectOfType<GenericSlidePointer>().SetPoints(pointer.pointA, pointer.pointB, pointer.pointDir, pointer.offset);
        if (pointer.type == PointerType.SingleSquare) FindObjectOfType<GenericSlidePointer>().PointToSquare(SpawnGrid.tiles[pointer.pointA].image.transform.position, pointer.pointDir, pointer.offset);

        if (currentStep.textYPos != -1) textHolder.transform.localPosition = new Vector2(textHolder.transform.localPosition.x, currentStep.textYPos);
        textBack.rectTransform.sizeDelta = new Vector2(textBack.rectTransform.sizeDelta.x, tutorialText.preferredHeight * 1.15f);

        if (currentStep.coord == Vector2.one * -1)
        {
            FindObjectOfType<EventSystem>().enabled = false;
            canTapToContinue = true;
        }
        else
        {
            FindObjectOfType<EventSystem>().enabled = true;
            canTapToContinue = false;
        }

        currentIndex++;
    }

    private void GetInput(MTileSelected message)
    {
        Debug.Log("Called 1" );
        if (message.coord == steps[currentIndex - 1].coord)
        {
            NextStep();         
        }            
    }

    private void NextStep()
    {
        steps[currentIndex - 1].StepEndEvent?.Invoke();
        ShowStepInfo(currentIndex);                
    }

    private void BallStopped()
    {
        NextStep();
    }

    private IEnumerator CheckStepHeld(Vector2 coord)
    {
        Debug.Log("RUNNING");
        float timer = 0;
        while (Input.GetMouseButton(0))
        {
            timer += Time.deltaTime;
            if(timer >= 0.75f)
            {
                Debug.Log("Yes 1");
                if (coord == steps[currentIndex - 1].coord)
                {
                    Debug.Log("Yes 2");
                    NextStep();
                    break;
                }
            }
            yield return null;
        }
        Debug.Log("FINISHED");
    }

    private void AssignTilePressAction(Image tile, int x, int y)
    {
        Vector2 tileCoord = new Vector2(x, y);
        var message = new MTileSelected { coord = tileCoord };        

        var entry = new EventTrigger.Entry() { eventID = EventTriggerType.PointerDown };
        entry.callback.AddListener((data) => StartCoroutine(CheckStepHeld(tileCoord)));

        tile.GetComponent<EventTrigger>().triggers.Add(entry);
    }


}

[System.Serializable]
public class TutorialStep
{    
    public string name;
    public Vector2 coord;
    public string stepText;
    public UnityEvent StepStartEvent;
    public UnityEvent StepEndEvent;
    public PointerEvent pointer;
    public float textYPos = -1;
}

[System.Serializable]
public class PointerEvent
{
    public PointerType type;
    public Vector2 pointA;
    public Vector2 pointB;
    public PointDirection pointDir;
    public float offset;
}

public enum PointerType
{
    NoPointer,
    SingleSquare,
    Slider
}