using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetScene : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start() { }

    // Update is called once per frame
    private void Update() { }

    public void DoReset()
    {
        Debug.LogWarning("RESTING SCENE");
        TextUpdates.Instance.HideDeathScreen();
        SceneManager.LoadScene("TitleScreen");
    }
}