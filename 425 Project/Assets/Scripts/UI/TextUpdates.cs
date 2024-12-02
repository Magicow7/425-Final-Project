using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class TextUpdates : MonoBehaviour
{
    public static TextUpdates Instance { get; private set; }

    [FormerlySerializedAs("taskText"),SerializeField] private TextMeshProUGUI _taskText;
    [FormerlySerializedAs("deathScreenText"),SerializeField] private GameObject _deathScreenText;

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

    public void ShowDeathScreen()
    {
        _deathScreenText.SetActive(true);
        MouseLook.instance.UnlockCursor();
    }

    public void HideDeathScreen()
    {
        _deathScreenText.SetActive(false);
        MouseLook.instance.LockCursor();
    }


    public void UpdateTaskText(string newText)
    {

        if (_taskText != null)
        {
            if (string.IsNullOrEmpty(newText))
            {
                _taskText.text = "";
            } else
            {
                StartCoroutine(Typewrite(newText));

            }
        } else
        {
            Debug.Log("nulltext");
        }
    }

    private IEnumerator Typewrite(string text)
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

            _taskText.text = finalText;
            yield return new WaitForSeconds(0.04f);
        }
        _taskText.text = text;
        yield return null;
    }
}
