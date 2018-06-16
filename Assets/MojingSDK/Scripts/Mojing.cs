//------------------------------------------------------------------------------
// Copyright 2016 Baofeng Mojing Inc. All rights reserved.
// 
// Author: Xu Xiang
//------------------------------------------------------------------------------

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
//using System.Runtime.InteropServices;
//using System.Text.RegularExpressions;

public class Mojing : MonoBehaviour
{
    // Distinguish the stereo eyes.
    public enum Eye
    {
        Left,
        Center,
        Right
    }

    // The singleton instance of the Mojing class.
    public static Mojing SDK
    {
        get
        {
            if (sdk == null && ConfigItem.MojingSDKActive)
            {
				try
				{
                	sdk = FindObjectOfType<Mojing>();
				}
				catch(Exception e)
				{
					Debug.Log (e.ToString ());
					sdk = null;
				}
            }
            if (sdk == null && ConfigItem.MojingSDKActive)
            {
                MojingLog.LogTrace("Creating Mojing SDK object");
                var go = new GameObject("Mojing");
                sdk = go.AddComponent<Mojing>();
                go.transform.localPosition = Vector3.zero;
            }
            return sdk;
        }
    }
    private static Mojing sdk = null;

    public string GlassesKey
    {
       get
       {
           return glassesKey;
       }
       set
       {
           MojingLog.LogTrace("Change glasses from " + glassesKey + " to " + value);
           if (value != glassesKey)
           {
               glassesKey = value;
               MojingSDK.ChangeMojingWorld(value);
                bWaitForMojingWord = true;
               // NeedDistortion = MojingSDK.Unity_IsGlassesNeedDistortion();
               MojingLog.LogTrace("New glasses is " + glassesKey);
           }
       }
    }
    private string glassesKey = null;

    public bool VRModeEnabled
    {
        get
        {
            return vrModeEnabled;
        }
        set
        {
            for (int i = 0; i < heads.Length; i++)
            {
                heads[i].VRModeEnabled = value;
            }
            if (vrModeEnabled != value)
            {
                vrModeEnabled = value;
                if (value)
                    OnEnable();
                else
                    OnDisable();
            }
        }
    }

    private bool vrModeEnabled = true;

    private MojingVRHead[] heads = null;

    // If the glasses need do image distortion
    public bool NeedDistortion
    {
        get
        {
            return needDistortion || ConfigItem.TW_STATE ;
        }
        set
        {
            MojingLog.LogTrace("needDistortion = " + value.ToString());
            {
                needDistortion = value;
                if (needDistortion)
                    MojingLog.LogTrace(glassesKey + " need distortion.");
                else
                    MojingLog.LogTrace(glassesKey + " DO NOT need distortion.");
            }
            MojingLog.LogTrace("Leave setNeedDistortion");
        }
    }
    private bool needDistortion = true;

    public bool bWaitForMojingWord = false;

    private bool bDuplicateMojing = false;

    private static Manufacturers manufacturers_list;
    private static Products product_list;
    public static Glasses glasses_list;
    public static List<string> glassesKeyList = new List<string>();
    public static List<string> glassesNameList = new List<string>();
    public static List<string> glassesIDList = new List<string>();
    private string resetID;
    //private bool initialized = false;
    private bool isPaused = false;
    void Awake()
    {
        MojingLog.LogTrace("Enter Mojing.Awake");
        if (sdk == null)
        {
            MojingLog.LogWarn("Mojing SDK object is not sets.");
            sdk = this;
#if  UNITY_EDITOR_OSX

#else
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_IOS
            
            MojingSDK.Unity_Init("Unity " + Application.unityVersion,
                                    "C3845343263589043", "3908657356234505", "faadffa1383206111ae162969b340ad9", "www.mojing.cn",
                                    Screen.width, Screen.height, 320, 320,
                                    GetProfilePath(),IntPtr.Zero);
#elif UNITY_ANDROID
            DisplayMetricsAndroid.InitDisplayMetricsAndroid();
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            MojingSDK.Unity_Init(   "Unity " + Application.unityVersion,
                                    "C3845343263589043", "3908657356234505", "faadffa1383206111ae162969b340ad9", "www.mojing.cn",
                                    DisplayMetricsAndroid.WidthPixels, DisplayMetricsAndroid.HeightPixels, DisplayMetricsAndroid.XDPI, DisplayMetricsAndroid.YDPI, 
                                    GetProfilePath(), activity.GetRawObject());
#endif
            MojingSDK.Unity_OnSurfaceChanged(Screen.width, Screen.height);
#endif
#if !UNITY_EDITOR && UNITY_IOS
            MojingSDK.Unity_SetGamePadAxisMode(0);
            /*
            mode:   0 --- default value, Send Axis's direction (LEFT/RIGHT/UP/DOWN/CENTER) 
                    1 --- Send Axis's position (x,y,z)
                    2 --- Send both Axis's direction and postion
            */
			
#endif
            MojingSDK.Unity_SetEngineVersion("Unity " + Application.unityVersion);
           //DontDestroyOnLoad(sdk);  //Remain Mojing GameObject even when change Scenes，just for Android

        }
        if (sdk != this)
        {
            MojingLog.LogWarn("Mojing SDK object should be a singleton.");
            bDuplicateMojing = true;
            enabled = false;
            return;
        }

        try
        {
            //清除Glasses列表
            glassesNameList.Clear();
            glassesKeyList.Clear();

            //CreateDummyCamera();
#if UNITY_IOS
            Application.targetFrameRate = 60;
#endif
            //StereoScreen = null;

            //解析json文件中的glass列表，获取glassesKeyList
            manufacturers_list = MojingSDK.GetManufacturers("zh");

            for(int i=0;i< manufacturers_list.ManufacturerList.Count;i++)
            {
                product_list = MojingSDK.GetProducts(manufacturers_list.ManufacturerList[i].KEY, "zh");
                for(int j=0;j< product_list.ProductList.Count;j++)
                {
                    glasses_list = MojingSDK.GetGlasses(product_list.ProductList[j].KEY, "zh");
                    for(int k=0;k< glasses_list.GlassList.Count;k++)
                    {
                        string GlassName = manufacturers_list.ManufacturerList[i].Display + " " + product_list.ProductList[j].Display + " " + glasses_list.GlassList[k].Display;
                        string GlassKey = glasses_list.GlassList[k].KEY;
                        string GlassID = manufacturers_list.ManufacturerList[i].ID + "/" + product_list.ProductList[j].ID + "/" + glasses_list.GlassList[k].ID;
                        glassesKeyList.Add(GlassKey);
                        glassesNameList.Add(GlassName);
                        glassesIDList.Add(GlassID);
                        glassesKey = FindGlassKey(manufacturers_list.ManufacturerList[i].ID, product_list.ProductList[j].ID, glasses_list.GlassList[k].ID);
                        //glassesKey = GlassKey;   //获取初始glassKey，mojingvrhead awake中用
                    }
                }
            }
        }
        catch (Exception e)
        {
            MojingLog.LogError(e.ToString());
        }

        MojingLog.LogTrace("Leave Mojing.Awake");
    }

    public static string FindGlassKey(int iMID, int iPID, int iGID)
    {
        string strRet = "";
        for(int i = 0; i< manufacturers_list.ManufacturerList.Count; i++)
        {
            if (manufacturers_list.ManufacturerList[i].ID != iMID)
                continue;

            Products PL = MojingSDK.GetProducts(manufacturers_list.ManufacturerList[i].KEY, "zh");
            for(int j = 0; j < PL.ProductList.Count; j++)
            {
                if (PL.ProductList[j].ID != iPID)
                    continue;
                Glasses GL = MojingSDK.GetGlasses(PL.ProductList[j].KEY, "zh");
                for(int k = 0; k< GL.GlassList.Count; k++)
                {
                    if (GL.GlassList[k].ID == iGID)
                    {
                        return GL.GlassList[k].KEY;
                    }
                }
            }
        }
        return strRet;
    }

    void EnterMojingWorld()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        DisplayMetricsAndroid.InitDisplayMetricsAndroid();
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            
        StartCoroutine(MojingSDK.Unity_EnterMojingWorld(GlassesKey, ConfigItem.TW_STATE , activity.GetRawObject()));
#else
        StartCoroutine(MojingSDK.Unity_EnterMojingWorld(GlassesKey, ConfigItem.TW_STATE, IntPtr.Zero));
#endif
    }

    void OnEnable()
    {
        MojingLog.LogTrace("Enter Mojing.OnEnable");
        if (MojingVRHead.Instance.VRModeEnabled)
        {
            EnterMojingWorld();
        }
#if UNITY_IOS
        MojingSDK.Unity_StartTracker(100);
#endif

        bWaitForMojingWord = true;
        heads = FindObjectsOfType<MojingVRHead>();
        if (!g_bStartEndofFrame)
        {
            g_bStartEndofFrame = true;
            getTextureID(iFrameIndex);
            StartCoroutine(EndOfFrame());
        }
        MojingLog.LogTrace("Leave Mojing.OnEnable");
    }

    private void CreateDummyCamera()
    {
        var go = gameObject;
        if (go.GetComponent<Camera>())
        {
            go = new GameObject("VR Dummy Camera");
            go.transform.parent = gameObject.transform;
        }
        var cam = go.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.Nothing;
        cam.cullingMask = 0;
        cam.useOcclusionCulling = false;
        cam.depth = 100;
    }


    public Camera getMainCamera()
    {
        for (int i = 0; i < heads.Length; i++)
        {
            Camera camera = heads[i].getMainCamera();
            if (camera != null)
                return camera;
        }
        return null;
    }

    private int m_ResetCount = 8;
    //Get Texture ID when Awake/OnApplicationFocus/OnSurfaceChanged
    public void ResetTextureID(string resetFlag)
    {
        if (resetFlag == "textureReset")
        {
            m_ResetCount = 8;
        }
    }
    IntPtr tID = IntPtr.Zero;
    IntPtr sID = IntPtr.Zero;
    public int iFrameIndex = 0;
    private static bool g_bStartEndofFrame = false;
    private static WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

    void getTextureID(int i)
    {
        // Metal为每一帧都重新获取TextureID
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Metal)
        {
            tID = StereoScreen[i * 2].colorBuffer.GetNativeRenderBufferPtr();
            sID = StereoScreen[i * 2 + 1].colorBuffer.GetNativeRenderBufferPtr();
        }
        else
        {
            tID = textureID[i * 2];
            sID = textureID[i * 2 + 1];
        }
    }

    IEnumerator EndOfFrame()
    {
        MojingLog.LogTrace("Enter Mojing.EndOfFrame thread");
        while (true)
        {
            yield return waitForEndOfFrame;
            if ((!bWaitForMojingWord) && MojingVRHead.Instance.VRModeEnabled && NeedDistortion)
            {
                getTextureID(iFrameIndex);
                MojingSDK.SetTextureID(tID, sID);
                MojingSDK.Unity_DistortFrame();
#if UNITY_ANDROID
				// iOS 不启用三缓冲
                iFrameIndex = ++iFrameIndex % (screenNum / 2);
#endif
            }
       }
       //MojingLog.LogTrace("Leave Mojing.EndOfFrame thread");
   }

   // need to account for the limits due to screen size.
    public class Lens
    {
        public void UpdateProfile()
        {
            MojingLog.LogTrace("Mojing.Lens.UpdateProfile");
            FOV = MojingSDK.Unity_GetGlassesFOV();
            Separation = MojingSDK.Unity_GetGlassesSeparation();
            DistortionR = MojingSDK.Unity_GetDistortionR();
        }
        public float Separation;
        public float DistortionR;
        public float FOV;
    }
    public Lens lens = new Lens();

    public class Mobile
    {
        public int nWidth;
        public int nHeight;
        public float width;   // The long edge of the phone.
        public float height;  // The short edge of the phone.
        public float border;  // Distance from bottom of the glasses to the bottom edge of screen.
        public void UpdateProfile()
        {
            MojingLog.LogTrace("Screen size old: " + width + " x " + height);
            MojingSDK.Unity_GetScreenSize(ref width, ref height);
            MojingLog.LogTrace("Screen size new: " + width + " x " + height);
            border = 0;
        }
    }
    public Mobile mobile = new Mobile();
    private void UpdateProfile()
    {
        lens.UpdateProfile();
        mobile.UpdateProfile();
    }

    public MutablePose3D headPose = new MutablePose3D();
    private Matrix4x4 headView = new Matrix4x4();

    // Simulated neck model in the editor mode.
    private static readonly Vector3 neckOffset = new Vector3(0, 0.075f, -0.08f);
    // Use mouse to emulate head in the editor.
    private float mouseX = 0;
    private float mouseY = 0;
    private float mouseZ = 0;
  private float neckModelScale = 0.0f;
  // Mock settings for in-editor emulation while playing.
  public bool autoUntiltHead = true;
  
    public void UpdateState()
    {
        try
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            Quaternion rot;
            bool rolled = false;
            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                mouseX += Input.GetAxis("Mouse X") * 5;
                if (mouseX <= -180)
                {
                    mouseX += 360;
                }
                else if (mouseX > 180)
                {
                    mouseX -= 360;
                }
                mouseY -= Input.GetAxis("Mouse Y") * 2.4f;
                mouseY = Mathf.Clamp(mouseY, -85, 85);
            }
            else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                rolled = true;
                mouseZ += Input.GetAxis("Mouse X") * 5;
                mouseZ = Mathf.Clamp(mouseZ, -85, 85);
            }
            if (!rolled && autoUntiltHead)
            {
                // People don't usually leave their heads tilted to one side for long.
                mouseZ = Mathf.Lerp(mouseZ, 0, Time.deltaTime / (Time.deltaTime + 0.1f));
            }
            rot = Quaternion.Euler(mouseY, mouseX, mouseZ);
            var neck = (rot * neckOffset - neckOffset.y * Vector3.up) * neckModelScale;
            headPose.Set(neck, rot);
#else
            MojingSDK.GetLastHeadView(ref headView);
            headPose.SetRightHanded(headView);
#endif
        }
        catch (Exception e)
        {
            MojingLog.LogError(e.ToString());
        }
    }

    private static float windowWidth;
    private static float windowHeight;
    void Update()
    {
        if (bWaitForMojingWord)
        {
            try
            {
                if (MojingSDK.IsInMojingWorld(sdk.GlassesKey))
                {
                    bWaitForMojingWord = false;
                    MojingSDK.SetCenterLine();
                    NeedDistortion = MojingSDK.Unity_IsGlassesNeedDistortion();
                    UpdateProfile();
                    VRModeEnabled = MojingVRHead.Instance.VRModeEnabled;
				}
            }
            catch (Exception e)
            {
                MojingLog.LogError(e.ToString());
            }
        }

        if (Screen.width != windowWidth || Screen.height != windowHeight)
        {
            MojingSDK.Unity_OnSurfaceChanged(Screen.width, Screen.height);
            windowWidth = Screen.width;
            windowHeight = Screen.height;
        }
    }

    void OnDisable()
    {
        MojingLog.LogTrace("Enter Mojing.OnDisable");
        MojingLog.LogTrace("Leave Mojing.OnDisable");
    }

    void OnLevelWasLoaded(int level)
    {
        try
        {
            MojingLog.LogTrace("Enter Mojing.OnLevelWasLoaded");

            if (!bDuplicateMojing)
            {
                heads = FindObjectsOfType<MojingVRHead>();
                VRModeEnabled = MojingVRHead.Instance.VRModeEnabled;
            }
        }
        catch (Exception e)
        {
            MojingLog.LogError(e.ToString());
        }
    }

    void OnDestroy()
    {
        MojingLog.LogTrace("Enter Mojing.OnDestroy");
        if (sdk == this){
            sdk = null;
        }
        MojingSDK.Initialized = false;
        MojingSDK.Unity_LeaveMojingWorld();
        StopAllCoroutines();
        g_bStartEndofFrame = false;
        MojingSDK.DestroyMojingWorld();
        MojingLog.LogTrace("Leave Mojing.OnDestroy");
    }

#if UNITY_ANDROID

    private void OnApplicationPause(bool pause)
    {
        MojingLog.LogTrace("Mojing.OnApplicationPause");
        SetPause(pause);
    }
#endif

    public void SetPause(bool pause)
    {
        if (!MojingSDK.Initialized || isPaused == pause)
            return;

        if (pause)
        {
            OnPause();
        }
        else
        {
            ResetTextureID("textureReset");
            StartCoroutine(OnResume());
        }
    }

    void OnPause()
    {
        MojingLog.LogTrace("Enter Mojing.OnPause");
        isPaused = true;
        StopAllCoroutines();
        MojingLog.LogTrace("Mojing.OnPause---StopAllCoroutines");
        g_bStartEndofFrame = false;
        MojingSDK.Unity_LeaveMojingWorld();
        MojingLog.LogTrace("Mojing.OnPause---Unity_LeaveMojingWorld");
        MojingLog.LogTrace("Leave Mojing.OnPause");
    }

    IEnumerator OnResume()
    {
        yield return new WaitForSeconds(0.5f);
        if (!g_bStartEndofFrame)
        { 
            EnterMojingWorld();
            getTextureID(iFrameIndex);
            g_bStartEndofFrame = true;
            StartCoroutine(EndOfFrame());
            isPaused = false;
            yield break;
        }
        MojingLog.LogTrace("Leave Mojing.OnResume");
    }

    void OnApplicationQuit()
    {
        OnPause();
    }

    string GetProfilePath()
    {
        string szProfilePath = Application.dataPath + "/StreamingAssets/MojingSDK";
        return szProfilePath;
    }

#if UNITY_ANDROID
    private static int screenNum = 6;
#else
	private static int screenNum = 2;
#endif

    private RenderTexture[] stereoScreen = new RenderTexture[screenNum];
	private RenderTexture[] defaultScreen = new RenderTexture[2];
    private IntPtr[] textureID = new IntPtr[screenNum];
    public RenderTexture[] StereoScreen
    {
        get
        {
            // Create RenderTexture when (1) No VRMode, (2) No NeedDistortion Glasses & No Timewarp
			if (defaultScreen==null && (!MojingVRHead.Instance.VRModeEnabled || !NeedDistortion))
            {
                MojingLog.LogTrace("Use default Render texture");
				for (int i = 0; i < 2; i++) {
					defaultScreen [i] = new RenderTexture (Screen.width, Screen.height, 24, RenderTextureFormat.Default);
					defaultScreen[i].anisoLevel = 0;
					defaultScreen[i].antiAliasing = Mathf.Max(QualitySettings.antiAliasing, 1);
                    defaultScreen[i].Create();
                    if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Metal){
						textureID[i] = defaultScreen[i].colorBuffer.GetNativeRenderBufferPtr();
					}else{
				    textureID[i] = defaultScreen[i].GetNativeTexturePtr();
					}
					Debug.Log("Texture " + i + " id = " +  textureID[i]);
				}
                return defaultScreen;
            }

            // Create RenderTexture when (1)VRMode & NeedDistortion Glasses, (2)VRMode & Timewarp.
            
            for (int i = 0; i < screenNum; i++)
            {
                if (stereoScreen[i] == null && NeedDistortion)
                {
                    int size = MojingSDK.Unity_GetTextureSize();
                    MojingLog.LogTrace("Creating new stereo screen texture with " + size.ToString() + " Pixels");
                    stereoScreen[i] = new RenderTexture(size, size, 24, RenderTextureFormat.Default);
                    stereoScreen[i].anisoLevel = 0;
                    stereoScreen[i].antiAliasing = Mathf.Max(QualitySettings.antiAliasing, 1);
                    stereoScreen[i].Create();
                    if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Metal){
                        textureID[i] = stereoScreen[i].colorBuffer.GetNativeRenderBufferPtr();
                    }else{
                    textureID[i] = stereoScreen[i].GetNativeTexturePtr();
                    }
                    Debug.Log("Texture " + i + " id = " +  textureID[i]);
                }
            }
            return stereoScreen;
        }
        set
        {
            MojingLog.LogTrace("Set Texture with size of " + stereoScreen.GetLength(0));

            if (value == stereoScreen)
            {
                return;
            }
            if (!NeedDistortion && value != null)
            {
                MojingLog.LogError("Can't set StereoScreen: No distortion correction is needed.");
                return;
            }
            if (stereoScreen != null)
            {
                for (int i = 0; i < screenNum; i++)
                {
                    stereoScreen[i].Release();
                }
            }
            stereoScreen = value;
        }
    }
}
