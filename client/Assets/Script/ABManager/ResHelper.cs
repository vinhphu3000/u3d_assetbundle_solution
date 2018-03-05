using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public static class ResHelper
{
    public delegate bool CheckAction(string id);
    static InstanceGameObjectCache modelPools   = new InstanceGameObjectCache();
    static InstanceGameObjectCache effectPools  = new InstanceGameObjectCache();
    public static Transform CacheParent;

    //同步加载模型，返回prefab对象
    public static GameObject LoadModel(string name)
    {
        var res = ModelAssetManager.Singleton.Load(name);
        return ((ModelAsset)res).GetMainAsset() as GameObject;
    } 

    //异步加载模型
    public static void AsyncLoadModel(string name, UnityAction<UnityEngine.Object> onCmp = null)
    {
        ModelAssetManager.Singleton.AsyncLoad(name, onCmp);
    }

    //同步实例化模型
    public static GameObject InstantiateModel(string name)
    {
        name = name.toLower();
        var obj = modelPools.PopItem(name);
        if (obj != null)
        {
            obj.transform.localScale = Vector3.one; 
        }
        else
        {
            var prefab = LoadModel(name);
            if (prefab != null)
            {
                obj = GameObject.Instantiate(prefab);
            }
        }
        return obj;
    }

    //异步实例化模型
    public static void AsyncInstantiateModel(string name, CheckAction check, UnityAction<GameObject> omComplete)
    {
        name = name.toLower();
        var obj = modelPools.PopItem(name);
        if (obj != null)
        {
            obj.transform.localScale = Vector3.one;
            if (check == null || check(name))
            {
                omComplete(obj);
            }
            else
            {
                ResHelper.DestroyGameObject(ref obj);
            }
        }
        else
        {
            UnityAction<UnityEngine.Object> onLoadComplete = (UnityEngine.Object mainAsset) =>
                {
                    if (check != null && !check(name))
                    {
                        return;
                    }
                    var prefab = mainAsset as GameObject;
                    GameObject newObj = null;
                    if (prefab != null)
                    {
                        newObj = GameObject.Instantiate(prefab);
                        newObj.transform.resetLocal();
                    }
                    omComplete(newObj);
                };
            AsyncLoadModel(name, onLoadComplete);
        }
    }
      
    /// 同步加载特效,返回prefab对象
    public static GameObject LoadEffect(string name)
    {
        var res = EffectAssetManager.Singleton.Load(name);
        var gobj = ((EffectAsset)res).GetMainAsset() as GameObject;
        if (gobj == null)
            gobj = new GameObject("_ErrorEffect");
        return gobj;
    }

    //同步实例化特效
    public static GameObject InstantiateEffect(string name)
    {
        name = name.toLower();
        var obj = effectPools.PopItem(name);
        if (obj != null)
        {
            obj.transform.localScale = Vector3.one; 
        }
        else
        {
            var prefab = LoadEffect(name);
            if (prefab != null)
            {
                obj = GameObject.Instantiate(prefab);
            }
        }
        return obj;
    }

    //加载场景
    public static void LoadScene(string name,UnityAction<float> onProcessUpdate, UnityAction OnComplete, LoadSceneMode mode)
    {
        SceneAssetManager.Singleton.LoadScene(name,onProcessUpdate,OnComplete,mode);
    }

    /// <summary>
    /// 播放声音
    /// </summary>
    /// <param name="name"></param>
    public static void PlaySound(string name)
    {
        AudioAssetManager.Singleton.PlaySound(name);
    }

    /// <summary>
    /// 播放全局背景音乐
    /// </summary>
    public static void PlayBGM(string name)
    {
        AudioAssetManager.Singleton.PlayBGM(name);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="obj"></param>
    public static void DestroyGameObject(ref GameObject gobj,bool isCache = true)
    {
        if (gobj == null)
            return;
        var insHolder = gobj.GetComponent<InstanceAssetRefHolder>();
        if (insHolder != null)
        {
            insHolder.Reset();
            switch (insHolder.resType)
            {
                //自动管理
                case ResourceType.Effect:
                    //重新设置父对象（相关的缓存对象使用的地方，需要再次设置父对象。）                    
                    gobj.transform.ResetParent(CacheParent);
                    var effectSetting = gobj.GetOrAddComponent<EffectSetting>();
                    if (effectSetting.CanCache)
                    {
                        if (isCache)
                        {
                            effectPools.PushItem(gobj, insHolder.assetName);
                        }
                        else
                        {
                            effectPools.DestroyExistItem(gobj, insHolder.assetName);
                        }
                    }
                    else
                    {
                        effectPools.DestroyExistItem(gobj, insHolder.assetName);
                    }
                    gobj = null;
                    break;
                    //自动管理 
                case ResourceType.Model:
                    //重新设置父对象（相关的缓存对象使用的地方，需要再次设置父对象。）                    
                    gobj.transform.ResetParent(CacheParent);
                    if (isCache)
                    {

                        modelPools.PushItem(gobj, insHolder.assetName);
                    }
                    else
                    {
                        modelPools.DestroyExistItem(gobj, insHolder.assetName);
                    }

                    gobj = null;
                    break;
            }
        }
        else
        {
            GameObject.Destroy(gobj);
        }
    }
}