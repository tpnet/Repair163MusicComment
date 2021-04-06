/**
* Create By: {Skyhand}
* Date: {2021-04-06}
* Desc: 
*/

using System.Text.RegularExpressions;


public static class Utils
{
    /// <summary>
    /// 判断字符串是否为Int数字
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsInt(string value)
    {
        return Regex.IsMatch(value, @"^[+-]?\d*$");
    }
}