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
    private float maxHealth, currentHealth;
    public bool flying;

    // health bar slider in this enemy’s attached screen space canvas
    public Slider healthSlider;

    // distance at which a waypoint is ‘cleared’ in pathfinding
    public float waypointClearDistance = 0.2f;

    // list of waypoints to hit
    private List<Vector2> waypoints;

    // current target waypoint
    private Vector2 targetWaypoint;
    private int targetWaypointIndex = 0;

    // sprites
    public Sprite groundEnemySprite, flyingEnemySprite;

    public UnityEvent onReachedTower, onKilled;

    private AudioSource source;
    public AudioClip hitSound;

    void start()
    {
        source = GetComponent<AudioSource>();
    }

    // called from GameManager
    public void InitEnemy(float hp, List<Vector2> newWaypoints, bool isFlying)
    {
        waypoints = newWaypoints;

        maxHealth = hp;
        currentHealth = maxHealth;

        targetWaypoint = waypoints[0];


        GetComponent<SpriteRenderer>().sprite = flying ? flyingEnemySprite : groundEnemySprite;
    }

    void update()
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
        // move this enemy
        transform.position = Vector3.MoveTowards(transform.position, targetWaypoint, moveSpeed * Time.deltaTime);
    }

    public void OnHit(Projectile p)
    {
        Tower originTower = p.originTower;

        float dmg = originTower.GetDamageInstance();

        // instantiate floating damage text

        TakeDamage(dmg);
    }

    public void TakeDamage(float dmg)
    {
        currentHealth -= dmg;

        if (currentHealth <= 0)
        {
            onKilled.Invoke();

            // if we’re allowed to use particles by this point, instantiate death particle prefab
            Destroy(gameObject);
        }
        else
        {
            source.PlayOneShot(hitSound);
            SetHealth(currentHealth);

        }
    }

    void SetHealth(float hp)
    {
        currentHealth = hp;
        healthSlider.SetValueWithoutNotify(currentHealth / maxHealth);
    }


}
