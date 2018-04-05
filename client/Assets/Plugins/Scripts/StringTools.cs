using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringTools {

    public static System.Text.Encoding CheckStrFormat(byte[] bytes)
    {
        System.Text.Encoding enc = null;
        byte[] bom = new byte[4]; // Get the byte-order mark, if there is one 
 
        System.Array.Copy(bytes, bom, 4);
        if ((bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) || // utf-8 
            (bom[0] == 0xff && bom[1] == 0xfe) || // ucs-2le, ucs-4le, and ucs-16le 
            (bom[0] == 0xfe && bom[1] == 0xff) || // utf-16 and ucs-2 
            (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff)) // ucs-4 
        {
            enc = System.Text.Encoding.Unicode;
        }
        else
        {
            enc = System.Text.Encoding.ASCII;
        }
        return enc;
    }

    public static string BytesToString(byte[] bytes)
    {
        string json = "";
        if (bytes != null)
        {
            System.Text.Encoding enc = CheckStrFormat(bytes);
            if (enc == null)
            {
                UnityEngine.Debug.LogError("无法识别的编码格式");
                return json;
            }
            if (enc == System.Text.Encoding.Unicode)
            {
                json = System.Text.Encoding.Unicode.GetString(bytes);
            }
            else if (enc == System.Text.Encoding.ASCII)
            {
                json = System.Text.Encoding.ASCII.GetString(bytes);
            }
        }
        return json;
    }           
}
