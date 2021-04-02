using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LitJson;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using File = TagLib.File;

/// <summary>
/// 逻辑类
/// </summary>
public partial class UIPanel163Key : MonoBehaviour
{
    private string mApi = "";

    //解密Comment的Aes密钥
    public const string AesKey = "#14ljk_!\\]&0U<\'(";

    //保存每首歌的信息
    private Dictionary<string, KeyInfo> mMapKeyInfo = new Dictionary<string, KeyInfo>();

    //忽略的数量
    private int mIgnoreNum = 0;

    //失败数量
    private int mFailNum = 0;

    //文件总数量
    private int mAllNum = 0;

    //修改的数量
    private int mDealNum = 0;

    private List<string> mLog = new List<string>();

    //获取详情api，后面接数组,需要加[]
    private string DetailApi = "http://music.163.com/api/song/detail?ids=";


    private void Awake()
    {
        mBtnEdit.onClick.AddListener(() =>
        {
            CheckEdit();
            mBtnEdit.interactable = false;
            StartCoroutine(EditComment(mIpOrigin.text));
        });

        StartCoroutine(ShowLog());
    }

    /// <summary>
    /// 检查输入数据
    /// </summary>
    private void CheckEdit()
    {
        mApi = mIpApiAddr.text.Trim();
        if (string.IsNullOrEmpty(mApi))
        {
            Debug.LogError("请输入api地址");
            mLog.Add("请输入api地址");
            return;
        }

        if (string.IsNullOrEmpty(mIpOrigin.text))
        {
            Debug.LogError("请输入文件夹地址");
            mLog.Add("请输入文件夹地址");
            return;
        }

        if (mApi.Substring(mApi.Length - 1).Equals("/"))
        {
            mApi = mApi.Substring(0, mApi.Length - 1);
        }
    }


    /// <summary>
    /// 显示log协程
    /// </summary>
    /// <returns></returns>
    private IEnumerator ShowLog()
    {
        while (true)
        {
            if (mLog.Count > 0)
            {
                mTvLog.text += "\n" + mLog[0];
                mLog.RemoveAt(0);
            }

            yield return new WaitForSeconds(0.1f);
        }
    }


    /// <summary>
    /// 修改歌曲信息协程
    /// </summary>
    /// <param name="dirPath"></param>
    /// <returns></returns>
    private IEnumerator EditComment(string dirPath)
    {
        mIgnoreNum = 0;
        mAllNum = 0;
        mFailNum = 0;
        mDealNum = 0;
        mMapKeyInfo.Clear();

        var pathList = Directory.GetFiles(dirPath);
        var startTime = Time.realtimeSinceStartup;
        mAllNum = pathList.Length;
        for (var i = 0; i < pathList.Length; i++)
        {
            var path = pathList[i];
            if (path.EndsWith("mp3"))
            {
                var name = "";
                var f = File.Create(path);

                if (!mTgCover.isOn && !string.IsNullOrEmpty(f.Tag.Comment))
                {
                    //不覆盖
                    Debug.LogWarning("跳过：" + f.Tag.Title);
                    mIgnoreNum++;
                    continue;
                }

                name = f.Tag.Title;

                if (string.IsNullOrEmpty(name))
                {
                    name = path.Substring(path.LastIndexOf("/") + 1);
                    name = name.Substring(0, name.IndexOf("."));
                }

                if (!string.IsNullOrEmpty(name))
                {
                    mDealNum++;
                    mTvStatus.text = "正在处理:" + mDealNum + "/" + mAllNum + "，忽略:" + mIgnoreNum + "，失败:" + mFailNum;

                    mLog.Add("正在处理 " + f.Tag.Title);
                    SongInfo songInfo = null;
                    yield return Search(path, name, (beanInfo) =>
                    {
                        //根据名字搜索歌曲
                        if (beanInfo != null)
                        {
                            songInfo = GetMatchSongInfo(f.Tag.Title, f.Tag.Performers, beanInfo.songs);
                        }
                        else
                        {
                            mFailNum++;
                            f.Dispose();
                        }
                    });

                    if (songInfo != null)
                    {
                        //获取歌曲详情
                        yield return GetSongDetail(new[] {songInfo.id}, (songs) =>
                        {
                            if (songs != null && songs.Count > 0)
                            {
                                songInfo = songs[0];
                                songInfo.bitrate = f.Properties.AudioBitrate * 1000;
                                var comment = JsonMapper.ToJson(songInfo.ToKeyInfo());
                                f.Tag.Comment = "163 key(Don't modify):" + AesUtils.Encrypt("music:" + comment, AesKey);
                                f.Save();
                            }
                            else
                            {
                                mFailNum++;
                            }
                        });
                        f.Dispose();
                    }
                }
            }
        }

        Debug.Log("搜索完毕,耗时：" + (Time.realtimeSinceStartup - startTime));
        // EditCommentInfo();
        mBtnEdit.interactable = true;
        mTvStatus.text = "处理完毕:" + mDealNum + "/" + mAllNum + "，忽略:" + mIgnoreNum + "，失败:" + mFailNum;
        mLog.Add(mTvStatus.text);
    }


    /// <summary>
    /// 搜索歌曲信息
    /// </summary>
    /// <param name="path"></param>
    /// <param name="keyword"></param> 
    /// <returns></returns>
    private IEnumerator Search(string path, string keyword, Action<SearchInfo> actionDone)
    {
        // var url = Api  + SearchApi + keyword + "&type=1";
        //limit是每次搜索的条数，数量越多识别得约精准
        var url = mApi + "?type=1&limit=20&s=" + UnityWebRequest.EscapeURL(keyword);
        Debug.Log("开始请求：" + url);
        var request = UnityWebRequest.Get(url);
        request.timeout = 5;
        //40次请求就更换请求头，防止出现操作操作频繁
        request.SetRequestHeader("User-Agent",HeaderUtils.GetHeader((int)(mDealNum / 40)));
        yield return request.SendWebRequest();
        if (request.isHttpError || request.isNetworkError)
        {
            Debug.Log("搜索请求失败:" + request.error);
            mLog.Add("搜索请求失败:" + request.error);
            actionDone.Invoke(null);
        }
        else if (request.isDone)
        {
            // Debug.Log("结果：" + request.downloadHandler.text);
            var searchList = JsonUtility.FromJson<ApiInfo<SearchInfo>>(request.downloadHandler.text);
            if (searchList.code != 200)
            {
                Debug.LogError("搜索结果错误:" + path + "，" + request.downloadHandler.text);
                mLog.Add("搜索结果错误:" + path + "，" + request.downloadHandler.text);
                actionDone.Invoke(null);
                yield break;
            }

            if (searchList?.result?.songs != null && searchList.result.songs.Count > 0)
            {
                actionDone.Invoke(searchList.result);
            }
        }
    }

    /// <summary>
    /// 获取歌曲详情
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="actionDone"></param> 
    /// <returns></returns>
    private IEnumerator GetSongDetail(int[] ids, Action<List<SongInfo>> actionDone)
    {
        // var url = Api  + SearchApi + keyword + "&type=1";
        //limit是每次搜索的条数，数量越多识别得约精准
        var url = DetailApi + UnityWebRequest.EscapeURL("[" + string.Join(",", ids) + "]");
        Debug.Log("开始请求详情：" + url);
        var request = UnityWebRequest.Get(url);
        request.timeout = 5;
        //当出现网络拥堵的时候，请更换请求头
        request.SetRequestHeader("User-Agent", HeaderUtils.GetHeader((int)(mDealNum / 40)));
        yield return request.SendWebRequest();
        if (request.isHttpError || request.isNetworkError)
        {
            Debug.Log("详情请求失败:" + request.error);
            mLog.Add("详情请求失败:" + request.error);
            actionDone.Invoke(null);
        }
        else if (request.isDone)
        {
            // Debug.Log("结果：" + request.downloadHandler.text);
            var searchList = JsonUtility.FromJson<SearchInfo>(request.downloadHandler.text);
            if (searchList.code != 200)
            {
                Debug.LogError("详情结果错误:" + ids + "，" + request.downloadHandler.text);
                mLog.Add("详情结果错误:" + ids + "，" + request.downloadHandler.text);
                actionDone.Invoke(null);
                yield break;
            }

            if (searchList?.songs != null && searchList.songs.Count > 0)
            {
                actionDone.Invoke(searchList.songs);
            }
        }
    }

    /// <summary>
    /// 匹配搜索列表里面最适合的歌曲
    /// </summary>
    /// <param name="songName"></param>
    /// <param name="songActor"></param>
    /// <param name="list"></param>
    /// <returns></returns>
    private SongInfo GetMatchSongInfo(string songName, string[] songActor, List<SongInfo> list)
    {
        if (list == null || list.Count == 0)
        {
            return null;
        }

        var info = list[0];
        foreach (var songInfo in list)
        {
            var name = "";
            var artist = new List<string>();
            if (songName.Equals(songInfo.name))
            {
                name = songInfo.name.Trim();
                if (songInfo.artists != null)
                {
                    foreach (var songInfoArtist in songInfo.artists)
                    {
                        artist.Add(songInfoArtist.name);
                    }
                }
            }

            if (name.Equals(songName.Trim()))
            {
                //判断歌名一模一样

                foreach (var s in songActor)
                {
                    //判断多个作者
                    if (artist.Contains(s.Trim()))
                    {
                        //作者名字只要有一个适配即可
                        return songInfo;
                    }
                }
            }
        }

        return info;
    }
}