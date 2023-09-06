using UnityEngine;
using UnityEngine.UI;

namespace TWV.Scenes
{
    public class JSInjectExample : MonoBehaviour
    {
        [SerializeField]
        private TextureWebView _webView = null;

        [Header("Inject Script Group")]
        [SerializeField]
        private GameObject _injectScriptGroup = null;

        [SerializeField]
        private InputField _scriptField = null;

        [SerializeField]
        private Button _injectButton = null;

        [SerializeField]
        private Text _injectResultText = null;

        private void Start()
        {
            _injectResultText.gameObject.SetActive(false);
            _injectScriptGroup.SetActive(false);

            _webView.PageStartedListener += OnPageStarted;
            _webView.PageFinishedListener += OnPageFinished;

            _injectButton.onClick.AddListener(OnInject);
        }

        private void OnPageStarted(string pageUrl)
        {
            _injectScriptGroup.SetActive(false);
        }

        private void OnPageFinished(string pageUrl)
        {
            _injectScriptGroup.SetActive(true);
        }

        public void OnInject()
        {
            if (!string.IsNullOrEmpty(_scriptField.text))
            {
                if (_webView != null)
                    _webView.EvaluateJavascript(_scriptField.text, (result, error) => {
                        if (string.IsNullOrEmpty(error))
                        {
                            if (!string.IsNullOrEmpty(result))
                            {
                                _injectResultText.gameObject.SetActive(true);
                                _injectResultText.text = string.Format("Result: {0}", result);
                            }
                            else
                            {
                                _injectResultText.gameObject.SetActive(false);
                            }
                        }
                        else
                        {
                            _injectResultText.gameObject.SetActive(true);
                            _injectResultText.text = string.Format("Error: {0}", error);
                        }
                    });
            }
            else
                Debug.LogError("Can't inject current script, because it's incorrect.");
        }
    }
}
