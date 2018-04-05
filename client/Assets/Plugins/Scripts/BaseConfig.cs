using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class BaseConfig :SingletonTemplate<BaseConfig>
{ 
    public string AppVersion;
    public string ResVersion;
    public string CodeVersion;
    public string BaseIndexServerUrl;
    public string ChannelId;

    public void Init()
    {
        var cfgStr = "";
        #if UNITY_EDITOR
        cfgStr = Resources.Load<TextAsset>("Config/editor_cfg").text;
        #elif UNITY_ANDROID
        cfgStr = Resources.Load<TextAsset>("Config/android_cfg").text; 
        #elif UNITY_IOS
        cfgStr = Resources.Load<TextAsset>("Config/ios_cfg").text; 
        #else
        Debug.LogError("不能发现基本配置！");
        #endif

        JsonData jsonData = JsonMapper.ToObject(cfgStr);
        this.AppVersion         = jsonData["AppVersion"].ToString();
        this.ResVersion         = jsonData["ResVersion"].ToString();
        this.CodeVersion        = jsonData["CodeVersion"].ToString();
        this.BaseIndexServerUrl = jsonData["BaseIndexServerUrl"].ToString();
        this.ChannelId          = jsonData["ChannelId"].ToString();
     }

    public string GetAppBigVersion()
    {
        var strs = AppVersion.Split(new char[]{ '.' });
        return strs[0] + "." + strs[1];
    }
}
