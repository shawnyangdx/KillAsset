using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace KA
{
    public interface IWorkflow
    {
        void Run();
        void OnGUI(MainWindow window);
    }

    public class WorkflowOverrideAttribute : Attribute
    {
        public string Name { get; private set; } = "";

        public WorkflowOverrideAttribute(string name)
        {
            this.Name = name;
        }
    }

    public class WorkflowIgnoreAttribute : Attribute { }
}

