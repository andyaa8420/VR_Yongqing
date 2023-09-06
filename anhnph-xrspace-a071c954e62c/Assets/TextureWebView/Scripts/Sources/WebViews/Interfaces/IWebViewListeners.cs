using UnityEngine;

namespace TWV
{
    public interface IPageListener:
        IPageStartedListener, 
        IPageLoadingListener,
        IPageFinishedListener,
        IPageImageReadyListener,
        IPageConsoleMessageListener,
        IPageErrorListener,
        IPageHttpErrorListener
    { }

    public interface IPageStartedListener
    {
        void OnPageStarted(string url);
    }

    public interface IPageLoadingListener
    {
        void OnPageLoading(int progress);
    }

    public interface IPageFinishedListener
    {
        void OnPageFinished(string url);
    }

    public interface IPageImageReadyListener
    {
        void OnPageImageReady(Texture2D viewTexture);
    }

    public interface IPageConsoleMessageListener
    {
        void OnPageConsoleMessage(string consoleMessage);
    }

    public interface IPageErrorListener
    {
        void OnPageError(string message);
    }

    public interface IPageHttpErrorListener
    {
        void OnPageHttpError(string message);
    }

    public interface IJsDataReceivedListener
    {
        void OnJsDataReceived(string value);
    }

    public interface IErrorListener
    {
        void OnError(string message);
    }

    public interface IInputManagerListener
    {
        /// <summary>
        /// Motion events callback for input system.
        /// </summary>
        /// <param name="target">Target that handle the motion event</param>
        /// <param name="action">The motion action that generated on target.</param>
        /// <param name="coord">Motion event position in relative coord (0..1)</param>
        void OnMotionEvents(GameObject target, MotionActions action, Vector2 coord);
    }
}
