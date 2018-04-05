using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceUpdate : SingletonTemplate<ResourceUpdate> {

    public void Enter()
    {
        //检测是否需要更新
        var localVer = GetLocalResVersion();
        var newVer = localVer;
        Debug.Log("本地资源版本:"+localVer);

        //先判断是否是白名单，如果是，判断test资源版本号
        //如果非白名单，则直接判断 
        if (PlayerPrefs.HasKey("whitelist") && PlayerPrefs.GetString("whitelist") == "true" && ServerList.Singleton.BeOpenRes)
        {
            newVer = ServerList.Singleton.TestResVersion;
            Debug.Log("白名单,且开启了测试资源版本!");
        }
        else
        {
            newVer = ServerList.Singleton.ResVersion; 
        }
        Debug.Log("服务器资源版本:"+newVer); 

        if (newVer.CompareTo(localVer)>0)
        {
            Debug.Log("资源需要更新，新版本号："+newVer);
        }
 
        //IOTools.writeStringToUpdateDir("resversion",localVer);
    }

    public string GetLocalResVersion()
    {
        //先判断更新目录是否存在资源版本号，如果没有则从包配置里读取
        var localVer = IOTools.GetResFileString("resversion", false);
        if (localVer == null || localVer == "")
            localVer = BaseConfig.Singleton.ResVersion;
        return localVer;
    }
}
