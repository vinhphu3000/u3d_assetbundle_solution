using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System;
using UnityEditor.SceneManagement;
using System.Reflection;
using UnityEngine.AI;

/// <summary>
/// 打包命令类
/// Date: 2016/1/5
/// Author: lxz
/// </summary>
public class BuildCommand
{
    [MenuItem("GAME/打包选择项 %#z")]
    [MenuItem("Assets/打包选择项")]
    public static void buildTarget()
    {
        BuildConfig.init();

        if (!checkCommonRes())
        {
            return;
        }

        foreach (UnityEngine.Object asset in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets))
        {
            BuildConfig.getBuildFunc(AssetDatabase.GetAssetOrScenePath(asset))(asset);
        }

        //编译完成删除无用文件
        var files = Directory.GetFileSystemEntries(BuildConfig.abOutputPath);
        foreach (var v in files)
        {
            if (!v.EndsWith(BuildConfig.abSuffix) && !v.EndsWith(".cfg") && !v.EndsWith(".bin"))
            {
                File.Delete(v);
            }
        }
    }

    static AssetBundleBuild commonBuildMap;
    public static bool checkCommonRes()
    {
        string absuffixNoPoint = BuildConfig.abSuffix.Replace(".", "");
        commonBuildMap = new AssetBundleBuild();
        commonBuildMap.assetBundleName = "common";
        commonBuildMap.assetBundleVariant = absuffixNoPoint;

        string abPath = "Assets/ArtResources/ShaderList.prefab";
        List<string> assetPaths = new List<string>();

        //找到shaderlist对象，设置其中的shader的assetbundle名为shaderlist
        var shaderlist = AssetDatabase.LoadAssetAtPath(abPath, typeof(GameObject)) as GameObject;
        if (shaderlist == null)
        {
            Debug.LogError("加载 " + abPath + " 出错,打包停止!!");
            return false;
        }

        foreach (var s in shaderlist.GetComponent<ShaderList>().Shaders)
        {
            if (s == null)
            {
                Debug.LogError("shaderlist shader is null..");
                return false;
            }
            string path = AssetDatabase.GetAssetPath(s);
            if (path == null || !path.Contains("Assets"))
            {
                Debug.LogError("shaderlist shader 不在工程内...," + path == null ? s.name : path);
                return false;
            }

            assetPaths.Add(path);
        }

        //添加公共字体资源
        //assetPaths.Add("Assets/ArtResources/FZY4JW.TTF");

        commonBuildMap.assetNames = assetPaths.ToArray();
        return true;
    }

    public static AssetBundleBuild getCommonDep()
    {
        return commonBuildMap;
    }

}
