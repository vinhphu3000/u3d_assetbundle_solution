using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ModelAsset : BundleAsset
{ 
    List<Asset> texResRefs = new List<Asset>();
    List<Asset> meshResRefs = new List<Asset>();
    List<string> depAnimList;
    GameObject asset = null;
    AnimationHolder clipHolder;
    string modelName;
    public ModelAsset(AssetManager rmgr, string name)
        : base(rmgr, "m_" + name + IOTools.abSuffix)
    {
        modelName = name;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public override UnityEngine.Object GetMainAsset()
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
            bundle.Unload(false);
            bundle = null;
            if (asset == null)
            {
                return;
            }
            #if UNITY_EDITOR
            EditorHelper.SetEditorShader(asset);
            #endif
            var refholder = asset.GetOrAddComponent<InstanceAssetRefHolder>();
            refholder.resType = ResourceType.Model;
            refholder.assetName = modelName;
            //加载模型贴图资源
            var matHolder = asset.GetComponent<PrefabRenderHolder>();
            if (matHolder != null)
            {
                texResRefs.Clear();
                meshResRefs.Clear();
                //GameObject.DestroyImmediate(matHolder, true);
                SyncLoadMatsTex(matHolder.rendersMatTexInfo, texResRefs);
                SyncLoadPrefabMesh(matHolder.rendersMeshInfo, meshResRefs);

                matHolder.rendersMatTexInfo = null;
                matHolder.rendersMeshInfo = null;
            }
            //加载动作clip资源
            clipHolder = asset.GetComponent<AnimationHolder>();
            if (clipHolder != null)
            {
                clipHolder.SyncLoadClips();
                depAnimList = clipHolder.depAnimList;
                GameObject.DestroyImmediate(clipHolder, true);
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
                UnityEngine.Debug.LogError("async load bundle error:" + name);
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
                bundle.Unload(false);
                bundle = null;
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
                var refholder = asset.GetOrAddComponent<InstanceAssetRefHolder>();
                refholder.resType = ResourceType.Model;
                refholder.assetName = modelName;
                var matHolder = asset.GetComponent<PrefabRenderHolder>();
                if (matHolder != null)
                {
                    texResRefs.Clear();
                    meshResRefs.Clear();
                    //GameObject.DestroyImmediate(matHolder, true);
                    //DestroyImmediate有问题

                    if (matHolder.rendersMatTexInfo != null && matHolder.rendersMatTexInfo.Count > 0)
                        yield return AsyncLoadMatsTex(matHolder.rendersMatTexInfo, texResRefs);

                    if (matHolder.rendersMeshInfo != null && matHolder.rendersMeshInfo.Count > 0)
                        yield return AsyncLoadPrefabMesh(matHolder.rendersMeshInfo, meshResRefs);

                    matHolder.rendersMeshInfo = null;
                    matHolder.rendersMatTexInfo = null;
                }

                //异步加载动作clip资源
                clipHolder = asset.GetComponent<AnimationHolder>();
                if (clipHolder != null)
                {
                    yield return clipHolder.AsyncLoadClips();
                    depAnimList = clipHolder.depAnimList;
                    GameObject.DestroyImmediate(clipHolder, true);
                }

                #endregion
                if (onCmp != null)
                    onCmp.Call(this);
                state = ResLoadingState.LOADSTATE_LOADED;
            }
            else
            {
                if (onCmp != null)
                    onCmp.Clear();
                UnityEngine.Debug.LogError("async load bundle error:" + name);
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
        removeMeshRef(meshResRefs);
        texResRefs.Clear();
        meshResRefs.Clear();
        //移除clip引用
        //if (clipHolder != null)
        //{
        //    clipHolder.removeClipsRef();
        //}

        if (depAnimList!=null)
        {
            var clipMgr = AnimationClipAssetManager.Singleton;

            for (int i = 0; i < depAnimList.Count; i++)
            {
                clipMgr.RemoveRef(depAnimList[i]);
            }
            depAnimList = null;
        }

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
