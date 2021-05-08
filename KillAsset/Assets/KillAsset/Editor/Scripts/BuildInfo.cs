using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace KA
{
    public class BuildInfo
    {
        private Dictionary<string, List<string>> sceneDependencies = 
            new Dictionary<string, List<string>>();

        private List<string> buildInAssets = new List<string>();

        public static BuildInfo Build()
        {
            BuildInfo info = new BuildInfo();
            return info;
        }

        public void AddBuildInAssets()
        {

        }
    }
}

