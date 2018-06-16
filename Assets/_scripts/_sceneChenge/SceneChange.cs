using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Scene_Change : MonoBehaviour {
    public void scene_Change(int number)
    {
        SceneManager.LoadScene(number);
    }
   
   
}
