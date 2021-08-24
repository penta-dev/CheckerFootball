using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


//TODO: Absolutely need to make a debugging tool to visualise all the script communications
//TODO: Create a message pool system to reduce GC
public class MessageHandler : MonoBehaviour
{
    private Dictionary<Type, List<Delegate>> subscribers = new Dictionary<Type, List<Delegate>>();

    public void Subscribe<T>(Action<T> action) where T : IMessage
    {
        Type type = typeof(T);
        if (!subscribers.ContainsKey(type)) subscribers[type] = new List<Delegate>();
        subscribers[type].Add(action);
    }

    public void Unsubscribe<T>(Action<T> action) where T : IMessage
    {
        Type type = typeof(T);
        if (!subscribers.ContainsKey(type)) return;
        subscribers[type].Remove(action);
    }

    public void SendMessage(IMessage msg)
    {
        Type type = msg.GetType();
        if (subscribers.ContainsKey(type))
        {
            foreach(var handler in subscribers[type])
            {
                handler.DynamicInvoke(msg);
            }
        }
    }

    public void Send()
    {

    }
}
