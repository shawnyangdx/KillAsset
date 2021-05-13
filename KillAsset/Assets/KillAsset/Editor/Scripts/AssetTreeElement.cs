using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine;

namespace KA
{
    public enum AssetType
    {
        None = 0,
        Scene = 1,
        Prefab = 2,
    }

    [Serializable]
    public class AssetTreeElement : TreeElement
    {
        [SerializeField] protected int m_assetType;
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

        public int AssetType
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

        public AssetType GetAssetType() { return (AssetType)m_assetType; }

        public List<AssetTreeElement> GetDependencies()
        {
            return dependencies;
        }

        public virtual void CollectDependicies() { }

        public List<AssetTreeElement> dependencies = new List<AssetTreeElement>();

        public static bool operator == (AssetTreeElement a, AssetTreeElement b)
        {
            return string.CompareOrdinal(a.Path, b.Path) == 0;
        }

        public static bool operator != (AssetTreeElement a, AssetTreeElement b)
        {
            return string.CompareOrdinal(a.Path, b.Path) != 0;
        }
    }
}


