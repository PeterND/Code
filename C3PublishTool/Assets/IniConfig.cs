using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IniConfig
{
    // 声明INI文件的读操作函数 GetPrivateProfileString()  
    [System.Runtime.InteropServices.DllImport("kernel32")]
    private static extern int GetPrivateProfileString(string section, string key, string def, System.Text.StringBuilder retVal, int size, string filePath);

    private string m_strPath = null;
    public IniConfig(string path)
    {
        this.m_strPath = path;
    }

    public string ReadValue(string section, string key)
    {
        System.Text.StringBuilder temp = new System.Text.StringBuilder(255);
        GetPrivateProfileString(section, key, "", temp, 255, m_strPath);
        return temp.ToString();
    }

}
