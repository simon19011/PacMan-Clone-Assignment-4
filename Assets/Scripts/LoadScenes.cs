using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScenes : MonoBehaviour
{
    public void LoadStartScene()
    {
        SceneManager.LoadScene("StartScene");
    }
    public void LoadLevel1()
    {
        SceneManager.LoadScene("Level1");
    }

    public void LoadLevel2()
    {
        if (Application.CanStreamedLevelBeLoaded("Level2"))
        {
            SceneManager.LoadScene("Level2");
        }

        else
        {
            Debug.Log("Sorry Level 2 is not available yet!");
        }
    }
}
