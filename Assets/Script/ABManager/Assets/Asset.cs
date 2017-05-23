using UnityEngine;
using System.Collections;

using System;
using System.Collections.Generic; 

/// <summary>
/// 资源基类，所有资源磊都应该继承此类
/// 提供基本接口，同步异步加载接口，加载完成回调接口
/// Date: 2016/1/4
/// Author: lxz  
/// </summary> 
public enum ResLoadingState
{
    /// <summary>
    /// 已经卸载
    /// </summary>
    LOADSTATE_UNLOADED,
    /// <summary>
    /// 加载中
    /// </summary>
    LOADSTATE_LOADING,
    /// <summary>
    /// 加载完成
    /// </summary>
    LOADSTATE_LOADED,
};

public class LoadCompleteTask
{
    private List<AssetAsyncHolder> funcs = null;

    public void AddTask(AssetAsyncHolder task)
    {
        if (funcs == null)
            funcs = new List<AssetAsyncHolder>();
        funcs.Add(task);
    }

    public void Call(Asset res)
    {
        if (funcs != null)
        {
            for (int i = 0; i < funcs.Count; i++)
            {
                try
                {
                    funcs[i].Complete(res);
                }
                catch(Exception e)
                {
                    Debug.LogError("LoadCompleteTask Call Exception:" + e.Message);
                }
            }
        }
        Clear();
    }

    public void Clear()
    {
        if (funcs != null)
        {
            for (int i = 0; i < funcs.Count; i++)
            {
                funcs[i].Clear();
            }
            funcs.Clear();
        }
    }
}

public abstract class Asset
{
    protected LoadCompleteTask onCmp = new LoadCompleteTask();

    protected ResLoadingState state = ResLoadingState.LOADSTATE_UNLOADED;
    protected AssetManager creator;
    public string name;
    protected long refCount = 0;

    Action onLoadError = null;

    public long ReferenceCount
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

    public Asset(AssetManager resMgr, string name)
    {
        this.creator = resMgr;
        this.name = name;
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
            // CoroutineManager.Singleton.AddCoroutine(asyncLoadReal());
            CoroutineManager.Singleton.StartCoroutine(asyncLoadReal());
        }
        else if (state == ResLoadingState.LOADSTATE_LOADED)
        {
            if (onCmp != null)
                onCmp.Call(this);
        }
    }

    /// <summary>
    /// 真正的异步加载
    /// </summary>
    protected virtual IEnumerator asyncLoadReal()
    {
        yield return null;
    }
     
    public void AddAsyncCompleteTask(AssetAsyncHolder task)
    {
        if (task == null)
            return; 
        onCmp.AddTask(task);
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
