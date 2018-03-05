using UnityEngine;
using System.Collections;
using UnityEditor;

/// <summary>
/// 检查材质错误等帮助函数类
/// Date: 2016/1/5
/// Author: lxz  
/// </summary>
public class MatHelper
{
    public static bool FindMatError(Material mat)
    {
        if (mat == null)
            return true;
        for (int si = 0; si < ShaderUtil.GetPropertyCount(mat.shader); si++)
        {
            ShaderUtil.ShaderPropertyType st = ShaderUtil.GetPropertyType(mat.shader, si);
            if (st == ShaderUtil.ShaderPropertyType.TexEnv)
            {
                string strProperty = ShaderUtil.GetPropertyName(mat.shader, si);
                Texture tex = mat.GetTexture(strProperty);
                if (tex == null)
                {
                    Debug.LogError("材质贴图丢失,材质名:" + mat.name);
                    return true;
                }
            }
        }
        return false;
    }

    public static void FildPrefabMatError(GameObject prefab)
    {
        if (prefab == null)
            return;
        //string path = AssetDatabase.GetAssetPath(prefab);
        //if (!path.EndsWith(".prefab"))
        //    return;
        GameObject gobj = prefab as GameObject;
        var pss = gobj.GetComponentsInChildren<ParticleSystem>(true);
        foreach (var ps in pss)
        {
            if (ps.GetComponent<Renderer>().sharedMaterial == null)
            {
                Debug.LogError("材质丢失,预制件名:" + gobj.name + "  ,丢失粒子名字:" + ps.gameObject.name);
            }
            else
            {
                FindMatError(ps.GetComponent<Renderer>().sharedMaterial);
            }
        }
        var mrs = gobj.GetComponentsInChildren<MeshRenderer>(true);
        foreach (var mr in mrs)
        {
            var mats = mr.sharedMaterials;
            if (mats != null)
            {
                foreach (var mat in mats)
                {
                    if (mat == null)
                    {
                        Debug.LogError("材质丢失,预制件名:" + gobj.name + "  ,丢失对象名字:" + mr.gameObject.name);
                    }
                    else
                    {
                        FindMatError(mat);
                    }
                }
            }
        }

        var smrs = gobj.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (var smr in smrs)
        {
            var mats = smr.sharedMaterials;
            if (mats != null)
            {
                foreach (var mat in mats)
                {
                    if (mat == null)
                    {
                        Debug.LogError("材质丢失,预制件名:" + gobj.name + "  ,丢失对象名字:" + smr.gameObject.name);
                    }
                    else
                    {
                        FindMatError(mat);
                    }
                }
            }
        } 
    }

    [MenuItem("Assets/查找材质贴图是否丢失")]
    static void FildMatsError()
    {
        foreach (var mat in Selection.GetFiltered(typeof(Material), SelectionMode.DeepAssets))
        {
            FindMatError(mat as Material);
        }
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
    }

    [MenuItem("Assets/查找预制体材质是否丢失")]
    static void FildPrefabsMatError()
    {
        foreach (Object prefab in Selection.GetFiltered(typeof(GameObject), SelectionMode.DeepAssets))
        {
            FildPrefabMatError(prefab as GameObject);
        }
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
    }

    [MenuItem("Assets/替换场景shader")]
    public static void ReplaceMatShader()
    {
        foreach (Object prefab in Selection.GetFiltered(typeof(GameObject), SelectionMode.DeepAssets))
        { 
            GameObject gobj = prefab as GameObject;
            var pss = gobj.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in pss)
            {
                var mats = ps.GetComponent<Renderer>().sharedMaterials;
                if (mats != null)
                {
                    foreach (var mat in mats)
                    {
                        if (mat != null)
                        {
                            var shader = Shader.Find(mat.shader.name);
                            if (shader != null)
                            {
                                mat.shader = shader;
                            }
                        }
                    }
                }
            }
            var mrs = gobj.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var mr in mrs)
            {
                var mats = mr.sharedMaterials;
                if (mats != null)
                {
                    foreach (var mat in mats)
                    {
                        if (mat != null)
                        {
                            var shader = Shader.Find(mat.shader.name);
                            if (shader != null)
                            {
                                mat.shader = shader;
                            }
                        }
                    }
                }
            }

            var smrs = gobj.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (var smr in smrs)
            {
                var mats = smr.sharedMaterials;
                if (mats != null)
                {
                    foreach (var mat in mats)
                    {
                        if (mat != null)
                        {
                            var shader = Shader.Find(mat.shader.name);
                            if (shader != null)
                            {
                                mat.shader = shader;
                            }
                        }
                    }
                }
            }
        }
    }
}