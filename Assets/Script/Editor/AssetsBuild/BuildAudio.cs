using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

/// <summary>
/// 打包音效资源
/// Date: 2016/1/21
/// Author: lxz  
/// </summary>
public class BuildAudio:BuildImpl
{
    /// <summary>
    /// 编译ui图集
    /// </summary>
    /// <param name="obj">Object.</param>
    public static new void Build(Object obj)
    {
        string assetPath = AssetDatabase.GetAssetPath(obj);
        if (!assetPath.ToLower().EndsWith(".ogg"))
        {
            Debug.LogError("资源不是ogg格式音效资源，打包失败!!"+assetPath);
            //return;
        } 
        //编译
        AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
        buildMap[0].assetBundleName = "snd_" + obj.name.ToLower() + BuildConfig.abSuffix;
        buildMap[0].assetNames = new string[] { assetPath };
        BuildPipeline.BuildAssetBundles(BuildConfig.abOutputPath,buildMap,BuildConfig.options,EditorUserBuildSettings.activeBuildTarget);
    }
}
