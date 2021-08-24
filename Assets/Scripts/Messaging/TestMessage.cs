using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMessage : MonoBehaviour
{

    MessageHandler handler;

    // Start is called before the first frame update
    void Start()
    {
        handler = FindObjectOfType<MessageHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            var msg = new MessageOne();
            handler.Subscribe<MessageOne>(TestActionOne);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            var msg = new MessageTwo();
            handler.Subscribe<MessageTwo>(TestActionTwo);
        }
    }

    void TestActionOne(MessageOne action)
    {
        
    }

    void TestActionTwo(MessageTwo action)
    {
        
    }
}

public class MessageOne: IMessage
{
    public int something;
}

public class MessageTwo: IMessage
{
    public bool somethingElse;
}
