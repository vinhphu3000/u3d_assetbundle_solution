using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Collections.Generic;

#if UNITY_ANDROID

public sealed class IOAndroidLoader
{
    private static IOAndroidLoader m_instance; // 实例
    bool isInit = false;
    IntPtr clazzPtr;
    IntPtr methodPtr1;
    IntPtr methodPtr2;

    public static IOAndroidLoader Instance
    {
        get
        {
            if (m_instance == null) m_instance = new IOAndroidLoader();
            return m_instance;
        }
    }

    public byte[] GetBytes(string path)
    {
        if (!isInit)
        {
            var _helper = new AndroidJavaClass("unity.tools.ReadAsset");
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                object jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
                _helper.CallStatic("init", jo);
            }

            clazzPtr = AndroidJNI.FindClass("unity/tools/ReadAsset");
            methodPtr1 = AndroidJNI.GetStaticMethodID(clazzPtr, "isFileExists", "(Ljava/lang/String;)Z");
            methodPtr2 = AndroidJNI.GetStaticMethodID(clazzPtr, "readFile", "(Ljava/lang/String;)[B");
            isInit = true;
        }

        byte[] data = null;

        object[] objs = new object[] { path };
        jvalue[] jvs = AndroidJNIHelper.CreateJNIArgArray(objs);

        IntPtr dataPtr = AndroidJNI.CallStaticObjectMethod(clazzPtr, methodPtr2, jvs);

        if (dataPtr != IntPtr.Zero)
        {
            data = AndroidJNI.FromByteArray(dataPtr);
            AndroidJNI.DeleteLocalRef(dataPtr);
        }
        AndroidJNIHelper.DeleteJNIArgArray(objs, jvs);

        if (data == null)
        {
            Debug.LogError("android Load package file error:" + path);
        }

        return data;
    }
}
#endif
public class IOTools
{
    public static string packageResBasePath;
    static string packageResBaseWWWPath;

    static string updateResBasePath;

    static Dictionary<string, bool> updateFiles = new Dictionary<string, bool>();

    public static string abSuffix
    {
        get
        {
#if UNITY_IOS
            return ".iab";
#elif UNITY_ANDROID
            return ".aab";
#else
            return ".oab";
#endif
        }
    }

    public static void Init()
    {
        if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor)
        {
#if UNITY_ANDROID
            packageResBasePath = Application.dataPath + "/../ResEx/android";
#elif UNITY_IOS
             packageResBasePath = Application.dataPath + "/../ResEx/ios";
#else
             packageResBasePath = Application.dataPath + "/../ResEx/other";
#endif   
            if (!Directory.Exists(packageResBasePath))
            {
                Directory.CreateDirectory(packageResBasePath);
            }
            packageResBasePath += "/";
            packageResBaseWWWPath = "file:///" + packageResBasePath;

            updateResBasePath = Application.dataPath + "/../Config/res";
            if (!Directory.Exists(updateResBasePath))
            {
                Directory.CreateDirectory(updateResBasePath);
            }
        }
        else
        {
            updateResBasePath = Application.persistentDataPath + "/Config/res";
            if (!Directory.Exists(updateResBasePath))
            {
                Directory.CreateDirectory(updateResBasePath);
            }

            if (Application.platform == RuntimePlatform.Android)
            {
                packageResBasePath = Application.dataPath + "!assets/ResEx/";
                packageResBaseWWWPath = Application.streamingAssetsPath + "/ResEx/";
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                packageResBasePath = Application.dataPath + "/Raw/ResEx/";
                packageResBaseWWWPath = "file:///" + packageResBasePath;
#if UNITY_IPHONE
                //UnityEngine.iOS.Device.SetNoBackupFlag(updateResBasePath);
#endif
            }else
            {
                packageResBasePath = Application.streamingAssetsPath + "/ResEx/";
                packageResBaseWWWPath = "file:///" + packageResBasePath;
            }
        }
        updateResBasePath += "/";
    }

    //得到资源在包里的路径
    public static string GetPackageResPath(string resName)
    {
        return packageResBasePath + resName;
    }

    public static string GetPackageResWWWPath(string resName)
    {
        return packageResBaseWWWPath + resName;
    }

    public static string GetUpdateBasePath()
    {
        return updateResBasePath;
    }

    /// <summary>
    /// 如果更新目录存在资源则加载，否则从包里读取
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static byte[] GetResFileData(string name, bool isResEx = false)
    {
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogError("GetResFileData name is null!");
        }
        string uppath = updateResBasePath + name;
        bool isUpdate = false;
        if (updateFiles.TryGetValue(name, out isUpdate))
        {
            if (isUpdate)
                return File.ReadAllBytes(uppath);
            else
                return GetPackageFileData(name, isResEx);
        }

        if (File.Exists(uppath))
        {
            updateFiles[name] = true;
            return File.ReadAllBytes(uppath);
        }

        updateFiles[name] = false;
        return GetPackageFileData(name, isResEx);
    }

    public static byte[] GetResFileDataFromUpdateDir(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogError("GetResFileData name is null!");
        }
        string uppath = updateResBasePath + name;
        bool isUpdate = false;
        if (updateFiles.TryGetValue(name, out isUpdate))
        {
            if (isUpdate)
                return File.ReadAllBytes(uppath);
            else
                return null;
        }

        if (File.Exists(uppath))
        {
            updateFiles[name] = true;
            return File.ReadAllBytes(uppath);
        }
        return null;
    }

    public static string GetResFileString(string name, bool isResEx = false)
    {
        var data = GetResFileData(name, isResEx);
        if (data != null)
        {
            using (TextReader tr = new StreamReader(new MemoryStream(data)))
            {
                return tr.ReadToEnd();
            }
        }
        return null;
    }

    public static bool IsResInUpdateDir(string name)
    {
        bool isUpdate = false;
        if (updateFiles.TryGetValue(name, out isUpdate))
        {
            if (isUpdate)
                return true;
            else
                return false;
        }

        string uppath = updateResBasePath + name;
        if (File.Exists(uppath))
        {
            updateFiles[name] = true;
            return true;
        }

        updateFiles[name] = false;
        return false;
    }

    public static byte[] GetPackageFileData(string name, bool isResEx = false)
    {
        string path;

#if UNITY_IPHONE
        if (isResEx)
        {
            path = packageResBasePath + name;
        }
        else
            path = Application.dataPath + "/Raw/" + name;
        if (File.Exists(path))
        {
            return File.ReadAllBytes(path);
        }
        else
        {
            Debug.LogError("IO ERROR>>File not exist:" + path);
            return null;
        }
#elif UNITY_ANDROID
        if (isResEx)
        {
            path = "ResEx/" + name;
        }
        else
        {
            path = name;
        }
        return IOAndroidLoader.Instance.GetBytes(path);
#else
        path = packageResBasePath + name;
        if (File.Exists(path))
            return File.ReadAllBytes(packageResBasePath + name);
        else
            return null;
#endif
    }

    //得到更新目录
    public static string getUpdateResPath(string resName)
    {
        return updateResBasePath + resName;
    }

    public static void writeFileToUpdateDir(string name, byte[] data)
    {
        if (data != null && data.Length > 0)
            File.WriteAllBytes(updateResBasePath + name, data);
    }

    public static void writeStringToUpdateDir(string name, string str)
    {
        File.WriteAllText(updateResBasePath + name, str);
    }

    public static void DeleteFileFromUpdateDir(string name)
    {
        string s = updateResBasePath + name;
        updateFiles.Remove(name);
        if (File.Exists(s))
            File.Delete(s);
    }
}
