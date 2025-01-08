using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FallingDeathTrigger : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Vector2 position = new Vector2(player.transform.position.x, transform.position.y);
            this.transform.position = position;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject player = GameObject.FindWithTag("Player");
        PlayerLivesManager playerLivesManager = player.GetComponent<PlayerLivesManager>();

        if (player != null && other.CompareTag("Player"))
        {
            playerLivesManager.ReduceLives();
        }

    }
}
