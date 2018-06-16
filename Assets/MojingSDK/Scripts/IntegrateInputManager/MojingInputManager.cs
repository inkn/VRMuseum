//------------------------------------------------------------------------------
// Copyright 2016 Baofeng Mojing Inc. All rights reserved.
//------------------------------------------------------------------------------
using System;
using UnityEngine;

namespace MojingSample.CrossPlatformInput.MojingInput
{
    public class MojingInputManager : MonoBehaviour
    {
        public static MojingInputManager Instance;
        [HideInInspector]
        public string state_down = "";
        [HideInInspector]
        public string state_up = "";
        [HideInInspector]
        public string state_long_down = "";

        private string[] field;
        private int field_num;
        private string current_key;
        private string[] key_field;
        [HideInInspector]
        public bool isTouchDown = false;
        [HideInInspector]
        public string current_axis;
        [HideInInspector]
        public string device_name_attach;
        [HideInInspector]
        public string device_name_detach;

        //------------------
        public string[] buttons = new string[18]{
                "OK",// = 66,
				"C",// = 4,
				"MENU",// = 82,

                //"TranslationType": 1---键模式
                "UP",// = 19
                "DOWN",// = 20
                "LEFT",// = 21 
                "RIGHT",// = 22
                "CENTER",// = 23

                "VOLUME_UP",// = 24
                "VOLUME_DOWN",// = 25

                "A",// = 96
                "B",// = 97
                "X",// = 99
                "Y",// = 100

                "BUTTON_L1",
                "BUTTON_R1",
                "BUTTON_L2",
                "BUTTON_R2",
            };

        //"TranslationType": 0---轴模式
        public string[] axes = new string[2] { "Horizontal", "Vertical" };

        [NonSerialized]
        public int numAxes, numButtons;
        [NonSerialized]
        public CrossPlatformInputManager.VirtualAxis[] _aHandles;
        [NonSerialized]
        public CrossPlatformInputManager.VirtualButton[] _bHandles;
        protected int m_Buttons, m_ButtonsPrev;

        //----------------- 
        public static Action<bool> DeviceAttachedCallback;
        public enum AttachState
        {
            Connected,
            Disconnected,
        };
        [HideInInspector]
        public AttachState attachstate = AttachState.Disconnected;

        public class Key
        {
            public enum KeyState
            {
                KEY_NOTHING,
                KEY_DOWN,
                KEY_UP
            }
            public KeyState keyState = KeyState.KEY_NOTHING;
            public bool IsKeyDown()
            {
                bool res = keyState == KeyState.KEY_DOWN;
                if (res)
                    keyState = KeyState.KEY_NOTHING;
                return res;
            }
            public bool IsKeyUp()
            {
                bool res = keyState == KeyState.KEY_UP;
                if (res)
                    keyState = KeyState.KEY_NOTHING;
                return res;
            }
        }
        public Key touchKey = new Key();

        //通过接收的串获取键值
        protected void getKeyCode(string CurrentBtn)
        {
            field = CurrentBtn.Split('/');
            field_num = CurrentBtn.Split('/').Length;
            if (field_num == 3)
                current_axis = field[field_num - 2];
            current_key = field[field_num - 1];
            if (current_key.Contains(","))
            {
                key_field = current_key.Split(',');
            }

        }
        
        void Awake()
        {
            Instance = this;
            Loom loom=Loom.Current;
        }

        void Start()
        {
            int i;
            numAxes = i = axes.Length;
            _aHandles = new CrossPlatformInputManager.VirtualAxis[numAxes];
            for (i = 0; i < numAxes; ++i)
            {
                _aHandles[i] = CrossPlatformInputManager.VirtualAxisReference(this, axes[i], true);
            }

            numButtons = i = buttons.Length;
            _bHandles = new CrossPlatformInputManager.VirtualButton[i];
            for (i = 0; i < numButtons; ++i)
            {
                _bHandles[i] = CrossPlatformInputManager.VirtualButtonReference(this, buttons[i], true);
            }
            m_ButtonsPrev = m_Buttons = 0;

            /*-----Joystick CallBack Mode .Android Platform-----*/
#if !UNITY_EDITOR && UNITY_ANDROID
            AndroidJavaClass player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = player.GetStatic<AndroidJavaObject>("currentActivity");
            CInputListenerCallBack AdListenerCB = new CInputListenerCallBack();
            activity.CallStatic("SetListenerCB", AdListenerCB);
#endif
            DeviceAttachedCallback += onDeviceAttachedCallback;
        }

        private void onDeviceAttachedCallback(bool obj)
        {
            if (obj)
                Debug.Log("设备名称： " + device_name_attach + " connected");
            else
                Debug.Log("设备名称： " + device_name_detach + " disconnected");
        }
        /*
        void OnGUI()
        {
            GUI.skin.label.fontSize = 30;
            GUI.color = Color.green;
            if (attachstate == AttachState.Connected)
                GUILayout.Label(device_name_attach + " connected", GUILayout.Width(1000));
            else if (attachstate == AttachState.Disconnected)
                GUILayout.Label(device_name_detach + " disconnected", GUILayout.Width(1000));
            GUILayout.Label(state_down + "\n" + state_up + "\n" + state_long_down);
            GUILayout.Label("Joystick_轴:" + current_axis + "      , 值:"+current_key);
        }
        */


#if !UNITY_EDITOR && UNITY_ANDROID
        public class CInputListenerCallBack : AndroidJavaProxy
        {
            public CInputListenerCallBack() : base("com.baofeng.mojing.unity.inputinterface.InputListenerCallBack")
            {
            }


            void onTouchPadStatusChange(string info)
            {
            Loom.QueueOnMainThread(() => {
                Debug.Log("----onTouchPadStatusChange:" + info);
                Instance.getKeyCode(info);
                if (Instance.current_key == "true")
                {
                    Instance.isTouchDown = true;
                }
                else
                {
                    Instance.isTouchDown = false;
                }
            });
            }

            void onTouchPadPos(string info)
            {
            Loom.QueueOnMainThread(() => {
                Debug.Log("----onTouchPadPos:" + info);
                Instance.getKeyCode(info);
                if (Instance.isTouchDown)
                {
                    //Debug.Log("----isTouchDown:" + Instance.isTouchDown);
                    Instance._aHandles[0].Update(float.Parse(Instance.key_field[0]));
                    Instance._aHandles[1].Update(float.Parse(Instance.key_field[1]));
                }
                //else
                //{
                //    Debug.Log("----isTouchUp:" + Instance.isTouchDown);
                //    //Instance._aHandles[0].Update(0);
                //    //Instance._aHandles[1].Update(0);
                //}
            });
            }

            //void onTouchPadPosTest(string info)
            //{
            //Loom.QueueOnMainThread(() => {
                //Debug.Log("----onTouchPadPosTest: " + info);
            //    Instance.getKeyCode(info);
                //Debug.Log("----isTouchDown:" + Instance.isTouchDown);
            //    if (Instance.isTouchDown)
            //    {
            //        Instance._aHandles[0].Update(int.Parse(Instance.key_field[0]));
            //        Instance._aHandles[1].Update(int.Parse(Instance.key_field[1]));
            //    }
            //    else
            //    {
                    //Debug.Log("Touch Up!");
            //    }
            //});
            //}


            void OnButtonDown(string currentButton)
            {
            Loom.QueueOnMainThread(()=>{
                Debug.Log("----" + currentButton);
                Instance.state_down = "Button down: " + currentButton;
                Instance.getKeyCode(currentButton);
                switch (Instance.current_key)
                {
                    case MojingKeyCode.KEYCODE_ENTER://mojing OK键
                        // Do as you wanna...
                        Instance._bHandles[0].Pressed();
                        break;

                    case MojingKeyCode.KEYCODE_BACK://Mojing C键 
                        // Do as you wanna...
                        Instance._bHandles[1].Pressed();
                        break;

                    case MojingKeyCode.KEYCODE_MENU://mojing menu键
                        // Do as you wanna...
                        Instance._bHandles[2].Pressed();
                        break;

                    case MojingKeyCode.KEYCODE_DPAD_UP://up
                        // Do as you wanna...
                        Instance._bHandles[3].Pressed();
                        break;

                    case MojingKeyCode.KEYCODE_DPAD_DOWN://down
                        // Do as you wanna...
                        Instance._bHandles[4].Pressed();
                        break;

                    case MojingKeyCode.KEYCODE_DPAD_LEFT://left
                        // Do as you wanna...
                        Instance._bHandles[5].Pressed();
                        break;

                    case MojingKeyCode.KEYCODE_DPAD_RIGHT://right
                        // Do as you wanna...
                        Instance._bHandles[6].Pressed();
                        break;

                    case MojingKeyCode.KEYCODE_DPAD_CENTER:
                        // Do as you wanna...
                        Instance._bHandles[7].Pressed();
                        break;

                    case MojingKeyCode.KEYCODE_VOLUME_UP:
                        // Do as you wanna...
                        Instance._bHandles[8].Pressed();
                        break;

                    case MojingKeyCode.KEYCODE_VOLUME_DOWN:
                        // Do as you wanna...
                        Instance._bHandles[9].Pressed();
                        break;
                    case MojingKeyCode.KEYCODE_BUTTON_A:
                        Instance._bHandles[10].Pressed();
                        break;

                    case MojingKeyCode.KEYCODE_BUTTON_B:
                        Instance._bHandles[11].Pressed();
                        break;

                    case MojingKeyCode.KEYCODE_BUTTON_X:
                        Instance._bHandles[12].Pressed();
                        break;

                    case MojingKeyCode.KEYCODE_BUTTON_Y:
                        Instance._bHandles[13].Pressed();
                        break;
                    case MojingKeyCode.KEYCODE_BUTTON_L1:
                        Instance._bHandles[14].Pressed();
                        break;

                    case MojingKeyCode.KEYCODE_BUTTON_R1:
                        Instance._bHandles[15].Pressed();
                        break;

                    case MojingKeyCode.KEYCODE_BUTTON_L2:
                        Instance._bHandles[16].Pressed();
                        break;

                    case MojingKeyCode.KEYCODE_BUTTON_R2:
                        Instance._bHandles[17].Pressed();
                        break;
                    case "home":
						// Do as you wanna...
						Debug.Log("home key down");
						break;
                    
                    default:

                        return;
                }
            });
            }
            void OnButtonUp(string currentButton)
            {
            Loom.QueueOnMainThread(()=>{
                Instance.state_up = "Button up: " + currentButton;
                Instance.getKeyCode(currentButton);

                switch (Instance.current_key)
                {
                    case MojingKeyCode.KEYCODE_ENTER://Mojing ok键
                        // Do as you wanna...
                        Instance._bHandles[0].Released();
                        break;
                    case MojingKeyCode.KEYCODE_BACK://Mojing C键 
                        Instance._bHandles[1].Released();
                        // Do as you wanna...
                        break;
                    case MojingKeyCode.KEYCODE_MENU://mojing menu键
                        // Do as you wanna...
                        Instance._bHandles[2].Released();
                        break;
                    case MojingKeyCode.KEYCODE_DPAD_UP://up
                        // Do as you wanna...
                        Instance._bHandles[3].Released();
                        break;
                    case MojingKeyCode.KEYCODE_DPAD_DOWN://down
                        // Do as you wanna...
                        Instance._bHandles[4].Released();
                        break;
                    case MojingKeyCode.KEYCODE_DPAD_LEFT://left
                        // Do as you wanna...
                        Instance._bHandles[5].Released();
                        break;
                    case MojingKeyCode.KEYCODE_DPAD_RIGHT://right
                        // Do as you wanna...
                        Instance._bHandles[6].Released();
                        break;
                    case MojingKeyCode.KEYCODE_DPAD_CENTER:
                        // Do as you wanna...
                        Instance._bHandles[7].Released();
                        break;
                    case MojingKeyCode.KEYCODE_VOLUME_UP:
                        // Do as you wanna...
                        Instance._bHandles[8].Released();
                        break;
                    case MojingKeyCode.KEYCODE_VOLUME_DOWN:
                        // Do as you wanna...
                        Instance._bHandles[9].Released();
                        break;
                    case MojingKeyCode.KEYCODE_BUTTON_A:
                        Instance._bHandles[10].Released();
                        break;
                    case MojingKeyCode.KEYCODE_BUTTON_B:
                        Instance._bHandles[11].Released();
                        break;
                    case MojingKeyCode.KEYCODE_BUTTON_X:
                        Instance._bHandles[12].Released();
                        break;
                    case MojingKeyCode.KEYCODE_BUTTON_Y:
                        Instance._bHandles[13].Released();
                        break;
                    case MojingKeyCode.KEYCODE_BUTTON_L1:
                        Instance._bHandles[14].Released();
                        break;

                    case MojingKeyCode.KEYCODE_BUTTON_R1:
                        Instance._bHandles[15].Released();
                        break;

                    case MojingKeyCode.KEYCODE_BUTTON_L2:
                        Instance._bHandles[16].Released();
                        break;

                    case MojingKeyCode.KEYCODE_BUTTON_R2:
                        Instance._bHandles[17].Released();
                        break;
                    case "home":
						// Do as you wanna...
						Debug.Log("home key up");
						break;
                }
            });
            }

            void OnButtonLongPress(string pressBtn)
            {
            Loom.QueueOnMainThread(()=>{
                Instance.state_long_down = "Long Press: " + pressBtn;
                Instance.getKeyCode(pressBtn);

                switch (Instance.current_key)
                {
                    case MojingKeyCode.KEYCODE_ENTER://Mojing ok键
                        // Do as you wanna...
                        Instance._bHandles[0].LongPressed();
                        break;
                    case MojingKeyCode.KEYCODE_BACK://Mojing C键
                        // Do as you wanna...
                        Instance._bHandles[1].LongPressed();
                        break;
                    case MojingKeyCode.KEYCODE_MENU://mojing menu键
                        // Do as you wanna...
                        Instance._bHandles[2].LongPressed();
                        break;
                    case MojingKeyCode.KEYCODE_DPAD_UP://up
                        // Do as you wanna...
                        Instance._bHandles[3].LongPressed();
                        break;
                    case MojingKeyCode.KEYCODE_DPAD_DOWN://down
                        // Do as you wanna...
                        Instance._bHandles[4].LongPressed();
                        break;
                    case MojingKeyCode.KEYCODE_DPAD_LEFT://left
                        // Do as you wanna...
                        Instance._bHandles[5].LongPressed();
                        break;
                    case MojingKeyCode.KEYCODE_DPAD_RIGHT://right
                        // Do as you wanna...
                        Instance._bHandles[6].LongPressed();
                        break;
                    case MojingKeyCode.KEYCODE_DPAD_CENTER:
                        // Do as you wanna...
                        Instance._bHandles[7].LongPressed();
                        break;
                    case MojingKeyCode.KEYCODE_VOLUME_UP:
                        // Do as you wanna...
                        Instance._bHandles[8].LongPressed();
                        break;
                    case MojingKeyCode.KEYCODE_VOLUME_DOWN:
                        // Do as you wanna...
                        Instance._bHandles[9].LongPressed();
                        break;
                    case MojingKeyCode.KEYCODE_BUTTON_A:
                        Instance._bHandles[10].LongPressed();
                        break;
                    case MojingKeyCode.KEYCODE_BUTTON_B:
                        Instance._bHandles[11].LongPressed();
                        break;
                    case MojingKeyCode.KEYCODE_BUTTON_X:
                        Instance._bHandles[12].LongPressed();
                        break;
                    case MojingKeyCode.KEYCODE_BUTTON_Y:
                        Instance._bHandles[13].LongPressed();
                        break;
                    case MojingKeyCode.KEYCODE_BUTTON_L1:
                        Instance._bHandles[14].LongPressed();
                        break;

                    case MojingKeyCode.KEYCODE_BUTTON_R1:
                        Instance._bHandles[15].LongPressed();
                        break;

                    case MojingKeyCode.KEYCODE_BUTTON_L2:
                        Instance._bHandles[16].LongPressed();
                        break;

                    case MojingKeyCode.KEYCODE_BUTTON_R2:
                        Instance._bHandles[17].LongPressed();
                        break;
                    case "home":
						// Do as you wanna...
						Debug.Log("home key long pressed");
						break;
                }
            });
            }

            //"TranslationType": 0---轴模式
            void onMove(string info)
            {
            Loom.QueueOnMainThread(()=>{
                Debug.Log("----"+info);
                Instance.getKeyCode(info);
                if (Instance.current_axis == MojingKeyCode.LETF_H)
                {
                    Instance._aHandles[0].Update(float.Parse(Instance.current_key));
                }
                else if (Instance.current_axis == MojingKeyCode.LETF_V)
                    Instance._aHandles[1].Update(float.Parse(Instance.current_key));

                else if (Instance.current_axis == MojingKeyCode.AXIS_DPAD)
                {
                    Instance._aHandles[0].Update(float.Parse(Instance.key_field[0]));
                    Instance._aHandles[1].Update(float.Parse(Instance.key_field[1]));
                }
            });
            }


            void onBluetoothAdapterStateChanged(string state)
            {
            Loom.QueueOnMainThread(()=>{
                switch (state)
                {
                    case "12":
                        // BluetoothAdapter.STATE_ON
                        //MojingLog.LogTrace("Bluetooth ON");
                        Debug.Log("----Bluetooth ON");
                        break;

                    case "10":
                        // BluetoothAdapter.STATE_OFF
                        //MojingLog.LogTrace("Bluetooth OFF");
                        Debug.Log("----Bluetooth OFF");
                        break;
                }
            });
            }

            public void OnTouchEvent(string touchEvent)
            {
            Loom.QueueOnMainThread(()=>{
                switch (touchEvent)
                {
                    case "ACTION_DOWN":
                        //MojingLog.LogTrace("OnTouchEvent: ACTION_DOWN");
                        Debug.Log("----OnTouchEvent: ACTION_DOWN");
                        Instance.touchKey.keyState = Key.KeyState.KEY_DOWN;
                        break;

                    case "ACTION_UP":
                        //MojingLog.LogTrace("OnTouchEvent: ACTION_UP");
                        Debug.Log("----OnTouchEvent: ACTION_UP");
                        Instance.touchKey.keyState = Key.KeyState.KEY_UP;
                        break;
                }
            });
            }
        }

#elif !UNITY_EDITOR && UNITY_IOS
        //-----Joystick UnityPlayer.UnitySendMessage Mode .IOS Platform-----
        //按键按下响应
        public void OnButtonDown(string currentBtn)
        {
            state_down = "Button down: " + currentBtn;
            getKeyCode(currentBtn);

            switch (current_key)
            {
                case MojingKeyCode.KEYCODE_ENTER://mojing OK键
                    // Do as you wanna...
                    _bHandles[0].Pressed();
                    break;

                case MojingKeyCode.KEYCODE_BACK://Mojing C键 
                    // Do as you wanna...
                    _bHandles[1].Pressed();
                    break;

                case MojingKeyCode.KEYCODE_MENU://mojing menu键
                    // Do as you wanna...
                    _bHandles[2].Pressed();
                    break;

                case MojingKeyCode.KEYCODE_DPAD_UP://up
                    // Do as you wanna...
                    _bHandles[3].Pressed();
                    break;

                case MojingKeyCode.KEYCODE_DPAD_DOWN://down
                    // Do as you wanna...
                    _bHandles[4].Pressed();
                    break;

                case MojingKeyCode.KEYCODE_DPAD_LEFT://left
                    // Do as you wanna...
                    _bHandles[5].Pressed();
                    break;

                case MojingKeyCode.KEYCODE_DPAD_RIGHT://right
                    // Do as you wanna...
                    _bHandles[6].Pressed();
                    break;

                case MojingKeyCode.KEYCODE_DPAD_CENTER:
                    // Do as you wanna...
                    _bHandles[7].Pressed();
                    break;

                case MojingKeyCode.KEYCODE_VOLUME_UP:
                    // Do as you wanna...
                    _bHandles[8].Pressed();
                    break;

                case MojingKeyCode.KEYCODE_VOLUME_DOWN:
                    // Do as you wanna...
                    _bHandles[9].Pressed();
                    break;
                
                case MojingKeyCode.KEYCODE_BUTTON_A:
                    _bHandles[10].Pressed();
                    break;

                case MojingKeyCode.KEYCODE_BUTTON_B:
                    _bHandles[11].Pressed();
                    break;

                case MojingKeyCode.KEYCODE_BUTTON_X:
                    _bHandles[12].Pressed();
                    break;

                case MojingKeyCode.KEYCODE_BUTTON_Y:
                    _bHandles[13].Pressed();
                    break;
                case MojingKeyCode.KEYCODE_BUTTON_L1:
                    _bHandles[14].Pressed();
                    break;

                case MojingKeyCode.KEYCODE_BUTTON_R1:
                    _bHandles[15].Pressed();
                    break;

                case MojingKeyCode.KEYCODE_BUTTON_L2:
                    _bHandles[16].Pressed();
                    break;

                case MojingKeyCode.KEYCODE_BUTTON_R2:
                    _bHandles[17].Pressed();
                    break;
                case "home":
					// Do as you wanna...
					Debug.Log("home key down");
					break;
                default:

                    return;
            }
        }

        //按键抬起响应
        public void OnButtonUp(string currentButton)
        {
            state_up = "Button up: " + currentButton;
            getKeyCode(currentButton);

            switch (current_key)
            {
                case MojingKeyCode.KEYCODE_ENTER://Mojing ok键
                    // Do as you wanna...
                    _bHandles[0].Released();
                    break;

                case MojingKeyCode.KEYCODE_BACK://Mojing C键 
                    _bHandles[1].Released();
                    // Do as you wanna...
                    break;

                case MojingKeyCode.KEYCODE_MENU://mojing menu键
                    // Do as you wanna...
                    _bHandles[2].Released();
                    break;

                case MojingKeyCode.KEYCODE_DPAD_UP://up
                    // Do as you wanna...
                    _bHandles[3].Released();
                    break;

                case MojingKeyCode.KEYCODE_DPAD_DOWN://down
                    // Do as you wanna...
                    _bHandles[4].Released();
                    break;

                case MojingKeyCode.KEYCODE_DPAD_LEFT://left
                    // Do as you wanna...
                    _bHandles[5].Released();
                    break;

                case MojingKeyCode.KEYCODE_DPAD_RIGHT://right
                    // Do as you wanna...
                    _bHandles[6].Released();
                    break;

                case MojingKeyCode.KEYCODE_DPAD_CENTER:
                    // Do as you wanna...
                    _bHandles[7].Released();
                    break;

                case MojingKeyCode.KEYCODE_VOLUME_UP:
                    // Do as you wanna...
                    _bHandles[8].Released();
                    break;

                case MojingKeyCode.KEYCODE_VOLUME_DOWN:
                    // Do as you wanna...
                    _bHandles[9].Released();
                    break;

                case MojingKeyCode.KEYCODE_BUTTON_A:
                    _bHandles[10].Released();
                    break;

                case MojingKeyCode.KEYCODE_BUTTON_B:
                    _bHandles[11].Released();
                    break;

                case MojingKeyCode.KEYCODE_BUTTON_X:
                    _bHandles[12].Released();
                    break;

                case MojingKeyCode.KEYCODE_BUTTON_Y:
                    _bHandles[13].Released();
                    break;
                case MojingKeyCode.KEYCODE_BUTTON_L1:
                    _bHandles[14].Released();
                    break;

                case MojingKeyCode.KEYCODE_BUTTON_R1:
                    _bHandles[15].Released();
                    break;

                case MojingKeyCode.KEYCODE_BUTTON_L2:
                    _bHandles[16].Released();
                    break;

                case MojingKeyCode.KEYCODE_BUTTON_R2:
                    _bHandles[17].Released();
                    break;
                case "home":
					// Do as you wanna...
					Debug.Log("home key up");
					break;

            }
        }
        //按键长按响应
        public void onButtonLongPress(string pressBtn)
        {
            state_long_down = "Long Press: " + pressBtn;
            getKeyCode(pressBtn);

            switch (current_key)
            {
                case MojingKeyCode.KEYCODE_ENTER://Mojing ok键
                    // Do as you wanna...
                    _bHandles[0].Released();
                    break;
                case MojingKeyCode.KEYCODE_BACK://Mojing C键
                    // Do as you wanna...
                    _bHandles[1].LongPressed();
                    break;
                case MojingKeyCode.KEYCODE_MENU://mojing menu键
                    // Do as you wanna...
                    _bHandles[2].LongPressed();
                    break;
                case MojingKeyCode.KEYCODE_DPAD_UP://up
                    // Do as you wanna...
                    _bHandles[3].LongPressed();
                    break;
                case MojingKeyCode.KEYCODE_DPAD_DOWN://down
                    // Do as you wanna...
                    _bHandles[4].LongPressed();
                    break;
                case MojingKeyCode.KEYCODE_DPAD_LEFT://left
                    // Do as you wanna...
                    _bHandles[5].LongPressed();
                    break;
                case MojingKeyCode.KEYCODE_DPAD_RIGHT://right
                    // Do as you wanna...
                    _bHandles[6].LongPressed();
                    break;
                case MojingKeyCode.KEYCODE_DPAD_CENTER:
                    // Do as you wanna...
                    _bHandles[7].LongPressed();
                    break;
                case MojingKeyCode.KEYCODE_VOLUME_UP:
                    // Do as you wanna...
                    _bHandles[8].LongPressed();
                    break;
                case MojingKeyCode.KEYCODE_VOLUME_DOWN:
                    // Do as you wanna...
                    _bHandles[9].LongPressed();
                    break;
                case MojingKeyCode.KEYCODE_BUTTON_A:
                    _bHandles[10].LongPressed();
                    break;

                case MojingKeyCode.KEYCODE_BUTTON_B:
                    _bHandles[11].LongPressed();
                    break;

                case MojingKeyCode.KEYCODE_BUTTON_X:
                    _bHandles[12].LongPressed();
                    break;

                case MojingKeyCode.KEYCODE_BUTTON_Y:
                    _bHandles[13].LongPressed();
                    break;
                case MojingKeyCode.KEYCODE_BUTTON_L1:
                    _bHandles[14].LongPressed();
                    break;

                case MojingKeyCode.KEYCODE_BUTTON_R1:
                    _bHandles[15].LongPressed();
                    break;

                case MojingKeyCode.KEYCODE_BUTTON_L2:
                    _bHandles[16].LongPressed();
                    break;

                case MojingKeyCode.KEYCODE_BUTTON_R2:
                    _bHandles[17].LongPressed();
                    break;
                case "home":
					// Do as you wanna...
					Debug.Log("home key long pressed");
					break;
            }
        }

        //"TranslationType": 0---轴模式
        public void onMove(string info)
        {
            getKeyCode(info);
            Debug.Log(info);
            if (current_axis == MojingKeyCode.LETF_H)
            {
                _aHandles[0].Update(float.Parse(current_key));
            }
            else if (current_axis == MojingKeyCode.LETF_V)
                _aHandles[1].Update(float.Parse(current_key));

            else if (current_axis == MojingKeyCode.AXIS_DPAD)
            {
                _aHandles[0].Update(float.Parse(key_field[0]));
                _aHandles[1].Update(float.Parse(key_field[1]));
            }
        }

        public void onBluetoothAdapterStateChanged(string state)
        {
            switch (state)
            {
                case "12":
                    // BluetoothAdapter.STATE_ON
                    //MojingLog.LogTrace("Bluetooth ON");
                    break;

                case "10":
                    // BluetoothAdapter.STATE_OFF
                    //MojingLog.LogTrace("Bluetooth OFF");
                    break;
            }
        }
        public void OnTouchEvent(string touchEvent)
        {
            switch (touchEvent)
            {
                case "ACTION_DOWN":
                    //MojingLog.LogTrace("OnTouchEvent: ACTION_DOWN");
                    Debug.Log("----OnTouchEvent: ACTION_DOWN");
                    touchKey.keyState = Key.KeyState.KEY_DOWN;
                    break;

                case "ACTION_UP":
                    //MojingLog.LogTrace("OnTouchEvent: ACTION_UP");
                    Debug.Log("----OnTouchEvent: ACTION_UP");
                    touchKey.keyState = Key.KeyState.KEY_UP;
                    break;
            }
        }

#endif

        public void onDeviceAttached(string deviceName)
        {
            device_name_attach = deviceName;
            attachstate = AttachState.Connected;
            //DeviceAttachedCallback(true);
            Debug.Log(deviceName + "----" + attachstate);
        }

        public void onDeviceDetached(string deviceName)
        {
            device_name_detach = deviceName;
            attachstate = AttachState.Disconnected;
            //DeviceAttachedCallback(false);
            Debug.Log(deviceName + "----" + attachstate);
        }
    }
}