//------------------------------------------------------------------------------
// Copyright 2016 Baofeng Mojing Inc. All rights reserved.
//------------------------------------------------------------------------------

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

/*
	A simple, shared PostProcessBuildPlayer script to enable Objective-C modules. This lets us add frameworks
	from our source files, rather than through modifying the Xcode project. 
*/

public class IOSBuildPostProcessor
{

    [PostProcessBuild(1500)] // We should try to run last
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
#if UNITY_IPHONE
		if (target != (BuildTarget)9/*BuildTarget.iOS*/) {
			return; // How did we get here?
		}
		
		UnityEngine.Debug.Log("IOSBuildPostProcessor: Enabling Objective-C modules");
		string pbxproj = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
		
		// Looking for the buildSettings sections of the pbxproj
		string insertKeyword = "buildSettings = {";
		string foundKeyword = "CLANG_ENABLE_MODULES"; // for checking if it's already inserted
		string modulesFlag = "				CLANG_ENABLE_MODULES = YES;";
		
		List<string> lines = new List<string>();
		
		foreach (string str in File.ReadAllLines(pbxproj)) {
			if (!str.Contains(foundKeyword)) { 
				lines.Add(str);
			}
			if (str.Contains(insertKeyword)) {
				lines.Add(modulesFlag);
			}
		}
		
		// Clear the file
		// http://stackoverflow.com/questions/16212127/add-a-new-line-at-a-specific-position-in-a-text-file
		using (File.Create(pbxproj)) {}
		
		foreach (string str in lines) {
			File.AppendAllText(pbxproj, str + Environment.NewLine);
		}
		
		
		//modify UnityAppController.mm
		UnityEngine.Debug.Log("IOSBuildPostProcessor: modify UnityAppController.mm");
		string srcUnity = path + "/Classes/UnityAppController.mm";
		
		string strFindInclude = "#import <OpenGLES/ES2/glext.h>";
		string strFindFuncRender = "- (void)shouldAttachRenderDelegate\t{}";
#if UNITY_5_4 || UNITY_5_5
		string strInclude = "#import <OpenGLES/ES2/glext.h>"
						  + "\n" + "#include \"Libraries/Plugins/iOS/UnityIOSAPI.h\""
						  + "\n" + "#include \"Unity/IUnityInterface.h\""
						  + "\n" + "extern \"C\" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces);"
						  + "\n" + "extern \"C\" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload();";
		
		string strFuncRender = "- (void)shouldAttachRenderDelegate" 
							 + "\n" + "{"
							 + "\n" + "\tUnityRegisterRenderingPlugin(&UnitySetGraphicsDevice, &UnityRenderEvent);"
							 + "\n" + "\tUnityRegisterRenderingPluginV5(&UnityPluginLoad, &UnityPluginUnload);"
						 	 + "\n" + "}";
#else
		string strInclude = "#import <OpenGLES/ES2/glext.h>"
						  + "\n" + "#include \"Libraries/Plugins/iOS/UnityIOSAPI.h\"";

		string strFuncRender = "- (void)shouldAttachRenderDelegate"
							 + "\n" + "{"
							 + "\n" + "    UnityRegisterRenderingPlugin(&UnitySetGraphicsDevice, &UnityRenderEvent);"
							 + "\n" + "}";
#endif
        string strFindFuncRegister = "UnitySetPlayerFocus(1);";
		string strFuncRegister = "UnitySetPlayerFocus(1);" 
							   + "\n" + "\tUnity_RegisterAllGamepad(_rootController);";

		string strCurrent = System.IO.File.ReadAllText(srcUnity);
		if (!strCurrent.Contains(strInclude))
		{
			strCurrent = strCurrent.Replace(strFindInclude, strInclude);
		}
		if (!strCurrent.Contains(strFuncRender))
		{
			strCurrent = strCurrent.Replace(strFindFuncRender, strFuncRender);
		}
		if (!strCurrent.Contains(strFuncRegister))
		{
			strCurrent = strCurrent.Replace(strFindFuncRegister, strFuncRegister);
		}

		using(File.Create(srcUnity)){}
		System.IO.File.WriteAllText(srcUnity, strCurrent);


		/*
		List<string> lstlines = new List<string>();
		
		foreach (string str in File.ReadAllLines(srcUnity)) {
			//replace
			if(str.Contains(strFindFuncRender)){
				lstlines.Add(strFuncRender);
				continue;
			}
			
			lstlines.Add(str);
			
			if(str.Contains(strFindInclude)){
				lstlines.Add(strInclude);
			}
			
			if(str.Contains(strFindFuncRegister)){
				lstlines.Add(strFuncRegister);
			}
		}
		
		// Clear the file
		// http://stackoverflow.com/questions/16212127/add-a-new-line-at-a-specific-position-in-a-text-file
		using (File.Create(srcUnity)) {}
		
		foreach (string str in lstlines) {
			File.AppendAllText(srcUnity, str + Environment.NewLine);
		}
		*/

		// Modify DisplayManager.mm
		srcUnity = path + "/Classes/Unity/DisplayManager.mm";
		string strUnityContentInterface = "extern \"C\" EAGLContext* UnityGetContextEAGL()\n{\n\treturn GetMainDisplay().surfaceGLES->context;\n}";
		string strMojingContentInterface = "extern \"C\" EAGLContext* UnityGetContextEAGL()\n{\n\treturn GetMainDisplay().surfaceGLES->context;\n}\nextern \"C\" CAMetalDrawableRef UnityGetMetalDrawable()\n{\n    return GetMainDisplay().surfaceMTL->drawable;\n}";

		string textDisplayManager = System.IO.File.ReadAllText(srcUnity);
		UnityEngine.Debug.Log(textDisplayManager);
		if (!textDisplayManager.Contains(strMojingContentInterface))
		{
			UnityEngine.Debug.Log("patch DisplayManager.mm");
			textDisplayManager = textDisplayManager.Replace(strUnityContentInterface, strMojingContentInterface);
		}

		using(File.Create(srcUnity)){}
		System.IO.File.WriteAllText(srcUnity, textDisplayManager);
		
		
#endif
    }

}
