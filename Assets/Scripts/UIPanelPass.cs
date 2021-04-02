using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelPass : MonoBehaviour
{
    
    public Button mBtnDecrypt;
    public Button mBtnEncrypt;
    public Button mBtnCopyOri;
    public Button mBtnCopyResult;
    public Button mBtnClose;
    
    
    public InputField mIpOrigin;
    public InputField mIpResult;

    // Start is called before the first frame update
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
            EncryptText(oriText);
        });
        
        mBtnClose.onClick.AddListener(() =>
        {
            this.gameObject.SetActive(false); 
        });
        
        mBtnCopyOri.onClick.AddListener(() =>
        {
            GUIUtility.systemCopyBuffer = mIpOrigin.text;
        });  
        mBtnCopyResult.onClick.AddListener(() =>
        {
            GUIUtility.systemCopyBuffer = mIpResult.text;
        });
    }

    
    /// <summary>
    /// 解密Comment
    /// </summary>
    /// <param name="ori"></param>
    private void DecryptText(string ori)
    {
        var result = AesUtils.Decrypt(ori, UIPanel163Key.AesKey);
        Debug.Log("" + result);
        mIpResult.text = result;
    }

    /// <summary>
    /// 加密Comment
    /// </summary>
    /// <param name="ori"></param>
    private void EncryptText(string ori)
    {
        var result = AesUtils.Encrypt("music:" + ori, UIPanel163Key.AesKey);
        mIpOrigin.text = result;
    }
}
