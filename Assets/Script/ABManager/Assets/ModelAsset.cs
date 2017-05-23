using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ModelAsset : BundleAsset
{
    public List<PrefabMaterialHolder.RenderMatTexPair> rendersMatTexInfo;
    List<Asset> texResRefs = new List<Asset>();

    GameObject asset = null;
    AnimationHolder clipHolder;
    string modelName;
    public ModelAsset(AssetManager rmgr, string name)
        : base(rmgr, "m_" + name.ToLower() + IOTools.abSuffix)
    {
        modelName = name.ToLower();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public GameObject GetAsset()
    {
        return asset;
    }

    /// <summary>
    ///  
    /// </summary>
    /// <param name="res">ResHelper.</param>
    protected override void onBaseResLoadCompleteSyncCall()
    {
        if (bundle != null)
        {
            asset = bundle.LoadAsset<GameObject>(modelName);
            if (asset == null)
            {
                return;
            }
#if UNITY_EDITOR
            EditorHelper.SetEditorShader(asset);
#endif
            var refholder = asset.AddComponent<InstanceAssetRefHolder>();
            refholder.resType = ResourceType.Model;
            refholder.assetName = modelName;
            //加载模型贴图资源
            var matHolder = asset.GetComponent<PrefabMaterialHolder>();
            if (matHolder != null)
            {
                rendersMatTexInfo = matHolder.rendersMatTexInfo;
                texResRefs.Clear();
                //GameObject.DestroyImmediate(matHolder, true);
                matHolder.rendersMatTexInfo = null;
                SyncLoadMatsTex(rendersMatTexInfo, texResRefs);
            }
            //加载动作clip资源
            clipHolder = asset.GetComponent<AnimationHolder>();
            if (clipHolder != null)
            {
                clipHolder.SyncLoadClips();
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
            {
                if (onCmp != null)
                    onCmp.Clear(); 
                Debug.LogError("async load bundle error:" + name);
                state = ResLoadingState.LOADSTATE_UNLOADED;
                yield break;
            }
            yield return req;

            bundle = req.assetBundle;
            refCount = 0;
            #region 异步加载贴图等资源，设置模型参数
            if (bundle != null)
            {
                //异步加载bundle里的资源
                var assetReq = bundle.LoadAssetAsync<GameObject>(modelName);
                yield return assetReq;
                asset = assetReq.asset as GameObject;
                if (asset == null)
                {
                    state = ResLoadingState.LOADSTATE_UNLOADED;
                    yield break;
                }
                state = ResLoadingState.LOADSTATE_LOADING;
#if UNITY_EDITOR
                EditorHelper.SetEditorShader(asset);
#endif 
                //异步加载模型贴图资源
                var refholder = asset.AddComponent<InstanceAssetRefHolder>();
                refholder.resType = ResourceType.Model;
                refholder.assetName = modelName;
                var matHolder = asset.GetComponent<PrefabMaterialHolder>();
                if (matHolder != null)
                {
                    rendersMatTexInfo = matHolder.rendersMatTexInfo;
                    texResRefs.Clear();
                    //GameObject.DestroyImmediate(matHolder, true);
                    //DestroyImmediate有问题
                    matHolder.rendersMatTexInfo = null;
                    yield return AsyncLoadMatsTex(rendersMatTexInfo, texResRefs);
                }

                //异步加载动作clip资源
                clipHolder = asset.GetComponent<AnimationHolder>();
                if (clipHolder != null)
                {
                    yield return clipHolder.AsyncLoadClips();
                }
                GameObject.DestroyImmediate(clipHolder, true);//zhouhui TODO: to delete clipHolder

                #endregion
                if (onCmp != null)
                    onCmp.Call(this);
                state = ResLoadingState.LOADSTATE_LOADED;
            }
            else
            {
                if (onCmp != null)
                    onCmp.Clear();
                Debug.LogError("async load bundle error:" + name);
                state = ResLoadingState.LOADSTATE_UNLOADED;
            }
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

        //移除公共贴图引用 
        removeTexturesRef(texResRefs);
        texResRefs.Clear();
        //移除clip引用
        if (clipHolder != null)
        {
            clipHolder.removeClipsRef();
        }

        GameObject.DestroyImmediate(asset, true);
        asset = null;
        if (bundle != null)
        {
            bundle.Unload(true);
        }
        base.Unload();
    }
}
