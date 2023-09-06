using UnityEngine;

namespace TWV
{
    interface IWebView
    {
        GameObject OutputObject { get; set; }
        WebViewManagerEvents EventManager { get; }
        bool IsReady { get; }
        float DisplayDensity { get; }
        bool DeviceKeyboard { set; }
        string PageUrl { get; }
        byte[] FramePixels { get; }

        WebStates State { get; }
        object StateValue { get; }
        int Width { get; }
        int Height { get; }

        void AddPageListener(IPageListener listener);
        void RemovePageListener(IPageListener listener);

        void LoadUrl(string url);
        void LoadData(string data);
        void UnLoad(bool resetTexture);
        void UnLoad();
        void Release();

        bool MoveForward();
        bool MoveBack();
        void ChangeSize(int width, int height);
        void SetInputText(string text);
        void SetMotionEvent(MotionActions action, float x, float y);
        void EvaluateJavascript(string script, JsResultDelegate resultCallback);
        void ClickTo(int x, int y);
        void ScrollBy(int x, int y, float scrollTime = 0.5f);
    }
}
