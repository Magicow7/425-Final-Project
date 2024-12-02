using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public class TextUpdates : MonoBehaviour
{
    public static TextUpdates Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI taskText;
    [SerializeField] private GameObject deathScreenText;

    [SerializeField] private TextMeshProUGUI killsUI;

    [SerializeField] private TextMeshProUGUI timeUI;

    [SerializeField] private TextMeshProUGUI finalScoreUI;

    private bool gameOver = false;

    private int kills;

    private float timeSurvived;


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
        if(!gameOver){
            gameOver = true;
            deathScreenText.SetActive(true);
            MouseLook.instance.UnlockCursor();
            StartCoroutine(DeathScreen());
        }
        
    }

    public void HideDeathScreen()
    {
        deathScreenText.SetActive(false);
        MouseLook.instance.LockCursor();
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

    public void setTimeAlive(float val){
        if(!gameOver){
            timeSurvived = val;
        }   
    }

    public void setKills(int val){
        if(!gameOver){
            kills = val;
        }   
    }

    private IEnumerator DeathScreen(){
    
        deathScreenText.transform.localScale = new Vector3(0,0,0);
        float timePassed = 0;
        while(timePassed < 3){
            timePassed+= Time.deltaTime;
            float tempVal = Mathf.Lerp(0,1,timePassed);
            deathScreenText.transform.localScale = new Vector3(tempVal, tempVal, tempVal);
            yield return null;
        }
        deathScreenText.transform.localScale = new Vector3(1,1,1);
        yield return new WaitForSeconds(1);
        killsUI.text = kills.ToString();
        SoundManager.PlaySound(SoundManager.Sound.MenuButtonPress);
        yield return new WaitForSeconds(1);
        string formattedTime = ((int)timeSurvived/60) + ":" + ((int)timeSurvived%60);
        timeUI.text = formattedTime;
        SoundManager.PlaySound(SoundManager.Sound.MenuButtonPress);
        yield return new WaitForSeconds(1);
        finalScoreUI.text = (((int)(timeSurvived)*5) + (kills * 100)).ToString();
        SoundManager.PlaySound(SoundManager.Sound.MenuButtonPress);

    }
}
