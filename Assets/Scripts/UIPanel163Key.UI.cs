using UnityEngine;
using UnityEngine.UI;


namespace Skyhand
{
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


        public GameObject mLogContent;
        public GameObject mPanelPass;

        private void Start()
        {
            mBtnPass.onClick.AddListener(() => { mPanelPass.gameObject.SetActive(true); });

            mTvStatus.text = "";
            mTvLog.text = "";
 
            //控制文件拖拽 
            mUniController.SetAllowDrop(true);
            mUniController.OnDropFiles += files =>
            {
                if (files.Length > 0)
                {
                    mLog.Enqueue("获取到拖放文件路径：" + files[0]);
                    mIpOrigin.text = files[0];
                }
            };
        }

        private void ClearLog()
        {
            mTvLog.text = "";
            // for (int i = 0; i < mLogContent.transform.childCount; i++)
            // {
            //     GameObject.Destroy(mLogContent.transform.GetChild(i).gameObject);
            // }
        }

    }
}