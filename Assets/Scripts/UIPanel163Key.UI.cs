using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI类
/// </summary>
public partial class UIPanel163Key : MonoBehaviour
{
    public UniWindowController mUniController;

    public InputField mIpApiAddr;
    public InputField mIpOrigin;
    public Toggle mTgCover;
    public Text mTvStatus;

    public Text mTvLog;

    public Button mBtnEdit;
    public Button mBtnPass;
    
    
    public GameObject mPanelPass;

    private void Start()
    {
        mBtnPass.onClick.AddListener(() =>
        {
            mPanelPass.gameObject.SetActive(true);
        });
        
        mTvStatus.text = "";
        mTvLog.text = "";

        
        //控制文件拖拽
        mUniController.SetAllowDrop(true);
        mUniController.OnDropFiles += files =>
        {
            if (files.Length > 0)
            {
                mIpOrigin.text = files[0];
            }
        };
    }

}