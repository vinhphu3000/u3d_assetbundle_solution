using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public delegate bool RepeatCallAction();
public static class LeanTweenExt
{
    static public int Value(float from, float to, float time, float timeInterval, Action<float> callOnUpdate, Action CompCallBack = null)
    { 
        float valueInterval = 0;
        if (time > 0)
        {
            valueInterval = Mathf.Abs((to - from) * timeInterval / time);
        }

        float lastValue = from;
        bool first = true;
        return LeanTween.value(from, to, time).setOnUpdate((float v) =>
       {
           if (Mathf.Abs(v - lastValue) >= valueInterval || UnityEngine.Mathf.Abs(v - to) <= 0.0001f || first)
           {
               lastValue = v;
               first = false;
               if (callOnUpdate != null)
               {
                   callOnUpdate(v);
               }
           }
       }).setOnComplete(CompCallBack).id;
    }

    static public int RepeatCall(float timeInterval, RepeatCallAction callOnUpdate)
    {
        Action at = null;
        int id = 0;
        LTDescr descr = null;
        at = () =>
        {
            if (!callOnUpdate())
            {
                LeanTween.cancel(id);
                descr = null;
                at = null;
            }
            else
            {
                if(descr.loopCount<=1)
                {
                    descr.setLoopCount(int.MaxValue -1);
                }
            }
        };
    
        descr = LeanTween.delayedCall(timeInterval, at).setRepeat(int.MaxValue - 1);
        id = descr.id;
        return id;
    }
 
    class WaitForNextFrameCallHolder
    {
        public IEnumerator Do(Action call)
        {
            yield return new WaitForEndOfFrame();
            call();
        }
    }

    static public long WaitForNextFrameCall(Action call)
    {
        return CoroutineManager.Singleton.AddCoroutine(new WaitForNextFrameCallHolder().Do(call));
    }

    static public void CancelWaitForNextFrameCall(long id)
    {
        CoroutineManager.Singleton.RemoveCoroutine(id);
    } 
}
