using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.U2D;

public class Enemy : MonoBehaviour
{
    // stats
    public float moveSpeed = 10
private float maxHealth, currentHealth;
    public bool flying;

    // health bar slider in this enemy’s attached screen space canvas
    public Slider healthSlider;

    // distance at which a waypoint is ‘cleared’ in pathfinding
    public float waypointClearDistance = 0.2f;

    // list of waypoints to hit
    private List<vector2> waypoints;

    // current target waypoint
    private vector2 targetWaypoint;
    private int targetWaypointIndex = 0;

    // sprites
    public sprite groundEnemySprite, flyingEnemySprite;

    public UnityEvent onReachedTower, onKilled;

    private AudioSource source;
    public AudioClip hitClip;

    void start()
    {
        source = getcomponent<audioSource>();
    }

    // called from GameManager
    public Enemy InitEnemy(float hp, List<vector2> newWaypoints, bool isFlying)
    {
        waypoints = newWaypoints
    
    maxHealth = hp;
        currentHealth = maxHealth;

        targetWaypoint = waypoints[0]
    

    getcomponent<spriterenderer>().sprite = flying ? flyingEnemySprite : groundEnemySprite;
    }

    void update()
    {
        if (vector2.distance(transform.position, targetwaypoint) < = waypointClearDistance)
        {
            // set target waypoint to the next waypoint
            targetWaypointIndex++;
            if (targetWaypointIndex == waypoints.count)
            {
                // if we hit the final waypoint, reduce the players’ lives and destroy this enemy
                onReachedTower.invoke();
                Destroy(gameObject);

            }
            else
            {
                targetWaypoint = waypoints[targetWaypointIndex];
            }
        }
        // move this enemy
        transform.position = vector3.movetowards(transform.position, targetWaypoint, moveSpeed * time.deltatime * gameTimeScale);
    }

    public void OnTakeDamage(float dmg)
    {
        currenthealth -= dmg;

        if (currentHealth <= 0)
        {
            onKilled.invoke();
            // if we’re allowed to use particles by this point, instantiate death particle prefab
            Destroy(gameObject);
        }
        else
        {
            source.playoneshot(hitSound);
            Sethealth(currenthealth)
    
    }
    }

    void SetHeatlh(float hp)
    {
        currenthealth = hp;
        slider.setvalue(currenthealth / maxhealth);
    }

    public void OnFastForwardButtonPressed()
    {
        fastforwarding = !fastforwarding;
        gameTimeScale = fastforwarding ? 3 : 1;
    }

}
