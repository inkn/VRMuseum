using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour {
    public void sceneChange(int number)
    {
        SceneManager.LoadScene(number);
    }
   
   
}
