using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceUpdate : SingletonTemplate<ResourceUpdate> {

    public void Enter()
    {
        //检测是否需要更新
        var localVer = GetLocalResVersion();
        Debug.Log("本地资源版本:"+localVer);

        //先判断是否是白名单，如果是，判断test资源版本号
        //如果非白名单，则直接判断 
        if (PlayerPrefs.HasKey("whitelist") && PlayerPrefs.GetString("whitelist") == "true")
        {
            
        }
        else
        {
            
        }

        if(ServerList.Singleton.Res)
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
