using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using CheckerFootball.Models;
using UnityEngine.UI;

public class BallCollider : MonoBehaviour
{

    public static event Action<Vector2> BallCollidedEvent;
    public static event Action BallFumbledEvent;
    private Rigidbody2D rigidBody;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        GetComponent<RectTransform>().sizeDelta = new Vector2(SpawnGrid.tileSize.x, SpawnGrid.tileSize.x); //J- Scale both on X to keep Aspect ratio (Assume X is smallest value
        GetComponent<CircleCollider2D>().radius = GetComponent<RectTransform>().sizeDelta.x / 3;
    }

    private void OnEnable()
    {
        EnableInteraction();
    }

    public void Pass(Vector2 direction, float force)
    {
        Debug.Log("Passing/Throwing/Kicking " + force);
        GetComponent<Image>().raycastTarget = false;

        rigidBody.AddForce(direction * force, ForceMode2D.Impulse);
        Debug.Log(direction * force);
        StartCoroutine(CheckFumble());
    }

    private IEnumerator CheckFumble()
    {
        while (true)
        {
            if(rigidBody.velocity.magnitude < 15)
            {
                rigidBody.velocity = Vector2.zero;
                BallFumbledEvent?.Invoke();                
                break;
            }
            yield return null;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("COLLIDED");
        string s = collision.transform.name;
        //Gets piece coord from name of piece
        Vector2 v = new Vector2(float.Parse(s[0].ToString()), float.Parse(s[1].ToString()) );
        BallCollidedEvent?.Invoke(v);
        StopCoroutine(CheckFumble());
    }

    private void DisableInteraction()
    {
        GetComponent<Image>().raycastTarget = false;
        GetComponent<CircleCollider2D>().enabled = false;
    }

    private void EnableInteraction()
    { 
        GetComponent<Image>().raycastTarget = true;
        GetComponent<CircleCollider2D>().enabled = true;
    }

}
