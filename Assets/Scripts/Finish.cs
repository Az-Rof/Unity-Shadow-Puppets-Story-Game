using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
public class Finish : MonoBehaviour
{
    // void OnTriggerEnter2D(Collider2D collision)
    // {
    //     GameObject player = GameObject.FindWithTag("Player");
    //     if (player != null && collision.CompareTag("Player"))
    //     {
    //         if (collision.gameObject.tag == "Player")
    //         {
    //             SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    //             if (SceneManager.GetActiveScene().buildIndex == null)
    //             {
    //                 PopupField popupField = new PopupField();

    //             }

    //         }
    //     }
    // }   


    void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null && collision.CompareTag("Player"))
        {
            if (collision.gameObject.tag == "Player")
            {
                int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
                if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
                {
                    SceneManager.LoadScene(nextSceneIndex);
                }
                else
                {
                    StartCoroutine(ShowEndGameMessage());
                }
            }
        }
    }

    IEnumerator ShowEndGameMessage()
    {
        // Display end game message
        Debug.Log("Terima kasih sudah bermain game ini");

        // Tunggu 2 detik
        yield return new WaitForSeconds(2);

        // Kembali ke index 0
        SceneManager.LoadScene(0);
    }
}
