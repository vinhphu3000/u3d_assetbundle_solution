using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Events;

public class HttpDownload
{

    public class MyWWW
    {
        public string ID { private set; get; }
        private float mTimeOut;
        private UnityEngine.WWW mWWW;
        private float mPassedTime;
        private UnityAction<UnityEngine.WWW> mCmpCallBack;
        private UnityAction mTimeOutCallBack;
        private UnityAction mFailCallBack;
        private UnityAction<float> monProgress;

        public MyWWW(string url, float outTime, UnityAction<WWW> cmpCallBack = null, UnityAction timeOutCallBack = null, UnityAction failCallback = null, string postfix = "")
        { 
            mWWW = new UnityEngine.WWW(url);
            mFailCallBack = failCallback;
            mCmpCallBack = cmpCallBack;
            mTimeOutCallBack = timeOutCallBack;
            mTimeOut = outTime;
            mPassedTime = 0;
            ID = IdAssginer.getId(IdAssginer.IdType.WWW).ToString() + postfix;
        }
        public MyWWW(string url, float outTime, UnityAction<WWW> cmpCallBack = null, UnityAction timeOutCallBack = null, UnityAction failCallback = null, UnityAction<float> onProgress = null, string postfix = "")
        {
            mWWW = new UnityEngine.WWW(url);
            mFailCallBack = failCallback;
            mCmpCallBack = cmpCallBack;
            mTimeOutCallBack = timeOutCallBack;
            mTimeOut = outTime;
            mPassedTime = 0;
            ID = IdAssginer.getId(IdAssginer.IdType.WWW).ToString() + postfix;
            monProgress = onProgress;
        }

        private float mLastProgress = 0;
        private float mLastHotProgress = 0;

        public void update(float deltaTime)
        {
            mPassedTime += deltaTime;
            if (ID.Contains(UpdatePostfix))
            {
                float deltaProgrss = mWWW.progress - mLastProgress;
                mLastProgress = mWWW.progress;                       
            }

            if (monProgress != null)
            {
                monProgress(mWWW.progress);
            }
        }

        public IEnumerator DownLoadCoroutine()
        {
            while (true)
            {
                if (mWWW == null || !string.IsNullOrEmpty(mWWW.error))
                {
                    Debug.LogError(mWWW.url + ": " + mWWW.error);
                    if (mFailCallBack != null)
                        mFailCallBack();
                    mWWW.Dispose();
                    mWWW = null;
                    yield break;
                }
                else if (mWWW.isDone)
                {
                    if (mCmpCallBack != null)
                        mCmpCallBack(mWWW);
                    mWWW.Dispose();
                    mWWW = null;
                    yield break;
                }
                else
                {
                    update(Time.deltaTime);
                }
                yield return null;
            }
        }
    }

    public const string UpdatePostfix = "_Update";
    public const string hotUpdate = "_UpdateRes"; 

    public static void Request(string url, float timeout, UnityAction<WWW> cmpCallback = null, UnityAction timeOutCallback = null, UnityAction failCallback = null, string postfix = "")
    {
        MyWWW mw = new MyWWW(url, timeout, cmpCallback, timeOutCallback, failCallback, postfix);
        CoroutineManager.Singleton.startCoroutine(mw.DownLoadCoroutine());
    }
    public static void Request(string url, float timeout, UnityAction<WWW> cmpCallback, UnityAction timeOutCallback, UnityAction failCallback, UnityAction<float> onProgress, string postfix="")
    {
        MyWWW mw = new MyWWW(url, timeout, cmpCallback, timeOutCallback, failCallback, onProgress, postfix);
        CoroutineManager.Singleton.startCoroutine(mw.DownLoadCoroutine());
    }


    public static string GetString(WWW www)
    {
        if (www == null || !www.isDone)
            return "";
        
        if (www.text != "" || www.text != null)
            return www.text;
        
        if (www.bytes == null || www.bytes.Length == 0)
            return StringTools.BytesToString(www.bytes);

        return "";
    }
}