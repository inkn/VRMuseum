using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstiateModles : MonoBehaviour {


public GameObject position;
public GameObject[] modles;
public GameObject[] PANL;
private int i = 0;
private bool isOK = false;

void Update() {

   // INS();

    if (Input.GetKeyDown(KeyCode.Alpha1))
    {
        InstantiateModle(1);
    } if (Input.GetKeyDown(KeyCode.Alpha2))
    {
        InstantiateModle(2);
    } if (Input.GetKeyDown(KeyCode.Alpha3))
    {
        InstantiateModle(3);
    } if (Input.GetKeyDown(KeyCode.Alpha0))
    {
        InstantiateModle(0);
    }
   
}
    //按数组实例化模型成子物体
void InstantiateModle(int i) {
    if (gameObject.transform.childCount > 0)
    {
        //实例化之前销毁上一个实例化的模型
        Destroy(gameObject.transform.GetChild(0).gameObject);
    }
    GameObject prefabs = Instantiate(modles[i]);
    prefabs.transform.parent = gameObject.transform;
    prefabs.transform.position = gameObject.transform.position;
}

//private void Panl() {
//    PANL[i].SetActive(true);
//}
//private void INS() {
//    if(isOK)
//    InstantiateModle(i);
//    isOK = false;
//}
//public void SetValue1()
//{
//    i = 1;
//    isOK = true;
//}
//public void SetValue2()
//{
//    i = 2;
//    isOK = true;
//}
//public void SetValue3()
//{
//    i = 3;
//    isOK = true;
//}
	
}
