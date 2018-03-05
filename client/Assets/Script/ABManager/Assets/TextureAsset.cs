using UnityEngine;
using System.Collections;

/// <summary>
/// 公共贴图资源类
/// Date: 2016/1/18
/// Author: lxz  
/// </summary>
public class TextureAsset :BundleAsset
{ 
    Texture texture = null; 
    string textureName;

    public TextureAsset(AssetManager rmgr,string name):base(rmgr,"st_"+name+IOTools.abSuffix)
    { 
        textureName = name;
    }
 
    public override UnityEngine.Object GetMainAsset()
    {
        return texture;
    }

    /// <summary>
    /// 加载图集sprite资源
    /// </summary>
    /// <param name="res">ResHelper.</param>
    protected override void onBaseResLoadCompleteSyncCall()
    { 
        if(texture!=null)
        {
            UnityEngine.Debug.LogError("share texture need Unload:"+name);
        }
        if (bundle != null)
        {
            texture = bundle.LoadAsset<Texture>(textureName);
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

        if(texture!=null)
        { 
            GameObject.DestroyImmediate(texture,true);
            texture = null;
        }

        base.Unload();

        state = ResLoadingState.LOADSTATE_UNLOADED;
    }
}
