using Kirurobo;
using UnityEngine;

/**
* Create By: {Skyhand}
* Date: {2021-04-01}
* Desc: Window和Mac 控制拖放文件，参考Github： UniWindowController
*/ 
public class UniWindowController: MonoBehaviour
{
    /// <summary>
    /// Occurs after files or folders were dropped
    /// </summary>
    public event OnDropFilesDelegate OnDropFiles;
    public delegate void OnDropFilesDelegate(string[] files);
    
    
    /// <summary>
    /// Set editable the bool property
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class BoolPropertyAttribute : PropertyAttribute { }

    private void Awake()
    {
        uniWinCore = new UniWinCore();
    }
    void OnDestroy()
    {
        uniWinCore?.Dispose();
    }
    
    void Update()
    {
        if (uniWinCore == null) return;

        if (uniWinCore.ObserveDroppedFiles(out var files))
        {
            OnDropFiles?.Invoke(files);
        }
    }
    
    /// <summary>
    /// Low level class
    /// </summary>
    private UniWinCore uniWinCore = null;
    
    
    public void SetAllowDrop(bool enabled)
    {
        if (uniWinCore == null) return;
        Debug.Log("开启拖拽文件功能");
        uniWinCore.AttachMyWindow();
        uniWinCore.SetAllowDrop(enabled);
        _allowDropFiles = enabled;
    }
    [SerializeField, BoolProperty, Tooltip("Enable file or folder dropping")]
    private bool _allowDropFiles = true;
} 