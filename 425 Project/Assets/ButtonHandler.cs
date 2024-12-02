using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class ButtonHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BeginGame()
    {
        Debug.LogWarning("LOADING PLAYING SCENE");
        //TextUpdates.Instance.HideDeathScreen();
        SceneManager.LoadScene("MergedDungeonScene");
    }
}
