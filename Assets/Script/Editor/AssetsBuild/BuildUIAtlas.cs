using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

/// <summary>
/// 打包ui图集
/// Date: 2016/1/5
/// Author: lxz  
/// </summary>
public class BuildUIAtlas:BuildImpl
{
    /// <summary>
    /// 编译ui图集
    /// </summary>
    /// <param name="obj">Object.</param>
    public static void Build(Object obj)
    {
        string texPath = AssetDatabase.GetAssetPath(obj);
        //得到所有资源路径
        var allAssets = AssetDatabase.LoadAllAssetsAtPath(texPath);
        bool isSpriteAsset = false; 
        foreach (var v in allAssets)
        {
            if(v as Sprite != null)
            {
                isSpriteAsset = true;
            }  
        }

        if (!isSpriteAsset)
        {
            Debug.LogError(">>>>asset is not a ui atlas! "+obj.name);
            return;
        }
        //编译
        AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
        buildMap[0].assetBundleName = "uit_" + obj.name.ToLower() + BuildConfig.abSuffix;
        buildMap[0].assetNames = new string[]{ texPath };
        BuildPipeline.BuildAssetBundles(BuildConfig.abOutputPath,buildMap,BuildConfig.options,EditorUserBuildSettings.activeBuildTarget);
    }
}
