using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TestEventScript : MonoBehaviour
{
    public RectTransform banana;

    public UnityEvent OnTimerComplete;

    public float timerTime = 5f;
    private float timer;
    private bool timerComplete = false;

    private void Start()
    {
        timer = Time.time + timerTime;
    }

    public void MouseJustEnteredImage()
    {
        print("mouse entered the image!");
        banana.localScale = Vector3.one * 1.2f;
    }

    public void MouseJustExitedImage()
    {
        print("mouse exited the image!");
        banana.localScale = Vector3.one;
    }

    private void Update()
    {
       if (Time.time > timer && !timerComplete)
        {
            timerComplete = true;
            OnTimerComplete.Invoke();
        }   
    }
}
