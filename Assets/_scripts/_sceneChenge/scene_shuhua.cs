using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class scene_shuhua : MonoBehaviour
{
    void scene_Change(int number)
    {
        SceneManager.LoadScene(number);
    }
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            scene_Change(8);
        }

    }
}

