using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Cinemachine;

public class KnightController : MonoBehaviour
{
    private SpriteRenderer sr;
    private Animator anim;

    public float moveSpeed = 10f;
    private bool attacking = false;

    private AudioSource source;
    public AudioClip footstepClip;

    public Tilemap tilemap;
    public Tile grassTile;

    private CinemachineImpulseSource impulseSource;

    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        impulseSource = GetComponent<CinemachineImpulseSource>();
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

    public void Footstep()
    {
        Vector3Int pos = tilemap.WorldToCell(transform.position);
        TileBase t = tilemap.GetTile(pos);
        if (t != null && t != grassTile)
        {
            source.PlayOneShot(footstepClip);
        }

        impulseSource.GenerateImpulse();
    }
}
