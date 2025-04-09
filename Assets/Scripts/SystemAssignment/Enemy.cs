using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    // stats
    public float moveSpeed = 10;
    public float maxHealth, currentHealth;
    public bool flying;

    // health bar slider in this enemy’s attached screen space canvas
    public Slider healthSlider;

    // distance at which a waypoint is ‘cleared’ in pathfinding
    public float waypointClearDistance = 0.2f;

    // list of waypoints to hit
    private List<Vector3> waypoints;

    // current target waypoint
    private Vector3 targetWaypoint;
    private int targetWaypointIndex = 0;

    // sprites
    public Sprite groundEnemySprite, flyingEnemySprite;

    public UnityEvent onReachedTower = new UnityEvent(), onKilled = new UnityEvent();

    public Color poisonedColor = Color.green, regularColor = Color.red, frozenColor = Color.blue;

    // poison / freeze variables
    private float freezeSlowRatio, poisonSlowRatio;
    private float slowTimer, poisonTimer;
    private bool poisoned = false, frozen = false;


    // ref to gamemanager
    public GameManager gameManager;

    void Start()
    {

    }

    // called from GameManager
    public void InitEnemy(float hp, List<Vector3> newWaypoints, bool isFlying)
    {
        waypoints = newWaypoints;

        maxHealth = hp;
        currentHealth = maxHealth;

        targetWaypoint = waypoints[0];
        this.flying = isFlying;


        GetComponent<SpriteRenderer>().sprite = flying ? flyingEnemySprite : groundEnemySprite;


        
    }

    void Update()
    {
        if (Vector2.Distance(transform.position, targetWaypoint) <= waypointClearDistance)
        {
            // set target waypoint to the next waypoint
            targetWaypointIndex++;
            if (targetWaypointIndex == waypoints.Count)
            {
                // if we hit the final waypoint, reduce the players’ lives and destroy this enemy
                onReachedTower.Invoke();
                Destroy(gameObject);

            }
            else
            {
                targetWaypoint = waypoints[targetWaypointIndex];
            }
        }

        // calculate speed ratio based on poison and freeze slows
        float speedRatio = 1;
        if (poisoned) speedRatio *= poisonSlowRatio;
        if (frozen) speedRatio *= freezeSlowRatio;

        // move this enemy
        transform.position = Vector3.MoveTowards(transform.position, targetWaypoint, moveSpeed * speedRatio * Time.deltaTime);

        if (poisoned && Time.time > poisonTimer)
        {
            GetComponent<SpriteRenderer>().color = frozen ? frozenColor : regularColor;
            poisoned = false;
        }
        if (frozen && Time.time > slowTimer)
        {
            GetComponent<SpriteRenderer>().color = poisoned ? poisonedColor : regularColor;
            frozen = false;
        }
    }

    public void OnHit(Projectile p)
    {
        Tower originTower = p.originTower;

        float dmg = originTower.GetDamageInstance();

        // instantiate floating damage text

        // apply other tower effects
        if (originTower.gemType == 0)
        {
            OnPoison(originTower.poisonDamage, originTower.poisonTime, originTower.poisonSlow);
        }
        else if (originTower.gemType == 1)
        {
            OnSlow(originTower.freezeSlow, originTower.freezeTime);
        }

        TakeDamage(dmg);
    }

    private void OnPoison(float pDamage, float pTime, float pSlow)
    {
        StartCoroutine(DoPoisonDamage(pDamage, pTime));
        poisonTimer = Time.time + pTime;
        poisonSlowRatio = pSlow;
        GetComponent<SpriteRenderer>().color = poisonedColor;

        poisoned = true;
    }

    private IEnumerator DoPoisonDamage(float damage, float time)
    {
        // deal poison damage each second over poison duration
        // calculate # of damage instances
        int n = Mathf.RoundToInt(time / 1f);

        for (int i = 0; i < n; i++)
        {
            yield return new WaitForSeconds(1);

            TakeDamage(damage);
        }
    }

    private void OnSlow(float slow, float sTime)
    {
        slowTimer = Time.time + sTime;
        freezeSlowRatio = slow;

        GetComponent<SpriteRenderer>().color = frozenColor;

        frozen = true;

    }

    public void TakeDamage(float dmg)
    {
        currentHealth -= dmg;

        // if health is less than or equal to zero, destroy this enemy and give the player gold
        if (currentHealth <= 0)
        {

            onKilled.Invoke();

            // if we’re allowed to use particles by this point, instantiate death particle prefab
            Destroy(gameObject);
        }
        else
        {
            SetHealth(currentHealth);

        }

        Vector3 offset = Random.insideUnitCircle * Random.Range(0, 0.2f);
        gameManager.SpawnFloatingText($"-{Mathf.RoundToInt(dmg)}", transform.position + (Vector3.up * 0.1f) + offset, Color.red, 0.5f, 1.5f, 35);
    }

    void SetHealth(float hp)
    {
        currentHealth = hp;
        healthSlider.SetValueWithoutNotify(currentHealth / maxHealth);
    }



}
