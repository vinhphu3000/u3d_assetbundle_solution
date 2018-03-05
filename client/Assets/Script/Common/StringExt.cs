using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StringExt  {
    
    public static string toLower(this string str)
    {
        if (str == null)
            return str;

        bool isLower = true;
        int count = str.Length;
        for (int i = 0; i < count; i++)
        {
            char c = str[i];
            if (c >= 'A' && c <= 'Z')
            {
                isLower = false;
                break;
            }
        }
        if(isLower)
        {
            return str;
        }else
        {
            return str.ToLower();
        }
    }

}
