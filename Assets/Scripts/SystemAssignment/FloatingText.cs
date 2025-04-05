using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    public TextMeshProUGUI text;

    public float floatDirection = 1;

    public float floatDistance = 100;
    public float lifetime = 3;

    // the ratio amount of time before the end of lifetime that the text will start to fade out
    public float fadeAlphaTime = 0.33f;

    public AnimationCurve floatSpeedCurve;

    private float lifeTimer;

    private Vector3 originPos;

    public void InitFloatingText(string text, Vector3 pos, Color c, float floatDirection, float lifetime, float fontSize)
    {
        transform.position = pos;
        this.floatDirection = floatDirection;
        this.lifetime = lifetime;

        lifeTimer = Time.time + this.lifetime;

        originPos = pos;

        this.text.text = text;
        this.text.fontSize = fontSize;
        this.text.color = c;

    }

    public void SetFontSize(float fontSize)
    {
        text.fontSize = fontSize;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > lifeTimer)
        {
            // destroy object once timer expires
            Destroy(gameObject);
        } else
        {
            // get # between 0 and 1 representing how far we are through lifetime and set position + alpha
        }
        float ratioThroughLifetime = 1 - (lifeTimer - Time.time) / lifetime;

        transform.position = originPos + Vector3.up * (floatDirection * floatSpeedCurve.Evaluate(ratioThroughLifetime) * floatDistance);

        // fade alpha
        if (ratioThroughLifetime > (1 - fadeAlphaTime))
        {
            Color c = text.color;
            c.a = (1 - ratioThroughLifetime) / fadeAlphaTime;
            text.color = c;
        }
    }
}
