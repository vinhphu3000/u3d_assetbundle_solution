using UnityEngine;
using System.Collections;
using FairyGUI;
using System.Collections.Generic;
using System;

public class BaseWindow: GComponent
{ 
    // 启动的协程列表,关闭窗口的时候  需要清理协程
    protected List<long> mCoroutines = new List<long>();

    protected bool isClose = false;
    protected bool isHide = false; 

    public bool IsClose
    {
        protected set { isClose = value; }
        get { return isClose; }
    }
    public bool IsHide
    {
        protected set { isHide = value; }
        get { return isHide; }
    }

    static char[] pathSegChar = new char[]{'/'};
     
    public bool bringToFontOnClick { get; set; }
    GComponent _frame;
    GComponent _root;
    GComponent _contentPane;
    GObject _modalWaitPane;
    GObject _closeButton;
    GObject _dragArea;
    GObject _contentArea; 
     
    bool _inited;
    bool _loading; 

    protected BaseWindow(string name):base()
    { 
        this.focusable = true;
        bringToFontOnClick = UIConfig.bringWindowToFrontOnClick;

        displayObject.onAddedToStage.Add(_onAddedToStage);
        displayObject.onRemovedFromStage.Add(_onRemovedFromStage);
        displayObject.onTouchBegin.AddCapture(_onTouchBegin);

        this.rootContainer.gameObject.name = name;
        this.name = name;
        base.name = name;
    }

    public GComponent contentPane
    {
        set
        {
            if (_contentPane != value)
            {
                if (_contentPane != null)
                    RemoveChild(_contentPane);
                _contentPane = value;
                if (_contentPane != null)
                {
                    AddChild(_contentPane);
                    this.SetSize(_contentPane.width, _contentPane.height);
                    _contentPane.AddRelation(this, RelationType.Size);
                    _contentPane.fairyBatching = true;
                    _frame = _contentPane.GetChild("frame") as GComponent;
                    if (_frame != null)
                    {
                        this.closeButton = _frame.GetChild("closeButton");
                        this.dragArea = _frame.GetChild("dragArea");
                        this.contentArea = _frame.GetChild("contentArea");
                    }
                }
                else
                    _frame = null;
            }
        }
        get
        {
            return _contentPane;
        }
    }

    public GComponent frame
    {
        get { return _frame; }
    }

    public GObject closeButton
    {
        get { return _closeButton; }
        set
        {
            if (_closeButton != null)
                _closeButton.onClick.Remove(Close);
            _closeButton = value;
            if (_closeButton != null)
                _closeButton.onClick.Add(Close);
        }
    }

    public GObject dragArea
    {
        get { return _dragArea; }
        set
        {
            if (_dragArea != value)
            {
                if (_dragArea != null)
                {
                    _dragArea.draggable = false;
                    _dragArea.onDragStart.Remove(__dragStart);
                }

                _dragArea = value;
                if (_dragArea != null)
                {
                    if ((_dragArea is GGraph) && ((GGraph)_dragArea).displayObject == null)
                        ((GGraph)_dragArea).DrawRect(_dragArea.width, _dragArea.height, 0, Color.clear, Color.clear);
                    _dragArea.draggable = true;
                    _dragArea.onDragStart.Add(__dragStart);
                }
            }
        }
    }

    public GObject contentArea
    {
        get { return _contentArea; }
        set { _contentArea = value; }
    }

    public GObject modalWaitingPane
    {
        get { return _modalWaitPane; }
    }
        

    public void Show()
    {
        if (_root == null)
        {
            Debug.LogError("窗口root节点为空");
            return;
        }

        rootContainer.gameObject.SetActive(true);
        touchable = true;

        IsHide = false;
        isClose = false;
        if(parent!=_root)
            _root.AddChild(this);
        isHide = false;
        //GameEventDispatcher.Singleton.DispatchEvent(EventID.WindowOpen, name);
        //GameEventDispatcher.Singleton.DispatchEvent(EventID.GuideOnWindowOpen, name);
        OnShown();
    }

    public void SetRoot(GComponent root)
    {
        _root = root;
    }

    public void Hide()
    {
        IsHide = true; 
        isClose = false;
        HideWindowImmediately(false);

        //GameEventDispatcher.Singleton.DispatchEvent(EventID.WindowClose, name);
        //GameEventDispatcher.Singleton.DispatchEvent(EventID.GuideOnWindowClose, name);

        OnHide();
    }

    public void Close()
    {
        isClose = true;
        OnClose();
        HideWindowImmediately(true);
        RemoveAllCoroutine();
    } 

    public void CenterOn(GRoot r, bool restraint)
    {
        this.SetXY((int)((r.width - this.width) / 2), (int)((r.height - this.height) / 2));
        if (restraint)
        {
            this.AddRelation(r, RelationType.Center_Center);
            this.AddRelation(r, RelationType.Middle_Middle);
        }
    } 

    public bool isShowing
    {
        get { return parent != null; }
    }

    public bool isTop
    {
        get { return parent != null && parent.GetChildIndex(this) == parent.numChildren - 1; }
    }

    public void BringToFront()
    {
        if (parent == null)
            return;
        int cnt = parent.numChildren;
        int i = cnt - 1;

        parent.SetChildIndex(this, i);
    }

    public void Init()
    {
        if (_inited || _loading)
            return;

        _inited = true;
        OnInit();  
    }

  
    /// <summary>
    ///当窗口第一次打开调用 
    /// </summary>
    protected virtual void OnInit()
    {    
        
    }
     
    /// <summary>
    /// 当窗口显示的时候 调用
    /// </summary>
    protected virtual void OnShown()
    { 
    }

    /// <summary>
    /// 当窗口hide的时候调用
    /// </summary>
    protected virtual void OnHide()
    {
    }

    /// <summary>
    /// 当窗口关闭释放的时候调用
    /// </summary>
    protected virtual void OnClose()
    { 
        
    }

    public void HideWindowImmediately(bool dispose)
    {
        if(dispose)
        {
            if (_root != null)
                _root.RemoveChild(this, dispose);
            if (dispose)
                this.Dispose();
        }
        else
        {
            touchable = false;
            rootContainer.gameObject.SetActive(false);
        }

    }

    override public void Dispose()
    {
        if (_modalWaitPane != null && _modalWaitPane.parent == null)
            _modalWaitPane.Dispose();

        base.Dispose();
    }

    void _onAddedToStage()
    {
        if (!_inited)
            Init(); 
    }

    void _onRemovedFromStage()
    {
        Hide();
    }

    private void _onTouchBegin(EventContext context)
    {
        if (this.isShowing && bringToFontOnClick)
        {
            BringToFront();
        }
    }

    private void __dragStart(EventContext context)
    {
        context.PreventDefault();

        this.StartDrag((int)context.data);
    }

    /// <summary>
    /// 处理窗口消息
    /// </summary> 
    public virtual void HandleMessage(int msgId,object para)
    {
        
    }

    /// <summary>
    /// 窗口内部启用的携程  需要在窗口关闭的时候停止的话 就必须要用这个接口
    /// </summary>
    public long AddCoroutine(IEnumerator co)
    {
        long ret = CoroutineManager.Singleton.startCoroutine(co);
        mCoroutines.Add(ret);
        return ret;
    }

    /// <summary>
    /// 停止一个协程
    /// </summary>
    /// <param name="id"></param>
    public void RemoveCoroutine(long id)
    {
        CoroutineManager.Singleton.stopCoroutine(id);
        int idx = mCoroutines.IndexOf(id);
        if (idx >= 0)
        {
            mCoroutines.RemoveAt(idx);
        }
    }
 
    public void RemoveAllCoroutine()
    {
        for (int i = 0;i< mCoroutines.Count;i++)
        {
            CoroutineManager.Singleton.stopCoroutine(mCoroutines[i]); 
        }
        mCoroutines.Clear();
    } 

    public GComponent GetUIObjectByPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return null;

        string[] strs = path.Split(pathSegChar);

        GComponent ret = contentPane;

        for (int i = 0; i < strs.Length; i++)
        {
            if (strs[i] == "")
                continue;
             ret = ret.GetChild(strs[i]).asCom;
            if (ret == null)
            {
                Debug.LogError("不能找到ui对象,win name:" + name + ",:"+ path);
                return null;
            }
        }
        return ret;
    } 
}
