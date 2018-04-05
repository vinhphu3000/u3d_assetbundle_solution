using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppUpdate:SingletonTemplate<AppUpdate> {

    //规则：版本号一二位代表大版本号，最后一位是小版本号，大版本号变动，需要强更，小版本号变动，不更新app
    public void Enter()
    {
        var bigVer = BaseConfig.mSingleton.GetAppBigVersion();
        var newBigVer = ServerList.Singleton.GetAppBigVersion();

        if (bigVer != newBigVer)
        {
            Debug.Log("app需要更新，浏览器打开更新页面....");
            //TUDO
        }
        else
        {
            Debug.Log("app不需要更新，检查资源更新....");
            //检测通过，进入资源更新流程
            ResourceUpdate.Singleton.Enter();
        }
    }
}
