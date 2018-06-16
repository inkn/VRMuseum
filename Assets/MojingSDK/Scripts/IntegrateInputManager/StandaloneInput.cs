using System;
using UnityEngine;

namespace MojingSample.CrossPlatformInput.PlatformSpecific
{
    public class StandaloneInput : VirtualInput
    {
        private string pressKey = "";
        private float pressTime = 0;
        private const float longPressTime = 3;
        public override float GetAxis(string name, bool raw)
        {
            return raw ? Input.GetAxisRaw(name) : Input.GetAxis(name);
        }


        public override bool GetButton(string name)
        {
            return Input.GetButton(name);
        }


        public override bool GetButtonDown(string name)
        {
            if (Input.GetButtonDown(name))
            {
                if (pressKey != name)
                {
                    pressKey = name;
                    pressTime = Time.realtimeSinceStartup;
                }

                return true;
            }
            return false;
        }

        public override bool GetButtonLongPressed(string name)
        {
            if (pressKey == name && (Time.realtimeSinceStartup - pressTime > longPressTime))
            {
                return true;
            }
            return false;
        }

        public override bool GetButtonUp(string name)
        {
            if (Input.GetButtonUp(name))
            {
                pressKey = "";
                return true;
            }
            return false;
        }


        public override void SetButtonDown(string name)
        {
            throw new Exception(
                " This is not possible to be called for standalone input. Please check your platform and code where this is called");
        }


        public override void SetButtonUp(string name)
        {
            throw new Exception(
                " This is not possible to be called for standalone input. Please check your platform and code where this is called");
        }


        public override void SetAxisPositive(string name)
        {
            throw new Exception(
                " This is not possible to be called for standalone input. Please check your platform and code where this is called");
        }


        public override void SetAxisNegative(string name)
        {
            throw new Exception(
                " This is not possible to be called for standalone input. Please check your platform and code where this is called");
        }


        public override void SetAxisZero(string name)
        {
            throw new Exception(
                " This is not possible to be called for standalone input. Please check your platform and code where this is called");
        }


        public override void SetAxis(string name, float value)
        {
            throw new Exception(
                " This is not possible to be called for standalone input. Please check your platform and code where this is called");
        }


        public override Vector3 MousePosition()
        {
            return Input.mousePosition;
        }
    }
}