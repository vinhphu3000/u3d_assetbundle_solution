using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;


/// <summary>
/// 打包特效及其依赖项
/// Date: 2016/1/17
/// Author: lxz  
/// </summary>
public class BuildEffect : BuildImpl
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="obj">Object.</param>
    public static new void Build(Object obj)
    {
        string prefabPath = AssetDatabase.GetAssetPath(obj);
        if (!prefabPath.EndsWith(".prefab"))
        {
            return;
        }

        //创建临时预制体 
        string temp_main_path = "Assets/temp/" + obj.name + ".prefab";
        AssetDatabase.DeleteAsset(temp_main_path);
        AssetDatabase.CopyAsset(prefabPath, temp_main_path);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        var tempPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(temp_main_path);
        if (tempPrefab == null)
        {
            Debug.LogError("加载临时effect prefab失败：" + temp_main_path);
            return;
        }

        //删除默认粒子图片。。。
        //var oldMatInfos = RemovePrefabMatTex(tempPrefab);

        var texsDeps = GetDepTexs(tempPrefab);

        EditorUtility.SetDirty(tempPrefab);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        //打包预制件
        AssetBundleBuild[] buildMap = new AssetBundleBuild[2 + texsDeps.Count];
        buildMap[0] = BuildCommand.getCommonDep();
        for(int i=0;i< texsDeps.Count;i++)
        {
            buildMap[i + 1] = texsDeps[i];
        }
        buildMap[buildMap.Length-1].assetBundleName = "eft_" + obj.name.ToLower() + BuildConfig.abSuffix;
        buildMap[buildMap.Length - 1].assetNames = new string[] { temp_main_path };
        BuildPipeline.BuildAssetBundles(BuildConfig.abOutputPath, buildMap, BuildConfig.options, EditorUserBuildSettings.activeBuildTarget);
        //处理raw image

        //还原材质
        //RevertMatFile(tempPrefab, oldMatInfos);

        //删除临时资源
        AssetDatabase.DeleteAsset(temp_main_path);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
    }
}
