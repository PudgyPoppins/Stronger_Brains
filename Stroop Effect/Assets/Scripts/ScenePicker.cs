using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ScenePicker : MonoBehaviour {
    public void StartGame(string scenename)
    {
        SceneManager.LoadScene(scenename);
    }
}
