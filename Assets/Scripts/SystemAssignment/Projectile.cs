using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float projectileSpeed = 10f;
    public float hitDistance = 0.05f;
    public float damage;
    private Enemy target;
    private bool targetSet = false;
    private int gemType;

    private float slowTime;
    private float poisonTime;
    private float poisonDamage;

    public void Init(int gemType, float dmg, Enemy target)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        // set spriterenderer’s color based on gemtype

        targetSet = true;
        damage = dmg;
        this.gemType = gemType;
    }

    public void SetSlowTime(float slowTime)
    {
        this.slowTime = slowTime;
    }

    public void SetPoison(float poisonDmg, float poisonTime)
    {
        poisonDamage = poisonDmg;
        this.poisonTime = poisonTime;
    }

    void Update()
    {
        if (targetSet)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, projectileSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, target.transform.position) <= hitDistance)
            {
                // register hit on enemy
                target.TakeDamage(damage);

                //spawn floating text with damage number above enemy

                //if gemType = sapphire, slow the enemy

                //if gemType = emerald, slow and poison the enemy

            }
        }
    }

}
