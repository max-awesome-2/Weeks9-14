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

    // stats
    int gemType;
    float damage;
    float range;
    float attackSpeed;

    // special stats
    float opalBonus;
    float poisonDamage;
    float poisonTime;
    float poisonSlow;
    float freezeSlow;
    float freezeTime;

    // prefabs
    public GameObject projectilePrefab;

    // events
    public UnityEvent<string, GameObject> onMouseEnter;
    public UnityEvent onMouseExit;

    public void InitTower(int xPos, int yPos, int type, float dmg, float range, float atkspd)
    {
        //set xPos, yPos, type, tier, dmg, range, atkspd variables
        x = xPos;
        y = yPos;
        gemType = type;
        damage = dmg;
        this.range = range;
        attackSpeed = atkspd;

    }

    public void OnKept(UnityEvent roundStart, UnityEvent roundEnd, List<Enemy> enemyList)
    {
        roundStart.AddListener(OnRoundStart);


        roundEnd.AddListener(OnRoundEnd);
    
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
        enemiesInRange.Clear();

        // then, if there are any enemies within range, shoot projectiles
        // while ready to fire, check every frame if there are any enemies in range
        while (true)
        {
            if (enemiesInRange.Count == 0)
            {
                CheckIfEnemiesInRange();
                yield return null;
            }
            else
            {
                // update enemies in range list
                CheckIfEnemiesInRange();

                // if there’s still an enemy in range, fire and wait the delay
                if (enemiesInRange.Count > 0)
                {
                    FireProjectile(enemiesInRange[0]);
                    yield return new WaitForSeconds(1 / attackSpeed);

                }
            }
        }
    }

    void FireProjectile(Enemy target)
    {
        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        Projectile p = proj.GetComponent<Projectile>();

        p.Init(gemType, damage, target);

        //if (type == sapphire) p.SetSlowTime(…)
        //else if (type == emerald) p.setpoison(…)
    }

    void CheckIfEnemiesInRange()
    {
        // add enemies that are in range, and remove enemies that are not in range
        for (int i = 0; i < allEnemies.Count; i++)
        {
            Enemy e = allEnemies[i];
            //if e.flying and we are a diamond tower, OR !e.flying and we are an amethyst tower, continue;
            float dist = Vector3.Distance(transform.position, e.transform.position);
            if (enemiesInRange.Contains(e))
            {
                if (dist > range)
                {
                    // enemy exited range – remove it from list
                    enemiesInRange.Remove(e);
                }
            }
            else
            {
                if (dist <= range)
                {
                    // enemy has entered our range – add it in
                    enemiesInRange.Add(e);
                }
            }
        }
    }

    void OnMouseEnter()
    {
        if (towerState != 2)
        {
            //invoke OnMouseEnter with a string describing this gem’s stats
            //(so that they get displayed in the hover box)
    
        }
    }

    void OnMouseExit()
    {
        if (towerState != 2)
        {
            // OnMouseExit(so that hover box gets dismissed)
    
        }
    }

    public void SetPoisonStats(float poisonDmg, float poisonTime, float poisonSlow)
    {
        this.poisonDamage = poisonDmg;
        this.poisonTime = poisonTime;
        this.poisonSlow = poisonSlow;
    }

    public void SetFreezeStats(float freezeSlow, float freezeTime)
    {
        this.freezeSlow = freezeSlow;
        this.freezeTime = freezeTime;
    }
}
