//------------------------------------------------------------------------------
// Copyright 2016 Baofeng Mojing Inc. All rights reserved.
//------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MojingLoginPay : MonoBehaviour {
    void Awake()
    {
        MojingSDK.Unity_SetEngineVersion("Unity " + Application.unityVersion);
    }
    void Start () {
		//绑定事件
		ArrayList btnsName = new ArrayList();
		btnsName.Add ("SingleLogin");
		btnsName.Add ("DoubleLogin");
		btnsName.Add ("syncMjAppLoginState");
		btnsName.Add ("Register");
		btnsName.Add ("SinglePay");
		btnsName.Add ("DoublePay_1");
        btnsName.Add ("DoublePay_2");
        btnsName.Add ("GetBalance");
        /*
		foreach (string btnName in btnsName ) {
			GameObject btnObj = GameObject.Find(btnName);
			Button btn = btnObj.GetComponent<Button>();
			btn.onClick.AddListener(delegate() {    OnClick(btnObj);    });
		}
        */
        for (int i=0;i< btnsName.Count;i++)
        {
            GameObject btnObj = GameObject.Find(btnsName[i].ToString());
            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(delegate () { OnClick(btnObj); });
        }
	}

    AndroidJavaClass javaClass;
    AndroidJavaObject javaObject;
    void Init()
    {
        javaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        javaObject = javaClass.GetStatic<AndroidJavaObject>("currentActivity");
    }

    //单屏登录
    public void SingleLogin()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        Init();
		javaObject.CallStatic ("mjCallSingleLogin");
#endif
        Debug.Log("SingleLogin");
    }
    //双屏登录
    public void DoubleLogin()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        Init();
        javaObject.CallStatic("mjCallDoubleLogin");
#endif
        Debug.Log("DoubleLogin");
    }
    //同步App登录状态
    public void syncMjAppLoginState()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        Init();
        javaObject.CallStatic("syncMjAppLoginState");
#endif
    }
    //注册
    public void Register()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        Init();
        javaObject.CallStatic("mjCallRegister");
#endif
    }
    //单屏支付1魔币
    public void SinglePay()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        Init();
        javaObject.CallStatic("mjGetPayToken", "1.0", "clientOrder", "0");
#endif
    }
    public void DoublePay1()
	{
#if !UNITY_EDITOR && UNITY_ANDROID
		Init();
		javaObject.CallStatic("mjGetPayToken", "1.0", "clientOrder", "1");
#endif
    }
    public void DoublePay2()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
		Init();
		javaObject.CallStatic("mjGetPayToken", "1.0", "clientOrder", "2");
#endif
    }

    public void Pay(string token)
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        Init();
		javaObject.CallStatic("mjPayMobi", "1.00", "北京区服", token , "clientOrder");
#endif
    }
    public void GetBalance()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        Init();
	    javaObject.CallStatic("mjGetBalance");
#endif
    }
    //ButtonClickedEvent
    void OnClick(GameObject btnObj) {
		switch (btnObj.name) {
			case "SingleLogin": //单屏登录
                SingleLogin();
			break;

			case "DoubleLogin": //双屏登录
                DoubleLogin();
			break;

			case "syncMjAppLoginState": //同步App登录状态
                syncMjAppLoginState();
			break;

			case "Register": //注册
                Register();
			break;

			case "SinglePay": //获取支付Token(单屏)
				SinglePay();
			break;

			case "DoublePay_1": //获取支付Token(双屏-手柄)
				DoublePay1();
			break;

            case "DoublePay_2": //获取支付Token(双屏-头控)
                DoublePay2();
                break;

            case "GetBalance": //获取余额
				GetBalance();
			break;
		}

	}
}
