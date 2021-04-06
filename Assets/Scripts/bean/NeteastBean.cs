using SQLite.Attributes;


///Users/skyhand/Library/Containers/com.netease.163music/Data/Documents/storage/sqlite_storage.sqlite3
///的表的实体，以表名作为实体类的名字
public class web_offline_track
{
    /// <summary>
    /// 歌曲id
    /// </summary> 
    public int track_id
    {
        get;
        set;
    }
    [PrimaryKey]
    public int relative_path
    {
        get;
        set;
    }
}