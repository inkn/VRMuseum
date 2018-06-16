using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;





public class RotateModle : MonoBehaviour
{
    public float speed = 0.5F;
    //设置一个空物体为模型父物体方便模型旋转
    public GameObject father;
    public GameObject camer;
    private Camera cam;
    private Touch old1;
    private Touch old2;
    private bool isdo=false;



    void Start() {
        
        cam = camer.GetComponent<Camera>();
        
    
    }
  
    void Update()
    {    
        //单指操控
        if (!isdo)
        {
            // Get movement of the finger since last frame
            Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;
            //捕获手指在水平方向移动的距离，使这个参数成为模型绕Y轴旋转角度的参数
            transform.Rotate(0, -touchDeltaPosition.x * speed, 0);
            //捕获手指在竖直方向移动的距离，使这个参数成为模型绕其父物体X轴旋转角度的参数
            father.transform.Rotate(touchDeltaPosition.y * speed, 0, 0);

        }
       // 双指操控
        if (Input.touches.Length >1) { 
            isdo = true;
        }
        else isdo = false;
       
        if (Input.touchCount > 1 && isdo)
        {
            Touch new1 = Input.GetTouch(0);
            Touch new2 = Input.GetTouch(1);
            if (new2.phase == TouchPhase.Began)
            {
                old1 = new1;
                old2 = new2;
                return;
            }
            float olddis = Vector2.Distance(old1.position, old2.position);
            float newdis = Vector2.Distance(new1.position, new2.position);
            float offset = olddis - newdis;
            int index = (int)(offset / 10); 
            cam.fieldOfView += index;
          //  放大缩小范围由
            if (cam.fieldOfView < 36) cam.fieldOfView = 36;
            if (cam.fieldOfView > 110) cam.fieldOfView = 110;
            
            
            old1 = new1;
            old2 = new2;
      
        }
       

    }

   
 
}