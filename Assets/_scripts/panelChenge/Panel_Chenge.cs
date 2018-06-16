using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panel_Chenge : MonoBehaviour {
    public int sign;
    public GameObject[] button;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0))
        {
            sign = 0;
            Debug.Log(sign);
        }
	}
}
