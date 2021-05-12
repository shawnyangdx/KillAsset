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
        public static AssetTreeElement CreateRoot()
        {
            AssetTreeElement element = new AssetTreeElement();
            element.id = 0;
            element.depth = -1;
            element.name = "Root";
            return element;
        }

        public AssetType GetAssetType() { return (AssetType)m_assetType; }

        public List<AssetTreeElement> GetDependencies()
        {
            return dependencies;
        }

        public virtual void CollectDependicies() { }

        public List<AssetTreeElement> dependencies = new List<AssetTreeElement>();
    }

    public class SceneAssetItem : AssetTreeElement
    {
        public SceneAssetItem(Scene scene) : base()
        {
            this._scene = scene;
            id = SerializeBuildInfo.Inst.BuildID;
            depth = 0;
            name =  _scene.name;
            m_assetType = (int)AssetType.Scene;
        }

        public override void CollectDependicies()
        {
            dependencies = AssetTreeHelper.CollectAssetTreeElement(_scene.path, this.depth);
        }

        Scene _scene;
    }
}


