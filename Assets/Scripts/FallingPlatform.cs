using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    public BoxCollider2D trigger;
    private Rigidbody2D rb;
    private Animator animator;
    private bool hasFallen = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        trigger = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hasFallen && collision.CompareTag("Player"))
        {
            hasFallen = true;
            StartCoroutine(Fall());
        }
    }

    IEnumerator Fall()
    {
        if (animator != null)
        {
            animator.SetTrigger("fall");
        } 
        yield return new WaitForSeconds(3f);
        rb.bodyType = RigidbodyType2D.Dynamic;
    }
}
