﻿/*
 * file CoroutineManager.cs
 *
 * author: 
 * date:   2014/10/9
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineManager : MonoBehaviour
{
    /// <summary>
    /// 内部辅助类
    /// </summary>
    private class CoroutineTask
    {
        public Int64 Id { get; set; }
        public bool Running { get; set; }
        public bool Paused { get; set; }

        public CoroutineTask(Int64 id)
        {
            Id = id;
            Running = true;
            Paused = false;
        }

        public IEnumerator CoroutineWrapper(IEnumerator co)
        {
            IEnumerator coroutine = co;
            while (Running)
            {
                if (Paused)
                    yield return null;
                else
                {
                    if (coroutine != null && coroutine.MoveNext())
                        yield return coroutine.Current;
                    else
                        Running = false;
                }
            }
            mCoroutines.Remove(Id.ToString());
        }
    }


    private static Dictionary<string, CoroutineTask> mCoroutines;
    public static CoroutineManager Singleton { get; private set; }

    void Awake()
    {
        Singleton = this;
        mCoroutines = new Dictionary<string, CoroutineTask>();
    }

    /// <summary>
    /// 启动一个协程
    /// </summary>
    /// <param name="co"></param>
    /// <returns></returns>
    public Int64 AddCoroutine(IEnumerator co)
    {
        if (this.gameObject.activeSelf)
        {
            CoroutineTask task = new CoroutineTask(IdAssginer.getId(IdAssginer.IdType.CoroutineId));
            mCoroutines.Add(task.Id.ToString(), task);
            StartCoroutine(task.CoroutineWrapper(co));
            return task.Id;
        }
        return -1;
    }

    /// <summary>
    /// 停止一个协程
    /// </summary>
    /// <param name="id"></param>
    public void RemoveCoroutine(Int64 id)
    {
        CoroutineTask task = null;
        mCoroutines.TryGetValue(id.ToString(), out task);
        if (task != null)
        {
            task.Running = false;
            mCoroutines.Remove(id.ToString());
        }
    }

    /// <summary>
    /// 暂停协程的运行
    /// </summary>
    /// <param name="id"></param>
    public void PauseCoroutine(Int64 id)
    {
        CoroutineTask task = null;
        mCoroutines.TryGetValue(id.ToString(), out task);
        if (task != null)
        {
            task.Paused = true;
        }
        else
        {
            Debug.LogError("coroutine: " + id.ToString() + " is not exist!");
        }
    }

    /// <summary>
    /// 恢复协程的运行
    /// </summary>
    /// <param name="id"></param>
    public void ResumeCoroutine(Int64 id)
    {
        CoroutineTask task = null;
        mCoroutines.TryGetValue(id.ToString(), out task);
        if (task != null)
        {
            task.Paused = false;
        }
        else
        {
            Debug.LogError("coroutine: " + id.ToString() + " is not exist!");
        }
    }

    public long DelayedCall(float delayedTime, Action callback)
    {
        return AddCoroutine(delayedCallImpl(delayedTime, callback));
    }

    private IEnumerator delayedCallImpl(float delayedTime, Action callback)
    {
        if (delayedTime >= 0)
            yield return new WaitForSeconds(delayedTime);
        callback();
    }


    public long DelayedCall(float delayedTime, Action<object> callback, object param)
    {
        return AddCoroutine(delayedCallImpl(delayedTime, callback, param));
    }

    private IEnumerator delayedCallImpl(float delayedTime, Action<object> callback, object param)
    {
        if (delayedTime >= 0)
            yield return new WaitForSeconds(delayedTime);
        callback(param);
    }

    void OnDestroy()
    {
        foreach (CoroutineTask task in mCoroutines.Values)
        {
            task.Running = false;
        }
        mCoroutines.Clear();
    }

    internal void AddCoroutine(object onUpdate)
    {
        throw new NotImplementedException();
    }
}
