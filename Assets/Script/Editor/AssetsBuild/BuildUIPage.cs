using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

/// <summary>
/// 打包ui页面
/// Date: 2016/1/5
/// Author: lxz  
/// </summary>
public class BuildUIPage:BuildImpl
{
    /// <summary>
    /// 编译ui
    /// </summary>
    /// <param name="obj">Object.</param>
    public static void Build(Object obj)
    {
        string prefabPath = AssetDatabase.GetAssetPath(obj);
        if (!prefabPath.EndsWith(".prefab"))
        { 
            return;  
        }
        //去掉图集和字体依赖  
        string temp_main_path = "Assets/temp/" + obj.name + ".prefab";
        AssetDatabase.DeleteAsset(temp_main_path); 
        AssetDatabase.CopyAsset(prefabPath, temp_main_path);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        //加载临时资源
        var tempPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(temp_main_path);
        if (tempPrefab == null)
        {
            Debug.LogError("加载临时ui prefab失败："+temp_main_path);
            return;
        }
        var images = tempPrefab.GetComponentsInChildren<UnityEngine.UI.Image>();
        var texts = tempPrefab.GetComponentsInChildren<UnityEngine.UI.Text>();

        Dictionary<Texture,int> needBuildAtlas = new Dictionary<Texture, int>();
        if (images != null)
        {
            var holder = tempPrefab.AddComponent<GuiHolder>();
            holder.depAtlass = new List<GuiHolder.ImageDic>();
        
            //保存image sprite信息
            foreach (var image in images)
            {
                if (image.sprite == null)
                    continue;
                holder.depAtlass.Add(
                    new GuiHolder.ImageDic
                    {
                        img = image,
                        textureName = image.sprite.texture.name,
                        spriteName = image.sprite.name
                    }
                ); 
                needBuildAtlas[image.sprite.texture] = 0;
                image.sprite = null;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
         
        //打包图集
        foreach (var v in needBuildAtlas)
        {
            BuildUIAtlas.Build(v.Key);
        } 

        //打包预制件
        AssetBundleBuild[] buildMap = new AssetBundleBuild[2];
        buildMap[0] = BuildCommand.getCommonDep();
        buildMap[1].assetBundleName = "uiw_" + obj.name.ToLower() + BuildConfig.abSuffix;
        buildMap[1].assetNames = new string[]{ temp_main_path };
        BuildPipeline.BuildAssetBundles(BuildConfig.abOutputPath,buildMap,BuildConfig.options,EditorUserBuildSettings.activeBuildTarget);
        //处理raw image


        //删除临时资源
        AssetDatabase.DeleteAsset(temp_main_path); 
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        //所有UI窗口特效通过代码添加,所以这里不处理特效图片分离

        //得到所有资源路径  
        }
}
