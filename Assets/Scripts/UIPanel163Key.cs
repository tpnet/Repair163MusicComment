using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using LitJson;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using File = TagLib.File;


namespace Skyhand
{
    /// <summary>
    /// 逻辑类
    /// </summary>
    public partial class UIPanel163Key : MonoBehaviour
    {
        private string mApi = "";

        //解密Comment的Aes密钥
        public const string AesKey = "#14ljk_!\\]&0U<\'(";
        public const string MatchTag = "match";

        //保存每首歌的信息。key为路径
        private Dictionary<string, SongInfo> mMapKeyInfo = new Dictionary<string, SongInfo>();


        //忽略的数量
        private int mIgnoreNum = 0;

        //失败数量
        private int mFailNum = 0;

        //文件总数量
        private int mAllNum = 0;

        //修改的数量
        private int mDealNum = 0;


        //队列日志，先进先出
        private Queue<string> mLog = new Queue<string>();

        //获取详情api，后面接数组,需要加[]
        private string DetailApi = "http://music.163.com/api/song/detail?ids=";

        //多少条内容获取一次详情
        private const int DetailNum = 20;


        //是文件还是文件夹，true为文件
        private bool mIsFile = false;

        //diy的歌曲id
        private int mDiySongID = 0;

        //数据库管理工具
        private NeteaseDataService mDataDataService;

        //缓存歌曲路径
        private const string DbPath =
            "/Containers/com.netease.163music/Data/Documents/storage/sqlite_storage.sqlite3";

        //是否需要重启网易云生效
        private bool mNeedReOpen = false;

        //协程是否正在执行
        private bool mCorRunning;

        private void Awake()
        {
            mBtnEdit.onClick.AddListener(() =>
            {
                if (mCorRunning)
                {
                    StopEdit();
                }
                else
                {
                    if (CheckEdit())
                    {
                        CheckDataService();
                        ClearLog();
                        mBtnEdit.GetComponentInChildren<Text>().text = "停止";
                        StartCoroutine(nameof(EditComment), mIpOrigin.text);
                    }
                }
            });

            //开启日志协程
            StartCoroutine(ShowLog());
        }


        /// <summary>
        /// 初始化数据库
        /// </summary>
        private void CheckDataService()
        {
            var dbPath = Utils.GetLibiaryPath() + DbPath;
            if (System.IO.File.Exists(dbPath))
            {
                mDataDataService = new NeteaseDataService(dbPath);
                if (mDataDataService == null)
                {
                    Debug.LogError("连接数据库失败，修改完毕Comment之后，需要手动清除网易云缓存,路径：" + dbPath);
                    mLog.Enqueue("<color=#ff0000>连接数据库失败，修改完毕Comment之后，需要手动清除网易云缓存，路径为：" + dbPath + "</color>");
                }
                else
                {
                    Debug.LogWarning("连接数据库成功：" + dbPath);
                    mNeedReOpen = true;
                }
            }
            else
            {
                Debug.LogWarning("数据库不存在：" + dbPath);
                mNeedReOpen = true;
            }
        }

        /// <summary>
        /// 检查输入数据
        /// </summary>
        private bool CheckEdit()
        {
            mDiySongID = 0;
            mIsFile = false;

            mApi = mIpApiAddr.text.Trim();
            if (string.IsNullOrEmpty(mApi))
            {
                Debug.LogError("请输入api地址");
                mLog.Enqueue("请输入api地址");
                return false;
            }

            var filePath = mIpOrigin.text.Trim();
            if (string.IsNullOrEmpty(filePath))
            {
                Debug.LogError("请输入 mp3文件/文件夹 地址");
                mLog.Enqueue("<color=#ff0000>请输入 <b>mp3文件/文件夹</b> 地址</color>");
                return false;
            }
            else
            {
                if (System.IO.File.Exists(filePath) && System.IO.Path.GetExtension(filePath).Equals(".mp3"))
                {
                    mIsFile = true;
                }
                else if (Directory.Exists(filePath))
                {
                    mIsFile = false;
                }
                else
                {
                    Debug.LogError("请输入正确的 mp3文件/文件夹 地址");
                    mLog.Enqueue("<color=#ff0000>请输入正确的 <b>mp3文件/文件夹</b> 地址</color>");
                    return false;
                }
            }

            if (!mIsFile && mTgDiyUrl.isOn)
            {
                Debug.LogError("只有单文件的方式才能diy修改");
                mLog.Enqueue("<color=#ff0000>只有单文件的方式才能diy修改</color>");
                return false;
            }

            if (mTgDiyUrl.isOn)
            {
                var diyUrl = mIpDiyUrl.text.Trim();
                var charList = diyUrl.Split('/');
                if (charList.Length <= 0)
                {
                    Debug.LogError("请输入正确的 自定义修改的歌曲链接 地址");
                    mLog.Enqueue("<color=#ff0000>请输入正确的 <b>自定义修改的歌曲链接</b> 地址</color>");
                    return false;
                }

                for (var i = 0; i < charList.Length; i++)
                {
                    if (charList[i].Equals("song") && charList.Length > (i + 1))
                    {
                        var idStr = charList[i + 1];
                        if (idStr.Contains("?"))
                        {
                            idStr = idStr.Split('?')[0];
                        }

                        if (!string.IsNullOrEmpty(idStr) && Utils.IsInt(idStr))
                        {
                            mDiySongID = int.Parse(idStr);
                        }
                    }
                }

                if (mDiySongID <= 0)
                {
                    Debug.LogError("请输入正确的 自定义修改的歌曲链接 地址");
                    mLog.Enqueue("<color=#ff0000>请输入正确的 <b>自定义修改的歌曲链接</b> 地址</color>");
                    return false;
                }
            }


            if (!mIsFile)
            {
                if (mApi.Substring(mApi.Length - 1).Equals("/"))
                {
                    mApi = mApi.Substring(0, mApi.Length - 1);
                }
            }

            return true;
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
                    mTvLog.text += "\n" + mLog.Dequeue();
                }

                yield return new WaitForSeconds(0.1f);
            }
        }


        private void StopEdit()
        {
            StopCoroutine(nameof(EditComment));
            mCorRunning = false;
            mBtnEdit.GetComponentInChildren<Text>().text = "开始";
        }

        /// <summary>
        /// 修改歌曲信息协程
        /// </summary>
        /// <param name="dirPath"></param>
        /// <returns></returns>
        private IEnumerator EditComment(string dirPath)
        {
            mCorRunning = true;
            mIgnoreNum = 0;
            mAllNum = 0;
            mFailNum = 0;
            mDealNum = 0;
            mMapKeyInfo.Clear();

            var pathList = new[] {dirPath};
            if (!mIsFile)
            {
                //判断单文件还是文件夹
                pathList = Directory.GetFiles(dirPath);
            }

            var startTime = Time.realtimeSinceStartup;
            mAllNum = pathList.Length;

            for (var i = 0; i < pathList.Length; i++)
            {
                var path = pathList[i];
                if (path.EndsWith("mp3"))
                {
                    var name = "";
                    var f = File.Create(path);

                    if (!mTgCover.isOn && f.Tag.Comment.Contains("163"))
                    {
                        //不覆盖
                        Debug.LogWarning("不覆盖已经有Comment的，跳过：" + f.Tag.Title);
                        mIgnoreNum++;
                        continue;
                    } 

                    if (!mTgCoverMatch.isOn && MatchTag.Equals(f.Tag.Description))
                    {
                        //不覆盖已经准确匹配过的
                        Debug.LogWarning("不覆盖已经准确匹配过的，跳过：" + f.Tag.Title);
                        mIgnoreNum++;
                        continue;
                    }


                    name = f.Tag.Title.Trim();

                    if (string.IsNullOrEmpty(name))
                    {
                        //Tag里面为空就从路径里面获取
                        name = Path.GetFileNameWithoutExtension(path);
                        // name = path.Substring(path.LastIndexOf("/") + 1);
                        // name = name.Substring(0, name.IndexOf("."));
                    }

                    Debug.Log("歌曲名字： " + name);

                    if (!string.IsNullOrEmpty(name))
                    {
                        if (f.Tag.Performers != null && f.Tag.Performers.Length > 0)
                        {
                            //搜索增加名字
                            name += " " + f.Tag.Performers[0];
                        }

                        mDealNum++;

                        mTvStatus.text = "正在处理:" + mDealNum + "/" + mAllNum + "，忽略:" + mIgnoreNum + "，失败:" + mFailNum;
                        mLog.Enqueue("正在处理 " + f.Tag.Title);

                        if (mTgDiyUrl.isOn)
                        {
                            //自定义修改
                            Debug.Log("自定义修改： " + mDiySongID);
                            mMapKeyInfo.Add(path, new SongInfo()
                            {
                                id = mDiySongID,
                            });
                        }
                        else
                        {
                            yield return Search(f.Tag, path, name, (songInfo) =>
                            {
                                if (songInfo != null)
                                {
                                    mMapKeyInfo.Add(path, songInfo);
                                }
                                else
                                {
                                    mFailNum++;
                                }

                                f.Dispose();
                            }); 
                        }


                        if (i == pathList.Length - 1 || mMapKeyInfo.Count == DetailNum)
                        {
                            //每20条内容或者最后一条了，就获取详情
                            Debug.Log("开始获取详情：" + mDealNum);
                            mLog.Enqueue("开始获取详情：" + mDealNum);
                            var idList = new List<int>();
                            foreach (var keyValuePair in mMapKeyInfo)
                            {
                                idList.Add(keyValuePair.Value.id);
                            }

                            //获取歌曲详情
                            yield return GetSongDetail(idList.ToArray(), (songs) =>
                            {
                                if (songs != null && songs.Count > 0)
                                {
                                    var keyList = mMapKeyInfo.Keys.ToArray();
                                    foreach (var key in keyList)
                                    {
                                        mMapKeyInfo.TryGetValue(key, out var outInfo);
                                        if (outInfo != null)
                                        {
                                            var isMatch = outInfo.IsMatch;
                                            var songInfo = songs.Find(v => v.id == outInfo.id);
                                            songInfo.IsMatch = isMatch;
                                            //替换搜索的实体为详情得到的实体
                                            mMapKeyInfo[key] = songInfo; 
                                        }
                                    }

                                    foreach (var keyValuePair in mMapKeyInfo)
                                    {
                                        var songInfo = keyValuePair.Value;

                                        if (songInfo != null)
                                        {
                                            var fCache = File.Create(keyValuePair.Key);
                                            songInfo.bitrate = fCache.Properties.AudioBitrate * 1000;
                                            var comment = JsonMapper.ToJson(songInfo.ToKeyInfo());
                                            fCache.Tag.Comment = "163 key(Don't modify):" +
                                                                 AesUtils.Encrypt("music:" + comment, AesKey);
                                            //定义是否完全匹配
                                            fCache.Tag.Description = (songInfo.IsMatch ? MatchTag : "");
                                            fCache.Save();
                                            fCache.Dispose();

                                            var fileName = "/" + Path.GetFileName(keyValuePair.Key);
                                            //根据文件名称来删除数据库
                                            mDataDataService?.DeleteRow(fileName);
                                        }
                                        else
                                        {
                                            mFailNum++;
                                        }
                                    }
                                }
                                else
                                {
                                    mFailNum += idList.Count;
                                }
                            });

                            //清空当前歌曲库
                            mMapKeyInfo.Clear();
                        }
                    }
                }
            }

            mCorRunning = false;
            mBtnEdit.GetComponentInChildren<Text>().text = "开始";
            yield return null;

            Debug.Log("搜索完毕,耗时：" + (Time.realtimeSinceStartup - startTime) + "秒");
            mTvStatus.text = "处理完毕:" + mDealNum + "/" + mAllNum + "，忽略:" + mIgnoreNum + "，失败:" + mFailNum;
            mLog.Enqueue(mTvStatus.text);

            if (mNeedReOpen)
            {
                if (mDealNum > 0)
                {
                    Debug.Log("需要重启网易云生效");
                    mLog.Enqueue("请重启网易云生效");
                }

                mDataDataService?.Close();
            }
        }

        /// <summary>
        /// 搜索，进行了3次重试的
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="path"></param>
        /// <param name="keyword"></param>
        /// <param name="actionDone"></param>
        /// <returns></returns>
        private IEnumerator Search(TagLib.Tag tag, string path, string keyword, Action<SongInfo> actionDone)
        {
            var limit = 30;
            var MaxTryNum = 3;

            var canBreak = false;
            var currCount = 0;
            SongInfo songInfo = null;

            for (int i = 0; i <= MaxTryNum; i++)
            {
                if (canBreak)
                {
                    break;
                }

                if (i > 0)
                {
                    Debug.Log(tag.Title + ",重试：" + i);
                }

                var canContinue = false;
                yield return Search(path, keyword, limit, currCount, (searchInfo) =>
                {
                    //根据名字搜索歌曲
                    if (searchInfo != null)
                    {
                        var cSongInfo = GetMatchSongInfo(tag.Title, tag.Performers, searchInfo.songs);
                        if (cSongInfo.IsMatch)
                        {
                            //匹配成功
                            songInfo = cSongInfo;
                            canBreak = true;
                        }
                        else
                        {
                            if (i == 0)
                            {//保存第一首歌
                                songInfo = cSongInfo;
                            }
                        }

                        if (searchInfo.songs != null && searchInfo.songs.Count > 0)
                        {
                            currCount += searchInfo.songs.Count;
                        }
                        else
                        {
                            songInfo = cSongInfo;
                            canBreak = true;
                        }
                    }
                    else
                    {
                        canBreak = true;
                    }

                    canContinue = true;
                });

                yield return new WaitUntil(() => canContinue);
            }

            actionDone?.Invoke(songInfo);
            yield return null;
        }

        /// <summary>
        /// 搜索歌曲信息
        /// </summary>
        /// <param name="path"></param>
        /// <param name="keyword"></param> 
        /// <param name="limit"></param> 
        /// <param name="offset"></param> 
        /// <returns></returns>
        private IEnumerator Search(string path, string keyword, int limit, int offset, Action<SearchInfo> actionDone)
        {
            // var url = Api  + SearchApi + keyword + "&type=1";
            //limit是每次搜索的条数，数量越多识别得约精准
            var url = $"{mApi}?type=1&limit={limit}&offset={offset}&s={UnityWebRequest.EscapeURL(keyword)}";
            Debug.Log((int) (mDealNum / 40) + "开始请求搜索：" + url);
            var request = UnityWebRequest.Get(url);
            request.timeout = 5;
            //40次请求就更换请求头，防止出现操作操作频繁
            request.SetRequestHeader("User-Agent", HeaderUtils.GetHeader((int) (mDealNum / 40)));
            yield return request.SendWebRequest();
            if (request.isHttpError || request.isNetworkError)
            {
                Debug.Log("搜索请求失败:" + request.error);
                mLog.Enqueue("<color=#ff0000>搜索请求失败:" + request.error + "</color>");
                actionDone.Invoke(null);
            }
            else if (request.isDone)
            {
                // Debug.Log("结果：" + request.downloadHandler.text);
                var searchList = JsonUtility.FromJson<ApiInfo<SearchInfo>>(request.downloadHandler.text);
                if (searchList.code != 200)
                {
                    Debug.LogError("搜索结果错误:" + path + "，" + request.downloadHandler.text);
                    mLog.Enqueue("<color=#ff0000>搜索结果错误:" + path + "，" + request.downloadHandler.text + "</color>");
                    actionDone.Invoke(null);
                    yield break;
                }

                if (searchList?.result?.songs != null && searchList.result.songs.Count > 0)
                {
                    actionDone.Invoke(searchList.result);
                }
                else
                {
                    actionDone.Invoke(null);
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
            Debug.Log((int) (mDealNum / 40) + "开始请求详情：" + url);
            var request = UnityWebRequest.Get(url);
            request.timeout = 5;
            //当出现网络拥堵的时候，请更换请求头
            request.SetRequestHeader("User-Agent", HeaderUtils.GetHeader((int) (mDealNum / 40)));
            yield return request.SendWebRequest();
            if (request.isHttpError || request.isNetworkError)
            {
                Debug.Log("详情请求失败:" + request.error);
                mLog.Enqueue("<color=#ff0000>详情请求失败:" + request.error + "</color>");
                actionDone.Invoke(null);
            }
            else if (request.isDone)
            {
                // Debug.Log("结果：" + request.downloadHandler.text);
                var searchList = JsonUtility.FromJson<SearchInfo>(request.downloadHandler.text);
                if (searchList.code != 200)
                {
                    Debug.LogError("详情结果错误:" + ids + "，" + request.downloadHandler.text);
                    mLog.Enqueue("<color=#ff0000>详情结果错误:" + ids + "，" + request.downloadHandler.text + "</color>");
                    actionDone.Invoke(null);
                    if (searchList.code == -460)
                    {
                        //被判定到是机器人,停止协程
                        StopEdit();
                    }

                    yield break;
                }

                if (searchList?.songs != null && searchList.songs.Count > 0)
                {
                    actionDone.Invoke(searchList.songs);
                }
                else
                {
                    actionDone.Invoke(null);
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
                            songInfo.IsMatch = true;
                            return songInfo;
                        }
                    }
                }
            }

            return info;
        }
    }
}