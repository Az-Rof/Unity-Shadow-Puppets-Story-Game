using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rigidbody2D;
    Animator animator;

    [SerializeField] float speed = 10f;
    [SerializeField] float jumpforce = 5f;
    [SerializeField] bool onground;
    [SerializeField] bool isjump;

    [SerializeField] float jumpCooldown = 0.5f;
    [SerializeField] float jumpCooldownTimer = 0f;
    public LayerMask groundLayer;
    //public GameObject backgroundfollower;


    void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        isGrounded();
        movement();
    }

    void Update()
    {
        jump();
        //MaintainBackgroundPosition();
    }

    void movement()
    {
        float h = Input.GetAxis("Horizontal");
        if (h != 0)
        {
            if (h <0)
            {
                transform.localScale = new Vector3(1f, 1f, 1f);
            }
            else if (h > 0)
            {
                transform.localScale = new Vector3(-1f, 1f, 1f);
            }
            //rigidbody2D.velocity = new Vector2(h, rigidbody2D.velocity.y) * speed * Time.deltaTime;
            Vector2 newVelocity = rigidbody2D.velocity;
            newVelocity.x = h * speed * Time.deltaTime;
            rigidbody2D.velocity = newVelocity;
            animator.SetFloat("hMove", MathF.Abs(h));
            //animator.SetFloat("hSpeed", MathF.Abs(speed) * MathF.Abs(h));
            //AudioManager.Instance.PlaySFX("Walking");
        }
        animator.SetFloat("yMove", MathF.Abs(rigidbody2D.velocity.y));
    }

    void jump()
    {
        if (!isjump && onground && Input.GetKeyDown(KeyCode.Space) && jumpCooldownTimer <= 0f)
        {
            jumpCooldownTimer = jumpCooldown;
            StartCoroutine(PrepareJump());
        }
        if (jumpCooldownTimer > 0f)
        {
            jumpCooldownTimer -= Time.deltaTime;
        }

    }

    IEnumerator PrepareJump()
    {
        animator.SetBool("onGround", false);
        yield return new WaitForSeconds(0.25f);
        rigidbody2D.AddForce(new Vector2(0, jumpforce), ForceMode2D.Impulse);
        AudioManager.Instance.PlaySFX("Jump");
    }

    void isGrounded()
    {
        Vector2 position = transform.position;
        Vector2 direction = Vector2.down;
        float distance = 0.5f;
        Debug.DrawRay(position, direction, Color.green);
        RaycastHit2D hit = Physics2D.Raycast(position, direction, distance, groundLayer);
        if (hit.collider != null)
        {
            onground = true;
            isjump = false;
            animator.SetBool("onGround", true);
        }
        else
        {
            isjump = true;
            onground = false;
            animator.SetBool("onGround", false);
        }
    }

    // void MaintainBackgroundPosition(){
    //     Vector3 backgroundPosition = backgroundfollower.transform.position;
    //     backgroundPosition = new Vector3(transform.position.x, transform.position.y);
    //     backgroundfollower.transform.position = backgroundPosition;
    // }
}