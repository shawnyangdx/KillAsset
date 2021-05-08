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
            for (int i = 0; i < report.files.Length; i++)
            {
                Debug.Log(report.files[i]);
            }
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log(report.summary.outputPath);
            Debug.Log(report.summary.platform);
        }

        public void OnProcessScene(Scene scene, BuildReport report)
        {
        }
    }
}

