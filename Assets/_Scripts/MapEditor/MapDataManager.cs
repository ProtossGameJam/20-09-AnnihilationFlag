using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using UnityEngine;
using MapData = System.Collections.Generic.Dictionary<ProjectAF.TilePosition, ProjectAF.TileType>;

namespace ProjectAF
{

    [Serializable]
    public struct TilePosition
    {
        public int x;
        public int y;
        public TilePosition(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public static class MapDataManager
    {
        public static bool IsValidFilename(string testName)
        {
            Regex containsABadCharacter = new Regex("["
                  + Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars())) + "]");
            if (containsABadCharacter.IsMatch(testName)) { return false; };

            return true;
        }

        public static bool Save(MapData data, string fileName)
        {
            if (!IsValidFilename(fileName))
                return false;

            var fs = new FileStream($"{GameConfig.MapDataPath}/{fileName}.afm", FileMode.Create);
            try
            {
                var bf = new BinaryFormatter();
                bf.Serialize(fs, data);
            }
            catch (SerializationException e)
            {
                UnityEngine.Debug.LogError("Failed to serialize. " + e.Message);
                return false;
            }
            finally
            {
                fs.Close();
            }

            return true;
        }

        public static MapData Load(FileInfo fileInfo)
        {
            MapData data = null;

            var fs = fileInfo.Open(FileMode.Open);
            try
            {
                var bf = new BinaryFormatter();
                data = (MapData) bf.Deserialize(fs);
            }
            catch (SerializationException e)
            {
                Debug.LogError("Failed to deserialize. " + e.Message);
                return null;
            }
            finally
            {
                fs.Close();
            }

            return data;
        }
    }
}
