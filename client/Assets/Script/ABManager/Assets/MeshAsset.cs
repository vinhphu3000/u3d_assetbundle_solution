using UnityEngine;
using System.Collections;

/// <summary>
/// 公共贴图资源类
/// Date: 2016/1/18
/// Author: lxz  
/// </summary>
public class MeshAsset :BundleAsset
{
    Mesh mesh = null;

    public MeshAsset(AssetManager rmgr,string name):base(rmgr,"sm_" +name+IOTools.abSuffix)
    { 
    }


    public override long ReferenceCount
    {
        get
        {
            return refCount;
        }
    }

    public override UnityEngine.Object GetMainAsset()
    {
        return mesh;
    }
        
    protected override void onBaseResLoadCompleteSyncCall()
    {
        if (mesh != null)
        {
            UnityEngine.Debug.LogError("share mesh need Unload:" + name);
        }
        if (bundle != null)
        {
            var assets = bundle.LoadAllAssets();
            if (assets != null && assets.Length > 0)
            {
                mesh = assets[0] as Mesh;
            }
            bundle.Unload(false);
            bundle = null;
        }
    } 
     
    /// <summary>
    /// 卸载资源
    /// </summary>
    public override void Unload()
    { 

        if(state!= ResLoadingState.LOADSTATE_LOADED)
            return;

        if(mesh!=null)
        {
            GameObject.DestroyImmediate(mesh, true);
            mesh = null;
        }

        base.Unload();

        state = ResLoadingState.LOADSTATE_UNLOADED;
    }
}
