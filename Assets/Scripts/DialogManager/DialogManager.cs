using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogManager : MonoBehaviour
{
    public TextMeshProUGUI dialogText;
    public GameObject dialogBox;
    public TextMeshProUGUI characterNameText;
    public float typingSpeed = 0.05f;
    
    private Queue<string> sentences;
    private bool isTyping = false;

    void Start()
    {
        sentences = new Queue<string>();
        dialogBox.SetActive(false);
    }

    public void StartDialog(string characterName, List<string> dialogSentences)
    {
        dialogBox.SetActive(true);
        characterNameText.text = characterName;
        sentences.Clear();

        foreach (string sentence in dialogSentences)
        {
            sentences.Enqueue(sentence);
        }
        
        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogText.text = sentences.Peek();
            isTyping = false;
            return;
        }

        if (sentences.Count == 0)
        {
            EndDialog();
            return;
        }

        string sentence = sentences.Dequeue();
        StartCoroutine(TypeSentence(sentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    void EndDialog()
    {
        dialogBox.SetActive(false);
    }
}
