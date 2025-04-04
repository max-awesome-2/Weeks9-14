using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
    public List<Enemy> allEnemies;

    private List<Enemy> enemiesInRange = new List<Enemy>();

    // stats
    public int gemType;
    public int gemTier;
    public float damage;
    public float range;
    public float attackSpeed;

    // special stats
    public float opalBonus;
    public float poisonDamage;
    public float poisonTime;
    public float poisonSlow;
    public float freezeSlow;
    public float freezeTime;

    public float rubySplashRange = 1f;

    // keeps track of the total bonus attack speed given to this tower by all opal towers (including itself)
    private float totalOpalBonusRatio = 1f;

    // prefabs
    public GameObject projectilePrefab;

    // events
    public UnityEvent<string> onMouseEnter;
    public UnityEvent onMouseExit;

    // ref to gamemanager
    public GameManager gameManager;

    // the range indicator sprite attached to this tower
    public GameObject rangeIndicator;

    // debug
    public TextMeshProUGUI tierText;


    // hover box info string
    private string infoString;

    public bool waitingForKeep = false;

    public void InitTower(int type, int tier, float dmg, float range, float atkspd)
    {
        //set type, tier, dmg, range, atkspd variables
        gemType = type;
        gemTier = tier;
        damage = dmg;
        this.range = range;
        attackSpeed = atkspd;

        tierText.text = gemTier.ToString();

        // set range indicator size to match tower range
        Transform prevParent = rangeIndicator.transform.parent;
        rangeIndicator.transform.parent = null;
        rangeIndicator.transform.localScale = Vector3.one * range;
        rangeIndicator.transform.SetParent(prevParent);

        // generate info string
        infoString = $"<b>{gameManager.tierNames[gemTier]} {gameManager.gemNames[gemType]}<b>\n\n" + 
            $"Damage: {dmg}\n" +
            $"Range: {dmg}\n" +
            $"Attack Speed: {atkspd}\n\n";

        if (gemType == 0)
        {
            infoString += $"Attacks poison enemies for {poisonTime} seconds, slowing them by {Mathf.RoundToInt(poisonSlow * 100)}% and dealing {Mathf.RoundToInt(poisonDamage)} damage per second.";
        }
        else if (gemType == 1)
        {
            infoString += $"Attacks chill enemies for {freezeTime} seconds, slowing them by {Mathf.RoundToInt(freezeSlow * 100)}%";
        }
        else if (gemType == 2)
        {
            infoString += $"Only attacks flying units (every 5 rounds, all enemies are flying).";
        }
        else if (gemType == 3)
        {
            infoString += $"25% chance to deal double damage. Only attacks ground units.";
        }
        else if (gemType == 4)
        {
            infoString += $"Attacks ALL enemies in range.";
        }
        else if (gemType == 5)
        {
            infoString += $"Attacks much faster than other gems.";
        }
        else if (gemType == 6)
        {
            infoString += $"Gives all other gems within range a {Mathf.RoundToInt(opalBonus * 100)}% attack speed bonus.";
        }
        else if (gemType == 7)
        {
            infoString += $"Deals splash damage in a range of {rubySplashRange}.";
        }

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
                    if (gemType == 4)
                    {
                        for (int i = 0; i < enemiesInRange.Count; i++)
                        {
                            FireProjectile(enemiesInRange[i]);
                        }
                    } else
                    {
                        Enemy firstNonNull = null;
                        for (int i = 0; i < enemiesInRange.Count; i++)
                        {
                            if (enemiesInRange[i] != null)
                            {
                                firstNonNull = enemiesInRange[i];
                                break;
                            }
                        }

                        if (firstNonNull != null) FireProjectile(firstNonNull);
                    }

                    yield return new WaitForSeconds(1 / attackSpeed);

                }
            }
        }
    }

    void FireProjectile(Enemy target)
    {
        if (target == null) return;

        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        proj.transform.localScale = Vector3.one * proj.transform.localScale.x * gameManager.tileSize;

        Projectile p = proj.GetComponent<Projectile>();

        p.Init(this, target);

    }

    void CheckIfEnemiesInRange()
    {
        // add enemies that are in range, and remove enemies that are not in range
        for (int i = 0; i < allEnemies.Count; i++)
        {
            Enemy e = allEnemies[i];
            if (e == null) continue;
            //if e.flying and we are a diamond tower, OR !e.flying and we are an amethyst tower, continue;
            if (e.flying && gemType == 3 || !e.flying && gemType == 2) continue;

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

            string outInfoString = infoString;

            if (waitingForKeep)
            {
                outInfoString = "(RIGHT CLICK TO KEEP THIS GEM)\n" + outInfoString;
            }

            if (onMouseEnter != null) onMouseEnter.Invoke(infoString);

            rangeIndicator.SetActive(true);
        }
    }

    void OnMouseExit()
    {
        if (towerState != 2)
        {
            // OnMouseExit(so that hover box gets dismissed)
            rangeIndicator.SetActive(false);

        }

        if (onMouseExit != null) onMouseExit.Invoke();
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

    public void SetOpalStats(float bonus)
    {
        opalBonus = bonus;

    }
     
    // adds an attack speed bonus from an opal tower
    public void AddOpalRatio(float bonus)
    {
        totalOpalBonusRatio += bonus - 1;
    }

    // returns one specific instance of damage - apply diamond bonus here
    public float GetDamageInstance()
    {
        bool crit = gemType == 3 && Random.value <= 0.25f;

        float returnDamage = damage;
        if (crit) returnDamage *= 2;

        return returnDamage;
    }

}
