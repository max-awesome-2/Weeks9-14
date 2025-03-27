using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightController : MonoBehaviour
{
    private SpriteRenderer sr;
    private Animator anim;

    public float moveSpeed = 10f;
    private bool attacking = false;

    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (attacking) return;

        float x = Input.GetAxis("Horizontal"), y = Input.GetAxis("Vertical");

        Vector3 move = (x * Vector3.right + y * Vector3.up);
        if (move.magnitude > 1) move = move.normalized;

        transform.Translate(move * moveSpeed * Time.deltaTime);

        if (x > 0 && sr.flipX) sr.flipX = false;
        else if (x < 0 && !sr.flipX) sr.flipX = true;

        anim.SetFloat("speed", move.magnitude);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            anim.SetTrigger("attack");
        }

    }

    public void StartAttack()
    {
        attacking = true;
    }

    public void EndAttack()
    {
        attacking = false;
    }
}
