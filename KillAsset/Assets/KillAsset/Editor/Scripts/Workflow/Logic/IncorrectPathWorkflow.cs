using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace KA
{
    [WorkflowOverride("Incorrect Path")]
    public class IncorrectPathWorkflow : AssetWorkflow
    {
        static string ChineseRegex = @"[\u4e00-\u9fa5]";

        public override void Clear()
        {
        }

        public override void Run()
        {
            List<string> checkList = Helper.Path.CollectAssetPaths(EditorConfig.Inst.RootPath);
            List<string> pathList = new List<string>();
            for (int i = 0; i < checkList.Count; i++)
            {
                if (Regex.IsMatch(checkList[i], ChineseRegex))  //chinese regex
                {
                    pathList.Add(checkList[i]);
                }
                else if (checkList[i].IndexOf(" ") >= 0)  //chinese regex
                {
                    pathList.Add(checkList[i]);
                }
            }

            RefreshTreeView(pathList);
        }
    }
}

