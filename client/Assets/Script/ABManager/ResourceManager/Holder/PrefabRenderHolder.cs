using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


/// <summary>
/// 保存预制件打包时候去掉的贴图依赖信息
/// Date: 2016/1/19
/// Author: lxz  
/// </summary>
public class PrefabRenderHolder : MonoBehaviour
{
    [System.Serializable]
    public class MaterialTextureInfo
    {
        public string attribute;
        public string tex2dName;
    }

    [System.Serializable]
    public class MaterialAllTextureInfo
    {
        public int matIndex;
        public MaterialTextureInfo[] matTexInfos;
    }

    [System.Serializable]
    public class RenderMatTexPair
    {
        public Renderer renderObj;
        public MaterialAllTextureInfo[] matAllInfos;
    }

    public List<RenderMatTexPair> rendersMatTexInfo;

    [System.Serializable]
    public class RenderMeshPair
    {
        public MeshFilter meshFilterObj;
        public string meshName;
    }
    public List<RenderMeshPair> rendersMeshInfo;

    void OnDestroy()
    {
        //if (isRemoveRefOnDestroy)
        //{
        //    removeTexturesRef();
        //}
    }
}
