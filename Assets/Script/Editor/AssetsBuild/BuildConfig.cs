using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEditor;
using System.IO;

public enum BuildResType
{
    uiAtlas = 1,
    uiPage,
    effect,
    scene,
    Audio,
    Model,
}

/// <summary>
/// 打包配置类
/// Date: 2016/1/5
/// Author: lxz
/// </summary>
public class BuildConfig
{ 
    public static string shaderListPath
    {
        get
        {
            return "Assets/ArtResources/ShaderList.prefab";
        }
    }

    /// <summary>
    /// 编译选项
    /// </summary>
    public static BuildAssetBundleOptions options = BuildAssetBundleOptions.DeterministicAssetBundle |
        BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.ForceRebuildAssetBundle;

    /// <summary>
    /// 资源后缀
    /// </summary>
    /// <value>The ab suffix.</value>
    public static string abSuffix
    {
        get
        {
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                return ".aab";
            }
            else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            {
                return ".iab";
            }
            else return ".oab";
        }
    }


    static string baseOutputPath = null;
    public static string abOutputPath
    {
        get
        {
            return baseOutputPath;
        }
    }

    /// <summary>
    /// 初始化路径等参数
    /// </summary>
    public static void init()
    {
        if (baseOutputPath == null)
        {
#if UNITY_ANDROID
            baseOutputPath = Application.dataPath + "/../ResEx/android";
#elif UNITY_IOS
            baseOutputPath = Application.dataPath + "/../ResEx/ios";
#else
            baseOutputPath = Application.dataPath + "/../ResEx/other";
#endif
        }
        if (!Directory.Exists(baseOutputPath))
        {
            Directory.CreateDirectory(baseOutputPath);
        }

        if (!Directory.Exists(Application.dataPath + "/temp"))
        {
            Directory.CreateDirectory(Application.dataPath + "/temp");
        }
    }


    /// <summary>
    /// 资源配置项
    /// </summary>
    public class ResBuildConfig
    {
        public string rootPath;
        public Action<UnityEngine.Object> buildFunc;
    }

    public static Dictionary<BuildResType, ResBuildConfig> uiPathRoots = new Dictionary<BuildResType, ResBuildConfig>
    {
        ////UI图集资源
        //{
        //    BuildResType.uiAtlas,
        //    new ResBuildConfig{
        //        rootPath = "Assets/UI/UIAtlas",
        //        buildFunc = BuildUIAtlas.Build
        //    }
        //},
        ////UI窗口资源目录
        //{
        //    BuildResType.uiPage,
        //    new ResBuildConfig{
        //        rootPath = "Assets/UI/Pages",
        //        buildFunc = BuildUIPage.Build
        //    }
        //},

        {
            BuildResType.effect,
            new ResBuildConfig{
                rootPath = "Assets/ArtResources/ABExport/Effects",
                buildFunc = BuildEffect.Build
            }
        },
        //场景资源目录
        {
            BuildResType.scene,
            new ResBuildConfig{
                rootPath = "Assets/ArtResources/ABExport/Scenes",
                buildFunc = BuildScene.Build
            }
        },

        {
            BuildResType.Audio,
            new ResBuildConfig{
                rootPath = "Assets/ArtResources/ABExport/Audios",
                buildFunc = BuildAudio.Build
            }
        },
         {
            BuildResType.Model,
            new ResBuildConfig{
                rootPath = "Assets/ArtResources/ABExport/Models",
                buildFunc = BuildModel.Build
            }
        }
    };


    /// <summary>
    /// 通过资源路径得到编译函数
    /// </summary> 
    public static Action<UnityEngine.Object> getBuildFunc(string resPath)
    {
        foreach (var v in uiPathRoots)
        {
            if (resPath.Contains(v.Value.rootPath))
            {
                return v.Value.buildFunc;
            }
        }
        return BuildImpl.Build;
    }
}
