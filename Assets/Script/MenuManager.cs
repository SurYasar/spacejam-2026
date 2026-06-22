using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void ChangeScene(string name)
    {
        SceneManager.LoadScene(name);
    }
    public void ExitGame()
    {
        // Exits the application when built
        Application.Quit();

        // If running in the editor, log this instead
        #if UNITY_EDITOR
            Debug.Log("Game is exiting...");
        #endif
    }
}
