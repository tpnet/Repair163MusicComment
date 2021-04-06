using System;
using System.IO;
using System.Text.RegularExpressions;

/**
* Create By: {Skyhand}
* Date: {2021-04-06}
* Desc: 
*/
public class Utils
{
    /// <summary>
    /// 获取资源库路径
    /// </summary>
    /// <returns></returns>
    public static string GetLibiaryPath()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library");
    }
    
    
    /// <summary>
    /// 判断是否为整形
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsInt(string value)
    {
        return Regex.IsMatch(value, @"^[+-]?\d*$");
    }
    
}