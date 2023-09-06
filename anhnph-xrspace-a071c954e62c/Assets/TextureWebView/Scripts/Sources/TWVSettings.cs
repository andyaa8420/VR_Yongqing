using System;
using System.IO;
using UnityEngine;

namespace TWV
{
    [Serializable]
    public class TWVSettings : ScriptableObject
    {
        private const string SETTINGS_FILE_NAME = "TWVSettings";

        [SerializeField]
        private bool _useCustomAssetPath;
        [SerializeField]
        private string _assetPath;

        public const string ASSET_NAME = "TextureWebView";
        public const string PLUGINS_FOLDER_NAME = "Plugins";

        public static TWVSettings Instance
        {
            get
            {
                var settingsData = Resources.Load<TWVSettings>(SETTINGS_FILE_NAME);

                if (settingsData == null)
                {
                    Debug.LogWarning(ASSET_NAME + ": Could not find settings file [" + SETTINGS_FILE_NAME + "] in resource folder.");
                    settingsData = CreateInstance<TWVSettings>();
                }

                return settingsData;
            }
        }

        public bool UseCustomAssetPath
        {
            get { return _useCustomAssetPath; }
            set
            {
                if (!value)
                    _assetPath = string.Empty;

                if (_useCustomAssetPath != value)
                {
                    _useCustomAssetPath = value;
                }
            }
        }

        public bool IsValidAssetPath
        {
            get
            {
                return Directory.Exists(AssetPath) && Directory.GetFiles(AssetPath).Length > 0;
            }
        }

        public string AssetPath
        {
            get
            {
                if (string.IsNullOrEmpty(_assetPath))
                {
                    _assetPath = Path.Combine("Assets", ASSET_NAME);
                    _assetPath = _assetPath.Replace(@"\", "/");
                }

                return _assetPath;
            }
            set
            {
                if (_assetPath != value)
                {
                    _assetPath = value;
                }
            }
        }
    }
}
