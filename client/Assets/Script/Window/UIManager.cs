using UnityEngine;
using System.Collections;
using FairyGUI;
using System;
using System.Collections.Generic;

public enum UILayer
{
    Bottom = 0,
    MainUI,
    HUD,          //场景自带UI
    HUD2,
    Popup,          //所有从HUD中弹出的窗口
    Notice,         //提示层（滚动条之类）
    Loading,        //loading界面 在最上层 
    Guide,          //引导层 
    God,            //最顶层
}

public class UIManager : SingletonTemplate<UIManager> {
     
    /// <summary>
    /// 外部UI资源加载器
    /// 加载单独图片或者通过单独ab加载的tex
    /// </summary>
    class MyGLoader : GLoader
    {
        protected override void LoadExternal()
        {
            //IconManager.inst.LoadIcon(this.url, OnLoadSuccess, OnLoadFail);
        }

        protected override void FreeExternal(NTexture texture)
        {
            texture.refCount--;
        }

        void OnLoadSuccess(NTexture texture)
        {
            if (string.IsNullOrEmpty(this.url))
                return;

            this.onExternalLoadSuccess(texture);
        }

        void OnLoadFail(string error)
        {
            Debug.Log("load " + this.url + " failed: " + error);
            this.onExternalLoadFailed();
        }
    }

    /// <summary>
    /// 窗口信息
    /// </summary>
    class WindowInfo
    { 
        public Type type;
        public string Name;
        public BaseWindow ins;
        public bool isCloseOnCloseAll;
        public UILayer layer;
        public bool isHideAllHud = false;
    }

    /// <summary>
    /// 保存所有窗口信息
    /// </summary>
    Dictionary<string,WindowInfo> winInfos = new Dictionary<string, WindowInfo>(); 
    Dictionary<UILayer, GComponent> layerRoots = new Dictionary<UILayer, GComponent>();
 
    public void Init()
    {
        UIObjectFactory.SetLoaderExtension(typeof(MyGLoader));

        UIConfig.defaultFont = "FZZhengHeiS-EL-GB";
        UIConfig.bringWindowToFrontOnClick = false;
        //load base res
        LoadPackage("基础资源包");
        LoadPackage("战斗窗口");
        LoadPackage("Icon包");
        LoadPackage("所有小贴士");


        //注册窗口
        //CSWindowRegister.Register();

        //添加层级
        var layer = UIPackage.CreateObject("基础资源包", "UILayer").asCom;
        if (layer != null)
        {
            layer.fairyBatching = true;
            layer.SetSize(GRoot.inst.width, GRoot.inst.height);
            layer.AddRelation(GRoot.inst, RelationType.Size);
            GRoot.inst.AddChild(layer);

            for (int i = 0; i <= (int)UILayer.God; i++)
            {
                UILayer le = (UILayer)i;
                var str = le.ToString();
                var com  = layer.GetChild(str).asCom;
                com.gameObjectName = str;
                layerRoots[le] = com;
            }
        }
    }

    /// <summary>
    /// load package
    /// </summary> 
    public void LoadPackage(string packageName)
    {
        if (UIPackage.GetByName(packageName) == null)
        {
            // TODO 这里需要判断是否是ab加载（更新）
            
            UIPackage.AddPackage("UI/" + packageName);
        }
    }

    public string GetItemUrl(string pkgName, string resName)
    {
        return UIPackage.GetItemURL(pkgName, resName);
    }

    public GObject CreateObject(string pkgName,string resName)
    {
        return UIPackage.CreateObject(pkgName, resName);
    }

    /// <summary>
    /// Removes the package res
    /// </summary> 
    public void RemovePackage(string packageName)
    {
        if (string.IsNullOrEmpty(packageName))
        {
            Debug.LogError("RemovePackage,packageName is null");
            return;
        }
        UIPackage.RemovePackage(packageName,true);
    }

    /// <summary>
    /// 打开view(主要针对资源，窗口逻辑脚本不在这里). 所有的页面都是基于窗口的逻辑
    /// </summary>
    public GComponent CreateUIPage(string packageName,string resName,bool resize = true)
    {  
        LoadPackage(packageName);
        var view = UIPackage.CreateObject(packageName, resName).asCom;
        if (view != null)
        {
            view.fairyBatching = true;
            if (resize)
            {
                view.SetSize(GRoot.inst.width, GRoot.inst.height);
                view.AddRelation(GRoot.inst, RelationType.Size);
            }
            //GRoot.inst.AddChild(view); 
        }
        return view;  
    } 

    /// <summary>
    /// 注册窗口类型
    /// </summary> 
    public void ReqisterWindowType(string name,Type type)
    {  
        WindowInfo info;
        winInfos.TryGetValue(name,out info);
        if (info != null)
        {
            if (info.ins != null)
            {
                Debug.LogError("注册的窗口已经被创建:" + name);
                return; 
            }
        }
        else
        {
            info = new WindowInfo();
            winInfos[name] = info;
        }

        info.ins = null; 
        info.type = type; 
        info.Name = name;
    }


    public GComponent GetLayerRoot(UILayer layer)
    {
        return layerRoots[layer];
    }

    public BaseWindow OpenWindow(Type type, UILayer layer, bool isCloseOnCloseAll = true, int sortingOrder = 0, bool isFront = true)
    {
        return OpenWindow(type.Name, layer, isCloseOnCloseAll, sortingOrder, isFront);
    }

    /// <summary>
    /// 打开窗口
    /// </summary> 
    Type[] tCache = new Type[] { };
    object[] pCache = new object[] { };
    public BaseWindow OpenWindow(string name, UILayer layer, bool isCloseOnCloseAll = true, int sortingOrder = 0, bool isFront = true)
    {
        WindowInfo info;
        winInfos.TryGetValue(name, out info);
        if (info != null)
        {
            info.isCloseOnCloseAll = isCloseOnCloseAll;

            info.isHideAllHud = false;
            info.layer = layer;
            //if window is hide
            if (info.ins != null && !info.ins.IsClose)
            {
                if (info.ins.IsHide)
                {
                    info.ins.SetRoot(layerRoots[layer]);
                    info.isHideAllHud = false; 
                    info.ins.Show();
                }
            }
            else
            {
                //   Logger.err(info.type.ToString());
                info.ins = (BaseWindow)(info.type.GetConstructor(tCache).Invoke(pCache));
                info.ins.SetRoot(layerRoots[layer]);
                info.ins.Show();
            }
             
            if(sortingOrder==0) 
            {
                info.ins.BringToFront();
            }
            else
            { 
                info.ins.sortingOrder = sortingOrder;
            }

            return info.ins;
        }
        else
        {
            Debug.LogError("窗口类没有注册:"+name);
        }
        return null;
    }

    public void HideWindow(string name)
    {
        WindowInfo info;
        winInfos.TryGetValue(name, out info);
        if (info != null)
        {
            if (info.ins != null)
            {
                if (!info.ins.IsClose && !info.ins.IsHide)
                {
                    if (info.isHideAllHud)
                    {
                        info.isHideAllHud = false;
                        info.ins.visible = true;
                    }
                    info.ins.Hide();
                }
            }
        }
    }
 


    /// <summary>
    /// 关闭窗口
    /// </summary> 
    public void CloseWindow(string name)
    {
        WindowInfo info;
        winInfos.TryGetValue(name,out info);
        if (info != null)
        {
            if (info.ins != null)
            {
                var ins = info.ins;
                info.ins = null;
                if (!ins.IsClose)
                    ins.Close();
            } 
        }
    }
 
    /// <summary>
    /// 得到打开的窗口
    /// </summary>
    /// <returns>The window.</returns>
    /// <param name="name">Name.</param>
    public BaseWindow GetWindow(string name)
    {
        WindowInfo info;
        winInfos.TryGetValue(name,out info);
        if (info != null)
        {
            if (info.ins != null)
            { 
                if (!info.ins.IsClose)
                    return info.ins;  
            }
        }
        return null;
    }


    /// <summary>
    /// 判断窗口是否打开
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool IsWindowOpen(string name)
    {
        var win = GetWindow(name);
        if(win!=null)
            return (!win.IsClose && !win.IsHide);
        return false;
    }

    public bool IsWindowInit(string name)
    {
        var win = GetWindow(name);
        return win != null;
    }

    public UILayer GetOpenWindowLayer(string name)
    {
        WindowInfo info = null;
        winInfos.TryGetValue(name,out info);
        if(info!=null)
        {
            return info.layer;
        }

        return UILayer.HUD;
    }

    public int GetOpenWindowSortingOrder(string name)
    {
        var win = GetWindow(name);
        if (win != null)
            return win.sortingOrder;
        return -1;
    }

    /// <summary>
    /// 向指定窗口发送事件
    /// </summary>
    /// <param name="win">Window.</param>
    /// <param name="msgId">Message identifier.</param>
    /// <param name="data">Data.</param>
    public void SendWindowMessage(string winName,int msgId,object data)
    {
        var winIns = GetWindow(winName);
        if(winIns!=null)
        {
            winIns.HandleMessage(msgId,data);
        }
    }

    /// <summary>
    /// 关闭所有窗口，并释放所有窗口资源
    /// </summary>
    public void CloseAllWindow(bool force = false)
    {
        foreach (var win in winInfos)
        {
            if (force || win.Value.isCloseOnCloseAll)
            { 
                CloseWindow(win.Value.Name);
            }
        } 
    }

    /// <summary>
    /// 隐藏layer下所有窗口
    /// </summary>
    /// <param name="layer"></param>
    public void HideAllHUD()
    {
        foreach (var kv in winInfos)
        {
            var winInfo = kv.Value;
            if (winInfo.ins != null && winInfo.layer == UILayer.HUD && !winInfo.ins.IsClose && !winInfo.ins.IsHide)
            {
                winInfo.ins.touchable = false;
                winInfo.ins.rootContainer.gameObject.SetActive(false);
                winInfo.isHideAllHud = true; 
            }
        }
    }

    public void OpenAllHUD()
    {
        foreach (var kv in winInfos)
        {
            var winInfo = kv.Value;
            if (winInfo.ins != null && winInfo.isHideAllHud && winInfo.layer == UILayer.HUD && !winInfo.ins.IsClose && !winInfo.ins.IsHide)
            { 
                winInfo.ins.rootContainer.gameObject.SetActive(true);
                winInfo.isHideAllHud = false; 
                winInfo.ins.touchable = true; 
            }
        }
    }
}

