using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CuckooClock : MonoBehaviour
{
    // time for 1 "hour"
    public float hourTime = 5f;

    public UnityEvent hourStruck;

    public Transform minuteHand, hourHand;

    private float hourTimer;

    // Start is called before the first frame update
    void Start()
    {
        hourTimer = Time.time + hourTime;
    }

    // Update is called once per frame
    void Update()
    {
        minuteHand.Rotate(360f * Vector3.back / hourTime * Time.deltaTime);
        hourHand.Rotate(360f * Vector3.back / hourTime / 60 * Time.deltaTime);

        if (Time.time >= hourTimer)
        {
            hourTimer = Time.time + hourTime;
            hourStruck.Invoke();
        }

    }
}
