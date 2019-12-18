using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MenuScreenController : MonoBehaviour {
    public void StartGame(string scenename)
    {
        SceneManager.LoadScene(scenename);
    }
}
