//------------------------------------------------------------------------------
// Copyright 2016 Baofeng Mojing Inc. All rights reserved.
// 
// Author: Xu Xiang
//------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using LitJson;
using System.Collections.Generic;

public class GlassInfo
{
    public string URL { get; set; }
    public int ID { get; set; }
    public string DURL { get; set; }
    public string Display { get; set; }
    public string KEY { get; set; }
}

public class Glasses
{
    public string ClassName { get; set; }
    public string ReleaseDate { get; set; }
    public List<GlassInfo> GlassList = new List<GlassInfo>();
    public string ERROR { get; set; }
}

public class ProductInfo
{
    public string URL { get; set; }
    public int ID { get; set; }
    public string Display { get; set; }
    public string KEY { get; set; }
    public Glasses GlassList = null;
}

public class Products
{
    public string ClassName { get; set; }
    public string ReleaseDate { get; set; }
    public List<ProductInfo> ProductList = new List<ProductInfo>();
    public string ERROR { get; set; }
}

public class ManufacturerInfo
{
    public string URL { get; set; }
    public int ID { get; set; }
    public string Display { get; set; }
    public string KEY { get; set; }
    public Products ProductList = null;
}

public class Manufacturers
{
    public string ClassName{get;set;}
    public string ReleaseDate { get; set; }
    public List<ManufacturerInfo> ManufacturerList = new List<ManufacturerInfo>();
    public string ERROR { get; set; }
}

public class MojingSDK
{
    // 以下是宏定义
    public static int TEXTURE_LEFT_EYE = 1;
    public static int TEXTURE_RIGHT_EYE = 2;
    public static int TEXTURE_BOTH_EYE = TEXTURE_LEFT_EYE | TEXTURE_RIGHT_EYE;
    public static int TEXTURE_IS_UNITY_METAL_RENDER_TEXTURE = 4;

	//Save Current GlassesKey
	public static string cur_GlassKey = "";
	private static float[] viewMatrix = new float[16];
    public static void GetLastHeadView(ref Matrix4x4 mat)
	{
		Unity_getLastHeadView(viewMatrix);
		
		int i = 0;
		for (int r = 0; r < 4; r++)
		{
			for (int c = 0; c < 4; c++, i++)
			{
				mat[r, c] = viewMatrix[i];
			}
		}
	}

#if UNITY_EDITOR_OSX

	public static bool Unity_Init(string szMerchantID, string szAppID, string szAppKey, string szChannelID)
	{
		return true;
	}
    public static bool Initialized = false;
	public static IEnumerator Unity_EnterMojingWorld(string sGlassesName, bool bTimeWarp, IntPtr pOtherParmams)
	{
        Initialized=true;
		yield return true;
	}
	public static bool Unity_SetMojingWorld(string sGlassedName, bool bTimeWarp, IntPtr pOtherParmams)
	{
		return true;
	}
	public static bool IsInMojingWorld(string sGlassedName)
	{
		return true;
	}
	
	public static bool ChangeMojingWorld(string sGlassesName)
	{
		return true;
	}
	
	private static string GetManufacturerList(string strLanguageCodeByISO963)
	{
        return "{\"ClassName\":\"ManufacturerList\",\"ReleaseDate\":\"20150618\",\"ManufacturerList\":[{\"URL\":\"www.mojing.com\",\"ID\":1,\"Display\":\"暴风魔镜\",\"KEY\":\"YZ9ZDF-4XWNFL-XG4BFT-CH9RA4-YYEXFF-DB2TAD\"}]}";
	}
    public static Manufacturers GetManufacturers(string strLanguageCodeByISO963)
    {
        try
        {
            string json = GetManufacturerList(strLanguageCodeByISO963);
            Manufacturers list = JsonMapper.ToObject<Manufacturers>(json);
            for (int i = 0; i < list.ManufacturerList.Count; i++)
            {
                list.ManufacturerList[i].ProductList = GetProducts(list.ManufacturerList[i].KEY, strLanguageCodeByISO963);
            }
            return list;
        }
        catch (Exception e)
        {
            MojingLog.LogTrace(e.ToString());
        }
        return null;
    }
	
	private static string GetProductList(string strManufacturerKey, string strLanguageCodeByISO963)
	{
        return "{\"ClassName\":\"ProductList\",\"ReleaseDate\":\"20150618\",\"ProductList\":[{\"URL\":\"http://www.mojing.cn/front/images/gallery/1.jpg\",\"ID\":1,\"Display\":\"暴风魔镜II\",\"KEY\":\"HG4SYV-CTYH8P-H3AEFR-C9H728-DQA4DD-S8SGXT\"},{\"URL\":\"http://www.mojing.cn/front/images/gallery/1.jpg\",\"ID\":2,\"Display\":\"暴风魔镜III\",\"KEY\":\"SFA8W8-Z8X3DF-AHQBWG-HF2X4S-F74FQQ-9R9YEB\"},{\"URL\":\"http://www.mojing.cn/front/images/gallery/1.jpg\",\"ID\":3,\"Display\":\"暴风魔镜IV\",\"KEY\":\"QTFB8L-DCW8FU-9R42A9-4YDSQ3-QR4X88-H74ZXQ\"},{\"URL\":\"http://www.mojing.cn/front/images/gallery/1.jpg\",\"ID\":5,\"Display\":\"观影镜\",\"KEY\":\"AD2GQH-FKH74X-SQWFC9-HT8MQG-Z5Y5ZL-FR8M98\"}]}";
	}
    public static Products GetProducts(string strManufacturerKey, string strLanguageCodeByISO963)
    {
        try
        {
            string json = GetProductList(strManufacturerKey, strLanguageCodeByISO963);
            Products list = JsonMapper.ToObject<Products>(json);
            for (int i=0;i< list.ProductList.Count;i++)
            {
                list.ProductList[i].GlassList = GetGlasses(list.ProductList[i].KEY, strLanguageCodeByISO963);
                for(int j=0; j < list.ProductList[i].GlassList.GlassList.Count; j++)
                {
                    if (list.ProductList[i].GlassList.GlassList[j].Display == null || list.ProductList[i].GlassList.GlassList[j].Display == "")
                    {
                        list.ProductList[i].GlassList.GlassList[j].Display = list.ProductList[i].Display;
                    }
                    else
                    {
                        list.ProductList[i].GlassList.GlassList[j].Display = list.ProductList[i].Display + " " + list.ProductList[i].GlassList.GlassList[j].Display;
                    }
                }
            }
            return list;
        }
        catch (Exception e)
        {
            MojingLog.LogTrace(e.ToString());
        }
        return null;
    }
	
	private static string GetGlassList(string strProductKey, string strLanguageCodeByISO963)
	{
		switch (strProductKey)
		{
		case "HG4SYV-CTYH8P-H3AEFR-C9H728-DQA4DD-S8SGXT":
			return "{\"ClassName\":\"GlassList\",\"ReleaseDate\":\"20150618\",\"GlassList\":[{\"URL\":\"UNKNOWN\",\"ID\":1,\"KEY\":\"CTX3X8-WZ4S2Y-2BEEQV-YVZ7XQ-QBCW8M-EEQY4H\"}]}";
			
		case "SFA8W8-Z8X3DF-AHQBWG-HF2X4S-F74FQQ-9R9YEB":
                return "{\"ClassName\":\"GlassList\",\"ReleaseDate\":\"20150618\",\"GlassList\":[{\"URL\":\"UNKNOWN\",\"ID\":3,\"KEY\":\"EFF5YN-CRX9Y3-WRXBYT-H54HWV-F7FK8N-YTE28M\"},{\"URL\":\"UNKNOWN\",\"ID\":4,\"Display\":\"Plus B镜片\",\"KEY\":\"Z3XXYU-SG4XWH-ESQ828-F7FF88-224BZR-8RHMWH\"},{\"URL\":\"UNKNOWN\",\"ID\":11,\"Display\":\"Plus A镜片\",\"KEY\":\"AWCBXV-86A8WZ-CFS2FN-SWDXDT-9YAWDH-S29VXW\"}]}";
		case "QTFB8L-DCW8FU-9R42A9-4YDSQ3-QR4X88-H74ZXQ":
                return "{\"ClassName\":\"GlassList\",\"ReleaseDate\":\"20150618\",\"GlassList\":[{\"URL\":\"UNKNOWN\",\"ID\":12,\"Display\":\"标准镜片(96)\",\"KEY\":\"HTSTHL-DYQ799-98DBYB-XNAF4Q-4YEQHF-Q7WHCF\"}]}";
		case "AD2GQH-FKH74X-SQWFC9-HT8MQG-Z5Y5ZL-FR8M98":
                return "{\"ClassName\":\"GlassList\",\"ReleaseDate\":\"20150618\",\"GlassList\":[{\"URL\":\"UNKNOWN\",\"ID\":12,\"Display\":\"观影镜\",\"KEY\":\"E2DSY3-4Z2H2T-ACSZEG-SF49HM-2TWZ2X-YZF585\"}]}";
		default:
			return "{\"ERROR\":\"Unknown Product Key\"}";
		}
	} 
	public static Glasses GetGlasses(string strProductKey, string strLanguageCodeByISO963)
    {
        try
        {
            string json = GetGlassList(strProductKey, strLanguageCodeByISO963);
            Glasses list = JsonMapper.ToObject<Glasses>(json);
            return list;
        }
        catch (Exception e)
        {
            MojingLog.LogTrace(e.ToString());
        }
        return null;
    }

	public static void Unity_DistortFrame()
	{
		return;
	}
	
	public static bool Unity_IsGlassesNeedDistortion()
	{
		return true;
	}
	
	public static bool Unity_IsGlassesNeedDistortionByName([MarshalAs(UnmanagedType.LPStr)]string szGlassesName)
	{
		return true;
	}
	
	public static bool Unity_LeaveMojingWorld()
	{
		return true;
	}
	
	public static bool Unity_StartTracker(int nSampleFrequence)
	{
		return true;
	}
	
	public static bool Unity_StartTrackerCalibration()
	{
		return true;
	}
	
	public static float Unity_IsTrackerCalibrated()
	{
		return 1;
	}
	
	public static void Unity_StopTracker()
	{
	}
	public static void Unity_getLastHeadView(float[] viewMatrix)
	{
		for (int i=0; i<16; i++)
		{
			viewMatrix[i] = 0;
		}
		viewMatrix[0] = viewMatrix[5] = viewMatrix[10] = viewMatrix[15] = 1.0f;
	}
	
	public static int Unity_GetTextureSize()
	{
		return Screen.width / 2;
	}
	
	public static float Unity_GetGlassesFOV()
	{
		return 60.0f;
	}
	
	public static float Unity_GetGlassesSeparation()
	{
		return 0.063f;
	}
	
	public static int Unity_GetGlassesSeparationInPixel()
	{
		return Screen.width / 2;
	}
	
	public static void Unity_GetProjectionMatrix(int eye, bool bVrMode, float fFOV, float fNear, float fFar, float[] pfProjectionMatrix, float[] pfViewRect)
	{
		
	}
	public static bool Unity_OnSurfaceChanged(int newWidth, int newHeight)
	{
		return false;
	}
	
	public static void Unity_GetScreenSize(ref float fWidth, ref float fHeight)
	{
		// 5.5 inch
		fWidth = 4.4f * 0.0254f;
		fHeight = 3.3f * 0.0254f;
	}
	
	public static void SetTextureID(IntPtr left_textureID,  IntPtr right_textureID)
	{
	}
	
	public static void Unity_ResetSensorOrientation()
	{
	}
	
	public static void Unity_ResetTracker()
	{
	}
	
	public static void SetCenterLine()
	{ 
	}
	public static void SetCenterLine(int iWidth, Color color)
	{
	}
	
	public static void Unity_SetGamePadAxisMode(int mode)
	{ 
	}
	
	private static string getVersion;
	public static string GetSDKVersion()
	{
		return getVersion;
	}
	
	public static bool Unity_SetEngineVersion(string lpszEngine)
	{
		return true;
	}
	
	public static void Unity_AppPageStart(string szPageName)
	{ 
	}
	public static void Unity_AppPageEnd(string szPageName)
	{
	}
	
	public static void Unity_AppSetEvent(string szEventName, string szEventChannelID, string szEventInName, float eInData, string szEventOutName, float eOutData)
	{ 
	}
	
	public static void Unity_AppResume()
	{ 
	}
	
	public static void Unity_AppPause()
	{ 
	}
	public static void Unity_AppReportLog(int iLogType, string szTypeName, string szLogContent)
	{
	}
	
	public static void Unity_SetOverlay(IntPtr iLeftOverlayTextureID, IntPtr iRightOverlayTextureID, float fLeft, float fTop, float fWidth, float fHeight)
	{ 
	}
	public static void Unity_SetOverlay3D(int iEyeType, IntPtr iTextureID, float fWidth, float fHeight, float fDistanceInMetre)
	{
	}

    public struct backerInfo
    {
        public uint texid;
        public uint x;
        public uint y;
        public uint width;
        public uint height;
        public uint desX;
        public uint desY;
    };

    public static bool Unity_GetBackerTexID(ref int texid)
    {
        return false;
    }
    public static void Unity_SetBackerTexture(int texid, int x, int y, int width, int height, int desX, int desY)
    {
    }
    public static void Unity_StartBacker(int width, int height, backerInfo[] info, int infolen)
    {
    }
    public static void Unity_SetOverlay3D_V2(int iEyeType, IntPtr iTextureID, float fLeft, float fTop, float fWidth, float fHeight, float fDistanceInMetre)
    {
    }
    public static void Unity_SetEnableTimeWarp(bool enable)
    {
    }
    public static void DestroyMojingWorld()
    {
    }
    public static float Unity_Device_GetInfo(int iID, float[] pQuart, float[] pAngularAccel, float[] pLinearAccel, float[] pPosition, uint[] pKeystatus)
    {
        return Time.time;
    }
    
    public static float Unity_Device_GetFixInfo(int iID, float[] pQuart, float[] pAngularAccel, float[] pLinearAccel, float[] pPosition)
    {
        return Time.time;
    }

    public static float Unity_Device_GetFixCurrentInfo(int iID, float[] pQuart, float[] pAngularAccel, float[] pLinearAccel, float[] pPosition, uint[] pKeystatus)
    {
        return Time.time;
    }
    public static float Unity_GetDistortionR()
    {
        return -1.0f;
    }

#else
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
    private const string dllName = "libmojing";   
    private static void Unity_getLastHeadView(float[] viewMatrix)
    {
        for (int i=0; i<16; i++)
        {
           viewMatrix[i] = 0;
        }
        viewMatrix[0] = viewMatrix[5] = viewMatrix[10] = viewMatrix[15] = 1.0f;
    }
    public static bool Unity_ktxLoadTextureN(string filename, ref int pTexture, ref int pTarget, ref int width, ref int height, ref int depth, ref bool pIsMipmapped, ref int pGlerror, ref int ktxError)
    {
        return false;
    }
    public static bool Unity_IsLowPower()
    {
        return false;
    }
 
    public static float Unity_Device_GetInfo(int iID, float[] pQuart, float[] pAngularAccel, float[] pLinearAccel, float[] pPosition, uint[] pKeystatus)
    {
        if (null != pQuart && pQuart.Length == 4)
        {
            pQuart[0] = 25f;
            pQuart[1] = 0f;
            pQuart[2] = 5f;
            pQuart[3] = -1f;
        }
        return Time.time;
    }

    public static float Unity_Device_GetFixInfo(int iID, float[] pQuart, float[] pAngularAccel, float[] pLinearAccel, float[] pPosition)
    {
        if (null != pQuart && pQuart.Length == 4)
        {
            pQuart[0] = 25f;
            pQuart[1] = 0f;
            pQuart[2] = 5f;
            pQuart[3] = -1f;
        }
        return Time.time;
    }


    public static float Unity_Device_GetFixCurrentInfo(int iID, float[] pQuart, float[] pAngularAccel, float[] pLinearAccel, float[] pPosition, uint[] pKeystatus)
    {
        if (null != pQuart && pQuart.Length == 4)
        {
            pQuart[0] = 25f;
            pQuart[1] = 0f;
            pQuart[2] = 5f;
            pQuart[3] = -1f;
        }
        return Time.time;
    }
#elif UNITY_ANDROID
	private const string dllName = "mojing";
        [DllImport(dllName)]
	public static extern void Unity_getLastHeadView(float[] viewMatrix);
    
    [DllImport(dllName)]
	public static extern bool Unity_ktxLoadTextureN([MarshalAs(UnmanagedType.LPStr)]string filename, ref int pTexture, ref int pTarget, ref int width, ref int height, ref int depth, ref bool pIsMipmapped, ref int pGlerror, ref int ktxError);

    [DllImport(dllName)]
	public static extern bool Unity_IsLowPower();

    [DllImport(dllName)]
    public static extern float Unity_Device_GetInfo(int iID, float[] pQuart, float[] pAngularAccel, float[] pLinearAccel, float[] pPosition, uint[] pKeystatus);

    [DllImport(dllName)]
    public static extern float Unity_Device_GetFixInfo(int iID, float[] pQuart, float[] pAngularAccel, float[] pLinearAccel, float[] pPosition);

    [DllImport(dllName)]
    public static extern float Unity_Device_GetFixCurrentInfo(int iID, float[] pQuart, float[] pAngularAccel, float[] pLinearAccel, float[] pPosition, uint[] pKeystatus);


#elif UNITY_IOS
	private const string dllName = "__Internal";
	[DllImport(dllName)]
	public static extern void Unity_getLastHeadView(float[] viewMatrix);
    [DllImport(dllName)]
    public static extern void Unity_AppResume();
    [DllImport(dllName)]
    public static extern void Unity_AppPause();
    [DllImport(dllName)]
	public static extern bool Unity_ktxLoadTextureN([MarshalAs(UnmanagedType.LPStr)]string filename, ref int pTexture, ref int pTarget, ref int width, ref int height, ref int depth, ref bool pIsMipmapped, ref int pGlerror, ref int ktxError);
    public static float Unity_Device_GetInfo(int iID, float[] pQuart, float[] pAngularAccel, float[] pLinearAccel, float[] pPosition, uint[] pKeystatus)
    {
        return Time.time;
    }
    public static float Unity_Device_GetFixInfo(int iID, float[] pQuart, float[] pAngularAccel, float[] pLinearAccel, float[] pPosition)    
    {
        return Time.time;
    }

    public static float Unity_Device_GetFixCurrentInfo(int iID, float[] pQuart, float[] pAngularAccel, float[] pLinearAccel, float[] pPosition, uint[] pKeystatus)    
    {
        return Time.time;
    }
#endif

    [DllImport(dllName)]
    public static extern bool Unity_Init([MarshalAs(UnmanagedType.LPStr)]string szEngineVersion, [MarshalAs(UnmanagedType.LPStr)]string szMerchantID, [MarshalAs(UnmanagedType.LPStr)]string szAppID, [MarshalAs(UnmanagedType.LPStr)]string szAppKey, [MarshalAs(UnmanagedType.LPStr)]string szChannelID, int nWidth, int nHeight, float xdpi, float ydpi, [MarshalAs(UnmanagedType.LPStr)]string szProfilePath, IntPtr pOtherParmams);


    [DllImport(dllName)]
    private static extern IntPtr Unity_GetSDKVersion();
    public static string GetSDKVersion()
    {
        IntPtr ptr = Unity_GetSDKVersion();
        string result = Marshal.PtrToStringAnsi(ptr);
        Unity_FreeString(ptr);
        return result;
    }
	
	enum UnityEventID
	{
		EnterMojingWorld,
		ChangeMojingWorld,
		LeaveMojingWorld,
        DestroyMojingWorld,
		DistortFrame,
		SetTextureID,
        SetCenterLine,
        BackerTexture
	};


    [DllImport(dllName)]
	private static extern bool Unity_SetMojingWorld([MarshalAs(UnmanagedType.LPStr)]string sGlassesName, bool bTimeWarp, IntPtr pOtherParmams);
    public static bool Initialized = false;
    public static bool IsInMojingWorld(string glassesKey)
    {
        MojingLog.LogTrace("IsInMojingWorld:" + Unity_IsInMojingWorld(glassesKey));
        if (Unity_IsInMojingWorld(glassesKey))
        {
            Initialized = true;
            return true;
        }
        else
        {
            return false;
        }
    }

    [DllImport(dllName)]
	private static extern bool Unity_IsInMojingWorld([MarshalAs(UnmanagedType.LPStr)]string sGlassesName);

    [DllImport(dllName)]
	public static extern bool Unity_ChangeMojingWorld([MarshalAs(UnmanagedType.LPStr)]string sGlassesName);

    [DllImport(dllName)]
    private static extern IntPtr Unity_GetRenderEventFunc();
    private static IntPtr _MojingRenderEvent = IntPtr.Zero;
    private static void Unity_IssuePluginEvent(int iEventID)
    {
        _MojingRenderEvent = Unity_GetRenderEventFunc();
            GL.IssuePluginEvent(_MojingRenderEvent, iEventID);
    }

    public static void DestroyMojingWorld()
    {
        Unity_IssuePluginEvent((int)UnityEventID.DestroyMojingWorld);
    }

    [DllImport(dllName)]
    private static extern bool Unity_CanEnterMojingWorld();
    public static IEnumerator Unity_EnterMojingWorld(string sGlassesName, bool bTimeWarp, IntPtr pOtherParmams)
    {
        MojingLog.LogTrace("---> Unity_EnterMojingWorld");
        if (Unity_SetMojingWorld(sGlassesName, bTimeWarp, pOtherParmams))
        {
            yield return new WaitUntil(() => Unity_CanEnterMojingWorld() == true);
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGL2 || SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore)
                Unity_IssuePluginEvent((int)UnityEventID.EnterMojingWorld);
#else
            Unity_IssuePluginEvent((int)UnityEventID.EnterMojingWorld);
#endif  
            MojingLog.LogTrace("---> Unity_SetMojingWorld ok");
        }
    }

	public static bool ChangeMojingWorld(string sGlassesName)
    {
        if (Unity_ChangeMojingWorld(sGlassesName))
        {
            Unity_IssuePluginEvent((int)UnityEventID.ChangeMojingWorld);
            return true;
        }
        return false;
    }

    public static void Unity_DistortFrame()
    {
        Unity_IssuePluginEvent((int)UnityEventID.DistortFrame);
        GL.InvalidateState();
    }

	public static bool Unity_LeaveMojingWorld()
    {
        Unity_IssuePluginEvent((int)UnityEventID.LeaveMojingWorld);
        return true;
    }

    [DllImport(dllName)]
    private static extern void Unity_FreeString(IntPtr pReleaseString);

	private static string PtrToString(IntPtr ptr)
	{
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
		return Marshal.PtrToStringAnsi(ptr);
#else
		return Marshal.PtrToStringAuto(ptr);
#endif
	}

    [DllImport(dllName)]
	private static extern IntPtr Unity_GetManufacturerList([MarshalAs(UnmanagedType.LPStr)]string strLanguageCodeByISO963);
    private static string GetManufacturerList(string strLanguageCodeByISO963)
    {
        IntPtr ptr = Unity_GetManufacturerList(strLanguageCodeByISO963);
        string result = PtrToString(ptr);
        Unity_FreeString(ptr);
        //MojingLog.LogTrace("Unity_GetManufacturerList: " + result);
        return result;
    }
    public static Manufacturers GetManufacturers(string strLanguageCodeByISO963)
    {
        try
        {
            string json = GetManufacturerList(strLanguageCodeByISO963);
            Manufacturers list = JsonMapper.ToObject<Manufacturers>(json);

            for (int i = 0; i < list.ManufacturerList.Count; i++)
            {
                //MojingLog.LogTrace("To get product list of " + list.ManufacturerList[i].Display);
                list.ManufacturerList[i].ProductList = GetProducts(list.ManufacturerList[i].KEY, strLanguageCodeByISO963);
            }
            return list;
        }
        catch (Exception e)
        {
            MojingLog.LogTrace(e.ToString());
        }
        return null;
    }

    [DllImport(dllName)]
    private static extern IntPtr Unity_GetProductList([MarshalAs(UnmanagedType.LPStr)]string strManufacturerKey, [MarshalAs(UnmanagedType.LPStr)]string strLanguageCodeByISO963);
    private static string GetProductList(string strManufacturerKey, string strLanguageCodeByISO963)
    {
        IntPtr ptr = Unity_GetProductList(strManufacturerKey, strLanguageCodeByISO963);
        string result = PtrToString(ptr);
        Unity_FreeString(ptr);
        //MojingLog.LogTrace("Unity_GetManufacturerList: " + result);
        return result;
    }
    public static Products GetProducts(string strManufacturerKey, string strLanguageCodeByISO963)
    {
        try
        {
            string json = GetProductList(strManufacturerKey, strLanguageCodeByISO963);
            Products list = JsonMapper.ToObject<Products>(json);

            for (int i=0;i< list.ProductList.Count;i++)
            {
                list.ProductList[i].GlassList = GetGlasses(list.ProductList[i].KEY, strLanguageCodeByISO963);
                for(int j=0; j < list.ProductList[i].GlassList.GlassList.Count; j++)
                {
                    if (list.ProductList[i].GlassList.GlassList[j].Display == null || list.ProductList[i].GlassList.GlassList[j].Display == "")
                    {
                        list.ProductList[i].GlassList.GlassList[j].Display = list.ProductList[i].Display;
                    }
                    else
                    {
                        list.ProductList[i].GlassList.GlassList[j].Display = list.ProductList[i].Display + " " + list.ProductList[i].GlassList.GlassList[j].Display;
                        //MojingLog.LogTrace("To get glass list of " + list.ProductList[i].GlassList.GlassList[j].Display);
                    }
                }
            }
            return list;
        }
        catch (Exception e)
        {
            MojingLog.LogTrace(e.ToString());
        }
        return null;
    }

    [DllImport(dllName)]
    private static extern IntPtr Unity_GetGlassList([MarshalAs(UnmanagedType.LPStr)]string strProductKey, [MarshalAs(UnmanagedType.LPStr)]string strLanguageCodeByISO963);
    private static string GetGlassList(string strProductKey, string strLanguageCodeByISO963)
    {
        IntPtr ptr = Unity_GetGlassList(strProductKey, strLanguageCodeByISO963);
        string result = PtrToString(ptr);
        Unity_FreeString(ptr);
        //MojingLog.LogTrace("Unity_GetGlassList: " + result);
        return result;
    }
    public static Glasses GetGlasses(string strProductKey, string strLanguageCodeByISO963)
    {
        try
    {
            string json = GetGlassList(strProductKey, strLanguageCodeByISO963);
            Glasses list = JsonMapper.ToObject<Glasses>(json);
            return list;
    }
        catch (Exception e)
    {
            MojingLog.LogTrace(e.ToString());
        }
        return null;
    }

    [DllImport(dllName)]
    private static extern IntPtr Unity_GetGlassInfo([MarshalAs(UnmanagedType.LPStr)]string strGlassKey, [MarshalAs(UnmanagedType.LPStr)]string strLanguageCodeByISO963);
    public static string GetGlassInfo(string strGlassKey, string strLanguageCodeByISO963)
    {
        IntPtr ptr = Unity_GetGlassInfo(strGlassKey, strLanguageCodeByISO963);
        string result = PtrToString(ptr);
        Unity_FreeString(ptr);
        //MojingLog.LogTrace("Unity_GetGlassInfo: " + result);
        return result;
    }

    [DllImport(dllName)]
    private static extern IntPtr Unity_GenerationGlassKey([MarshalAs(UnmanagedType.LPStr)]string strProductQRCode, [MarshalAs(UnmanagedType.LPStr)]string strGlassQRCode);
    public static string GetGlassKey(string strProductQRCode, string strGlassQRCode)
    {
        IntPtr ptr = Unity_GenerationGlassKey(strProductQRCode, strGlassQRCode);
        string result = PtrToString(ptr);
        Unity_FreeString(ptr);
        //MojingLog.LogTrace("Unity_GenerationGlassKey: " + result);
        return result;
    }

    [DllImport(dllName)]
	public static extern bool Unity_IsGlassesNeedDistortion();

    [DllImport(dllName)]
    public static extern bool Unity_IsGlassesNeedDistortionByName([MarshalAs(UnmanagedType.LPStr)]string szGlassesName);

    [DllImport(dllName)]
	public static extern bool Unity_StartTracker(int nSampleFrequence);
	
    [DllImport(dllName)]
	public static extern int Unity_StartTrackerCalibration();

    [DllImport(dllName)]
    public static extern float Unity_IsTrackerCalibrated();

	[DllImport(dllName)]
	public static extern void Unity_StopTracker();
	
	[DllImport(dllName)]
	public static extern int Unity_GetTextureSize();

    [DllImport(dllName)]
	public static extern float Unity_GetGlassesFOV();

    [DllImport(dllName)]
    public static extern float Unity_GetGlassesSeparation();

    [DllImport(dllName)]
    public static extern float Unity_GetDistortionR();

    [DllImport(dllName)]
    public static extern int Unity_GetGlassesSeparationInPixel();

    [DllImport(dllName)]
    public static extern void Unity_GetProjectionMatrix(int eye, bool bVrMode, float fFOV, float fNear, float fFar, float[] pfProjectionMatrix, float[] pfViewRect);

	[DllImport(dllName)]
	public static extern bool Unity_OnSurfaceChanged(int newWidth, int newHeight);

    [DllImport(dllName)]
    private static extern void Unity_GetScreenSize(float[] pSize);
    public static void Unity_GetScreenSize(ref float fWidth, ref float fHeight)
    {
        float[] fSize = { 0.0f, 0.0f };
        Unity_GetScreenSize(fSize);
        fWidth = fSize[0];
        fHeight = fSize[1];
    }

	[DllImport(dllName)]
	private static extern void Unity_SetTextureID(IntPtr iLeftTextureID, IntPtr iRightTextureID);

    public static void SetTextureID(IntPtr left_textureID, IntPtr right_textureID)
    {
        try
        {
            Unity_SetTextureID(left_textureID,right_textureID);
            Unity_IssuePluginEvent((int)UnityEventID.SetTextureID);
        }
        catch (Exception e)
        {
            MojingLog.LogError(e.ToString());
        }
    }
    
    [DllImport(dllName)]
	public static extern void Unity_ResetSensorOrientation();

	[DllImport(dllName)]
	public static extern void Unity_ResetTracker();

	[DllImport(dllName)]
	private static extern void Unity_SetCenterLine(int iWdith, int iColR , int iColG , int iColB , int iColA ); 

    static Color _CenterLineColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    static int _CenterLineWidth = 4;
    public static void SetCenterLine()
    {
       Unity_SetCenterLine(_CenterLineWidth, (int)(_CenterLineColor.r * 255),  (int)(_CenterLineColor.g * 255), (int)(_CenterLineColor.b * 255), (int)(_CenterLineColor.a * 255));
        Unity_IssuePluginEvent((int)UnityEventID.SetCenterLine);
    }

    public static void SetCenterLine(int iWidth, Color color)
    {
        _CenterLineColor = color;
        _CenterLineWidth = iWidth;
        SetCenterLine();
    }
    
    [DllImport(dllName)]
	public static extern void Unity_SetGamePadAxisMode(int mode);

    [DllImport(dllName)]
    public static extern bool Unity_SetEngineVersion([MarshalAs(UnmanagedType.LPStr)]string lpszEngine);
    [DllImport(dllName)]
    public static extern void Unity_AppPageStart([MarshalAs(UnmanagedType.LPStr)]string szPageName);
    [DllImport(dllName)]
    public static extern void Unity_AppPageEnd([MarshalAs(UnmanagedType.LPStr)]string szPageName);
    [DllImport(dllName)]
    public static extern void Unity_AppSetEvent([MarshalAs(UnmanagedType.LPStr)]string szEventName, [MarshalAs(UnmanagedType.LPStr)]string szEventChannelID, [MarshalAs(UnmanagedType.LPStr)]string szEventInName, float eInData, [MarshalAs(UnmanagedType.LPStr)]string szEventOutName, float eOutData);
    [DllImport(dllName)]
    public static extern void Unity_AppReportLog(int iLogType, [MarshalAs(UnmanagedType.LPStr)]string szTypeName, [MarshalAs(UnmanagedType.LPStr)]string szLogContent);
    [DllImport(dllName)]
    public static extern void Unity_SetOverlay(IntPtr iLeftOverlayTextureID, IntPtr iRightOverlayTextureID, float fLeft, float fTop, float fWidth, float fHeight);
    [DllImport(dllName)]
    public static extern void Unity_SetOverlay3D(int iEyeType, IntPtr iTextureID, float fWidth, float fHeight, float fDistanceInMetre);
    [DllImport(dllName)]
    public static extern void Unity_SetOverlay3D_V2(int iEyeType, IntPtr iTextureID, float fLeft, float fTop, float fWidth, float fHeight, float fDistanceInMetre);
    [DllImport(dllName)]
    public static extern void Unity_SetEnableTimeWarp(bool enable);

    /*以下是为了TextureBacker而设立的接口*/
    [StructLayout(LayoutKind.Sequential)]
    public struct backerInfo
    {
        [MarshalAs(UnmanagedType.U4)]
        public uint texid;
        [MarshalAs(UnmanagedType.U4)]
        public uint x;
        [MarshalAs(UnmanagedType.U4)]
        public uint y;
        [MarshalAs(UnmanagedType.U4)]
        public uint width;
        [MarshalAs(UnmanagedType.U4)]
        public uint height;
        [MarshalAs(UnmanagedType.U4)]
        public uint desX;
        [MarshalAs(UnmanagedType.U4)]
        public uint desY;
    };

    //[DllImport(dllName)]
	//public static extern bool Unity_ktxLoadTextureN_1([MarshalAs(UnmanagedType.LPStr)]string filename, ref int pTexture, ref int pTarget, bool[] pIsMipmapped, ref int pGlerror, ref int pKvdLen);

    [DllImport(dllName)]
    public static extern bool Unity_GetBackerTexID(ref int texid);

    [DllImport(dllName)]
    public static extern void Unity_SetBackerTexture(int texid, int x, int y, int width, int height, int desX, int desY);

    [DllImport(dllName)]
    private static extern void Unity_SetStartBacker(int width, int height, backerInfo[] info, int infolen);

    public static void Unity_StartBacker(int width, int height, backerInfo[] info, int infolen)
    {
        MojingLog.LogTrace("Unity_StartBacker_Unity_StartBacker");
        Unity_SetStartBacker(width, height, info, infolen);
        try
        {
            Unity_IssuePluginEvent((int)UnityEventID.BackerTexture);
        }
        catch (Exception e)
        {
            MojingLog.LogError(e.ToString());
        }
    }
    /*以上是为了TextureBacker而设立的接口*/

    [DllImport(dllName)]
    private static extern IntPtr Unity_GetUserSettings();
    public static string GetUserSettings()
    {
        IntPtr ptr =  Unity_GetUserSettings();
        string result = PtrToString(ptr);
        Unity_FreeString(ptr);
        //MojingLog.LogTrace("Unity_GenerationGlassKey: " + result);
        return result;
    }

    [DllImport(dllName)]
    public static extern bool Unity_SetUserSettings([MarshalAs(UnmanagedType.LPStr)]string sUserSettings);

	[DllImport(dllName)]
	private static extern IntPtr Unity_GetLastMojingWorld([MarshalAs(UnmanagedType.LPStr)]string strLanguageCodeByISO963);
	public static string GetLastMojingWorld(string strLanguageCodeByISO963)
	{
		IntPtr ptr = Unity_GetLastMojingWorld(strLanguageCodeByISO963);
        string result = PtrToString(ptr);
        Unity_FreeString(ptr);
        //MojingLog.LogTrace("Unity_GenerationGlassKey: " + result);
        return result;
    }
	
#endif
}
