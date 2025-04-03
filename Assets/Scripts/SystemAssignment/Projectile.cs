using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float projectileSpeed = 10f;
    public float hitDistance = 0.05f;
    private Enemy target;
    private bool targetSet = false;

    public Tower originTower;

    public void Init(Tower origin, Enemy target)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        // set spriterenderer’s color based on gemtype
        sr.color = origin.gameManager.gemTypeColors[origin.gemType];

        this.target = target;
        targetSet = true;
        originTower = origin;
    }

    void Update()
    {
        if (targetSet)
        {
            if (target != null)
            {
                transform.position = Vector3.MoveTowards(transform.position, target.transform.position, projectileSpeed * Time.deltaTime);
                if (Vector3.Distance(transform.position, target.transform.position) <= hitDistance)
                {
                    // register hit on enemy
                    if (originTower.gemType == 7)
                    {
                        // deal splash damage - hit every enemy within splash range
                        foreach (Enemy e in originTower.allEnemies)
                        {
                            if (e == null) continue;
                            if (Vector3.Distance(target.transform.position, e.transform.position) < originTower.rubySplashRange)
                            {
                                e.OnHit(this);
                            }
                        }
                    } else
                    {
                        target.OnHit(this);
                    }

                    Destroy(gameObject);

                }
            } else
            {
                Destroy(gameObject);
            }

           
        }
    }

}
