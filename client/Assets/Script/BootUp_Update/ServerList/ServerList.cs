  using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using LitJson;

public class ServerInfo
{
    public int      Id;
    public string   Name;
    public string   Ip;
    public int      Port;
    public int      Status;
    public string   Desc;
    public string   TimeZone;
    public string   Param;
}
    

/// <summary>
/// 服务器列表
/// </summary>
public class ServerList:SingletonTemplate<ServerList>
{
    public string BeOpenRes;            //白名单测试资源
    public string TestResVersion;       //白名单测试资源版本号

    public string BeOpenApp;            //白名单测试强更
    public string TestAppVersion;       //白名单测试强更版本号

    public bool   IsWhiteList;          //前端是否处于白名单 

    public string AppVersion;           //app版本号 
    public string ResVersion;           //当前正式最新的资源版本号

    public string BackCode;             //后台激活码
 
    public string ResUpdateUrl;         //资源服务器 
    public string NoticeUrls;           //公告服务器 
    public string AppUpdateUrls;        //app强更地址 
    public bool   IsPassed;             //是否通过审核,有些功能是ios过审后才开启
        
    public List<ServerInfo> Servers;    //游戏服列表

    static ServerList ins = null;

    public static ServerList Singleton
    {
        get
        {
            if (ins == null)
            {
                ins = new ServerList();
            }
            return ins;
        }
    }

    public void Init(string json)
    {
        JsonData jsonData   = JsonMapper.ToObject(json);
        this.AppVersion     = jsonData["AppVersion"].ToString();
        this.ResVersion     = jsonData["ResourceVersion"].ToString();
        this.IsPassed       = Convert.ToBoolean(jsonData["Passed"].ToString());
        this.BackCode       = jsonData["Back"].ToString();
        this.BeOpenRes      = jsonData["BeOpenRes"].ToString();
        this.TestResVersion = jsonData["TestResVersion"].ToString();
        this.BeOpenApp      = jsonData["BeOpenApp"].ToString();
        this.TestAppVersion = jsonData["TestAppVersion"].ToString();

        var servers = jsonData["ServerLists"] as JsonData;
        int count = servers.Count;
        List<ServerInfo> serverInfos = new List<ServerInfo>();
        for (int i=0;i<count;i++)
        {
            var serData = servers[i];
            var serInfo = new ServerInfo();
            serInfo.Id      = Convert.ToInt32(serData["ServerId"].ToString());
            serInfo.Name    = serData["ServerName"].ToString();
            serInfo.Desc    = serData["ServerNameDesc"].ToString();
            serInfo.Ip      = serData["ServerUrl"].ToString();
            serInfo.Port    = Convert.ToInt32(serData["Port"].ToString());
            serInfo.Status  = Convert.ToInt32(serData["ServerStatus"].ToString());
            serInfo.TimeZone= serData["TimeZone"].ToString();
            serInfo.Param   = serData["Para"].ToString();
            serverInfos.Add(serInfo);
        }
    }


    public string GetAppBigVersion()
    {
        var strs = AppVersion.Split(new char[]{ '.' });
        return strs[0] + "." + strs[1];
    }
}