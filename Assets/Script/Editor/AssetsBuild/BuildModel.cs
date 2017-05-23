using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using UnityEngine.AI;

/// <summary>
/// 打包模型资源
/// Date: 2016/1/25
/// Author: lxz  
/// </summary>
public class BuildModel : BuildImpl
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="obj">Object.</param>
    public static new void Build(Object obj)
    {
        string assetPath = AssetDatabase.GetAssetPath(obj);
        if (!assetPath.EndsWith(".prefab"))
        {
            return;
        } 

        //创建临时预制体
        string temp_main_path = "Assets/temp/" + Path.GetFileName(assetPath);
        AssetDatabase.DeleteAsset(temp_main_path);
        //AssetDatabase.CopyAsset(assetPath, temp_main_path);
        //AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);


        //var tempPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(temp_main_path);
        //if (tempPrefab == null)
        //{
        //    Debug.LogError("加载临时effect prefab失败：" + temp_main_path);
        //    return;
        //} 




        var tempPrefab = GameObject.Instantiate(obj as GameObject);

        //去掉贴图
        //var oldMatInfos = RemovePrefabMatTex(tempPrefab);

        var texsDeps = GetDepTexs(tempPrefab);

        /*   //记录需要打包的动画控制器  animator
           Dictionary<RuntimeAnimatorController, string> aniCtrs = new Dictionary<RuntimeAnimatorController, string>();

           //去掉动画控制器
           List<Animator> anis = new List<Animator>();
           tempPrefab.GetComponentsInChildren<Animator>(anis);
           var mah = tempPrefab.AddComponent<ModelAnimatorHolder>();
           int count = 0;
           foreach (var v in anis)
           {
               if (v.runtimeAnimatorController == null)
                   continue;
               count++;
               //得到文件目录名字
               string abName = AssetDatabase.GetAssetPath(v.runtimeAnimatorController);

               abName = abName.Replace( Path.GetFileName(abName),"");
               abName.Replace("\\","/");
               string [] strs = abName.Split(new char[]{'/'});
               if (strs.Length < 3)
                   abName = v.runtimeAnimatorController.name + BuildConfig.abSuffix;
               else
               {
                   abName = strs[strs.Length - 3] + "_" + strs[strs.Length - 2] + "_"+v.runtimeAnimatorController.name + BuildConfig.abSuffix;
               }

               abName = abName.Replace(" ", "");
               abName = abName.ToLower();
               abName = "ac_" + abName;

               if (!aniCtrs.ContainsKey(v.runtimeAnimatorController))
               { 
                   aniCtrs[v.runtimeAnimatorController] = abName; 
               }

               if (mah.animatorCtrs == null)
                   mah.animatorCtrs = new List<ModelAnimatorHolder.AnimatorControllerPair>();
               mah.animatorCtrs.Add(new ModelAnimatorHolder.AnimatorControllerPair {  animator = v,controllerABName = abName});

               v.runtimeAnimatorController = null;
           }
           if (count == 0)
           {
               GameObject.DestroyImmediate(mah, true);
           }
           else
           {
               //打包动画控制器资源
               foreach (var v in aniCtrs)
               {
                   AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
                   buildMap[0].assetBundleName = v.Value;
                   buildMap[0].assetNames = new string[] { AssetDatabase.GetAssetPath(v.Key) };
                   BuildPipeline.BuildAssetBundles(BuildConfig.abOutputPath, buildMap, BuildConfig.options, EditorUserBuildSettings.activeBuildTarget);
               }
           }
         * */

        //animation
        //看是否存在animation组件
        var animation = tempPrefab.GetComponent<Animation>();

        Dictionary<string, string> clipAssets = new Dictionary<string, string>();

        //单独记录一个clip列表，以免存在重复添加clip的情况 
        List<AnimationClip> clips = new List<AnimationClip>();

        AnimationHolder aniHolder = null;

        if (animation != null)
        {
            aniHolder = tempPrefab.AddComponent<AnimationHolder>();

            aniHolder.ani = animation;

            aniHolder.depAnimList = new List<string>();

            foreach (AnimationState state in animation)
            {
                string fbxPath = AssetDatabase.GetAssetPath(state.clip);
                clips.Add(state.clip);
                string fbxName = null;
                if (fbxPath.EndsWith("fbx"))
                {
                    string[] arr = fbxPath.Split('/');
                    fbxName = arr[arr.Length - 1].ToLower();
                    fbxName = fbxName.Split('@')[0];
                    fbxName = fbxName.Split('.')[0];
                }
                else
                {
                    string[] arr = fbxPath.Split('/');
                    fbxName = arr[arr.Length - 2].ToLower();
                }

                string clipAbName = "c_" + fbxName + "_" + state.clip.name.ToLower() + BuildConfig.abSuffix;

                //保存默认clip信息
                if (animation.clip == state.clip)
                {
                    aniHolder.defaultAnim = clipAbName;
                    animation.clip = null;
                }

                //将动作资源单独保存一个临时的...因为我实在不知道怎么打包fbx里的动作资源... 
                string temp_clip_path = "Assets/temp/" + state.clip.name + ".asset";
                AssetDatabase.CreateAsset(Object.Instantiate(state.clip), temp_clip_path);

                clipAssets[clipAbName] = temp_clip_path;
            }

            //移除prefab所有clip
            foreach (var clip in clips)
            {
                animation.RemoveClip(clip);
            }

            foreach (var clipInfo in clipAssets)
            {
                //保存依赖的动作资源信息
                aniHolder.depAnimList.Add(clipInfo.Key);
            }
        }

        //保存临时修改
        PrefabUtility.CreatePrefab(temp_main_path, tempPrefab);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        //打包公共依赖 
        AssetBundleBuild[] buildMap1 = new AssetBundleBuild[2 + clipAssets.Count + texsDeps.Count];
        buildMap1[0] = BuildCommand.getCommonDep();

        int i = 0;
        for (i = 0; i < texsDeps.Count; i++)
        {
            buildMap1[i + 1] = texsDeps[i];
        }

        i = 1 + texsDeps.Count;
        //打包动作文件
        foreach (var clipInfo in clipAssets)
        {
            buildMap1[i].assetBundleName = clipInfo.Key;
            buildMap1[i].assetNames = new string[] { clipInfo.Value };
            i++;
        }

        //打包临时prefab本身
        buildMap1[buildMap1.Length - 1].assetBundleName = "m_" + obj.name.ToLower() + BuildConfig.abSuffix;
        buildMap1[buildMap1.Length - 1].assetNames = new string[] { temp_main_path };

        BuildPipeline.BuildAssetBundles(BuildConfig.abOutputPath, buildMap1, BuildConfig.options, EditorUserBuildSettings.activeBuildTarget);
        //还原材质
        //RevertMatFile(tempPrefab, oldMatInfos);

        //delete tmp obj
        GameObject.DestroyImmediate(tempPrefab);

        //删除临时资源
        AssetDatabase.DeleteAsset(temp_main_path);

        foreach (var clipInfo in clipAssets)
        {
            AssetDatabase.DeleteAsset(clipInfo.Value);
        }

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
    }
}
