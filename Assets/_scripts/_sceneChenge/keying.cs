using UnityEngine;
using System.Collections;

public class keying : MonoBehaviour {
    public GameObject panel;
	// Use this for initialization
	void Start () {
        panel.SetActive(false);
	}
	
	// Update is called once per frame
	void Update(){
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            panel.SetActive(true);
        }
    
	}
    public void quit()
    {
        Application.Quit();
    }
    public void closePanel()
    {
        panel.SetActive(false);
    }
}
