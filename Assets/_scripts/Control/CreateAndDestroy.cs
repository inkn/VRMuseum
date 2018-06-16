using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateAndDestroy : MonoBehaviour {

    public GameObject prefab;
	// Use this for initialization
	void Start () {
        GameObject.Instantiate(prefab,Vector3.forward,Quaternion.identity);
        Vector3 polition = new Vector3(0, 0, 1);
       // GameObject instance=Instantiate(_prefab.Load<GameObject>("tray"));
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
