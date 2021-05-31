using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KA
{
    public static class StringExtension
    {
        public static string NormalizePath(this string path)
        {
            return path.Replace("\\", "/");
        }
    }
}

