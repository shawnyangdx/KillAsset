using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace KA
{
    public enum AssetType
    {
        InValid = -1,
        Scene,
    }

    [Serializable]
    public class AssetTreeElement : TreeElement
    {
        public static AssetTreeElement CreateRoot()
        {
            AssetTreeElement element = new AssetTreeElement();
            element.id = 0;
            element.depth = -1;
            element.name = "Root";
            return element;
        }

        public virtual AssetType GetAssetType() { return AssetType.InValid; }

        public virtual string Path { get; }

        public List<string> GetDependencies()
        {
            return dependencies;
        }

        public virtual void CollectDependicies() { }

        public List<string> dependencies = new List<string>();
    }

    public class SceneAssetItem : AssetTreeElement
    {
        public SceneAssetItem(Scene scene) : base()
        {
            this._scene = scene;
            name = _scene.name;
        }

        public override AssetType GetAssetType()
        {
            return AssetType.Scene;
        }

        public override string Path => _scene.path;


        public override void CollectDependicies()
        {
            dependencies = AssetDatabase.GetDependencies(this._scene.path, true).ToList();
        }

        Scene _scene;
    }
}


