using System.Collections.Generic;
using System.IO;
using SqlCipher4Unity3D;
using UnityEngine;

#if !UNITY_EDITOR
using System.Collections;
using System.IO;
#endif

/**
* Create By: {Skyhand}
* Date: {2021-04-06}
* Desc: 连接下载管理缓存数据库工具
*/
public class NeteaseDataService
{
    private readonly SQLiteConnection _connection;

    public NeteaseDataService(string dbPath)
    {
        //路径为：/Users/skyhand/Library/Containers/com.netease.163music/Data/Documents/storage/sqlite_storage.sqlite3 
        _connection = new SQLiteConnection(dbPath, "");
       
    }

 
    /// <summary>
    /// 删除行
    /// </summary>
    /// <param name="id"></param>
    public void DeleteRow(string id)
    {
        if (!string.IsNullOrEmpty(id))
        {
            var row = _connection.Delete<web_offline_track>(id);
            Debug.Log("删除行索引：" + row + "," + id);
        }
       
    }

    public void Close()
    {
        _connection?.Dispose(); 
    }
}