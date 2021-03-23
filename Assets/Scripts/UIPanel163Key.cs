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

    public Button mBtnDecrypt;
    public Button mBtnEncrypt;
    public Button mBtnEdit;

    // TODO 这里输入你的网易云api地址
    private string Api = "https://xxxxxxxxxxxx.vercel.app/";
    private string SearchApi = "search?keywords=";
    // private string DetailApi = "song/detail?ids=";

    private string AesKey = "#14ljk_!\\]&0U<\'(";
    // Start is called before the first frame update


    private Dictionary<string, KeyInfo> mMapKeyInfo = new Dictionary<string, KeyInfo>();

    void Start()
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
            Api = mIpApiAddr.text.Trim();
            StartCoroutine(EditCommon(mIpOrigin.text));
        });
    }


    private IEnumerator EditCommon(string dirPath)
    {
        mMapKeyInfo.Clear();
        var pathList = Directory.GetFiles(dirPath);
        var startTime = Time.realtimeSinceStartup;
        Debug.Log("开始搜索:");

        for (var i = 0; i < pathList.Length; i++)
        {
            var path = pathList[i];
            if (path.EndsWith("mp3"))
            {
                var name = "";
                var f = File.Create(path);
                name = f.Tag.Title;
         
                if (string.IsNullOrEmpty(name))
                {
                    name = path.Substring(path.LastIndexOf("/") + 1);
                    name = name.Substring(0, name.IndexOf("."));
                }

                f.Dispose();

                if (!string.IsNullOrEmpty(name))
                {
                    yield return Search(path, name);
                }
            }
        }

        Debug.Log("搜索完毕,耗时：" + (Time.realtimeSinceStartup - startTime));
        mIpResult.text = "";
        foreach (var keyValuePair in mMapKeyInfo)
        {
            // var comment = JsonUtility.ToJson(keyValuePair.Value);
            var comment = JsonMapper.ToJson(keyValuePair.Value);
            Debug.Log("歌曲：" + keyValuePair.Key + ",json:" + comment);
            var f = File.Create(keyValuePair.Key);
            f.Tag.Comment = "163 key(Don't modify):" + AesUtils.Encrypt("music:" + comment, AesKey);
            f.Save();
            mIpResult.text += "修改歌曲：" + keyValuePair.Key + "\n";
        }
 
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="keyword"></param> 
    /// <returns></returns>
    private IEnumerator Search(string path, string keyword)
    {
        var url = Api + SearchApi + keyword;
        Debug.Log("开始请求：" + url );
        var request = UnityWebRequest.Get(url);
        request.timeout = 5;
        yield return request.SendWebRequest();
        if (request.isHttpError || request.isNetworkError)
        {
            Debug.Log("搜索失败:" + request.error);
        }
        else if (request.isDone)
        {
            var searchList = JsonUtility.FromJson<ApiInfo<SearchInfo>>(request.downloadHandler.text);
            if (searchList.code != 200)
            {
                Debug.LogError("搜索错误，400:" + path);
                yield break;
            }

            if (searchList?.result?.songs != null && searchList.result.songs.Count > 0)
            { 
                mMapKeyInfo.Add(path, searchList.result.songs[0].ToKeyInfo());
            }
        }
    }

    
    private void DecryptText(string ori)
    {
        string result = AesUtils.Decrypt(ori, AesKey);
        Debug.Log("" + result);
        mIpResult.text = result;
    }

    private void EncryptText(string ori)
    {
        string result = AesUtils.Encrypt("music:" + ori, AesKey);
        mIpOrigin.text = result;
    }
}