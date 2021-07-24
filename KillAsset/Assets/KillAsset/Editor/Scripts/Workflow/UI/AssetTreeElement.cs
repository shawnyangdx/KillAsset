using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine;

namespace KA
{
    [Serializable]
    public class AssetTreeElement : TreeElement
    {
        [SerializeField] protected Type m_assetType;
        [SerializeField] protected string m_path;
        protected string m_assetGuid;
        protected long m_size;
        string m_relativePath; //without 'asset/'
        public static AssetTreeElement CreateRoot()
        {
            AssetTreeElement element = new AssetTreeElement();
            element.id = -1;
            element.depth = -1;
            element.name = "Root";
            element.AssetType = element.GetType();
            element.Path = "";
            element.Guid = "";
            element.Size = 0;
            return element;
        }

        private static Dictionary<Type, Texture> iconDictionary = new Dictionary<Type, Texture>();

        public Type AssetType
        {
            get { return m_assetType; }
            set { m_assetType = value; }
        }

        public string Path
        {
            get { return m_path; }
            set { m_path = value; }
        }

        public string RelativePath
        {
            get
            {
                if(string.IsNullOrEmpty(m_relativePath))
                {
                    m_relativePath = m_path.Replace("Assets/", "");
                }

                return m_relativePath;
            }
        }

        public string Guid
        {
            get { return m_assetGuid; }
            set { m_assetGuid = value; }
        }

        //file size , is bytes
        public long Size
        {
            get { return m_size; }
            set { m_size = value; }
        }

        public Texture Icon
        {
            get
            {
                if (m_assetType == null)
                    return null;

                if (!iconDictionary.TryGetValue(AssetType, out Texture image))
                {
                    image = EditorGUIUtility.ObjectContent(null, m_assetType).image;
                    iconDictionary.Add(m_assetType, image);
                }

                return image;
            }
        }
    }
}


