using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WarningEffectHolder : MonoBehaviour
{
    [System.Serializable]
    public class RenderInfo
    {
        [Header("需要修改的meshRenderer")]
        public MeshRenderer mr;
        [Header("颜色属性的名字")]
        public string colorName;
        [Header("需要修改成的颜色")]
        public Color color;
    }

    public RenderInfo normalShow;
    public RenderInfo cancelShow;

    public void SetNormal()
    {
        if (normalShow == null || normalShow.mr == null || normalShow.mr.material==null)
            return;
        normalShow.mr.material.SetColor(normalShow.colorName, normalShow.color);
    }

    public void SetCancel()
    {
        if (cancelShow == null || cancelShow.mr == null || cancelShow.mr.material == null)
            return;
        cancelShow.mr.material.SetColor(cancelShow.colorName, cancelShow.color);
    }
}


