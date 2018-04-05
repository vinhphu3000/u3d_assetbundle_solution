using UnityEngine;
using System.Collections;

using System;
using System.Collections.Generic;
using UnityEngine.Events;

/// <summary>
/// 资源基类，所有资源都应该继承此类
/// 提供基本接口，同步异步加载接口，加载完成回调接口
/// Date: 2016/1/4
/// Author: lxz  
/// </summary> 
public enum ResLoadingState
{
    ///已经卸载
    LOADSTATE_UNLOADED,
    ///加载中
    LOADSTATE_LOADING,
    ///加载完成
    LOADSTATE_LOADED,    
};

public class LoadCompleteTask {

    private List<UnityAction<Asset>> assetFuncs = null;
    private List<UnityAction<UnityEngine.Object>> objFuncs = null;

    public void AddTask(UnityAction<Asset> f)
    {
        if (assetFuncs == null)
            assetFuncs = new List<UnityAction<Asset>>();
        assetFuncs.Add(f);
    }
    public void AddTask(UnityAction<UnityEngine.Object> f)
    {
        if (objFuncs == null)
            objFuncs = new List<UnityAction<UnityEngine.Object>>();
        objFuncs.Add(f);
    }

    public void Call(Asset res)
    {
        if (assetFuncs != null)
        {
            for (int i = 0; i < assetFuncs.Count; i++)
            {
                assetFuncs[i](res);
            }
        }

        if (objFuncs != null)
        {
            for (int i = 0; i < objFuncs.Count; i++)
            {
                objFuncs[i](res.GetMainAsset());
            }
        } 

        Clear();
    }

    public void Clear()
    {
        if (assetFuncs != null)
        {
            assetFuncs.Clear();
        }

        if (objFuncs != null)
        {
            objFuncs.Clear();
        }
    }
}

public abstract class Asset
{    
    protected LoadCompleteTask onCmp = null;

    protected ResLoadingState state = ResLoadingState.LOADSTATE_UNLOADED;
    protected AssetManager creator;
    public string name;
    protected long refCount = 0;

    UnityAction onLoadError = null;

    protected long asyncCoroId = -1;

    public virtual long ReferenceCount
    {
        get 
        {
            return refCount;
        }
    }

    /// <summary>
    /// 当前资源状态
    /// </summary>
    public ResLoadingState ResState
    {
        get 
        {
            return state;
        }
        set
        {
            state = value;
        }
    }

    public Asset(AssetManager resMgr,string name)
    {
        this.creator = resMgr;
        this.name = name;
    }


    public virtual UnityEngine.Object GetMainAsset()
    {
        return null;
    }

    /// <summary>
    /// 加载完成的时候首先调用
    /// </summary>
    protected virtual void onBaseResLoadCompleteSyncCall()
    {

    }

    /// <summary>
    /// 同步加载
    /// </summary>
    public abstract void Load();

    public void AsyncLoad()
    {
        if (state == ResLoadingState.LOADSTATE_UNLOADED)
        {
            asyncCoroId = CoroutineManager.Singleton.startCoroutine(asyncLoadReal());
        }
        else if(state== ResLoadingState.LOADSTATE_LOADED)
        {
            if (onCmp != null)
                onCmp.Call(this);
        }
    }

    protected virtual IEnumerator asyncLoadReal()
    {
        yield return null;
    }


    public void AddCompleteTask(UnityAction<Asset> f)
    {
        if (state== ResLoadingState.LOADSTATE_LOADED)
        {
            f(this);
            return;
        }
        if (onCmp == null)
            onCmp = new LoadCompleteTask();
        onCmp.AddTask(f);
    }


    public void AddCompleteTask(UnityAction<UnityEngine.Object> f)
    {
        if (state == ResLoadingState.LOADSTATE_LOADED)
        {
            f(GetMainAsset());
            return;
        }
        if (onCmp == null)
            onCmp = new LoadCompleteTask();
        onCmp.AddTask(f);
    }


    public void AddLoadErrorTask(UnityAction action)
    {
        if (onLoadError == null)
            onLoadError = action;
        else
        {
            onLoadError += action;
        }
    } 

    /// <summary>
    /// 卸载资源 
    /// </summary>
    public abstract void Unload();

    public AssetManager GetCreator()
    {
        return creator;
    }

    public virtual void AddRef()
    { 
        if (refCount <= 0)
        {
            refCount = 0;
        }
        refCount++; 
    }
    public virtual void RemoveRef()
    {
        refCount--;
        if (refCount <= 0)
        {
            refCount = 0;
        }
    }
}
