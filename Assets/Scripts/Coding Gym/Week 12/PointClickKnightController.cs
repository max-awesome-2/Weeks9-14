using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Cinemachine;

public class PointClickKnightController : MonoBehaviour
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

    private Vector3 target;
    private bool reachedTarget = true;

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

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0;

            Vector3Int pos = tilemap.WorldToCell(worldPos);
            TileBase t = tilemap.GetTile(pos);

            if (t != null && t != grassTile)
            {
                target = worldPos;

                reachedTarget = false;

                sr.flipX = target.x < transform.position.x;

                anim.SetFloat("speed", 1f);
            }

        }

        transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * moveSpeed);


        if (!reachedTarget && Vector3.Distance(transform.position, target) <= 0.05f)
        {
            reachedTarget = true;
            anim.SetFloat("speed", 0f);
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
