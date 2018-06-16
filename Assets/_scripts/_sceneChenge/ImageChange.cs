using UnityEngine;
using System.Collections;

public class ImageChange : MonoBehaviour
{
    public GameObject[] Images;
    public GameObject[] Text;
    //public KeyCode[] key;
    private float time = 5f;    //间隔时间
    private float currentTime;
    public int flag = 1;
    

    // Use this for initialization
    void Start()
    {
        currentTime = 0f;
        Text[1].SetActive(false);
        Text[2].SetActive(false);
        Text[3].SetActive(false);
        Text[4].SetActive(false);
        

    }

    // Update is called once per frame
    void Update()
    {
        currentTime = currentTime + Time.deltaTime;          //计时
        if (currentTime > time)
        {
            switch (flag)
            {
                case 1:
                    Images[0].SetActive(false);
                    Images[1].SetActive(true);
                    Text[0].SetActive(false);
                    Text[1].SetActive(true);
                    flag = 2;
                    break;
                case 2:
                    Images[1].SetActive(false);
                    Images[2].SetActive(true);
                    Text[1].SetActive(false);
                    Text[2].SetActive(true);
                    flag = 3;                
                    break;
                case 3:
                    Images[2].SetActive(false);
                    Images[3].SetActive(true);
                    Text[2].SetActive(false);
                    Text[3].SetActive(true);
                    flag = 4;
                    break;
                case 4:
                    Images[3].SetActive(false);
                    Images[4].SetActive(true);
                    Text[3].SetActive(false);
                    Text[4].SetActive(true);
                    flag = 5;
                    break;
                case 5:
                    Images[4].SetActive(false);
                    Images[0].SetActive(true);
                    Text[4].SetActive(false);
                    Text[0].SetActive(true);
                    flag = 1;
                    break;
                default:
                    break;
            }
            currentTime = 0f;
        }
       
    }
 
}