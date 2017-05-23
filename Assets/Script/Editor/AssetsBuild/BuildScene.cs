using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using UnityEditor.SceneManagement;

/// <summary>
/// 打包场景及其依赖项
/// 针对terrain组件特殊处理
/// 带有terrain的地形资源，采用ab依赖的方式处理...
/// Date: 2016/1/20
/// Author: lxz  
/// </summary>
public class BuildScene : BuildImpl
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="obj">Object.</param>
    public static new void Build(Object obj)
    {
        string assetPath = AssetDatabase.GetAssetPath(obj);
        if (!assetPath.EndsWith(".unity"))
        {
            return;
        }
        //创建临时场景 
        string temp_main_path = "Assets/temp/" + Path.GetFileName(assetPath);
        AssetDatabase.DeleteAsset(temp_main_path);
        AssetDatabase.CopyAsset(assetPath, temp_main_path);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        //打开场景 
        var scene = EditorSceneManager.OpenScene(temp_main_path);
        //查找根节点对象
        if (scene.rootCount > 1)
            Debug.LogError("场景不止一个根节点："+scene.name);
        var root = scene.GetRootGameObjects()[0];
        if (root == null)
        {
            Debug.LogError("打包场景失败：" + obj.name + ", 场景必须有一个root对象，且root对象名称和场景名称一致");
            AssetDatabase.DeleteAsset(temp_main_path);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            return;
        }

  
        //去掉贴图引用
        //var oldMatInfos = RemovePrefabMatTex(root);
        var texsDeps = GetDepTexs(root);

        /*
#region //去掉mesh资源?
        List<MeshFilter> mfs = new List<MeshFilter>();
        root.GetComponentsInChildren<MeshFilter>(mfs);
        var mah = root.AddComponent<MeshAssetHolder>();

        Dictionary<Mesh, string> needBuildMeshs = new Dictionary<Mesh, string>();
        foreach (var v in mfs)
        {
            string abname = AssetDatabase.GetAssetPath(v.sharedMesh);
            if (!abname.Contains("Assets"))
                continue;
            abname = Path.GetFileNameWithoutExtension(abname).ToLower();
            abname = abname.Replace(" ", "");
            abname = "sm_" + abname + "_" + v.sharedMesh.name.ToLower() + BuildConfig.abSuffix;
            mah.meshAssets.Add(new MeshAssetHolder.MeshFilterPair { meshFilter = v, meshAbName = abname });
            needBuildMeshs[v.sharedMesh] = abname;
            //设置成空
            v.sharedMesh = null;
        }

        //编译mesh资源
        foreach (var v in needBuildMeshs)
        {
            //创建mesh资源在临时路径
            string temp_mesh_path = "Assets/temp/" + v.Key.name + ".asset";
            AssetDatabase.CreateAsset(Object.Instantiate(v.Key), temp_mesh_path);
            AssetDatabase.Refresh();

            AssetBundleBuild[] bm = new AssetBundleBuild[1];
            bm[0].assetBundleName = v.Value;
            bm[0].assetNames = new string[] { temp_mesh_path };
            BuildPipeline.BuildAssetBundles(BuildConfig.abOutputPath, bm, BuildConfig.options, EditorUserBuildSettings.activeBuildTarget);

            AssetDatabase.DeleteAsset(temp_mesh_path);
        }

        EditorUtility.SetDirty(root);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
#endregion
         * */

        /*
        //分离地形ab资源
        var buildmap = getTerrainDepAssets();

        //将依赖的ab写入文件
        BinaryWriter depWriter = new BinaryWriter(new FileStream(BuildConfig.abOutputPath + "/" + "s_" + obj.name.ToLower() + "_dep" + BuildConfig.abSuffix, FileMode.Create));
        depWriter.Write(buildmap.Count);
        foreach (var dep in buildmap)
        {
            depWriter.Write(dep.assetBundleName);
        }
        depWriter.Flush();
        depWriter.Close();
        
        
        //打包预制件 
        AssetBundleBuild[] buildMap = new AssetBundleBuild[2 + buildmap.Count];
        buildMap[0] = BuildCommand.getCommonDep();

        for (int i = 1; i < buildmap.Count; i++)
        {
            buildMap[i] = buildmap[i - 1];
        }

        buildMap[buildMap.Length - 1].assetBundleName = "s_" + obj.name.ToLower() + BuildConfig.abSuffix;
        buildMap[buildMap.Length - 1].assetNames = new string[] { temp_main_path };
        BuildPipeline.BuildAssetBundles(BuildConfig.abOutputPath, buildMap, BuildConfig.options, EditorUserBuildSettings.activeBuildTarget);
           * */

        //打包预制件 
        AssetBundleBuild[] buildMap = new AssetBundleBuild[2 + texsDeps.Count];
        buildMap[0] = BuildCommand.getCommonDep();

        for (int i = 0; i < texsDeps.Count; i++)
        {
            buildMap[i + 1] = texsDeps[i];
        }

        buildMap[buildMap.Length - 1].assetBundleName = "s_" + obj.name.ToLower() + BuildConfig.abSuffix;
        buildMap[buildMap.Length - 1].assetNames = new string[] { temp_main_path };
        BuildPipeline.BuildAssetBundles(BuildConfig.abOutputPath, buildMap, BuildConfig.options, EditorUserBuildSettings.activeBuildTarget);
        //还原材质
        //RevertMatFile(root, oldMatInfos);


        ///////////////////////////////////////////////
        //打开场景 
        EditorSceneManager.OpenScene(assetPath);

        AssetDatabase.DeleteAsset(temp_main_path);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
    }

    /// <summary>
    /// 处理地形数据依赖
    /// </summary>
    /// <returns></returns>
    static List<AssetBundleBuild> getTerrainDepAssets()
    {
        List<AssetBundleBuild> buildAssets = new List<AssetBundleBuild>();

        Terrain[] ts = GameObject.FindObjectsOfType<Terrain>();
        if (ts != null)
        {
            Dictionary<Texture, int> temp = new Dictionary<Texture, int>();
            Dictionary<GameObject, int> tempObjs = new Dictionary<GameObject, int>();
            string path = "";
            foreach (var terrain in ts)
            {
                //处理地形贴图
                var splats = terrain.terrainData.splatPrototypes;
                if (splats != null)
                {
                    foreach (var splat in splats)
                    {
                        var texture = splat.texture;
                        if (texture != null && (path = AssetDatabase.GetAssetPath(texture)).Contains("Assets"))
                        {
                            if (!temp.ContainsKey(texture))
                            {
                                AssetBundleBuild abb = new AssetBundleBuild();
                                abb.assetBundleName = "st_" + texture.name.ToLower() + BuildConfig.abSuffix;
                                abb.assetNames = new string[] { path };
                                //add asset
                                buildAssets.Add(abb);
                            }
                            temp[texture] = 0;
                        }
                    }
                }
                //处理花儿纹理
                var details = terrain.terrainData.detailPrototypes;
                if (details != null)
                {
                    foreach (var detail in details)
                    {
                        var texture = detail.prototypeTexture;
                        if (texture != null && (path = AssetDatabase.GetAssetPath(texture)).Contains("Assets"))
                        {
                            if (!temp.ContainsKey(texture))
                            {
                                AssetBundleBuild abb = new AssetBundleBuild();
                                abb.assetBundleName = "st_" + texture.name.ToLower() + BuildConfig.abSuffix;
                                abb.assetNames = new string[] { path };
                                //add asset
                                buildAssets.Add(abb);
                            }
                            temp[texture] = 0;
                        }

                        var prefab = detail.prototype;
                        if (prefab != null && (path = AssetDatabase.GetAssetPath(prefab)).Contains("Assets"))
                        {
                            if (!tempObjs.ContainsKey(prefab))
                            {
                                AssetBundleBuild abb = new AssetBundleBuild();
                                abb.assetBundleName = "smod_" + prefab.name.ToLower() + BuildConfig.abSuffix;
                                abb.assetNames = new string[] { path };
                                //add asset
                                buildAssets.Add(abb);
                            }
                            tempObjs[prefab] = 0;
                        }
                    }
                }
                //处理树prefab
                var trees = terrain.terrainData.treePrototypes;
                if (trees != null)
                {
                    foreach (var tree in trees)
                    {
                        var prefab = tree.prefab;
                        if (prefab != null && (path = AssetDatabase.GetAssetPath(prefab)).Contains("Assets"))
                        {
                            if (!tempObjs.ContainsKey(prefab))
                            {
                                AssetBundleBuild abb = new AssetBundleBuild();
                                abb.assetBundleName = "smod_" + prefab.name.ToLower() + BuildConfig.abSuffix;
                                abb.assetNames = new string[] { path };
                                //add asset
                                buildAssets.Add(abb);
                            }
                            tempObjs[prefab] = 0;
                        }
                    }
                }
            }
        }
        return buildAssets;
    }
}
