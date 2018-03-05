using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary> 
/// 公共纹理资源管理类
/// Date: 2016/1/4
/// Author: lxz  
/// </summary>
public class MeshAssetManager : AssetManager
{
    static MeshAssetManager ins = null;
    public static MeshAssetManager Singleton
    {
        get
        {
            if (ins == null)
            {
                ins = new MeshAssetManager();
                AssetManager.AddResourceMgr(ResourceType.Mesh, ins);
            }
            return ins;
        }
    }
    protected override Asset getOrCreateResourceRefObj(string name)
    {
        var lname = name.toLower();
        Asset res = null;
        if (!assets.TryGetValue(lname, out res))
        {
            res = new MeshAsset(this, lname);
            assets[lname] = res;
        }
        return res;
    }
}
