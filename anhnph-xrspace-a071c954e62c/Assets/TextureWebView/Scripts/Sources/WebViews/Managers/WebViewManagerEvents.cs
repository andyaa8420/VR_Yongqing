using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace TWV
{
    public enum WebStates
    {
        Empty,
        Started,
        Loading,
        Finished,
        ImageReady,
        JsDataReceived,
        ConsoleMessage,
        ContentError,
        HttpError,
        Error
    }

    public class WebViewManagerEvents
    {
        internal class WebEvent
        {
            private WebStates _state;
            private object _arg;

            public WebEvent(WebStates state, object arg)
            {
                _state = state;
                _arg = arg;
            }

            public WebStates State
            {
                get
                {
                    return _state;
                }
            }

            public object Arg
            {
                get
                {
                    return _arg;
                }
                set
                {
                    _arg = value;
                }
            }

            public int GetIntArg
            {
                get
                {
                    return (_arg != null && _arg is int) ? (int)_arg : 0;
                }
            }

            public float GetFloatArg
            {
                get
                {
                    return (_arg != null && _arg is float) ? (float)_arg : 0f;
                }
            }

            public long GetLongArg
            {
                get
                {
                    return (_arg != null && _arg is long) ? (long)_arg : 0;
                }
            }

            public string GetStringArg
            {
                get
                {
                    return (_arg != null && _arg is string) ? (string)_arg : string.Empty;
                }
            }
        }
        
        private MonoBehaviour _monoObject;
        private IWebView _webView;
        private Queue<WebEvent> _webViewEvents;
        private IEnumerator _stateListenerEnum;
        private WebStates _replaceState;
        private WebEvent _replaceEvent;

        internal WebViewManagerEvents(MonoBehaviour monoObject, IWebView webView)
        {
            _monoObject = monoObject;
            _webView = webView;
            _webViewEvents = new Queue<WebEvent>();
        }

        private WebEvent Event
        {
            get
            {
                return new WebEvent(_webView.State, _webView.StateValue);
            }
        }

        private IEnumerator EventManager()
        {
            while (true)
            {
                var currentEvent = Event;
                if (currentEvent != null && currentEvent.State != WebStates.Empty)
                    _webViewEvents.Enqueue(currentEvent);
                
                if (_webViewEvents.Count <= 0)
                {
                    yield return null;
                    continue;
                }

                CallEvent();
            }
        }

        private void CallEvent()
        {
            var eventValue = _webViewEvents.Dequeue();

            if (_replaceState == eventValue.State)
            {
                _replaceState = WebStates.Empty;
                eventValue = _replaceEvent;
            }

            switch (eventValue.State)
            {
                case WebStates.Started:
                    if (_pageStartedListener != null)
                        _pageStartedListener(eventValue.GetStringArg);
                    break;

                case WebStates.Loading:
                    if (_pageLoadingListener != null)
                        _pageLoadingListener((int)eventValue.GetFloatArg);
                    break;

                case WebStates.Finished:
                    if (_pageFinishedListener != null)
                        _pageFinishedListener(eventValue.GetStringArg);
                    break;

                case WebStates.ImageReady:
                    if (_pageImageReadyListener != null)
                        _pageImageReadyListener((Texture2D)eventValue.Arg);
                    break;

                case WebStates.JsDataReceived:
                    if (_jsDataReceivedListener != null)
                        _jsDataReceivedListener(eventValue.GetStringArg);
                    break;

                case WebStates.ConsoleMessage:
                    if (_pageConsoleMessageListener != null)
                        _pageConsoleMessageListener(eventValue.GetStringArg);
                    break;

                case WebStates.ContentError:
                    if (_pageErrorListener != null)
                        _pageErrorListener(eventValue.GetStringArg);
                    break;

                case WebStates.HttpError:
                    if (_pageHttpErrorListener != null)
                        _pageHttpErrorListener(eventValue.GetStringArg);
                    break;

                case WebStates.Error:
                    if (_errorListener != null)
                        _errorListener(eventValue.GetStringArg);
                    break;
            }
        }

        internal void SetEvent(WebStates state)
        {
            _webViewEvents.Enqueue(new WebEvent(state, null));
        }

        internal void SetEvent(WebStates state, object arg)
        {
            _webViewEvents.Enqueue(new WebEvent(state, arg));
        }

        internal void ReplaceEvent(WebStates replaceState, WebStates newState, object arg)
        {
            _replaceState = replaceState;
            _replaceEvent = new WebEvent(newState, arg);
        }

        public void StartListener()
        {
            _webViewEvents.Clear();
            if (_stateListenerEnum != null)
                _monoObject.StopCoroutine(_stateListenerEnum);

            _stateListenerEnum = EventManager();
            _monoObject.StartCoroutine(_stateListenerEnum);
        }

        public void StopListener()
        {
            if (_stateListenerEnum != null)
                _monoObject.StopCoroutine(_stateListenerEnum);

            while (_webViewEvents.Count > 0)
            {
                CallEvent();
            }
        }

        public void RemoveAllEvents()
        {
            if (_pageStartedListener != null)
            {
                foreach (Action<string> eh in _pageStartedListener.GetInvocationList())
                {
                    _pageStartedListener -= eh;
                }
            }

            if (_pageLoadingListener != null)
            {
                foreach (Action<int> eh in _pageLoadingListener.GetInvocationList())
                {
                    _pageLoadingListener -= eh;
                }
            }

            if (_pageFinishedListener != null)
            {
                foreach (Action<string> eh in _pageFinishedListener.GetInvocationList())
                {
                    _pageFinishedListener -= eh;
                }
            }

            if (_pageImageReadyListener != null)
            {
                foreach (Action<Texture2D> eh in _pageImageReadyListener.GetInvocationList())
                {
                    _pageImageReadyListener -= eh;
                }
            }

            if (_jsDataReceivedListener != null)
            {
                foreach (Action<string> eh in _jsDataReceivedListener.GetInvocationList())
                {
                    _jsDataReceivedListener -= eh;
                }
            }

            if (_pageConsoleMessageListener != null)
            {
                foreach (Action<string> eh in _pageConsoleMessageListener.GetInvocationList())
                {
                    _pageConsoleMessageListener -= eh;
                }
            }

            if (_pageErrorListener != null)
            {
                foreach (Action<string> eh in _pageErrorListener.GetInvocationList())
                {
                    _pageErrorListener -= eh;
                }
            }

            if (_pageHttpErrorListener != null)
            {
                foreach (Action<string> eh in _pageHttpErrorListener.GetInvocationList())
                {
                    _pageHttpErrorListener -= eh;
                }
            }

            if (_errorListener != null)
            {
                foreach (Action<string> eh in _errorListener.GetInvocationList())
                {
                    _errorListener -= eh;
                }
            }
        }

        #region Actions
        private event Action<string> _pageStartedListener;

        /// <summary>
        /// Notify the host application that a page has started loading
        /// </summary>
        public event Action<string> PageStartedListener
        {
            add
            {
                _pageStartedListener = (Action<string>)Delegate.Combine(_pageStartedListener, value);
            }
            remove
            {
                if (_pageStartedListener != null)
                    _pageStartedListener = (Action<string>)Delegate.Remove(_pageStartedListener, value);
            }
        }

        private event Action<int> _pageLoadingListener;

        /// <summary>
        /// Tell the host application the current progress of loading a page
        /// </summary>
        public event Action<int> PageLoadingListener
        {
            add
            {
                _pageLoadingListener = (Action<int>)Delegate.Combine(_pageLoadingListener, value);
            }
            remove
            {
                if (_pageLoadingListener != null)
                    _pageLoadingListener = (Action<int>)Delegate.Remove(_pageLoadingListener, value);
            }
        }

        private event Action<string> _pageFinishedListener;

        /// <summary>
        /// Notify the host application that a page has finished loading
        /// </summary>
        public event Action<string> PageFinishedListener
        {
            add
            {
                _pageFinishedListener = (Action<string>)Delegate.Combine(_pageFinishedListener, value);
            }
            remove
            {
                if (_pageFinishedListener != null)
                    _pageFinishedListener = (Action<string>)Delegate.Remove(_pageFinishedListener, value);
            }
        }

        private event Action<Texture2D> _pageImageReadyListener;

        /// <summary>
        /// Notify the host application that a page image has ready to use
        /// </summary>
        public event Action<Texture2D> PageImageReadyListener
        {
            add
            {
                _pageImageReadyListener = (Action<Texture2D>)Delegate.Combine(_pageImageReadyListener, value);
            }
            remove
            {
                if (_pageImageReadyListener != null)
                    _pageImageReadyListener = (Action<Texture2D>)Delegate.Remove(_pageImageReadyListener, value);
            }
        }

        private event Action<string> _pageConsoleMessageListener;

        /// <summary>
        /// Report a JavaScript error message to the host application.
        /// </summary>
        public event Action<string> PageConsoleMessageListener
        {
            add
            {
                _pageConsoleMessageListener = (Action<string>)Delegate.Combine(_pageConsoleMessageListener, value);
            }
            remove
            {
                if (_pageConsoleMessageListener != null)
                    _pageConsoleMessageListener = (Action<string>)Delegate.Remove(_pageConsoleMessageListener, value);
            }
        }

        private event Action<string> _jsDataReceivedListener;

        /// <summary>
        /// Notify the host application that an custom js data has been received from the web view component
        /// </summary>
        internal event Action<string> JsDataReceivedListener
        {
            add
            {
                _jsDataReceivedListener = (Action<string>)Delegate.Combine(_jsDataReceivedListener, value);
            }
            remove
            {
                if (_jsDataReceivedListener != null)
                    _jsDataReceivedListener = (Action<string>)Delegate.Remove(_jsDataReceivedListener, value);
            }
        }

        private event Action<string> _pageErrorListener;

        /// <summary>
        /// Report web resource loading error to the host application.
        /// </summary>
        public event Action<string> PageErrorListener
        {
            add
            {
                _pageErrorListener = (Action<string>)Delegate.Combine(_pageErrorListener, value);
            }
            remove
            {
                if (_pageErrorListener != null)
                    _pageErrorListener = (Action<string>)Delegate.Remove(_pageErrorListener, value);
            }
        }

        private event Action<string> _pageHttpErrorListener;

        /// <summary>
        /// Notify the host application that an HTTP error has been received from the server while loading a resource. 
        /// HTTP errors have status codes >= 400. 
        /// This callback will be called for any resource (iframe, image, etc.), not just for the main page.
        /// </summary>
        public event Action<string> PageHttpErrorListener
        {
            add
            {
                _pageHttpErrorListener = (Action<string>)Delegate.Combine(_pageHttpErrorListener, value);
            }
            remove
            {
                if (_pageHttpErrorListener != null)
                    _pageHttpErrorListener = (Action<string>)Delegate.Remove(_pageHttpErrorListener, value);
            }
        }

        private event Action<string> _errorListener;

        /// <summary>
        /// Report web view loading error.
        /// </summary>
        public event Action<string> ErrorListener
        {
            add
            {
                _errorListener = (Action<string>)Delegate.Combine(_errorListener, value);
            }
            remove
            {
                if (_errorListener != null)
                    _errorListener = (Action<string>)Delegate.Remove(_errorListener, value);
            }
        }
        #endregion
    }
}
