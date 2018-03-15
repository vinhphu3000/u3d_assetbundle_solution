using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class BuildFGUIResBundle {


    public static string UIResourceBasePath = "assets/resources/ui";
    public static string bundlePath = Application.dataPath + "/../ResEX_UI/" +EditorUserBuildSettings.activeBuildTarget.ToString();


    public static BuildAssetBundleOptions options = BuildAssetBundleOptions.DeterministicAssetBundle |
        BuildAssetBundleOptions.ChunkBasedCompression |
        BuildAssetBundleOptions.ForceRebuildAssetBundle;

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
                return ".png";
            }
            return ".jpg";
        }
    }

    [MenuItem("GAME/打包/Build FGUI Bundles")]
    public static void Builde()
    { 
        foreach (var asset in Selection.objects)
        {
            var assetPath = AssetDatabase.GetAssetPath(asset).Replace("\\","/");
            //判断是否在fgui资源目录下
            if (assetPath.ToLower().Contains(UIResourceBasePath))
            {
                var assetBaseDir = assetPath.Replace(Path.GetFileName(assetPath), "");
                //得到包所有的资源 
                var packageName = Path.GetFileNameWithoutExtension(assetPath).Split('@')[0]; 
                 
                var abpath = Path.GetDirectoryName(Path.GetFullPath( Application.dataPath + "/../"+assetPath));
                //得到路径下所有资源，找出同一个包的资源
                List<string> assetsPaths = new List<string>();
                foreach (var file in Directory.GetFiles(abpath))
                {
                    var fileName = Path.GetFileName(file);
                    if (fileName.Contains(packageName)&&!fileName.EndsWith("meta"))
                    { 
                        assetsPaths.Add(Path.Combine(assetBaseDir,fileName)); 
                        Debug.LogError(Path.Combine(assetBaseDir,fileName));
                    }
                }
                //打包
                if (assetsPaths.Count > 0)
                {
                    AssetBundleBuild[] buildMap = new AssetBundleBuild[1]; 
                    buildMap[0].assetBundleName = packageName + abSuffix;
                    buildMap[0].assetNames = assetsPaths.ToArray();

                    BuildPipeline.BuildAssetBundles(bundlePath, buildMap,options,EditorUserBuildSettings.activeBuildTarget); 
                }
            }
        } 

        //删除多余文件 
        foreach (var v in Directory.GetFileSystemEntries(bundlePath))
        {
            if (!v.EndsWith(".png") && !v.EndsWith(".jpg"))
            {
                File.Delete(v);
            }
        } 
    }
}
 