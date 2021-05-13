using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace KA
{
    public enum PipelineGroup
    {
        Filtration,
        FindReference,
        Other,
    }

    public class PipelineAttrAttribute : Attribute
    {
        public string Name { get; private set; } = "";
        public PipelineGroup Group { get; private set; }

        public PipelineAttrAttribute(string name)
        {
            this.Name = name;
            this.Group = PipelineGroup.Other;
        }


        public PipelineAttrAttribute(string name , PipelineGroup group)
        {
            this.Name = name;
            this.Group = group;
        }
    }

    public interface IPipeline
    {
        List<string> GetObjectPath();

        void OnGUI(MainWindow window);
    }


    //[PipelineAttr(name: "Scene", group: PipelineGroup.FindReference)]
    //public class ScenePipeline : IPipeline
    //{
    //    public List<string> GetObjectPath()
    //    {
    //        return EditorBuildSettings.scenes.Where(v => v.enabled).Select(v => v.path).ToList<string>();
    //    }
    //}
}

