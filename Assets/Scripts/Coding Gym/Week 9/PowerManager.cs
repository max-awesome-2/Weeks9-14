using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class PowerManager : MonoBehaviour
{
    public TextMeshProUGUI powerText;
    private bool powerOn = true;

    public GameObject powerPrefab;

    public Transform prefabSpawnLocation;
    public float spawnDelay = 1f;
    private float spawnTimer;

    public UnityEvent<bool> OnSetPower;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TogglePower();
        }

        if (powerOn) spawnTimer += Time.deltaTime;


        if (spawnTimer >= spawnDelay)
        {
            spawnTimer = 0;
            GameObject g = Instantiate(powerPrefab, prefabSpawnLocation);
            OnSetPower.AddListener(g.GetComponent<PowerPrefab>().SetPower);
        }
    }

    public void TogglePower()
    {
        powerOn = !powerOn;
        OnSetPower.Invoke(powerOn);
    }

    public void SetPowerText(bool on)
    {
        powerText.text = "Power " + (on ? "on!" : "off!");
    }
}
