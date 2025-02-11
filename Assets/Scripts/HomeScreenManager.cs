using UnityEngine;
using UnityEngine.SceneManagement;


public class HomeScreenManager: MonoBehaviour
{

    public void PlayGame()
    {
        SceneManager.LoadScene(1);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
    
}
