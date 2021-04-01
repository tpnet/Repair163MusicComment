using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LitJson;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using File = TagLib.File;

public class UIPanel163Key : MonoBehaviour
{
    public InputField mIpApiAddr;
    public InputField mIpOrigin;
    public InputField mIpResult;
    public Toggle mTgCover;
    public Text mTvStatus;

    public Button mBtnDecrypt;
    public Button mBtnEncrypt;
    public Button mBtnEdit;

    private string mApi = "";

    //解密Comment的Aes密钥
    private string AesKey = "#14ljk_!\\]&0U<\'(";

    //保存每首歌的信息
    private Dictionary<string, KeyInfo> mMapKeyInfo = new Dictionary<string, KeyInfo>();

    private void Start()
    {
        mBtnDecrypt.onClick.AddListener(() =>
        {
            var oriText = mIpOrigin.text;
            oriText = oriText.Replace("163 key(Don't modify):", "");
            DecryptText(oriText);
        });
        mBtnEncrypt.onClick.AddListener(() =>
        {
            var oriText = mIpResult.text;
            // oriText = oriText.Replace("163 key(Don't modify):", "");
            EncryptText(oriText);
        });
        mBtnEdit.onClick.AddListener(() =>
        {
            mApi = mIpApiAddr.text.Trim();
            if (string.IsNullOrEmpty(mApi))
            {
                Debug.LogError("请输入api地址");
                return;
            }

            if (string.IsNullOrEmpty(mIpOrigin.text))
            {
                Debug.LogError("请输入文件夹地址");
                return;
            }

            if (mApi.Substring(mApi.Length - 1).Equals("/"))
            {
                mApi = mApi.Substring(0, mApi.Length - 1);
            }

            mBtnEdit.interactable = false;
            StartCoroutine(EditCommon(mIpOrigin.text));
        });

        mTvStatus.text = "";
    }

    //忽略的数量
    private int mIgnoreNum = 0;
    //失败数量
    private int mFailNum = 0; 
    //文件总数量
    private int mAllNum = 0;

    //修改的数量
    private int mDealNum = 0;

    private IEnumerator EditCommon(string dirPath)
    {
        mIgnoreNum = 0;
        mAllNum = 0;
        mFailNum = 0;
        mDealNum = 0;
        mMapKeyInfo.Clear();

        var pathList = Directory.GetFiles(dirPath);
        var startTime = Time.realtimeSinceStartup;
        Debug.Log("开始搜索:");
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
                    yield return Search(path, name, (beanInfo) =>
                    {
                        if (beanInfo != null)
                        {
                            var keyInfo = GetMatchSongInfo(f.Tag.Title,f.Tag.Performers,beanInfo.songs);
                            var comment = JsonMapper.ToJson(keyInfo);
                            f.Tag.Comment = "163 key(Don't modify):" + AesUtils.Encrypt("music:" + comment, AesKey);
                            f.Save();
                        }
                        else
                        {
                            mFailNum++;
                        } 
                        mTvStatus.text = "正在处理:" + mDealNum + "/" + mAllNum + "，忽略:" + mIgnoreNum + "，失败:" + mFailNum;
                        f.Dispose();
                    });
                }
            }
        }

        Debug.Log("搜索完毕,耗时：" + (Time.realtimeSinceStartup - startTime));
        // EditCommentInfo();
        mBtnEdit.interactable = true;
    }

    private void EditCommentInfo()
    {
        mTvStatus.text = "正在修改...";
        mIpResult.text = "";
        mDealNum = 0;
        foreach (var keyValuePair in mMapKeyInfo)
        {
            // var comment = JsonUtility.ToJson(keyValuePair.Value);
            var comment = JsonMapper.ToJson(keyValuePair.Value);
            Debug.Log("歌曲：" + keyValuePair.Key + ",json:" + comment);
            var f = File.Create(keyValuePair.Key);
            f.Tag.Comment = "163 key(Don't modify):" + AesUtils.Encrypt("music:" + comment, AesKey);
            f.Save();
            // mIpResult.text += "修改歌曲：" + keyValuePair.Key + "\n";
            mDealNum++;
        }

        mTvStatus.text = "修改完毕" + mDealNum + "首mp3，忽略文件" + mIgnoreNum + "个";
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
        var url = mApi + "?s=" + keyword + "&type=1";
        Debug.Log("开始请求：" + url);
        var request = UnityWebRequest.Get(url);
        request.timeout = 5;
        yield return request.SendWebRequest();
        if (request.isHttpError || request.isNetworkError)
        {
            Debug.Log("搜索失败:" + request.error);
            actionDone.Invoke(null);
        }
        else if (request.isDone)
        {
            // Debug.Log("结果：" + request.downloadHandler.text);
            var searchList = JsonUtility.FromJson<ApiInfo<SearchInfo>>(request.downloadHandler.text);
            if (searchList.code != 200)
            {
                Debug.LogError("搜索错误，400:" + path);
                actionDone.Invoke(null);
                yield break;
            }

            if (searchList?.result?.songs != null && searchList.result.songs.Count > 0)
            {
                actionDone.Invoke(searchList.result);
                // mMapKeyInfo.Add(path, searchList.result.songs[0].ToKeyInfo());
            }
        }
    }

    /// <summary>
    /// 匹配最适合的歌曲
    /// </summary>
    /// <param name="songName"></param>
    /// <param name="songActor"></param>
    /// <param name="list"></param>
    /// <returns></returns>
    private KeyInfo GetMatchSongInfo(string songName, string[] songActor, List<SongInfo> list)
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
                
                foreach (var s in songActor)
                {
                    if (!artist.Contains(s.Trim()))
                    {//作者名字适配规则
                        goto for1;
                    }
                }
                
                //都相等
                info = songInfo;
            }
            
            for1: ;
        }

        return info.ToKeyInfo();
    }


    /// <summary>
    /// 解密Comment
    /// </summary>
    /// <param name="ori"></param>
    private void DecryptText(string ori)
    {
        var result = AesUtils.Decrypt(ori, AesKey);
        Debug.Log("" + result);
        mIpResult.text = result;
    }

    /// <summary>
    /// 加密Comment
    /// </summary>
    /// <param name="ori"></param>
    private void EncryptText(string ori)
    {
        var result = AesUtils.Encrypt("music:" + ori, AesKey);
        mIpOrigin.text = result;
    }
}