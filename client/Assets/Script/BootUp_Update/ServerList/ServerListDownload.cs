using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerListDownload :SingletonTemplate<ServerListDownload> {

    public void Download()
    {
        //下载serverlist
        var serverlistUrl = BaseConfig.Singleton.BaseIndexServerUrl + "/serverlist/ServerList.json"+ BaseConfig.Singleton.ChannelId;
        Debug.Log("ServerList地址:"+serverlistUrl);

        HttpDownload.Request(serverlistUrl,15,onCmp,onTimeout,onFail,onUpdate);
    }

    private void onCmp(WWW www)
    {
        var text = HttpDownload.GetString(www); 
        ServerList.Singleton.Init(text);
        Debug.Log("serverlist下载成功:"+text);
        //检测是否需要更新app
        AppUpdate.Singleton.Enter();
    }   

    private void onTimeout()
    {
        Debug.Log("serverlist下载超时"); 
    }   

    private void onFail()
    {
        Debug.Log("serverlist下载失败");  
    }

    private void onUpdate(float p)
    {
        Debug.Log("serverlist下载进:"+p);  
    }
}
