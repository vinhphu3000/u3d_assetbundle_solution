using UnityEngine;
using System.Collections;
using System.Collections.Generic; 


public class InstanceGameObjectCache
{
    private Dictionary<string, int> maxCountDict;//string:对象的名字，int:该类对象能够缓存的最大个数。  
    public int maxCount = 50;
    public Dictionary<string, List<GameObject>> itemDict;

    public InstanceGameObjectCache()
    {
        maxCountDict = new Dictionary<string, int>();
        itemDict = new Dictionary<string, List<GameObject>>(); 
    }

    //设置某一类资源的最大缓存个数
    public void SetGameobjectMaxCount(string resName, int count)
    {
        if (resName != null) { resName = resName.toLower(); }
        maxCountDict.Add(resName, count);
    }

    public int GetGameobjectMaxCount(string resName)
    {
        if (resName != null) { resName = resName.toLower(); }
        if (maxCountDict.ContainsKey(resName))
        {
            return maxCountDict[resName];
        }

        return maxCount;
    }

    public int GetCurCount(string resName)
    {
        if (resName != null) { resName = resName.toLower(); }
        List<GameObject> list;
        itemDict.TryGetValue(resName, out list);
        if (list != null)
        {
            return list.Count;
        }

        return 0;
    }

    //重置最大资源个数为默认值
    public void ResetMaxCount()
    {
        maxCountDict.Clear();
    }

    //放入一个           
    public void PushItem(GameObject item, string resName)
    {
        if (item != null)
        {  
            if (resName != null)
            { 
                resName = resName.toLower(); 
            }
            else
            {
                return;
            }

            int max = maxCountDict.ContainsKey(resName) ? maxCountDict[resName]: maxCount;

            List<GameObject> list;
            itemDict.TryGetValue(resName, out list);  
            if (list == null)
            {
                list = new List<GameObject>();
                itemDict.Add(resName, list);
            }     

            //防止外部多次push调用时，添加相同的对象
            if (!list.Contains(item))
            {
                if (list.Count < max)
                {
                    item.SetActive(false);
                    item.transform.position = new Vector3(50000f, 50000f, 0f);
                    list.Add(item);
                }
                else
                {
                    //超过缓存容量的直接释放掉     
                    GameObject.DestroyImmediate(item);
                }
            } 

            item = null;
        }
    }

    //推出一个
    public GameObject PopItem(string key)
    {
        GameObject popItem = null;
        if (key != null) { key = key.toLower(); }
        List<GameObject> list;
        itemDict.TryGetValue(key, out list);
        if (list != null && list.Count > 0)
        {
            popItem = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
        }

        if (popItem != null && popItem.activeSelf == false)
        {
            popItem.SetActive(true);
        }

        return popItem;
    }

 

    //特殊的地方会要求删除已经存在的对象
    public void DestroyExistItem(GameObject item, string resName)
    {
        if (item == null)
            return;
        if (resName != null) { resName = resName.toLower(); } 
        List<GameObject> list;
        itemDict.TryGetValue(resName, out list);
        if (list != null && list.Contains(item))
        {
            list.Remove(item);
        }
        GameObject.DestroyImmediate(item);
    }

 
    public void cleanAll()
    {
        foreach (List<GameObject> list in itemDict.Values)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null)
                    GameObject.DestroyImmediate(list[i]);
            }
        } 
        maxCountDict.Clear();
    }

    public void clearSomeObj()
    {
        foreach (List<GameObject> list in itemDict.Values)
        {
            #if UNITY_ANDROID
            int count = Mathf.Min(list.Count / 5 + 1, list.Count);
            #else
            int count = Mathf.Min(list.Count / 2 + 1, list.Count);
            #endif
            if (count > 0)
            {
                count = list.Count - count;
                for (int i = list.Count - 1; i >= count; i--)
                {
                    if (list[i] != null)
                    {
                        GameObject.DestroyImmediate(list[i]);
                    }
                    list.RemoveAt(i);
                }
            }

        }
    }

    int clearHeroFlag = 0;
    public void clearSomeHeroObj()
    {
        if (clearHeroFlag++ < 3)
        {
            return;
        }
        clearHeroFlag = 0;

        //hero判断条件为只有一个
        foreach (List<GameObject> list in itemDict.Values)
        {
            int count = Mathf.Min(list.Count / 2 + 1, list.Count);
            if (count == 1)
            {
                if (list[0] != null && list[0].name.toLower().Contains("hero"))
                    GameObject.DestroyImmediate(list[0]);
                list.Clear();
            }
        }
    }

    public bool contains(string key)
    {
        if (key != null) { key = key.toLower(); }
        List<GameObject> list;
        itemDict.TryGetValue(key,out list);
        return list!= null && list.Count > 0;
    }
}