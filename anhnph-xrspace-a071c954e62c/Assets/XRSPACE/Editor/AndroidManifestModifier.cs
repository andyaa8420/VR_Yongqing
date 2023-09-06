using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Android;
using System.IO;
using System.Text;
using System.Xml;

public class AndroidManifestModifier : IPostGenerateGradleAndroidProject
{
    public int callbackOrder
    {
        get
        {
            return 0;
        }
    }

    private const string _url = "http://schemas.android.com/apk/res/android";
    private const string _minVersion = "1.40.2";
    private const string _targetVersion = "1.40.2";

    private BuildConfigScriptableObject _config = null;

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        string manifestPath = path + "/src/main/AndroidManifest.xml";
        XmlDocument manifest = new XmlDocument();
        _config = FindBuildConfig();
        
        try
        {
            manifest.Load(manifestPath);
            AddCategory(manifest);
            AddExportedFalse(manifest);
            AddMetaDataInputMethod(manifest);
            AddMetaDataXRSpace(manifest);
            AddMinAPILevel(manifest);
            AddSDKVersion(manifest);
            AddSDKVersionCode(manifest);
            manifest.Save(manifestPath);
        }
        catch
        {
            Debug.Log("Modify manifest failed");
        }
    }

    private BuildConfigScriptableObject FindBuildConfig()
    {
        string[] guids = AssetDatabase.FindAssets("t:" + typeof(BuildConfigScriptableObject).Name);

        if(guids.Length == 0)
        {
            Debug.LogError("Can't find XR-BuildConfig.asset. Please right click on project window, select Create/XRSDK/Create build config and set values");
            return new BuildConfigScriptableObject();
        }
        else if(guids.Length > 1)
        {
            Debug.LogWarningFormat("Find XR-BuildConfig.asset more than one. Choose {0} for building", AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        return AssetDatabase.LoadAssetAtPath<BuildConfigScriptableObject>(AssetDatabase.GUIDToAssetPath(guids[0]));
    }

    private void AddCategory(XmlDocument document)
    {
        XmlNamespaceManager nsMgr = new XmlNamespaceManager(document.NameTable);
        nsMgr.AddNamespace("android", _url);

        XmlNode intentFilterNode = document.SelectSingleNode("/manifest/application/activity/intent-filter", nsMgr);
        XmlNodeList categoryNodes = document.SelectNodes("/manifest/application/activity/intent-filter/category", nsMgr);
        bool isFind = false;
        string categoryName = "com.xrspace.intent.category.MANOVA";
        foreach (XmlNode node in categoryNodes)
        {
            if(node.Name == categoryName)
            {
                isFind = true;
                break;
            }
        }

        if(!isFind)
        {
            XmlNode categoryNode = document.CreateNode(XmlNodeType.Element, "category", intentFilterNode.NamespaceURI);
            XmlAttribute attrName = document.CreateAttribute("android:name", _url);
            attrName.Value = categoryName;
            categoryNode.Attributes.Append(attrName);
            intentFilterNode.AppendChild(categoryNode);
        }
    }

    private void AddExportedFalse(XmlDocument document)
    {
        XmlNamespaceManager nsMgr = new XmlNamespaceManager(document.NameTable);
        nsMgr.AddNamespace("android", _url);

        XmlNode activityNode = document.SelectSingleNode("/manifest/application/activity", nsMgr);
        XmlAttribute attr = (XmlAttribute)activityNode.Attributes.GetNamedItem("android:exported");
        if(attr != null)
        {
            attr.Value =  _config.Exported ? "true" : "false";
        }
        else
        {
            attr = document.CreateAttribute("android:exported", _url);
            attr.Value = _config.Exported ? "true" : "false";
            activityNode.Attributes.Append(attr);
        }
    }

    private void AddMetaDataInputMethod(XmlDocument document)
    {
        XmlNamespaceManager nsMgr = new XmlNamespaceManager(document.NameTable);
        nsMgr.AddNamespace("android", _url);

        XmlNode applicationNode = document.SelectSingleNode("/manifest/application", nsMgr);
        XmlNodeList metaNodes = document.SelectNodes("/manifest/application/meta-data", nsMgr);
        string value = string.Empty;
        if (_config.InputMethod.Hand)
            value += value.Length == 0 ? "hand" :"|hand";
        if (_config.InputMethod.Controller)
            value += value.Length == 0 ? "controller" : "|controller";
        if (_config.InputMethod.Tracker)
            value += value.Length == 0 ? "tracker" : "|tracker";

        bool isFind = false;
        foreach (XmlNode node in metaNodes)
        {
            if (node.Name == "InputMethod")
            {
                node.Value = value;
                isFind = true;
                break;
            }
        }

        if (!isFind)
        {
            XmlNode meta = document.CreateNode(XmlNodeType.Element, "meta-data", applicationNode.NamespaceURI);
            XmlAttribute attrName = document.CreateAttribute("android:name", _url);
            XmlAttribute attrValue = document.CreateAttribute("android:value", _url);
            attrName.Value = "InputMethod";
            attrValue.Value = value;
            meta.Attributes.Append(attrName);
            meta.Attributes.Append(attrValue);
            applicationNode.AppendChild(meta);
        }
    }

    private void AddMetaDataXRSpace(XmlDocument document)
    {
        XmlNamespaceManager nsMgr = new XmlNamespaceManager(document.NameTable);
        nsMgr.AddNamespace("android", _url);

        XmlNode applicationNode = document.SelectSingleNode("/manifest/application", nsMgr);
        XmlNodeList metaNodes = document.SelectNodes("/manifest/application/meta-data", nsMgr);
        bool isFind = false;
        foreach(XmlNode node in metaNodes)
        {
            if(node.Name == "com.XRSpace.VR")
            {
                node.Value = "true";
                isFind = true;
                break;
            }
        }

        if (!isFind)
        {
            XmlNode meta = document.CreateNode(XmlNodeType.Element, "meta-data", applicationNode.NamespaceURI);
            XmlAttribute attrName = document.CreateAttribute("android:name", _url);
            XmlAttribute attrValue = document.CreateAttribute("android:value", _url);
            attrName.Value = "com.XRSpace.VR";
            attrValue.Value = "true";
            meta.Attributes.Append(attrName);
            meta.Attributes.Append(attrValue);
            applicationNode.AppendChild(meta);
        }
    }

    private void AddMinAPILevel(XmlDocument document)
    {
        XmlNamespaceManager nsMgr = new XmlNamespaceManager(document.NameTable);
        nsMgr.AddNamespace("android", _url);

        XmlNode applicationNode = document.SelectSingleNode("/manifest/application", nsMgr);
        XmlNode meta = document.CreateNode(XmlNodeType.Element, "meta-data", applicationNode.NamespaceURI);
        XmlAttribute attrName = document.CreateAttribute("android:name", _url);
        XmlAttribute attrValue = document.CreateAttribute("android:value", _url);
        attrName.Value = "minAPILevel";
        attrValue.Value = XRSpace.Platform.VRcore.XRManager.MIN_API_LEVEL;
        meta.Attributes.Append(attrName);
        meta.Attributes.Append(attrValue);
        applicationNode.AppendChild(meta);
    }

    private void AddSDKVersion(XmlDocument document)
    {
        XmlNamespaceManager nsMgr = new XmlNamespaceManager(document.NameTable);
        nsMgr.AddNamespace("android", _url);

        XmlNode applicationNode = document.SelectSingleNode("/manifest/application", nsMgr);
        XmlNode meta = document.CreateNode(XmlNodeType.Element, "meta-data", applicationNode.NamespaceURI);
        XmlAttribute attrName = document.CreateAttribute("android:name", _url);
        XmlAttribute attrValue = document.CreateAttribute("android:value", _url);
        attrName.Value = "SDK-Version";
        attrValue.Value = XRSpace.Platform.InputDevice.XRInputManager.VERSION;
        meta.Attributes.Append(attrName);
        meta.Attributes.Append(attrValue);
        applicationNode.AppendChild(meta);
    }

    private void AddSDKVersionCode(XmlDocument document)
    {
        XmlNamespaceManager nsMgr = new XmlNamespaceManager(document.NameTable);
        nsMgr.AddNamespace("android", _url);

        XmlNode applicationNode = document.SelectSingleNode("/manifest/application", nsMgr);
        XmlNode meta = document.CreateNode(XmlNodeType.Element, "meta-data", applicationNode.NamespaceURI);
        XmlAttribute attrName = document.CreateAttribute("android:name", _url);
        XmlAttribute attrValue = document.CreateAttribute("android:value", _url);
        attrName.Value = "SDK-VersionCode";
        attrValue.Value = ParseVersionCode().ToString();
        meta.Attributes.Append(attrName);
        meta.Attributes.Append(attrValue);
        applicationNode.AppendChild(meta);
    }

    private int ParseVersionCode()
    {
        string[] versionStrings = XRSpace.Platform.InputDevice.XRInputManager.VERSION.Split(new char[1] { '.' });
        if (versionStrings.Length >= 3)
        {
            return int.Parse(versionStrings[0]) * 1000000 + int.Parse(versionStrings[1]) * 10000 + int.Parse(versionStrings[2]);
        }

        return 0;
    }
}
