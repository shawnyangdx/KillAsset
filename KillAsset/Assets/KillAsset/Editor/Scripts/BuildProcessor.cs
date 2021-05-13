using UnityEngine;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.SceneManagement;

namespace KA
{
    public class BuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport, IProcessSceneWithReport
    {
        public int callbackOrder => 0;

        public void OnPostprocessBuild(BuildReport report)
        {
            //SerializeBuildInfo.Inst.OnPostprocessBuild(report);
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            //SerializeBuildInfo.Inst.OnPreprocessBuild(report);
        }

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            //SerializeBuildInfo.Inst.OnProcessScene(scene);
        }
    }
}

