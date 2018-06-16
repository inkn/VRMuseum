//------------------------------------------------------------------------------
// Copyright 2016 Baofeng Mojing Inc. All rights reserved.
// 
// Author: Xu Xiang
//------------------------------------------------------------------------------

using UnityEngine;
using System.Reflection;
using System;

[RequireComponent(typeof(Camera))]
public class MojingEye : MonoBehaviour
{
    // Whether this is the left eye or the right eye, or the mono eye.
    public Mojing.Eye eye;

    // The stereo controller in charge of this eye (and whose mono camera
    // we will copy settings from).
    private Camera _CurrentCamera;
    private Camera CurrentCamera
    {
        get
        {
            if(_CurrentCamera == null)
                _CurrentCamera = GetComponent<Camera>();
            return _CurrentCamera;
        }
        set
        {
            _CurrentCamera = value;
        }
    }
    private MojingVRHead head = null;
    public MojingVRHead Head
    {
        // This property is set up to work both in editor and in player.
        get
        {
            if (transform.parent == null)
            { 
                // Should not happen.
                return null;
            }
            if (head == null)
            {
                head = transform.parent.GetComponentInParent<MojingVRHead>();
            }
            return head;
        }
    }

    public bool VRModeEnabled
    {
        get
        {
            return vrModeEnabled;
        }
        set
        { 
            vrModeEnabled = value;
            UpdateVrMode();
        }
    }
    [SerializeField]
    private bool vrModeEnabled = true;

    public void UpdateVrMode()
    {
        try 
        {
            switch (eye)
            {
                case Mojing.Eye.Left:
                case Mojing.Eye.Right:
                    if (vrModeEnabled)
                        EnableEye(true);
                    else
                        EnableEye(false);
                    break;

                case Mojing.Eye.Center:
                    if (!vrModeEnabled)
                        EnableEye(true);
                    else
                        EnableEye(false);
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.Log( e.ToString());
        }
    }
    void SetTargetTex(Mojing.Eye eye)
    {
#if UNITY_EDITOR_OSX
#elif UNITY_EDITOR || UNITY_STANDALONE_WIN
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGL2 || SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore)
        {
            switch (eye)
            {
                case Mojing.Eye.Left:
                    CurrentCamera.targetTexture = Mojing.SDK.StereoScreen[Mojing.SDK.iFrameIndex * 2];
                    break;
                case Mojing.Eye.Right:
                    CurrentCamera.targetTexture = Mojing.SDK.StereoScreen[Mojing.SDK.iFrameIndex * 2 + 1];
                    break;
            }
        }
        else
        {
            Debug.LogWarning("UNITY_EDITOR, current Graphics API is D3D, Please transfer to OpenGL");
        }
#else
        switch (eye)
        {
            case Mojing.Eye.Left:
                CurrentCamera.targetTexture = Mojing.SDK.StereoScreen[Mojing.SDK.iFrameIndex * 2];
                break;
            case Mojing.Eye.Right:
                CurrentCamera.targetTexture = Mojing.SDK.StereoScreen[Mojing.SDK.iFrameIndex * 2 + 1];
                break;
        }
#endif

    }
    public void EnableEye(bool enable)
    {
        MojingLog.LogTrace("Enable Camera " + eye + ": " + enable);
        enabled = enable;
        //if (eye == Mojing.Eye.Left || eye == Mojing.Eye.Right)
        {
            // Setup FOV
			CurrentCamera.fieldOfView = MojingSDK.Unity_GetGlassesFOV();
            //*****Solve the problem of splash screen when start up
            if (enable)
            {
                if (Mojing.SDK.NeedDistortion)
                {
                    SetTargetTex(eye);
                }
                else
                {
                    CurrentCamera.targetTexture = null;
                   // SetUpEye();
                }
            }
        }
		CurrentCamera.enabled = enable;
    }

    void Awake()
    {
        CurrentCamera = GetComponent<Camera>();
    }

    void Start()
    {
        var ctlr = Head;
        if (ctlr == null)
        {
            Debug.LogError("MojingEye must be child of a MojingVRHead.");
            enabled = false;
        }
        SetUpEye();
    }

    //Render directly to Screen Mode, PerspectiveOffCenter() and CreateMatrix() is needed.
    static Matrix4x4 PerspectiveOffCenter(Rect rcView, float near, float far)
    {
      
        float right = rcView.xMax;
         float left = rcView.xMin;
         float top = rcView.yMin;
         float bottom = rcView.yMax;
        Matrix4x4 m = new Matrix4x4();
        /* 
       float r_width = 1.0f / (right - left);
        float r_height = 1.0f / (top - bottom);
        float r_depth = 1.0f / (near - far);
        float x = 2.0f * (near * r_width);
        float y = 2.0f * (near * r_height);
        float A = (right + left) * r_width;
        float B = (top + bottom) * r_height;
        float C = (far + near) * r_depth;
        float D = 2.0f * (far * near * r_depth);


      m[0 , 0] = x;
      m[0 , 1] = 0.0f;
      m[0 , 2] = 0.0f;
      m[0 , 3] = 0.0f;
      m[1 , 0] = 0.0f;
      m[1 , 1] = y;
      m[1 , 2] = 0.0f;
      m[1 , 3] = 0.0f;
      m[2 , 0] = A;
      m[2 , 1] = B;
      m[2 , 2] = C;
      m[2 , 3] = -1.0f;
      m[3, 0] = 0.0f;
      m[3, 1] = 0.0f;
      m[3, 2] = D;
      m[3, 3] = 0.0f;
      */



        float x = 2.0F * near / (rcView.width);
         float y = 2.0F * near / (rcView.height);
         float a = (right + left) / (right - left);
         float b = (top + bottom) / (top - bottom);
         float c = -(far + near) / (far - near);
         float d = -(2.0F * far * near) / (far - near);
         float e = -1.0F;

         m[0, 0] = x;
         m[0, 1] = 0;
         m[0, 2] = a;
         m[0, 3] = 0;
         m[1, 0] = 0;
         m[1, 1] = y;
         m[1, 2] = b;
         m[1, 3] = 0;
         m[2, 0] = 0;
         m[2, 1] = 0;
         m[2, 2] = c;
         m[2, 3] = d;
         m[3, 0] = 0;
         m[3, 1] = 0;
         m[3, 2] = e;
         m[3, 3] = 0;
        return m;
    }
    float near;
    float far;
    float widthSize;
    float heightSize;
    float Separation = 0;
//     float[] offSet_L = new float[4];
//     float[] offSet_R = new float[4];
    Matrix4x4 CreateMatrix()
    {
        
        Matrix4x4 m = new Matrix4x4();
        near = CurrentCamera.nearClipPlane;
        far = CurrentCamera.farClipPlane;
        widthSize = Mojing.SDK.mobile.width;
        heightSize = Mojing.SDK.mobile.height;
        Debug.Log("Screen Size = " + widthSize + " * " + heightSize);

        Separation = Mojing.SDK.lens.Separation; // 瞳距，物理尺寸，以米为单位
        
        float Angle = MojingSDK.Unity_GetGlassesFOV(); // FOV，以角度为单位
        Debug.Log("Fov = " + Angle);

        float DistortionR = Mojing.SDK.lens.DistortionR;// 成象尺寸
        Debug.Log("DistortionR = " + DistortionR);

        if (DistortionR <= 0)
        {
            // DistortionR = near * Mathf.Tan(Angle / 180f * Mathf.PI / 2);
            DistortionR = widthSize / 4;
        }
        Debug.Log("Fixd.DistortionR = " + DistortionR);


        /*1 理想情况下，屏幕能够放下整个畸变图形*/
        float fLeft = -DistortionR;
        float fTop = DistortionR;
        float fRight = DistortionR;
        float fBottom = -DistortionR;
        float fNear = DistortionR / Mathf.Tan(Angle / 180f * Mathf.PI / 2) ;// 通过FOV和畸变框的大小，算出来近平面的距离。依然是以米为单位的。
        float fZoom = fNear / near;
        Debug.Log("fNear = " + fNear + " , near = " + near + " , Zoom = " + fNear / near);
        
        /*2 处理屏幕的剪裁*/
        // 2.1 内测剪裁 - 畸变图像比瞳距大
        if (DistortionR > Separation / 2)
        {
            Debug.Log("Fix Inside");
            if (eye == Mojing.Eye.Left)
            {
                fRight = Separation / 2;
            }
            else
            {
                fLeft = -Separation / 2;
            }
        }
        // 2.2 屏幕对上、下边界剪裁
        if (heightSize < 2* Separation)
        {
            Debug.Log("Fix UP/BOTTOM");
            fTop = heightSize / 2;
            fBottom = -heightSize / 2;
        }
        // 2.3 屏幕对外侧边界的剪裁??
        
        if (Separation + 2 * DistortionR > widthSize)
        {
            Debug.Log("Fix Outside");
            float fOutside = (widthSize - Separation) / 2;
            if (eye == Mojing.Eye.Left)
            {
                fLeft = -fOutside;
            }
            else
            {
                fRight = fOutside;
            }
        }
        if (eye == Mojing.Eye.Left)
        {
            float fFOV_Inside = Mathf.Abs(Mathf.Atan( fRight / fNear) * 180f / Mathf.PI);
            float fFOV_Outside = Mathf.Abs(Mathf.Atan(fLeft / fNear) * 180f / Mathf.PI);

            float fFOV_Upside = Mathf.Abs(Mathf.Atan(fTop / fNear) * 180f / Mathf.PI);
            float fFOV_Lowside = Mathf.Abs(Mathf.Atan(fBottom / fNear) * 180f / Mathf.PI);

            Debug.Log("FOV.H = " + (fFOV_Inside + fFOV_Outside) + " , FOV.V = " + (fFOV_Upside + fFOV_Lowside));
        }
        Rect rcView = new Rect(fLeft / fZoom, fBottom / fZoom, (fRight - fLeft) / fZoom, (fTop - fBottom) / fZoom);
        m = PerspectiveOffCenter(rcView, near, far);

/*

        switch (eye)
        {
            case Mojing.Eye.Left:
                offSet_L[0] = -(widthSize - separation) * 2.0f;
                offSet_L[1] = separation * 2.0f;
                offSet_L[2] = -heightSize * 2.0f;
                offSet_L[3] = -offSet_L[2];
//                 float dist = separation / 2.0f * near;
//                 double rangle = Mathf.Atan(dist);
//                 rangle = (180 / Math.PI) * rangle;                
//                 float langle = Angle - (float)rangle;
//                 offSet_L[0] = Mathf.Tan(langle) * near;
//                 offSet_L[1] = separation /2.0f;
//                 offSet_L[2] = (offSet_L[0] + offSet_L[1]) / 2.0f;
//                 offSet_L[3] = offSet_L[2];
                m = PerspectiveOffCenter(offSet_L[0], offSet_L[1], offSet_L[2], offSet_L[3], near, far);
                break;
            case Mojing.Eye.Right:
                offSet_R[0] = -separation * 2.0f;
                offSet_R[1] = (widthSize - separation) * 2.0f;
                offSet_R[2] = -heightSize * 2.0f;
                offSet_R[3] = -offSet_R[2];   
//                 double angle2 = Mathf.Atan((-separation) / 2.0f * near);
//                 angle2 = (180 / Math.PI) * angle2;
//                 float langle2 = Angle - (float)angle2;
//                 offSet_R[0] = -separation / 2.0f * near;
//                 offSet_R[1] = -(Mathf.Tan(langle2) * near);
//                 offSet_R[2] = -((offSet_R[0]+ offSet_R[1]) / 2.0f);//(offSet_R[0]+ offSet_R[1]) / 2.0f;
//                 offSet_R[3] = offSet_R[2];
                m = PerspectiveOffCenter(offSet_R[0], offSet_R[1], offSet_R[2], offSet_R[3], near, far);
                break;
        }
        */
        return m;
    }
    public void OnPreCull()
    {
/*
        if (!Mojing.SDK.NeedDistortion && Mojing.SDK.VRModeEnabled)
            CurrentCamera.projectionMatrix = CreateMatrix();
        else
            CurrentCamera.ResetProjectionMatrix();
*/
        if (Mojing.SDK.bWaitForMojingWord)
        {
            EnableEye(false);
            return;
        }
		if ( CurrentCamera != null)
        {
            SetUpEye();
            // --madi--
            //mojing2 render directly
            if (!Mojing.SDK.NeedDistortion)
                return;
            SetTargetTex(eye);
        }
        else 
        {
            MojingLog.LogError(eye.ToString() + ": no camera found.");
        }
    }

    void OnDestroy()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        switch (eye)
        {
            case Mojing.Eye.Left:
                CurrentCamera.targetTexture = null;
                break;
            case Mojing.Eye.Right:
                CurrentCamera.targetTexture = null;
                break;
        }
#endif
    }

    public void SetUpEye()
    {

        // Do not change any settings of Center Camera except localtion
        if (eye == Mojing.Eye.Center)
        {
            transform.localPosition = 0 * Vector3.right;
        }
        else
        {
            // Setup the rect & transform
            Rect rect = new Rect(0, 0, 1, 1);
            float ipd = Mojing.SDK.lens.Separation; // *controller.stereoMultiplier;            

#if UNITY_EDITOR_OSX
			switch (eye)
			{
			case Mojing.Eye.Left:
				rect.width = 0.5f;
				transform.localPosition = (-ipd / 2) * Vector3.right;
				break;
				
			case Mojing.Eye.Right:
				rect.x = 0.5f;
				rect.width = 0.5f;
				transform.localPosition = (ipd / 2) * Vector3.right;
				break;
			}
#elif UNITY_EDITOR || UNITY_STANDALONE_WIN
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGL2 || SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore)
            {
                switch (eye)
                {
                    case Mojing.Eye.Left:
                        if (Mojing.SDK.NeedDistortion)
                        {
                            rect.width = 1.0f;
                        }
                        else
                        {
                            rect.width = 0.5f;
                        }
                        transform.localPosition = (-ipd / 2) * Vector3.right;
                        break;

                    case Mojing.Eye.Right:
                        if (Mojing.SDK.NeedDistortion)
                        {
                            rect.width = 1.0f;
                        }
                        else
                        {
                            rect.x = 0.5f;
                            rect.width = 0.5f;
                        }
                        transform.localPosition = (ipd / 2) * Vector3.right;
                        break;
                }
            }    
            else
            { 
                switch (eye)
                {
                    case Mojing.Eye.Left:
                        rect.width = 0.5f;
                        transform.localPosition = (-ipd / 2) * Vector3.right;
                        break;

                    case Mojing.Eye.Right:
                        rect.x = 0.5f;
                        rect.width = 0.5f;
                        transform.localPosition = (ipd / 2) * Vector3.right;
                        break;
                }
            }
#else
            switch (eye)
            {
                case Mojing.Eye.Left:
                    if (Mojing.SDK.NeedDistortion)
                    {
                        rect.width = 1.0f;
                    }
                    else
                    {
                        rect.width = 0.5f;
                    }
                    transform.localPosition = (-ipd / 2) * Vector3.right;
                    break;

                case Mojing.Eye.Right:
                    if (Mojing.SDK.NeedDistortion)
                    {
                        rect.width = 1.0f;
                    }
                    else
                    {
                        rect.x = 0.5f;
                        rect.width = 0.5f;
                    }
                    transform.localPosition = (ipd / 2) * Vector3.right;
                    break;
            }
#endif

            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            CurrentCamera.rect = rect;
        }
    }
}
