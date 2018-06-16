//------------------------------------------------------------------------------
// Copyright 2016 Baofeng Mojing Inc. All rights reserved.
//------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using System;
public class SetOverlay : MonoBehaviour 
{
    public static float HEADCTRL_WIDTH = 0.04f;
    public static float HEADCTRL_HEIGHT = 0.04f;

    private float HalfHeadCtrlWidth = HEADCTRL_WIDTH * 0.5f;
    private float HalfHeadCtrlHeight = HEADCTRL_HEIGHT * 0.5f;
    public static Vector2 HeadCtrlOffset;

    public bool MoveViewFree = true;
    Texture tex;
    IntPtr texID = IntPtr.Zero;
	IntPtr currentTextureID = IntPtr.Zero;
    RenderTexture texRend;
    Camera LCamera;
    Camera RCamera;
    int size = 0;
    Transform CenterPointer;

    void Start()
    {
        tex = Resources.Load("star") as Texture;
        LCamera = GameObject.Find("MojingMain/MojingVrHead/VR Camera Left").GetComponent<Camera>();
        RCamera = GameObject.Find("MojingMain/MojingVrHead/VR Camera Right").GetComponent<Camera>();
        CenterPointer = GameObject.Find("MojingMain/MojingVrHead/GazePointer").transform;
              
        size = MojingSDK.Unity_GetTextureSize();
        HeadCtrlOffset = new Vector2(size * 0.5f, size * 0.5f);
		texID = tex.GetNativeTexturePtr();
    }

    void Update()
    {
#if !UNITY_EDITOR && UNITY_IOS
		if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Metal) {
			if (currentTextureID != texID) {
				texID = tex.GetNativeTexturePtr();
				currentTextureID = texID;
			}
		}
#endif
        DrawOverlay();
    }

    // If TW, ATW or needDistortion enable, render by MojingSDK, Call MojingSDK.Unity_SetOverlay
    void DrawOverlay()
    {
        if (tex)
        {
            if (Mojing.SDK.NeedDistortion)
            {
               //  MojingSDK.Unity_SetOverlay3D(MojingSDK.TEXTURE_BOTH_EYE, IntPtr.Zero, 0.04f, 0.04f, CenterPointer.transform.position.magnitude);
                
                if (MoveViewFree)
                {
                    MojingSDK.Unity_SetOverlay3D(MojingSDK.TEXTURE_LEFT_EYE , texID,HEADCTRL_WIDTH, HEADCTRL_HEIGHT, Vector3.Distance(LCamera.transform.position, CenterPointer.position));
                    MojingSDK.Unity_SetOverlay3D(MojingSDK.TEXTURE_RIGHT_EYE , texID,HEADCTRL_WIDTH, HEADCTRL_HEIGHT, Vector3.Distance(RCamera.transform.position, CenterPointer.position));
                }
                else
                {
                    float x = Mathf.Clamp(1 - HeadCtrlOffset.x / size, HalfHeadCtrlWidth, 1 - HalfHeadCtrlWidth) - HalfHeadCtrlWidth;
                    float y = Mathf.Clamp(HeadCtrlOffset.y / size, HalfHeadCtrlHeight, 1 - HalfHeadCtrlHeight) - HalfHeadCtrlHeight;

                    MojingSDK.Unity_SetOverlay3D_V2(MojingSDK.TEXTURE_LEFT_EYE, texID, x, y, HEADCTRL_WIDTH, HEADCTRL_HEIGHT, Vector3.Distance(LCamera.transform.position, CenterPointer.position));
                    MojingSDK.Unity_SetOverlay3D_V2(MojingSDK.TEXTURE_RIGHT_EYE, texID, x, y, HEADCTRL_WIDTH, HEADCTRL_HEIGHT, Vector3.Distance(RCamera.transform.position, CenterPointer.position));
                }

                /*------
                    iEyeType:1----Left camera viewport draw
                            2----Right camera viewport draw
                            3---- Both left camera and right camera viewports draw
                    ------*/
            }
        }
        else
            Debug.Log("There is no Texture!");
    }

    void OnDestroy()
    {
        MojingSDK.Unity_SetOverlay3D(MojingSDK.TEXTURE_BOTH_EYE , IntPtr.Zero, 1, 1, 1);
    }
}
