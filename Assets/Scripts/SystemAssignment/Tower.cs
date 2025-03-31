using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class Tower : MonoBehaviour
{
    // x and y coords of gem in grid
    public int x, y;

    // state of this tower – 0 = placed but not kept, 1 = kept, 2 = unkept (rock)
    private int towerState = 0;

    // stores the attack coroutine – is started at the beginning of a round, and stopped at the end
    private IEnumerator attackCoroutine;

    // list of enemies that are currently alive in the round – reference to a variable kept + manipulated by GameManager
    private List<Enemy> allEnemies;

    private List<Enemy> enemiesInRange = new List<Enemy>();

    public void InitTower(int xPos, int yPos, int type, float dmg, float range, float atkspd)
    {
        set xPos, yPos, type, tier, dmg, range, atkspd variables
    }

    public void OnKept(UnityEvent roundStart, UnityEvent roundEnd, List<Enemy> enemyList)
    {
        roundStart.addlistener(OnRoundStart)
    
    roundEnd.addListener(OnRoundEnd)
    
    towerState = 1;
        allEnemies = enemyList;
    }

    public void OnNotKept()
    {
        towerState = 2;
    }

    private void OnRoundStart()
    {
        attackCoroutine = Attack();
        StartCoroutine(attackCoroutine);
    }

    private void OnRoundEnd()
    {
        StopCoroutine(attackCoroutine);
    }

    private IEnumerator Attack()
    {
        // this coroutine waits until an enemy is in range, and while an enemy is in range, attacks every (atkspeed) seconds
        // first, clear enemies in range list
        enemiesInRange.clear();

        // then, if there are any enemies within range, shoot projectiles
        // while ready to fire, check every frame if there are any enemies in range
        while (true)
        {
            if (enemiesInRange.count == 0)
            {
                CheckIfEnemiesInRange();
                yield return null;
            }
            else
            {
                // update enemies in range list
                CheckIfEnemiesInRange();

                // if there’s still an enemy in range, fire and wait the delay
                if (enemiesInRange.count > 0)
                {
                    FireProjectile(enemiesInRange[0]);
                    yield return new WaitForSeconds(1 / atkSpeed);

                }
            }
        }
    }

    void FireProjectile(Enemy target)
    {
        GameObject p = Instantiate(projectilePrefab, transform.position, quaternion.identity);
        Projectile p = p.getcomponent<Projectile>()
    p.Init(type, target, damage);
        if (type == sapphire) p.setslowtime(…)
        else if (type == emerald) p.setpoison(…)
    }

    void CheckIfEnemiesInRange()
    {
        // add enemies that are in range, and remove enemies that are not in range
        for (int i = 0; i < allEnemies.Count; i++)
        {
            Enemy e = allEnemies[i];
            //if e.flying and we are a diamond tower, OR !e.flying and we are an amethyst tower, continue;
            float dist = Vector3.distance(transform.position, e.transform.position);
            if (enemiesInRange.Contains(e))
            {
                if (dist > range)
                {
                    // enemy exited range – remove it from list
                    enemiesInRange.remove(e);
                }
            }
            else
            {
                if (dist <= range)
                {
                    // enemy has entered our range – add it in
                    enemiesInRange.add(e);
                }
            }
        }
    }

    void onmousenter()
    {
        if (towerstate != 2)
        {
            invoke OnMouseEnter with a string describing this gem’s stats
            (so that they get displayed in the hover box)
    
    }
    }

    void onmouseexit()
    {
        if (towerstate != 2)
        {
            invoke OnMouseExit(so that hover box gets dismissed)
    
    }
    }

}
