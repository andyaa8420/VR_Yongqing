using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

namespace TWV.Wrappers
{
    internal class WrapperAndroid : IWrapperView
    {
        #region JNI
        [DllImport(TWVSettings.ASSET_NAME)]
        private static extern void TWVSetTexture(int index, IntPtr texture);

        [DllImport(TWVSettings.ASSET_NAME)]
        private static extern void TWVSetTextureBuffer(int index, IntPtr buffer);

        [DllImport(TWVSettings.ASSET_NAME)]
        private static extern int TWVPluginEventsAmount(int index);

        [DllImport(TWVSettings.ASSET_NAME)]
        private static extern IntPtr TWVGetPluginEventCallback(int index);

        public enum PluginEvents
        {
            Empty,
            Start,
            Render,
            Buffer,
            Stop
        }

        /// <summary>
        /// Set texture into native code
        /// </summary>
        public void SetTexture(IntPtr texture)
        {
            TWVSetTexture(_index, texture);
        }

        /// <summary>
        /// Set buffer into native code, that will be used to copy into it texture FBO data 
        /// </summary>
        public void SetTextureBuffer(IntPtr buffer)
        {
            TWVSetTextureBuffer(_index, buffer);
        }

        /// <summary>
        /// Send special events into native code
        /// </summary>
        public void SendPluginEvent(PluginEvents e)
        {
            byte[] bytes = { (byte)e, (byte)_index, 0, 0 };

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            int value = BitConverter.ToInt32(bytes, 0);
            GL.IssuePluginEvent(TWVGetPluginEventCallback(_index), value);
        }

        /// <summary>
        /// Send special events into native code with possibility to wait when event is completed
        /// </summary>
        public IEnumerator SendAndWaitPluginEvent(PluginEvents e)
        {
            byte[] bytes = { (byte)e, (byte)_index, 0, 0 };

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            int value = BitConverter.ToInt32(bytes, 0);
            GL.IssuePluginEvent(TWVGetPluginEventCallback(_index), value);

            while (TWVPluginEventsAmount(_index) > 0)
                yield return null;
        }
        #endregion

        private const string CLASS_PATH_CORE = "easywayasset/texturewebview/TextureWebView";

        private AndroidJavaObject _coreObj;
        private int _index;

        public int Index
        {
            get
            {
                return _index;
            }
        }

        public WrapperAndroid(int width, int height)
        {
            _coreObj = new AndroidJavaObject(CLASS_PATH_CORE, width, height);

            if (_coreObj == null)
                throw new Exception("WrapperAndroid: Can't create Android java object from this class[" + CLASS_PATH_CORE + "]");

            _index = _coreObj.Call<int>("exportGetIndex");
        }

        #region Java
        /// <summary>
        /// Set web page based on current data.
        /// </summary>
        public string LoadData
        {
            set
            {
                _coreObj.Call("exportSetData", value);
            }
        }

        /// <summary>
        /// Set web page based on current url.
        /// </summary>
        public string LoadUrl
        {
            set
            {
                _coreObj.Call("exportSetUrl", value);
            }
        }

        /// <summary>
        /// Gets the URL for the current page. This is not always the same as the URL
        /// passed to Load because although the load for
        /// that URL has begun, the current page may not have changed.
        /// </summary>
        public string PageUrl
        {
            get
            {
                return _coreObj.Call<string>("exportGetUrl");
            }
        }

        /// <summary>
        /// Get The logical density of the display. This is a scaling factor for the 
        /// Density Independent Pixel unit, where one DIP is one pixel on an
        /// approximately 160 dpi screen(for example a 240x320, 1.5"x2" screen), 
        /// providing the baseline of the system's display. Thus on a 160dpi screen 
        /// this density value will be 1; on a 120 dpi screen it would be .75; etc.
        /// </summary>
        public float DisplayDensity
        {
            get
            {
                return _coreObj.Call<float>("exportDisplayDensity");
            }
        }

        /// <summary>
        /// Show default device keyboard 
        /// that allows a user to enter data 
        /// that is sent to a server for processing.
        /// </summary>
        public bool ShowKeyboard
        {
            set
            {
                if (IsViewReady)
                {
                    _coreObj.Call("exportShowKeyboard", value);
                    _coreObj.Call("exportSetLongClickable", value);
                }
            }
        }

        /// <summary>
        /// Get current web view state.
        /// </summary>
        public WebStates State
        {
            get
            {
                return (WebStates)_coreObj.Call<int>("exportGetState");
            }
        }

        /// <summary>
        /// Get current web view state value (can be float, long or string type).
        /// </summary>
        public object StateValue
        {
            get
            {
                object value = _coreObj.Call<float>("exportGetStateFloatValue");

                if ((float)value < 0)
                {
                    value = _coreObj.Call<long>("exportGetStateLongValue");
                    if ((long)value < 0)
                        value = _coreObj.Call<string>("exportGetStateStringValue");
                }

                return value;
            }
        }

        #region Platform dependent properties
        /// <summary>
        /// Get amount of rendered frames of current view.
        /// </summary>
        public long FramesCounter
        {
            get
            {
                return _coreObj.Call<int>("exportFramesCounter");
            }
        }

        /// <summary>
        /// View is correctly added into view group and can be used.
        /// </summary>
        public bool IsViewReady
        {
            get
            {
                return _coreObj.Call<bool>("exportIsViewReady");
            }
        }

        /// <summary>
        /// Enable/Disable long click logic, usable in VR to disable text selection.
        /// </summary>
        public bool UseLongClick
        {
            set
            {
                _coreObj.Call("exportSetLongClickable", value);
            }
        }
        #endregion

        /// <summary>
        /// Start webview rendering.
        /// </summary>
        public bool StartRender()
        {
            return _coreObj.Call<bool>("exportStartRender");
        }

        /// <summary>
        /// Stop webview rendering.
        /// </summary>
        public void StopRender()
        {
            _coreObj.Call("exportStopRender");
        }

        /// <summary>
        /// Release a current web view instance 
        /// </summary>
        public void Release()
        {
            _coreObj.Call("exportRelease");
        }

        /// <summary>
        /// Goes forward in the history of this WebView.
        /// </summary>
        /// <returns>True if this Webview has a forward history item</returns>
        public bool MoveForward()
        {
            return _coreObj.Call<bool>("exportMoveForward");
        }

        /// <summary>
        /// Goes back in the history of this WebView.
        /// </summary>
        /// <returns>True if this WebView has a back history item</returns>
        public bool MoveBack()
        {
            return _coreObj.Call<bool>("exportMoveBack");
        }

        /// <summary>
        /// Change the size of view (should be > 0)
        /// </summary>
        /// <param name="width">The new width for current view</param>
        /// <param name="height">The new height for current view</param>
        public void ChangeSize(int width, int height)
        {
            _coreObj.Call("exportChangeSize", width, height);
        }

        /// <summary>
        /// Set text to current active input form
        /// </summary>
        /// <param name="text">Text that will be set to input form</param>
        public void SetInputText(string text)
        {
            _coreObj.Call("exportSetInputText", text);
        }

        /// <summary>
        /// Send motion event with the specified coordinates.
        /// </summary>
        /// <param name="action">The motion action that will be sended into web view.</param>
        /// <param name="x">The coordinate of action, along the x-axis (horizontal).</param>
        /// <param name="y">The coordinate of action, along the y-axis (vertical).</param>
        public void SetMotionEvent(MotionActions action, float x, float y)
        {
            _coreObj.Call("exportSetMotionEvent", (int)action, x, y);
        }

        /// <summary>
        /// Asynchronously evaluates JavaScript in the context of the currently displayed page.
        /// </summary>
        /// <param name="script">The JavaScript to execute</param>
        /// <param name="callbackPointer">A callback to be invoked when the script execution
        /// completes with the result of the execution(if any).
        /// <param nme="errorPrefix">Prefix that will be used to check if return value is error message.</param>
        public void EvaluateJavascript(string script, IntPtr callbackPointer, string errorPrefix)
        {
            _coreObj.Call("exportEvaluateJavascript", script, callbackPointer.ToInt64(), errorPrefix);
        }
        #endregion
    }
}
