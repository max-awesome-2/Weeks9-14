using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerPrefab : MonoBehaviour
{

    private SpriteRenderer sprite;

    public Color offColor = Color.black, onColor = Color.green;

    public AnimationCurve growCurve;
    public float maxSize = 1.5f;

    private bool powerOn = true;
    private float t = 0;

    public float growSpeed = 3f;

    public float moveSpeed = 10f;

    // Start is called before the first frame update
    void Start()
    {
        t = 1;
        transform.localScale = Vector3.one * maxSize;
        sprite = GetComponent<SpriteRenderer>();

    }

    // Update is called once per frame
    void Update()
    {
        t = Mathf.Clamp(t + (powerOn ? 1 : -1) * growSpeed * Time.deltaTime, 0, 1);
        transform.localScale = Vector3.one * (1f + growCurve.Evaluate(t) * (maxSize - 1));

        if (powerOn) transform.position += Vector3.right * moveSpeed * Time.deltaTime;
    }

    public void SetPower(bool on)
    {
        sprite.color = on ? onColor : offColor;
        powerOn = on;
    }
}
