using UnityEngine;
using System.Collections;
using System;

using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// 二进制和文本资源基类
/// Date: 2016/1/4
/// Author: lxz  
/// </summary>

public class BundleAsset : Asset
{
    public AssetBundle bundle = null;

    public BundleAsset(AssetManager rmgr, string name) : base(rmgr, name)
    {
    }

    public override UnityEngine.Object GetMainAsset()
    {
        return bundle;
    }
    /// <summary>
    /// 同步加载ab
    /// </summary> 
    public static AssetBundle LoadAb(string name)
    {
        //UnityEngine.Debug.Log("begin load ab:" + name);
        //Stopwatch sw = new Stopwatch();
        //sw.Start();
        AssetBundle ab = null;
        if (IOTools.IsResInUpdateDir(name))
        {
            string path = IOTools.getUpdateResPath(name);
            ab = AssetBundle.LoadFromFile(path);
        }
        else
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                string path = IOTools.GetPackageResPath(name);
                ab = AssetBundle.LoadFromFile(path);
            }
            else  //ios 判断目录，没有直接下载
            { 
                string path = IOTools.GetPackageResPath(name);

                if (File.Exists(path))
                {
                    ab = AssetBundle.LoadFromFile(path);
                }
                else
                {
                    UnityEngine.Debug.LogError("不能加载到资源:" + path);
                } 
            }
        }

        if (ab == null)
        {
            UnityEngine.Debug.LogError("Load bundle error：" + name);
        }
        return ab;
    }

    /// <summary>
    /// 异步加载一个ab
    /// 返回AssetBundleCreateRequest
    /// </summary> 
    protected static AssetBundleCreateRequest asyncLoadAb(string name)
    {
        //Logger.err("异步加载ab");
        AssetBundleCreateRequest abReq = null;
        if (IOTools.IsResInUpdateDir(name))
        {
            string path = IOTools.getUpdateResPath(name);
            abReq = AssetBundle.LoadFromFileAsync(path);
        }
        else
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                string path = IOTools.GetPackageResPath(name);
                abReq = AssetBundle.LoadFromFileAsync(path);
            }
            else  //ios 判断目录
            {
                string path = IOTools.GetPackageResPath(name);
                if (File.Exists(path))
                {
                    abReq = AssetBundle.LoadFromFileAsync(path);
                }
            }
        }
        return abReq;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator asyncLoadReal()
    {
        if (state == ResLoadingState.LOADSTATE_UNLOADED)
        {
            state = ResLoadingState.LOADSTATE_LOADING;
            var req = asyncLoadAb(name);
            if (req == null)
                yield break;
            yield return req;

            bundle = req.assetBundle;

            refCount = 0;
            onBaseResLoadCompleteSyncCall();
            if (onCmp != null)
                onCmp.Call(this);
            state = ResLoadingState.LOADSTATE_LOADED;
        }
        yield return null;
    }

    /// <summary>
    /// 同步加载ab
    /// </summary> 
    public override void Load()
    {
        if (state == ResLoadingState.LOADSTATE_UNLOADED)
        {
            bundle = LoadAb(name);
            refCount = 0;
            onBaseResLoadCompleteSyncCall();
            if (onCmp != null)
                onCmp.Call(this);
            state = ResLoadingState.LOADSTATE_LOADED;
            refCount = 0;
        }
    }

    /// <summary>
    /// 卸载资源
    /// </summary>
    public override void Unload()
    {
        if (bundle != null)
        {
            bundle.Unload(true);
            bundle = null;
        }
        state = ResLoadingState.LOADSTATE_UNLOADED;
        refCount = 0;
    }

    public void SyncLoadMatsTex(List<PrefabRenderHolder.RenderMatTexPair> rendersMatTexInfo, List<Asset> texResRefs)
    {
        if (rendersMatTexInfo == null)
            return;
        int len = rendersMatTexInfo.Count;
        for (int i = 0; i < len; i++)
        {
            if (rendersMatTexInfo[i].renderObj == null)
            {
                continue;
            }
            var renderMatInfos = rendersMatTexInfo[i].matAllInfos;
            if (renderMatInfos != null)
            {
                if (renderMatInfos.Length == 1)
                {
                    var matTexInfos = renderMatInfos[0].matTexInfos;
                    for (int m = 0; m < matTexInfos.Length; m++)
                    {
                        var mat = rendersMatTexInfo[i].renderObj.sharedMaterial;
                        var attName = matTexInfos[m].attribute;
                        TextureAsset res = (TextureAsset)TextureAssetManager.Singleton.Load(matTexInfos[m].tex2dName);
                        var tex = res.GetMainAsset() as Texture;
                        if (tex != null)
                        {
                            mat.SetTexture(attName, tex);
                            res.AddRef();
                            //记录加载了哪些贴图
                            texResRefs.Add(res);
                        }
                    }
                }
                else
                {
                    var mats = rendersMatTexInfo[i].renderObj.sharedMaterials;
                    for (int j = 0; j < renderMatInfos.Length; j++)
                    {
                        if (mats.Length > renderMatInfos[j].matIndex && mats[j] != null)
                        {
                            var mtis = renderMatInfos[j].matTexInfos;
                            for (int n = 0; n < mtis.Length; n++)
                            {
                                var mat = mats[renderMatInfos[j].matIndex];
                                var attName = mtis[n].attribute;
                                TextureAsset res = (TextureAsset)TextureAssetManager.Singleton.Load(mtis[n].tex2dName);
                                mat.SetTexture(attName, res.GetMainAsset() as Texture);
                                res.AddRef();
                                //记录加载了哪些贴图
                                texResRefs.Add(res);
                            }
                        }
                    }
                    rendersMatTexInfo[i].renderObj.sharedMaterials = mats;
                }
            }
        }
    }

    public void SyncLoadPrefabMesh(List<PrefabRenderHolder.RenderMeshPair> rendersMeshInfos, List<Asset> meshResRefs)
    {
        if (rendersMeshInfos == null)
            return;
        int len = rendersMeshInfos.Count;
        for (int i = 0; i < len; i++)
        {
            if (rendersMeshInfos[i].meshFilterObj == null)
            {
                continue;
            } 

            MeshAsset res = (MeshAsset)MeshAssetManager.Singleton.Load(rendersMeshInfos[i].meshName);
            rendersMeshInfos[i].meshFilterObj.sharedMesh = res.GetMainAsset() as Mesh;
            res.AddRef();
            //记录加载了哪些贴图
            meshResRefs.Add(res);
        }
    }
    public IEnumerator AsyncLoadPrefabMesh(List<PrefabRenderHolder.RenderMeshPair> rendersMeshInfos, List<Asset> meshResRefs)
    {
        if (rendersMeshInfos == null)
            yield break;
        int len = rendersMeshInfos.Count;
        for (int i = 0; i < len; i++)
        {
            if (rendersMeshInfos[i].meshFilterObj == null)
            {
                continue;
            }

            MeshAsset res = (MeshAsset)MeshAssetManager.Singleton.Load(rendersMeshInfos[i].meshName);
            rendersMeshInfos[i].meshFilterObj.sharedMesh = res.GetMainAsset() as Mesh;
            res.AddRef();
            //记录加载了哪些贴图
            meshResRefs.Add(res);
            yield return null;
        }
    }

    public IEnumerator AsyncLoadMatsTex(List<PrefabRenderHolder.RenderMatTexPair> rendersMatTexInfo, List<Asset> texResRefs, Action<float> OnProcessUpdate = null)
    {
        if (rendersMatTexInfo == null)
            yield break;
        int len = rendersMatTexInfo.Count;

        int asyncStep = Mathf.Max(1, len / 15);

        for (int i = 0; i < len; i++)
        {
            if (rendersMatTexInfo[i]==null || rendersMatTexInfo[i].renderObj == null)
            {
                continue;
            }
            var renderMatInfos = rendersMatTexInfo[i].matAllInfos;
            if (renderMatInfos != null)
            {
                if (renderMatInfos.Length == 1)
                {
                    var matTexInfos = renderMatInfos[0].matTexInfos;
                    if(matTexInfos!=null)
                    {
                        for (int m = 0; m < matTexInfos.Length; m++)
                        {
                            var mat = rendersMatTexInfo[i].renderObj.sharedMaterial;
                            var attName = matTexInfos[m].attribute;
                            var res = TextureAssetManager.Singleton.Load(matTexInfos[m].tex2dName);
                            var tex = ((TextureAsset)res).GetMainAsset() as Texture;
                            if (tex != null)
                            {
                                mat.SetTexture(attName, tex);
                                res.AddRef();
                                //记录加载了哪些贴图
                                texResRefs.Add(res);
                            }
                            if (i % asyncStep == 0)
                                yield return null;
                        }
                    } 
                }
                else
                {
                    var mats = rendersMatTexInfo[i].renderObj.sharedMaterials;
                    for (int j = 0; j < renderMatInfos.Length; j++)
                    {
                        if (mats.Length > renderMatInfos[j].matIndex && mats[j] != null)
                        {
                            var mtis = renderMatInfos[j].matTexInfos;
                            for (int n = 0; n < mtis.Length; n++)
                            {
                                var mat = mats[renderMatInfos[j].matIndex];
                                var attName = mtis[n].attribute;
                                var res = TextureAssetManager.Singleton.Load(mtis[n].tex2dName);
                                var tex = ((TextureAsset)res).GetMainAsset() as Texture;
                                if (tex != null)
                                {
                                    mat.SetTexture(attName, tex);
                                    res.AddRef();
                                    texResRefs.Add(res);
                                }
                                if (i % asyncStep == 0)
                                    yield return null;
                            }
                        }
                    }
                    rendersMatTexInfo[i].renderObj.sharedMaterials = mats;
                }
            }
            if (i % asyncStep == 0)
                yield return null;
            if (OnProcessUpdate != null)
            {
                OnProcessUpdate(i * 1.0f / len);
            }
        }
        if (OnProcessUpdate != null)
        {
            OnProcessUpdate(1f);
        }
    }

    protected void removeTexturesRef(List<Asset> texResRefs)
    {
        for (int i = 0; i < texResRefs.Count; i++)
        {
            if (texResRefs[i] != null)
            {
                texResRefs[i].RemoveRef();
            }
        }
    }

    protected void removeMeshRef(List<Asset> meshResRefs)
    {
        for (int i = 0; i < meshResRefs.Count; i++)
        {
            if (meshResRefs[i] != null)
            {
                meshResRefs[i].RemoveRef();
            }
        }
    }
}
