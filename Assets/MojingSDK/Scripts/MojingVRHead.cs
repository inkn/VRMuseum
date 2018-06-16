//------------------------------------------------------------------------------
// Copyright 2016 Baofeng Mojing Inc. All rights reserved.
// 
// Author: Xu Xiang
//------------------------------------------------------------------------------
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

public class MojingVRHead : MonoBehaviour
{
    public static MojingVRHead Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<MojingVRHead>();

            }
            return instance;
        }
    }
    private static MojingVRHead instance = null;
    public bool VRModeEnabled
    {
        get
        {
            return vrModeEnabled;
        }
        set
        {
            vrModeEnabled = value;
            foreach (MojingEye eye in Eyes)
            {
                eye.VRModeEnabled = vrModeEnabled;
            }
        }
    }
    [SerializeField]
    private bool vrModeEnabled = true;
    /*
    public Camera getMainCamera()
    {
        foreach (MojingEye eye in Eyes)
        {
            if (eye.eye == Mojing.Eye.Center)
            {
				return eye.GetComponent<Camera>();
            }
        }
        return null;
    }
    */
    private Camera _mainCamera = null;
    public Camera getMainCamera()
    {
        if (_mainCamera != null)
            return _mainCamera;
        for (int i = 0; i < Eyes.Length; i++)
        {
            if (Eyes[i].eye == Mojing.Eye.Center)
            {
                _mainCamera = Eyes[i].GetComponent<Camera>();
                return _mainCamera;
            }
        }
        return null;
    }
    private MojingEye[] Eyes
    {
        get 
        {
            if (eyes == null)
                eyes = GetComponentsInChildren<MojingEye>(true);
            return eyes;
        }
    }
    private MojingEye[] eyes = null;
    private Mojing sdk = null;

    public GlassesTypes GlassesType
    {
        get{
            return glassesType;
        }
        set{
            glassesType = value;
        }
    }
    [SerializeField]
    private GlassesTypes glassesType = GlassesTypes.MojingIII;

    void Awake()
    {
#if DEBUG
        MojingLog.LogTrace("MojingSDK Unity Build in DEBUG Mode");
#else
        MojingLog.LogTrace("MojingSDK Unity Build in RELEASE Mode");
#endif
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        //UnityEditor.PlayerSettings.MTRendering = true;
        MojingLog.LogTrace("Enter MojingVRHead.Awake");
        gameObject.SetActive(ConfigItem.MojingSDKActive);
        sdk = Mojing.SDK;
        if (MojingSDK.cur_GlassKey == "")
        {
            //sdk.GlassesKey = Mojing.glassesKeyList[1];
            GetsdkGlassKey(GlassesType);
        }
        else
            sdk.GlassesKey = MojingSDK.cur_GlassKey;
        MojingLog.LogTrace("MojingSDK Glasses KEY: " + sdk.GlassesKey 
                            + ", Glasses FOV:" + MojingSDK.Unity_GetGlassesFOV() 
                            + ",MojingSDK Version: " + MojingSDK.GetSDKVersion());
        if (ConfigItem.ScreenNeverSleep)
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        else
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
        MojingLog.LogTrace("Leave MojingVRHead.Awake");
    }

    public void GetsdkGlassKey(GlassesTypes type)
    {
        string CurGlassesID = GetCurrentGlassesID(type);
        string[] ID = CurGlassesID.Split('/');
        sdk.GlassesKey = Mojing.FindGlassKey(int.Parse(ID[0]), int.Parse(ID[1]), int.Parse(ID[2]));
        MojingSDK.cur_GlassKey = sdk.GlassesKey;
    }

    void OnEnable()
    {
        // Disable all camera until we enter mojing world
        foreach (MojingEye eye in Eyes)
        {
            eye.EnableEye(false);
        }
    }


    // Which types of tracking this instance will use.
    public bool trackRotation = true;
    public bool trackPosition = true;

    // If set, the head transform will be relative to it.
    public Transform target;

    // Determine whether head updates early or late in frame.
    // Defaults to false in order to reduce latency.
    // Set this to true if you see jitter due to other scripts using this
    // object's orientation (or a child's) in their own LateUpdate() functions,
    // e.g. to cast rays.
    public bool updateEarly = false;

    // Where is this head looking?
    public Ray Gaze
    {
        get
        {
            UpdateHead();
            return new Ray(transform.position, transform.forward);
        }
    }

    private bool updated;

    void Update()
    {
        updated = false;  // OK to recompute head pose.
        if (updateEarly)
        {
            UpdateHead();
        }
    }


    // Normally, update head pose now.
    void LateUpdate()
    {
        UpdateHead();
    }

    //   Orientation--------------------------
    private void UpdateHead()
    {
        if (updated)
        {  // Only one update per frame, please.
            return;
        }
        updated = true;
        Mojing.SDK.UpdateState();

        if (trackRotation)
        {
            var rot = Mojing.SDK.headPose.Orientation;

            if (target == null)
            {
                transform.localRotation = rot;
            }
            else
            {
                transform.rotation = target.rotation * rot;
            }
        }
        if (trackPosition)
        {
            Vector3 pos = Mojing.SDK.headPose.Position;
            if (target == null)
            {
                transform.localPosition = pos;
            }
            else
            {
                transform.position = target.position + target.rotation * pos;
            }
        }
    }

    // Enumerate Glasses
    public enum GlassesTypes
    {
        MojingII,
        MojingIII,
        MojingIIIPlusB,
        MojingIIIPlusA,
        MojingIV,
        MojingMovie,
        MojingYoungD,
        MojingV,
        MojingRIO,
        MojingS1,
        深圳聚众创科技Vrbox,
        深圳聚众创科技Vrmini,
        深圳小宅科技z4,
        深圳小宅科技z3,
        北京小鸟看看科技PicolCop,//其余未列
    };

    public string GetCurrentGlassesID(GlassesTypes glassesType)
    {
        string CurGlassesID;
		try{
			switch (glassesType)
			{
			case GlassesTypes.MojingII:
                //sdk.GlassesKey = Mojing.glassesKeyList[0];
                CurGlassesID = Mojing.glassesIDList[0];
				break;
			case GlassesTypes.MojingIII:
                CurGlassesID = Mojing.glassesIDList[1];
				break;
			case GlassesTypes.MojingIIIPlusB:
                CurGlassesID = Mojing.glassesIDList[2];
				break;
			case GlassesTypes.MojingIIIPlusA:
                CurGlassesID = Mojing.glassesIDList[3];
				break;
			case GlassesTypes.MojingIV:
                CurGlassesID = Mojing.glassesIDList[4];
				break;
            case GlassesTypes.MojingMovie:
                CurGlassesID = Mojing.glassesIDList[5];
                break;
            case GlassesTypes.MojingYoungD:
                CurGlassesID = Mojing.glassesIDList[6];
                break;
            case GlassesTypes.MojingV:
                CurGlassesID = Mojing.glassesIDList[7];
                break;
            case GlassesTypes.MojingRIO:
                CurGlassesID = Mojing.glassesIDList[8];
                break;
            case GlassesTypes.MojingS1:
                CurGlassesID = Mojing.glassesIDList[9];
                break;
                case GlassesTypes.深圳聚众创科技Vrbox:
                CurGlassesID = Mojing.glassesIDList[10];
                break;
            case GlassesTypes.深圳聚众创科技Vrmini:
                CurGlassesID = Mojing.glassesIDList[11];
                break;
            case GlassesTypes.深圳小宅科技z4:
                CurGlassesID = Mojing.glassesIDList[12];
                break;
            case GlassesTypes.深圳小宅科技z3:
                CurGlassesID = Mojing.glassesIDList[13];
                break;
            case GlassesTypes.北京小鸟看看科技PicolCop:
                CurGlassesID = Mojing.glassesIDList[14];
                break;
            default:
                CurGlassesID = Mojing.glassesIDList[7];
				break;
			}
		}
		catch
		{
            CurGlassesID = Mojing.glassesIDList[0];
		}
        Debug.Log("MojingSDK Glasses ID: " + CurGlassesID);
        return CurGlassesID;
    }

}
