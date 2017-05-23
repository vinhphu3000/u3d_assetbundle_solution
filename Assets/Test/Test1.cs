using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test1 : MonoBehaviour {
    
	void Start () {
        IOTools.Init();
        //加载必要初始化资源 
        var shaderRes = BundleAsset.LoadAb("common" + IOTools.abSuffix);
#if UNITY_EDITOR
        shaderRes.LoadAllAssets();
#endif

        //加载测试模型
        GameObject.Instantiate(ResHelper.LoadModel("zhulong"));
    }
}
