using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ClockScript : MonoBehaviour
{
    public float timeAnHourTakes, t;

    public Transform minuteHand, hourHand;

    public UnityEvent<int> clockChime;

    public int hour;

    private IEnumerator clockCoroutine, moveHandsCoroutine;

    public TextMeshProUGUI hourText;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void ChimeClock(int hour)
    {
        print("Chime! " + hour);
        hourText.text = "Hour: " + hour;
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator MoveClockHands()
    {
        while (true)
        {
            moveHandsCoroutine = MoveTheClockHandsOneHour();
            yield return StartCoroutine(moveHandsCoroutine);
        }
    }

    IEnumerator MoveTheClockHandsOneHour()
    {
        if (t >= timeAnHourTakes) t = 0;
        while (t < timeAnHourTakes)
        {
            t += Time.deltaTime;
            minuteHand.Rotate(0, 0, -(360 / timeAnHourTakes) * Time.deltaTime);
            hourHand.Rotate(0, 0, -(360 / 12 / timeAnHourTakes) * Time.deltaTime);

            yield return null;

        }
        hour++;
        if (hour == 13) hour = 1;

        clockChime.Invoke(hour);
    }

    public void StopClock()
    {
        if (clockCoroutine != null) StopCoroutine(clockCoroutine);
        if (moveHandsCoroutine != null) StopCoroutine(moveHandsCoroutine);
    }

    public void StartClock()
    {
        clockCoroutine = MoveClockHands();
        StartCoroutine(clockCoroutine);
    }
}
