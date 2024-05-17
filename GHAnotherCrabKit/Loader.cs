using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GHAnotherCrabKit
{
    public class Loader
    {
        public static void Init()
        {
            _Load = new GameObject();
            _Load.AddComponent<Main>();
            UnityEngine.Object.DontDestroyOnLoad(_Load);
        }

        public static void UnLoad()
        {
            _Unload();
        }

        public static void _Unload()
        {
            UnityEngine.Object.Destroy(_Load);
        }

        private static GameObject _Load;
    }
}
