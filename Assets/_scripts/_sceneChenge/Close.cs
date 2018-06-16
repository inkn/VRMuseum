using UnityEngine;
using System.Collections;

public class Close : MonoBehaviour {

    public GameObject panel;
    public void ClosePanel()
    {
        panel.SetActive(false);
    }
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
