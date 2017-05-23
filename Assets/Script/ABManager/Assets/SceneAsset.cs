using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

/// <summary>
///场景资源加载
/// Date: 2016/1/20
/// Author: lxz  
/// </summary>
public class SceneAsset : BundleAsset
{ 
    public List<PrefabMaterialHolder.RenderMatTexPair> rendersMatTexInfo; 

    List<Asset> texResRefs = new List<Asset>();

    string sceneName = "";

    Action<float> onLoadUpdate;

    Action onAllComplete;

    string depConfResName = "";
    public SceneAsset(AssetManager rmgr, string name)
        : base(rmgr, "s_" + name.ToLower() + IOTools.abSuffix)
    {
        sceneName = name;
        depConfResName = "s_" + name.ToLower() + "_dep" + IOTools.abSuffix;
    }

    /// <summary>
    /// 添加更新进度回调
    /// </summary>
    /// <param name="onUpdate"></param>
    public void AddLoadUpdateCall(Action<float> onUpdate)
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
    public void AddAllCompleteCall(Action onComplete)
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
        //先加载过渡场景    
        //SceneManager.LoadScene("transition_scene");
        //在加载真正需要加载的场景
        AsyncOperation loadOpr = SceneManager.LoadSceneAsync(sceneName);

        float process = 0.2f;
        while (!loadOpr.isDone)
        {
            process = 0.2f + loadOpr.progress * 0.267f;
            if (onLoadUpdate != null)
                onLoadUpdate(process);
            yield return null;
        }

        var scene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(scene);
        if (bundle != null)
            bundle.Unload(false);
        bundle = null;

        if (scene.rootCount == 0 || scene.rootCount > 1)
            UnityEngine.Debug.LogError("场景设置错误，有多个或者没有root对象！");
        var root = scene.GetRootGameObjects()[0];// GameObject.Find(sceneName);
                                                 //根据性能配置 删除部分对象...暂时不写

        if (root != null)
        {
#if UNITY_EDITOR
            EditorHelper.SetEditorShader(root);
#endif
            var pfmh = root.GetComponent<PrefabMaterialHolder>();
            if (pfmh != null)
            {
                var iarh = root.AddComponent<InstanceAssetRefHolder>();//TODO:
                iarh.assetName = sceneName;
                iarh.resType = ResourceType.SceneObject;
                rendersMatTexInfo = pfmh.rendersMatTexInfo;
                SceneAssetManager.Singleton.addRef(sceneName);//TODO:
                texResRefs.Clear();
                //GameObject.DestroyImmediate(pfmh,true);
                pfmh.rendersMatTexInfo = null;
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
            Debug.LogError("得到场景root失败;" + sceneName);
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
        Debug.LogError("scene res sync Load error!!"); 
    } 
     
    public IEnumerator asyncLoad()
    {
        //先加载过渡场景    
        SceneManager.LoadScene("transition_scene");

        yield return new WaitForEndOfFrame(); 
        yield return new WaitForEndOfFrame(); 

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
       
    }

    public override void RemoveRef()
    {
        state = ResLoadingState.LOADSTATE_UNLOADED;
        base.RemoveRef();
        removeTexturesRef(texResRefs);
        RemoveTextures();
        texResRefs.Clear();
    }

    void RemoveTextures()
    {
        for (int i = 0; i < texResRefs.Count;i++)
        {
            if (texResRefs[i].ReferenceCount<=0)
            {
                texResRefs[i].Unload();
            }
        }
    }
}
