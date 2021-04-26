 
 
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ArtistInfo
{
    public int id;
    public string name;
    public string picUrl;
    public string img1v1Url;
    public int albumSize;
    public int picId;
    public int img1v1;
    public string[] alias;
}

[Serializable]
public class AlbumInfo
{
    public int id;
    public string name;
    public long publishTime;
    public long picId;
    public string picUrl;
    public int size;
    public int copyrightId;
    public int status;
    public int mark;
    public ArtistInfo artist;
}

[Serializable]
public class SongInfo
{
    public int id;
    public string name;
    public int duration;
    public int bitrate;  //自定义添加的字段，从文件里面读取
    public int copyrightId;

    public int status;

    // public string[] alias;
    public ArtistInfo[] artists;
    public AlbumInfo album;
    public int rtype;
    public int ftype;
    public int mvid;
    public int fee;
    public int rUrl;
    public int mark;

    //自定义字段，是否百分百匹配
    public bool IsMatch;

    public KeyInfo ToKeyInfo()
    {
        var art = new List<string[]>();
        for (var i = 0; i < artists?.Length; i++)
        {
            // Debug.Log("增加名字" + artists[i].name);
            art.Add(new[] {artists[i].name, artists[i].id + ""});
        }

        return new KeyInfo()
        {
            musicId = id,
            musicName = name,
            artist = art,
            
            albumId = album?.id ?? 0,
            album = album?.name ?? "",
            albumPicDocId = album?.picId.ToString() ?? "0",
            albumPic = album?.picUrl ?? "",
            
            // mp3DocId = "",  //这个不知道怎么来的
            bitrate = bitrate, //这个参考值
            duration = this.duration,
            mvId = this.mvid,
            alias = new string[] { },
            transNames = new string[] { },
            format = "mp3",
        };
    }
}

[Serializable]
public class SearchInfo
{
    public List<SongInfo> songs;
    public bool hasMore;
    public int songCount;
    public int code;
}

[Serializable]
public class ApiInfo<T>
{
    public T result;
    public int code;
}

[Serializable]
public class KeyInfo
{
    public string format;
    public int musicId;
    public string musicName;
    public List<string[]> artist;
    public string album;
    public int albumId;
    public string albumPicDocId;
    public string albumPic;

    public int mvId;
    // public int flag;

    public int bitrate;
    public int duration;

    public string[] alias;
    public string[] transNames;
}