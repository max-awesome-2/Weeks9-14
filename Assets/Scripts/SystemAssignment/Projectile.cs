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

        targetSet = true;
        originTower = origin;
    }

    void Update()
    {
        if (targetSet)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, projectileSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, target.transform.position) <= hitDistance)
            {
                // register hit on enemy
                target.OnHit(this);

            }
        }
    }

}
