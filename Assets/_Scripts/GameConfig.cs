using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectAF
{
    public static class GameConfig
    {
#if UNITY_EDITOR
        public static string MapDataPath = Application.dataPath + @"/BuiltInMaps";
#else
        public static string MapDataPath = Application.dataPath + @"/Maps";
#endif
        public static string MainMapDataPath = MapDataPath + @"/MainStage";
    }
}
