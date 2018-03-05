using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using UnityEngine.AI;

/// <summary>
/// 打包基类
/// Date: 2016/1/5
/// Author: lxz  
/// </summary>
public class BuildImpl
{
    public class MaterialTexturePair
    {
        public Material mat;
        public string attribute;
        public Texture tex;
    }
    public static void Build(Object obj)
    {
        Debug.LogError(">>>>>can not build object:" + AssetDatabase.GetAssetOrScenePath(obj));
    }


    public static void SaveOldMatFile(Object asset)
    {
        string[] depAssets = AssetDatabase.GetDependencies(new string[] { AssetDatabase.GetAssetPath(asset) });


        //Debug.LogError("asset:" + asset.name + ", assetPath:" + AssetDatabase.GetAssetPath(asset) + ", depAssets:" + (depAssets != null ? depAssets.Length.ToString() : "null"));

        if (depAssets != null)
            foreach (var path in depAssets)
            {
                //如果没发现材质错误
                if (path.EndsWith(".mat") && path.Contains("Assets"))
                {
                    Material mat = AssetDatabase.LoadAssetAtPath(path, typeof(Material)) as Material;
                    //如果没发现材质错误
                    if (!MatHelper.FindMatError(mat))
                    {
                        //强写到备份目录 
                        string physicPath = path.Replace("Assets/", Application.dataPath + "/");
                        string backupPath = path.Replace("Assets/", Application.dataPath + "/../BACKUP_NOT_TO_SVN/");
                        string backupDir = backupPath.Replace(Path.GetFileName(backupPath), "");

                        if (!Directory.Exists(backupDir))
                        {
                            try
                            {
                                Directory.CreateDirectory(backupDir);
                            }
                            catch (System.Exception ex)
                            {
                                Debug.LogWarning("无效路径!" + ex.Message + "   " + backupDir);
                                continue;
                            }
                        }
                        File.Copy(physicPath, backupPath, true);
                    }
                    else //如果发现错误，且备份目录有，则直接还原
                    {
                        string physicPath = path.Replace("Assets/", Application.dataPath + "/");
                        string backupPath = path.Replace("Assets/", Application.dataPath + "/../BACKUP_NOT_TO_SVN/");
                        if (File.Exists(backupPath))
                        {
                            File.Copy(backupPath, physicPath, true);
                        }
                    }
                }
            }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
    }

    public static void RevertMatFile(Object asset, List<MaterialTexturePair> oldMatTexInfos)
    {
        if (oldMatTexInfos != null)
        {
            foreach (var v in oldMatTexInfos)
            {
                v.mat.SetTexture(v.attribute, v.tex);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        EditorUtility.UnloadUnusedAssetsImmediate();

        string[] depAssets = AssetDatabase.GetDependencies(new string[] { AssetDatabase.GetAssetPath(asset) });
        if (depAssets != null)
        {
            foreach (var path in depAssets)
            {
                if (path.EndsWith(".mat") && path.Contains("Assets"))
                {
                    string physicPath = path.Replace("Assets/", Application.dataPath + "/");
                    string backupPath = path.Replace("Assets/", Application.dataPath + "/../BACKUP_NOT_TO_SVN/");
                    if (File.Exists(backupPath))
                        File.Copy(backupPath, physicPath, true);
                }
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="prefab"></param>
    public static List<MaterialTexturePair> RemovePrefabMatTex(GameObject prefab)
    {
        SaveOldMatFile(prefab);

        var pmh = prefab.GetComponent<PrefabRenderHolder>();
        if (pmh != null)
        {
            GameObject.DestroyImmediate(pmh, true);
        }
        pmh = prefab.AddComponent<PrefabRenderHolder>();//为场景根节点增加PrefabRenderHolder组件，用以保存场景对图片的依赖
        pmh.rendersMatTexInfo = new List<PrefabRenderHolder.RenderMatTexPair>();

        var oldMatTexInfos = new List<MaterialTexturePair>();
        //保存需要打包的图片
        var needBuildTexs = new Dictionary<Texture, string>();
        //得到所有render对象
        var renders = new List<Renderer>();
        prefab.GetComponentsInChildren<Renderer>(true, renders);//对非active物体也有效
        for (int i = 0; i < renders.Count; i++)
        {
            var mats = renders[i].sharedMaterials;
            if (mats != null && mats.Length > 0)
            {
                var rmp = new PrefabRenderHolder.RenderMatTexPair();
                rmp.renderObj = renders[i];
                //保存每个render下所有材质的贴图信息
                var matis = new List<PrefabRenderHolder.MaterialAllTextureInfo>();

                for (int j = 0; mats != null && j < mats.Length; j++)
                {
                    //保存每个材质的所有纹理信息
                    var mtis = new List<PrefabRenderHolder.MaterialTextureInfo>();
                    //得到每个材质的纹理信息 
                    for (int si = 0; mats[j] != null && si < ShaderUtil.GetPropertyCount(mats[j].shader); si++)
                    {
                        var st = ShaderUtil.GetPropertyType(mats[j].shader, si);
                        if (st == ShaderUtil.ShaderPropertyType.TexEnv)
                        {
                            string strProperty = ShaderUtil.GetPropertyName(mats[j].shader, si);
                            Texture tex = mats[j].GetTexture(strProperty);
                            if (tex == null)
                                continue;
                            string texPath = AssetDatabase.GetAssetPath(tex);
                            if (!texPath.Contains("Assets"))
                                continue;
                            //保存老材质信息
                            oldMatTexInfos.Add(new MaterialTexturePair { mat = mats[j], attribute = strProperty, tex = tex });
                            if (!needBuildTexs.ContainsKey(tex))
                            {
                                needBuildTexs.Add(tex, texPath);
                            }
                            //保存到打包信息对象 
                            mtis.Add(new PrefabRenderHolder.MaterialTextureInfo { attribute = strProperty, tex2dName = tex.name.ToLower() });
                        }
                    }
                    if (mtis.Count > 0)
                    {
                        matis.Add(new PrefabRenderHolder.MaterialAllTextureInfo { matIndex = j, matTexInfos = mtis.ToArray() });
                    }
                }
                if (matis.Count > 0)
                {
                    rmp.matAllInfos = matis.ToArray();
                    pmh.rendersMatTexInfo.Add(rmp);
                }
            }
        }

        //打包图片
        foreach (var v in needBuildTexs)
        {
            AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
            buildMap[0].assetBundleName = "st_" + v.Key.name.ToLower() + BuildConfig.abSuffix;
            buildMap[0].assetNames = new string[] { v.Value };
            BuildPipeline.BuildAssetBundles(BuildConfig.abOutputPath, buildMap, BuildConfig.options, EditorUserBuildSettings.activeBuildTarget);

        }
        //去掉材质图片信息
        foreach (var v in oldMatTexInfos)
        {
            v.mat.SetTexture(v.attribute, null);
        }
        return oldMatTexInfos;
    }

    /// <summary>
    /// 得到依赖的资源
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    public static List<AssetBundleBuild> GetDepTexs(GameObject prefab)
    {
        List<AssetBundleBuild> buildTexs = new List<AssetBundleBuild>();

        var pmh = prefab.GetComponent<PrefabRenderHolder>();
        if (pmh != null)
        {
            GameObject.DestroyImmediate(pmh, true);
        }

        pmh = prefab.AddComponent<PrefabRenderHolder>();//为场景根节点增加PrefabRenderHolder组件，用以保存场景对图片的依赖
        pmh.rendersMatTexInfo = new List<PrefabRenderHolder.RenderMatTexPair>();

        //保存需要打包的图片
        var needBuildTexs = new Dictionary<Texture, string>();
        //得到所有render对象
        var renders = new List<Renderer>();
        prefab.GetComponentsInChildren<Renderer>(true, renders);//对非active物体也有效
        for (int i = 0; i < renders.Count; i++)
        {
            var mats = renders[i].sharedMaterials;
            if (mats != null && mats.Length > 0)
            {
                var rmp = new PrefabRenderHolder.RenderMatTexPair();
                rmp.renderObj = renders[i];
                //保存每个render下所有材质的贴图信息
                var matis = new List<PrefabRenderHolder.MaterialAllTextureInfo>();

                for (int j = 0; mats != null && j < mats.Length; j++)
                {
                    //保存每个材质的所有纹理信息
                    var mtis = new List<PrefabRenderHolder.MaterialTextureInfo>();
                    //得到每个材质的纹理信息 
                    for (int si = 0; mats[j] != null && si < ShaderUtil.GetPropertyCount(mats[j].shader); si++)
                    {
                        var st = ShaderUtil.GetPropertyType(mats[j].shader, si);
                        if (st == ShaderUtil.ShaderPropertyType.TexEnv)
                        {
                            string strProperty = ShaderUtil.GetPropertyName(mats[j].shader, si);
                            Texture tex = mats[j].GetTexture(strProperty);
                            if (tex == null)
                                continue;
                            string texPath = AssetDatabase.GetAssetPath(tex);
                            if (!texPath.Contains("Assets"))
                                continue;
                            //保存老材质信息
                            if (!needBuildTexs.ContainsKey(tex))
                            {
                                needBuildTexs.Add(tex, texPath);
                            }
                            //保存到打包信息对象 
                            mtis.Add(new PrefabRenderHolder.MaterialTextureInfo { attribute = strProperty, tex2dName = tex.name.ToLower() });
                        }
                    }
                    if (mtis.Count > 0)
                    {
                        matis.Add(new PrefabRenderHolder.MaterialAllTextureInfo { matIndex = j, matTexInfos = mtis.ToArray() });
                    }
                }
                if (matis.Count > 0)
                {
                    rmp.matAllInfos = matis.ToArray();
                    pmh.rendersMatTexInfo.Add(rmp);
                }
            }
        }

        //打包图片
        foreach (var v in needBuildTexs)
        {
            AssetBundleBuild abb = new AssetBundleBuild();
            abb.assetBundleName = "st_" + v.Key.name.ToLower() + BuildConfig.abSuffix;
            abb.assetNames = new string[] { v.Value };
            buildTexs.Add(abb);
        }
        return buildTexs;
    }
}
