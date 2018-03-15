using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launch : MonoBehaviour {
    
	void Start () {

        TestSproto();

        GameObject.DontDestroyOnLoad(gameObject);

        ResHelper.CacheParent = gameObject.transform;

        gameObject.GetOrAddComponent<CoroutineManager>();

        IOTools.Init();
        //加载必要初始化资源 
        var shaderRes = BundleAsset.LoadAb("common" + IOTools.abSuffix);
#if UNITY_EDITOR
        shaderRes.LoadAllAssets();
#endif

        //加载场景
        ResHelper.LoadScene("test_scene",
            //场景加载进度回调
            (float v)=>
            {
                Debug.LogError("加载场景进度:"+ v);
            },
            //场景加载完成回调
            ()=>
            {
                Debug.LogError("加载场景完成:");
                //同步加载测试模型
                var model1 = ResHelper.InstantiateModel("zhulong");
                //加载特效
                GameObject.Instantiate(ResHelper.LoadEffect("test_eft"));
                //异步加载模型
                ResHelper.AsyncInstantiateModel("test_model",
                    //实例化完成检测函数，时序性和时效性验证
                    (name)=>
                    {
                        return true;
                    },
                    (GameObject obj)=>{
                        obj.transform.position = new Vector3(0,0,6);
                    }
                );

                //3s后删除部分对象
                LeanTween.delayedCall(3,
                    ()=>
                    {
                        ResHelper.DestroyGameObject(ref model1);
                    }
                );
            },
            UnityEngine.SceneManagement.LoadSceneMode.Additive
        );
  
    }

    //测试sproto
    void TestSproto()
    { 

        /*
        Sproto.SprotoPack spack = new Sproto.SprotoPack();
       
        proto.Test1 t1 = new proto.Test1();
        t1.A = 133;
        t1.B = true;
        t1.C = new List<proto.TTTT>();
        t1.C.Add(new proto.TTTT{A = 56,B = false });
        t1.C.Add(new proto.TTTT{A = 536,B = true });
        t1.D = proto.Test12.OTHER;

        var data = spack.pack(t1.encode());

        data = spack.unpack(data);

        t1 = new proto.Test1(data); 

        Debug.LogError(t1.A + "  " + t1.B + t1.C.Count + " "+t1.D);

        foreach (var v in t1.C)
        {
            Debug.LogError(v.A+"  "+v.B);
        }
        */
    }
}
