using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using LitJson;
using UnityEngine.UI;

public class DeviceInfo
{
    public int ID { get; set; }
    public string DeviceName { get; set; }
    public int Connect { get; set; }
}

public class DeviceJson
{
    public string ClassName { get; set; }

    public List<DeviceInfo> DeviceList = new List<DeviceInfo>();
    public string ERROR { get; set; }
}
public class Device
{
    public int id;
    public string name;
    public bool connected;          // 是否已经连接上
    public bool connectable;        // 是否可连接

    public Device()
    {

    }
    public Device(DeviceInfo info)
    {
        if(null != info)
        {
            id = info.ID;
            name = info.DeviceName;
            connectable = info.Connect != 0;
            connected = false;
        }            
    }
}
public class MojingController : MonoBehaviour {

    private AndroidJavaClass _javaClass;

    // 设备列表
    public List<Device> _deviceList = new List<Device>();
    /// <summary>
    /// 检查是否有设备在
    /// </summary>
    public bool HasDevice { get { return _deviceList.Count > 0; } }
    bool _javaInit = false;

    static int DevNum = 2;
    float[] _quartArray = new float[4];
    float[] _angularArray = new float[3];
    float[] _linearArray = new float[3];
    float[] _posArray = new float[3];
    uint[] _keyAray = new uint[256];

    public static Quaternion[] QuartArray = new Quaternion[DevNum];
    public static Vector3[] AngularArray = new Vector3[DevNum];
    public static Vector3[] LinearArray = new Vector3[DevNum];
    public static Quaternion[] FixQuate= new Quaternion[DevNum];
    // Use this for initialization
    void Start () 
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        _javaInit = this.initJavaClass();
#else
        _javaInit = true;
#endif
        _deviceList.Clear();
        /*
        // 测试数据，不从json获取，设备Id是1 和 2
        Device dev1 = new Device();
        dev1.id = 1;
        dev1.name = "major";
        dev1.connectable = true;
        dev1.connected = false;

        this._deviceList.Add(dev1);
        Device dev2 = new Device();
        dev2.id = 2;
        dev2.name = "minor";
        dev2.connectable = true;
        dev2.connected = false;        
        this._deviceList.Add(dev2);
        */
    }

    bool initJavaClass()
    {
        _javaClass = new AndroidJavaClass("com.baofeng.mojing.unity.MojingVrActivity");
        if (null == _javaClass)
            return false;
        return (_javaClass != null);
    }

    void Update()
    {
        if (!HasDevice)
        {
            queryDeviceList();
        }
        else
        {
            for (int id = 0; id < _deviceList.Count; id++)
            {
                updateController(_deviceList[id], id);
            }
        }
    }

    public void updateController(Device dev,int id)
    {
        // MojingLog.LogTrace("dev: " + dev.name+ ",  weapon:  "+ weapon.name);
        if (null != dev)
        {
            dev.connected = true;
            //MojingLog.LogTrace("dev.connected:  " + dev.connected);
            if (!dev.connected)
            { // 设备没有连上，先进行连接
                MojingLog.LogTrace("connectDevice: "+ connectDevice(dev.id)+ ",  dev.id: "+ dev.id);
                if (connectDevice(dev.id))
                {
                    dev.connected = true;
                }
            }
            else
            {   // 连接上了，更新设备位置
                //MojingSDK.Unity_Device_GetInfo(dev.id, _quartArray, _angularArray, _linearArray, _posArray, _keyAray);
                MojingSDK.Unity_Device_GetFixCurrentInfo(dev.id, _quartArray, _angularArray, _linearArray, _posArray, _keyAray);
                QuartArray[id] = new Quaternion(_quartArray[0], _quartArray[1], _quartArray[2], _quartArray[3]);
                AngularArray[id] = new Vector3(_angularArray[0], _angularArray[1], _angularArray[2]);
                LinearArray[id] = new Vector3(_linearArray[0], _linearArray[1], _linearArray[2]);
                //Debug.Log("ID: " + dev.id + "Quaternion:" + QuartArray[id]
                //        + "\n" + "angularArray:" + AngularArray[id]
                //        + "\n" + "angularArray:" + LinearArray[id]);
                MojingSDK.Unity_Device_GetFixInfo(dev.id, _quartArray, _angularArray, _linearArray, _posArray);
                FixQuate[id] = new Quaternion(_quartArray[0], _quartArray[1], _quartArray[2], _quartArray[3]);
            }
        }
    }

    public void queryDeviceList()
    {
        MojingLog.LogTrace("Enter queryDeviceList");
        if (!_javaInit)
            return;
//#if UNITY_ANDROID && !UNITY_EDITOR
//        string jsonStr = _javaClass.CallStatic<string>("getDeviceList");
//#else
        string jsonStr = "{ \"ClassName\":\"DeviceList\",\"DeviceList\":[{\"ID\":1,\"DeviceName\":\"AirMouse\",\"Connect\":1},{\"ID\":2,\"DeviceName\":\"AirMouse\",\"Connect\":1}]}";
//#endif
        MojingLog.LogTrace("json=" + jsonStr);
        DeviceJson devList = JsonMapper.ToObject<DeviceJson>(jsonStr);

        if (devList != null)
        {
            _deviceList.Clear();

            MojingLog.LogTrace("DeviceList count=" + devList.DeviceList.Count);

            for(int i=0; i<devList.DeviceList.Count; ++i)
            {
                Device dev = new Device(devList.DeviceList[i]);
                _deviceList.Add(dev);
                MojingLog.LogTrace("DeviceList " + i + "  ID: " + dev.id + ", name: " + dev.name + ", connectable: " + dev.connectable + ", connected: " + dev.connected);
            }
            DevNum = _deviceList.Count;
        }
    }

    /// <summary>
    /// 连接指定的设备
    /// </summary>
    /// <param name="devId"></param>
    bool connectDevice(int devId)
    {
        bool ret = true;
        if (!_javaInit)
            return false;
#if UNITY_ANDROID && !UNITY_EDITOR
        ret = _javaClass.CallStatic<bool>("connectDevice" , devId);
        return ret;
#else
        return ret;
#endif
    }

    bool disconnectDevice(int devId)
    {
        if (!_javaInit)
            return false;
#if UNITY_ANDROID && !UNITY_EDITOR
        return _javaClass.CallStatic<bool>("disconnectDevice" , devId);
#else
        return true;
#endif
    }
}
