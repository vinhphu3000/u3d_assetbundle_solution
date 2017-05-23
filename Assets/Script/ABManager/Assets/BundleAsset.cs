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
            else
            {
#if COMBINE_AB
                string path = IOTools.GetPackageResPath(name);
                if (name == ("common.jpg") || name == ("Config.jpg") || name == ("combine_msg.jpg"))//这三个没有合并，依旧使用ab
                {
                    if (File.Exists(path))
                    {
                        ab = AssetBundle.LoadFromFile(path);
                    }
                }
                else
                {
                    string combineFilePrefix = "";
                    GroupType targetType = GroupType.Scene;
                    if (name.StartsWith("s_"))//场景
                    {
                        combineFilePrefix = "s_combine_";
                        targetType = GroupType.Scene;
                    }
                    else if (name.StartsWith("st_"))//图片
                    {
                        combineFilePrefix = "st_combine_";
                        targetType = GroupType.Texture;
                    }
                    else if (name.StartsWith("m_"))//模型
                    {
                        combineFilePrefix = "m_combine_";
                        targetType = GroupType.Model;
                    }
                    else if (name.StartsWith("eft_"))//特效
                    {
                        combineFilePrefix = "eft_combine_";
                        targetType = GroupType.Effect;
                    }
                    else if (name.StartsWith("c_"))//动作
                    {
                        combineFilePrefix = "c_combine_";
                        targetType = GroupType.Clip;
                    }
                    else
                    {
                        UnityEngine.Debug.LogError("wrong file name:" + name);
                    }

                    var groupList = GameManager.Singleton.combineMsg.groupList;
                    bool isFind = false;
                    FileMsg filMsg = null;
                    for (int i = 0; i < groupList.Count; i++)
                    {
                        if (groupList[i].GroupType == targetType)
                        {
                            var fileList = groupList[i].FileList;
                            for (int j = 0; j < fileList.Count; j++)
                            {
                                if (fileList[j].FileName == name)
                                {
                                    isFind = true;
                                    filMsg = fileList[j];
                                    //UnityEngine.Debug.Log("从合并包里面读取 " + name + "，合并包名：" + IOTools.packageResBasePath + "s_combine_" + groupList[i].GroupIndex + ",        offset:" + (ulong)filMsg.Offset);
                                    ab = AssetBundle.LoadFromFile(IOTools.packageResBasePath + combineFilePrefix + groupList[i].GroupIndex, 0, (ulong)filMsg.Offset);
                                    break;
                                }
                            }
                            if (isFind)
                            {
                                break;
                            }
                        }
                    }
                }
#else
                string path = IOTools.GetPackageResPath(name);

                if (File.Exists(path))
                {
                    ab = AssetBundle.LoadFromFile(path);
                }
#endif
                //UnityEngine.Debug.Log("耗时：" + sw.ElapsedTicks);
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
        //UnityEngine.Debug.LogError("异步加载ab");
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
            yield return asyncLoadAb(name);
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

    public void SyncLoadMatsTex(List<PrefabMaterialHolder.RenderMatTexPair> rendersMatTexInfo, List<Asset> texResRefs)
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
                        var tex = ((TextureAsset)res).GetAsset();
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
                                mat.SetTexture(attName, ((TextureAsset)res).GetAsset());
                                res.AddRef();
                            }
                        }
                    }
                    rendersMatTexInfo[i].renderObj.sharedMaterials = mats;
                }
            }
        }
    }


    public IEnumerator AsyncLoadMatsTex(List<PrefabMaterialHolder.RenderMatTexPair> rendersMatTexInfo, List<Asset> texResRefs, Action<float> OnProcessUpdate = null)
    {
        if (rendersMatTexInfo == null)
            yield break;
        int len = rendersMatTexInfo.Count;

        int asyncStep = Mathf.Max(1, len / 15);

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
                        var res = TextureAssetManager.Singleton.Load(matTexInfos[m].tex2dName);
                        var tex = ((TextureAsset)res).GetAsset();
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
                                var tex = ((TextureAsset)res).GetAsset();
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

    public void removeTexturesRef(List<Asset> texResRefs)
    {
        for (int i = 0; i < texResRefs.Count; i++)
        {
            if (texResRefs[i] != null)
            {
                texResRefs[i].RemoveRef();
            }
        }
    }
}
