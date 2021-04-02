/*
 * UniWinCore.cs
 * 
 * Author: Kirurobo http://twitter.com/kirurobo
 * License: MIT
 */

using System;
using System.Linq;
using System.Runtime.InteropServices;
using AOT;
using Kirurobo;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kirurobo
{
    /// <summary>
    /// Windows / macOS のネイティブプラグインラッパー
    /// </summary>
    internal class UniWinCore : IDisposable
    {
        /// <summary>
        /// 透明化の方式
        /// </summary>
        public enum TransparentType
        {
            None = 0,
            Alpha = 1,
            ColorKey = 2,
        }

        #region Native functions
        protected class LibUniWinC
        {
            [UnmanagedFunctionPointer((CallingConvention.Cdecl))]
            public delegate void StringCallback([MarshalAs(UnmanagedType.LPWStr)] string returnString);

            [UnmanagedFunctionPointer((CallingConvention.Cdecl))]
            public delegate void IntCallback([MarshalAs(UnmanagedType.I4)] int value);

            
            [DllImport("LibUniWinC")]
            public static extern bool IsActive();
  
            [DllImport("LibUniWinC")]
            public static extern bool AttachMyWindow();

            [DllImport("LibUniWinC")]
            public static extern bool AttachMyOwnerWindow();

            [DllImport("LibUniWinC")]
            public static extern bool AttachMyActiveWindow();

            [DllImport("LibUniWinC")]
            public static extern bool DetachWindow();
  
            [DllImport("LibUniWinC")]
            public static extern bool RegisterDropFilesCallback([MarshalAs(UnmanagedType.FunctionPtr)] StringCallback callback);

            [DllImport("LibUniWinC")]
            public static extern bool UnregisterDropFilesCallback();
  
            [DllImport("LibUniWinC")]
            public static extern bool SetAllowDrop(bool enabled);
 
        }
        #endregion

        static string[] lastDroppedFiles;
        static bool wasDropped = false; 
#if UNITY_EDITOR
        // 参考 http://baba-s.hatenablog.com/entry/2017/09/17/135018
        /// <summary>
        /// ゲームビューのEditorWindowを取得
        /// </summary>
        /// <returns></returns>
        public static EditorWindow GetGameView()
        {
            var assembly = typeof(EditorWindow).Assembly;
            var type = assembly.GetType("UnityEditor.GameView");
            var gameView = EditorWindow.GetWindow(type);
            return gameView;
        }
#endif

        /// <summary>
        /// ウィンドウ操作ができる状態ならtrueを返す
        /// </summary>
        /// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
        public bool IsActive = false;

   
        #region Constructor or destructor
        /// <summary>
        /// ウィンドウ制御のコンストラクタ
        /// </summary>
        public UniWinCore()
        {
            IsActive = false;
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~UniWinCore()
        {
            Dispose();
        }

        /// <summary>
        /// 終了時の処理
        /// </summary>
        public void Dispose()
        {
            // 最後にウィンドウ状態を戻すとそれが目についてしまうので、あえて戻さないことにしてみるためコメントアウト
            //DetachWindow();

            // Instead of DetachWindow()
            LibUniWinC.UnregisterDropFilesCallback(); 
        }
        #endregion

        
        #region Callbacks
  
        /// <summary>
        /// ファイル、フォルダがドロップされた時に呼ばれるコールバック
        /// 文字列を配列に直すことと、フラグを立てるまで行う
        /// </summary>
        /// <param name="paths"></param>
        [MonoPInvokeCallback(typeof(LibUniWinC.StringCallback))]
        private static void _droppedFilesCallback([MarshalAs(UnmanagedType.LPWStr)] string paths)
        {
            // LF 区切りで届いた文字列を分割してパスの配列に直す
            char[] delimiters = { '\n', '\r', '\t', '\0' };
            string[] files = paths.Split(delimiters).Where(s => s != "").ToArray();
            
            if (files.Length > 0)
            {
                lastDroppedFiles = new string[files.Length];
                files.CopyTo(lastDroppedFiles, 0);

                wasDropped = true;
            }
        }
        
        #endregion
        
        #region Find, attach or detach 

        /// <summary>
        /// ウィンドウ状態を最初に戻して操作対象から解除
        /// </summary>
        public void DetachWindow()
        { 
            LibUniWinC.DetachWindow();
        }

        /// <summary>
        /// 自分のウィンドウ（ゲームビューが独立ウィンドウならそれ）を探して操作対象とする
        /// </summary>
        /// <returns></returns>
        public bool AttachMyWindow()
        {
#if UNITY_EDITOR_WIN
            // 確実にゲームビューを得る方法がなさそうなので、フォーカスを与えて直後にアクティブなウィンドウを取得
            var gameView = GetGameView();
            if (gameView)
            {
                gameView.Focus();
                LibUniWinC.AttachMyActiveWindow();
            }
#else
        LibUniWinC.AttachMyWindow();
#endif
            // Add event handlers
            LibUniWinC.RegisterDropFilesCallback(_droppedFilesCallback); 

            IsActive = LibUniWinC.IsActive();
            return IsActive;
        }

        /// <summary>
        /// 自分のプロセスで現在アクティブなウィンドウを選択
        /// エディタの場合、ウィンドウが閉じたりドッキングしたりするため、フォーカス時に呼ぶ
        /// </summary>
        /// <returns></returns>
        public bool AttachMyActiveWindow()
        {
            LibUniWinC.AttachMyActiveWindow();
            IsActive = LibUniWinC.IsActive();
            return IsActive;
        }

        #endregion
 

        #region About file dropping
        public void SetAllowDrop(bool enabled)
        {
            var allow = LibUniWinC.SetAllowDrop(enabled); 
        }

        /// <summary>
        /// Check files dropping and unset the dropped flag
        /// </summary>
        /// <param name="files"></param>
        /// <returns>true if files were dropped</returns>
        public bool ObserveDroppedFiles(out string[] files)
        {
            files = lastDroppedFiles;

            if (!wasDropped || files == null) return false;

            wasDropped = false;
            return true;
        }
   
        #endregion
 

    }
}