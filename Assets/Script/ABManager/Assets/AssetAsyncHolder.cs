using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate bool AssetTaskLoadedCheck(string name,long id);
public class AssetAsyncHolder
{
    private AssetTaskLoadedCheck check;
    private Action<Asset> onLoaded;
    private Action<string, AssetAsyncHolder> startLoadFunc;
    private string resName;
    public long id;
    public void SetPara(string resName, Action<string, AssetAsyncHolder> loadFunc,long id = -1)
    {
        this.id = id;
        this.resName = resName;
        this.startLoadFunc = loadFunc;
    }

    public AssetAsyncHolder SetCheck(AssetTaskLoadedCheck f)
    {
        if (check == null)
            check = f;
        else
            check += f;
         
        return this;
    }

    public AssetAsyncHolder SetOnComplete(Action<Asset> f)
    {
        if (onLoaded == null)
            onLoaded = f;
        else
            onLoaded += f;
        return this;
    }

    public void Complete(Asset asset)
    {
        if (check == null || check(resName,id))
        {
            if (onLoaded != null)
                onLoaded(asset);
        }
    }

    public void Start()
    {
        startLoadFunc(resName, this);
    }
    public void Clear()
    {
        check = null;
        onLoaded = null;
        startLoadFunc = null;
        id = -1;
    }
}
