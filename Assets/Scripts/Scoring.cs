using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scoring : MonoBehaviour
{
    public static int collectedCollectables;

    private void Start()
    {
        
        collectedCollectables = 0;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {  
        if (collision.CompareTag("Player"))
        {
            collectedCollectables++;
            Destroy(gameObject);
            AudioManager.Instance.PlaySFX("Collected");
        }
    }
}
