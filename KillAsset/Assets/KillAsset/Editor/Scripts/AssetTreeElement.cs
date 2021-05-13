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

        public static AssetTreeElement CreateRoot()
        {
            AssetTreeElement element = new AssetTreeElement();
            element.id = 0;
            element.depth = -1;
            element.name = "Root";
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

        public string Guid
        {
            get { return m_assetGuid; }
            set { m_assetGuid = value; }
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

        public List<AssetTreeElement> GetDependencies()
        {
            return dependencies;
        }

        public virtual void CollectDependicies() { }

        public List<AssetTreeElement> dependencies = new List<AssetTreeElement>();
    }
}


