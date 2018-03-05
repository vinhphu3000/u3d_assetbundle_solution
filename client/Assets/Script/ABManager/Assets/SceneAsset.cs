using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;
using UnityEngine.Events;

/// <summary>
///场景资源加载
/// Date: 2016/1/20
/// Author: lxz  
/// </summary>
public class SceneAsset : BundleAsset
{
    //prefab 所用到的贴图材质对应列表
    public List<PrefabRenderHolder.RenderMatTexPair> rendersMatTexInfo;
    //prefab所用到的贴图资源索引
    List<Asset> texResRefs = new List<Asset>();

    string sceneName = "";
    UnityAction<float> onLoadUpdate;
    UnityAction onAllComplete;
    public LoadSceneMode mode = LoadSceneMode.Additive;

    public SceneAsset(AssetManager rmgr, string name)
        : base(rmgr, "s_" + name.toLower() + IOTools.abSuffix)
    {
        sceneName = name;
    }

    /// <summary>
    /// 添加更新进度回调
    /// </summary>
    /// <param name="onUpdate"></param>
    public void AddLoadUpdateCall(UnityAction<float> onUpdate)
    {
        if (onLoadUpdate != null)
            onLoadUpdate += onUpdate;
        else
            onLoadUpdate = onUpdate;
    }

    /// <summary>
    /// 添加完成回调
    /// </summary>
    /// <param name="onComplete"></param>
    public void AddAllCompleteCall(UnityAction onComplete)
    {
        if (onAllComplete != null)
            onAllComplete += onComplete;
        else
            onAllComplete = onComplete;
    }

    /// <summary>
    /// 异步加载场景内容
    /// </summary>
    protected IEnumerator asyncLoadWillComplete()
    {
        AsyncOperation loadOpr = SceneManager.LoadSceneAsync(sceneName, mode);

        float process = 0.2f;
        while (!loadOpr.isDone)
        {
            process = 0.2f + loadOpr.progress * 0.267f;
            if (onLoadUpdate != null)
                onLoadUpdate(process);
            yield return null;
        }

        if (bundle != null)
        {
            bundle.Unload(false);
            bundle = null;
        }
        Scene sc = SceneManager.GetSceneByName(sceneName);
        if (sc != null)
        {
            SceneManager.SetActiveScene(sc);
        }

        Resources.UnloadUnusedAssets();

        var root = GameObject.Find(sceneName);
        //根据性能配置 删除部分对象...暂时不写

        if (root != null)
        {
            #if UNITY_EDITOR
            EditorHelper.SetEditorShader(root);
            #endif
            var pfmh = root.GetComponent<PrefabRenderHolder>();
            if (pfmh != null)
            {
                var iarh = root.GetOrAddComponent<InstanceAssetRefHolder>();
                iarh.assetName = sceneName;
                iarh.resType = ResourceType.SceneObject;
                rendersMatTexInfo = pfmh.rendersMatTexInfo;
                SceneAssetManager.Singleton.addRef(sceneName);
                texResRefs.Clear();
                //GameObject.DestroyImmediate(pfmh, true);
                //异步加载贴图
                yield return AsyncLoadMatsTex(rendersMatTexInfo, texResRefs, p =>
                    {
                        process = 0.467f + p * 0.52f;
                        if (onLoadUpdate != null)
                            onLoadUpdate(process);
                    });
            }
        }
        else
        {
            UnityEngine.Debug.LogError("得到场景root失败;" + sceneName);
        }

        if (onLoadUpdate != null)
        {
            onLoadUpdate(1);
            onLoadUpdate = null;
        }
        if (onAllComplete != null)
        {
            onAllComplete();
            onAllComplete = null;
        }

    }

    /// <summary>
    /// 场景默认不提供同步加载接口
    /// </summary>
    public override void Load()
    {
        UnityEngine.Debug.LogError("scene res sync Load error!!");
    }

    public IEnumerator asyncLoad()
    {
        if (state == ResLoadingState.LOADSTATE_LOADING)
            yield break;
        float process = 0.01f;
        if (onLoadUpdate != null)
            onLoadUpdate(process);
        yield return new WaitForEndOfFrame();
        base.Load();
        yield return CoroutineManager.Singleton.StartCoroutine(asyncLoadWillComplete());
    }

    /// <summary>
    /// 卸载资源
    /// </summary>
    public override void Unload()
    { 
        Scene sc = SceneManager.GetSceneByName(sceneName);
        if (sc != null)
        {
            SceneManager.UnloadScene(sceneName); 
        }

        if (state != ResLoadingState.LOADSTATE_LOADED)
            return;
        base.Unload();
        removeTexturesRef(texResRefs);
        state = ResLoadingState.LOADSTATE_UNLOADED;
    }
}
