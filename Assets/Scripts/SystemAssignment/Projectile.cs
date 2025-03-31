using System.Buffers.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;

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
        getcomponent spriterenderer
    
    set spriterenderer’s color based on gemtype
    targetSet = true;
        damage = dmg;
        this.gemType = gemType;
    }

    public void SetSlowTime(float slowTime)
    {
        this.slowtime = slowtime;
    }

    public void SetPoison(float poisonDmg, float poisonTime)
    {
        poisondamage = poisondmg;
        this.poisontime = poisontime;
    }

    void Update()
    {
        if (targetSet)
        {
            transform.position = vector3.movetowards(transform.position, target.transform.position, projectileSpeed * time.deltatime);
            if (vector3.distance(transform.position, target.transform.position) <= hitDistance)
            {
                // register hit on enemy
                target.takedamage(damage);
                spawn floating text with damage number above enemy
    
            if gemType = sapphire, slow the enemy
    
            if gemType = emerald, slow and poison the enemy
    
        }
        }
    }

}
