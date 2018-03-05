using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 特效资源，加载特效预制件ab同时，会加载贴图资源
/// Date: 2016/1/19
/// Author: lxz  
/// </summary>
public class EffectAsset : BundleAsset
{ 
    //[Describe("prefab所用到的贴图资源索引")]
    List<Asset> texResRefs = new List<Asset>();
 
    GameObject asset = null;
    string effectName;

    public EffectAsset(AssetManager rmgr, string name)
        : base(rmgr, "eft_" +name + IOTools.abSuffix)
    {
        effectName = name;
    }

    public override UnityEngine.Object GetMainAsset()
    {
        return asset;
    }

    /// <summary>
    /// 加载贴图
    /// </summary>
    /// <param name="res">ResHelper.</param>
    protected override void onBaseResLoadCompleteSyncCall()
    {
        if (asset != null)
        {
            UnityEngine.Debug.LogError("effect asset need Unload:" + effectName);
        }
        if (bundle != null)
        {
            asset = bundle.LoadAsset<GameObject>(effectName);
            bundle.Unload(false);
            bundle = null;

            if (asset != null)
            {
                #if UNITY_EDITOR
                EditorHelper.SetEditorShader(asset);
                #endif

                var insHolder = asset.GetOrAddComponent<InstanceAssetRefHolder>();
                insHolder.resType = ResourceType.Effect;
                insHolder.assetName = effectName;

                //针对设备性能，设置特效参数，删除一些配置 判断当前特效等级。如果中低特效，需要删除对应的对象，再加载对应的图片资源
                //TODO.... 

                //加载纹理资源
                var matHolder = asset.GetComponent<PrefabRenderHolder>();
                if (matHolder != null)
                { 
                    texResRefs.Clear(); 
                    SyncLoadMatsTex(matHolder.rendersMatTexInfo, texResRefs);
                    matHolder.rendersMatTexInfo = null;
                }
            }
        }
    }


    /// <summary>
    /// 真正异步加载部分重写
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
            #region 异步加载贴图等资源，设置模型参数
            if (bundle != null)
            {
                //异步加载bundle里的资源
                asset = bundle.LoadAsset<GameObject>(effectName);
                if (asset == null)
                {
                    state = ResLoadingState.LOADSTATE_UNLOADED;
                    yield break;
                }

                //针对设备性能，设置特效参数，删除一些配置 判断当前特效等级。如果中低特效，需要删除对应的对象，再加载对应的图片资源
                //TODO.... 

                state = ResLoadingState.LOADSTATE_LOADING;
                #if UNITY_EDITOR
                EditorHelper.SetEditorShader(asset);
                #endif 
                //异步加载模型贴图资源
                var refholder = asset.GetOrAddComponent<InstanceAssetRefHolder>();
                refholder.resType = ResourceType.Effect;
                refholder.assetName = effectName;
                var matHolder = asset.GetComponent<PrefabRenderHolder>();
                if (matHolder != null)
                { 
                    texResRefs.Clear();
                    //GameObject.DestroyImmediate(matHolder,true);
                    yield return AsyncLoadMatsTex(matHolder.rendersMatTexInfo, texResRefs); 
                    matHolder.rendersMatTexInfo = null;
                }
            }
            #endregion
            if (onCmp != null)
                onCmp.Call(this);
            state = ResLoadingState.LOADSTATE_LOADED;
        }
        else
        {
            if (onCmp != null)
                onCmp.Call(this);
        }
    }

    /// <summary>
    /// 卸载资源
    /// </summary>
    public override void Unload()
    {
        if (state != ResLoadingState.LOADSTATE_LOADED)
            return;

        //销毁公共贴图引用 
        removeTexturesRef(texResRefs);
        texResRefs.Clear();

        if (asset != null)
            GameObject.DestroyImmediate(asset, true);
        asset = null;

        if (bundle != null)
        {
            bundle.Unload(true);
        }
        base.Unload();
    }
}
