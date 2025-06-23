using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevel : MonoBehaviour
{
    public GameObject boss;
    public bool requirementsMet = false;

    void Update()
    {
        CheckRequirements();
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && requirementsMet)
        {

            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                Debug.Log("No more levels to load.");
            }

        }

    }

    // Requirements, Example : Defeat boss first
    public void CheckRequirements()
    {

        // Check if the boss is defeated
        if (boss == null)
        {
            requirementsMet = true;
        }
    }

}
