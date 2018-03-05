using UnityEngine;
using System.Collections;

/// <summary> 
/// 特效设置相关脚本
/// Date: 2016/1/19
/// Author: lxz  
/// </summary>
public class EffectSetting : MonoBehaviour
{
    [Header("是否能被缓存的对象")]
    public bool CanCache = true;

    [Header("中等效果需隐藏的对象")]
    public GameObject[] MiddleLevelHideObj;
    [Header("低等效果需隐藏的对象")]
    [Header("也会隐藏中等效果对象，所以可不必把中等特效对象再拖到这里面")]
    public GameObject[] LowLevelHideObj; 

}
