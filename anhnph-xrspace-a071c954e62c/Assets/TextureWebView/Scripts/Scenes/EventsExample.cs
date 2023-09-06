using UnityEngine;

namespace TWV.Scenes
{
    public class EventsExample : MonoBehaviour, IPageListener, IPageImageReadyListener
    {
        [SerializeField]
        private TextureWebView _webView = null;

        private void Start()
        {
            if (_webView != null)
            {
                _webView.PageStartedListener += OnPageStarted;
                _webView.PageLoadingListener += OnPageLoading;
                _webView.PageFinishedListener += OnPageFinished;
                _webView.PageImageReadyListener += OnPageImageReady;
                _webView.PageConsoleMessageListener += OnPageConsoleMessage;
                _webView.PageErrorListener += OnPageError;
                _webView.PageHttpErrorListener += OnPageHttpError;

                //Willbe added from component inspector window
                //_webView.MotionEventsListener += OnMotionEvents;
                //_webView.ErrorListener += OnError;
            }
        }

        public void OnPageStarted(string url)
        {
            Debug.Log("OnPageStarted: " + url);
        }

        public void OnPageLoading(int progress)
        {
            Debug.Log("OnPageLoading: " + progress);
        }

        public void OnPageFinished(string url)
        {
            Debug.Log("OnPageFinished: " + url);
        }

        public void OnPageImageReady(Texture2D videoTexture)
        {
            Debug.Log("OnPageImageReady: " + videoTexture);
        }

        public void OnPageConsoleMessage(string consoleMessage)
        {
            Debug.Log("OnPageConsoleMessage: " + consoleMessage);
        }

        public void OnPageError(string message)
        {
            Debug.Log("OnPageError: " + message);
        }

        public void OnPageHttpError(string message)
        {
            Debug.Log("OnPageHttpError: " + message);
        }

        public void OnMotionEvents(MotionActions action, Vector2 coord)
        {
            Debug.Log("OnMotionEvents(action: " + action + ", coord: " + coord + ")");
        }

        public void OnError(string message)
        {
            Debug.Log("OnError: " + message);
        }
    }
}
