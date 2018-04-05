using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


/// <summary> 
/// 场景资源管理类
/// Date: 2016/1/20
/// Author: lxz  
/// </summary>

public class SceneAssetManager : AssetManager
{
    static SceneAssetManager ins = null;
    public static SceneAssetManager Singleton
    {
        get
        {
            if (ins == null)
            {
                ins = new SceneAssetManager();
                AssetManager.AddResourceMgr(ResourceType.SceneObject, ins);
            }
            return ins;
        }
    }

    protected override Asset getOrCreateResourceRefObj(string name)
    {
        var lname = name.ToLower();
        Asset res = null;
        if (!assets.TryGetValue(lname, out res))
        {
            res = new SceneAsset(this, name);
            assets[lname] = res;
        }
        return res;
    }

    public void LoadScene(string name, UnityEngine.Events.UnityAction<float> onProcessUpdate, UnityEngine.Events.UnityAction OnComplete,UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        //SceneAsset sr = new SceneAsset(this, name);
        SceneAsset sr = SceneAssetManager.Singleton.getOrCreateResourceRefObj(name) as SceneAsset;
        sr.mode = mode;
        sr.AddLoadUpdateCall(onProcessUpdate);
        sr.AddAllCompleteCall(OnComplete);
        //启动异步加载
        CoroutineManager.Singleton.startCoroutine(sr.asyncLoad());
    }

    [Obsolete("scene asset not support sync Load. Use  LoadScene(string name,Action<float> onProcessUpdate, Action OnComplete) instead.", true)]
    public override Asset Load(string name)
    {
        Debug.LogError("Load scene error,use LoadScene function!!");
        return null;
    }
}
