using TMPro;
using UnityEngine;
using System.Collections;

public class TextUpdates : MonoBehaviour
{
    public static TextUpdates Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI taskText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject); 
        }
    }

    public void UpdateTaskText(string newText)
    {

        if (taskText != null)
        {
            if (string.IsNullOrEmpty(newText))
            {
                taskText.text = "";
            } else
            {
                StartCoroutine(typewrite(newText));

            }
        } else
        {
            Debug.Log("nulltext");
        }
    }

    private IEnumerator typewrite(string text)
    {
        char[] characters = text.ToCharArray();
        string finalText = "";

        for (int i = 0; i < characters.Length; i++)
        {
            finalText += characters[i];

            if (characters[i] == ' ')
            {
                continue;
            } 

            taskText.text = finalText;
            yield return new WaitForSeconds(0.04f);
        }
        taskText.text = text;
        yield return null;
    }
}
