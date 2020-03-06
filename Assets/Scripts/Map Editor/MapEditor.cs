using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class MapEditor : MonoBehaviour
{
    /// <summary>
    /// We could possibly have these save on a web server so everybody could use them? Add filters and such, note for after map editor is finished somewhat.
    /// </summary>
    public static string SavePath;

    public CustomMap activeMap = null; // Loaded into the scene
    public MapLoader mapLoader;

    public List<PrefabScriptableObject> Prefabs;

    private void Start()
    {
        SavePath = $"{Application.persistentDataPath}/Maps/";
        Prefabs = Resources.LoadAll<PrefabScriptableObject>("Map Editor").ToList();
        foreach(var prefab in Prefabs)
        {
            Debug.Log($"Loaded Prefab -> {prefab.Name}:{prefab.Identifier}");
        }

        if(activeMap != null && !activeMap.mapName.Equals(string.Empty))
        {
            mapLoader = gameObject.AddComponent<MapLoader>();
            mapLoader.Load(activeMap, Prefabs);
        }
    }

    [Serializable]
    public class CustomMap
    {
        /// <summary>
        /// The name of the map as seen in the custom map viewer
        /// </summary>
        public string mapName;
        /// <summary>
        /// The author of this map as seen in the editor and map viewer.
        /// </summary>
        public string mapAuthor;
        /// <summary>
        /// The last time the map was edited (generally the last save)
        /// Defaulted to the time it was created
        /// </summary>
        public DateTime mapLastEditedAt = DateTime.Now;
        /// <summary>
        /// The list of objects as seen in the map, generated on map save. 
        /// This is used to load the map into the map loader scene
        /// </summary>
        public List<CustomMapObject> mapObjects = new List<CustomMapObject>();

        public void Save()
        {
            string fileName = mapName.Replace(" ", "_").Replace(",", "_").Replace("\'", "").Replace(".", "_"); // Regex to verify file name maybe? This is temporary and a note for later
            File.WriteAllText($"{SavePath}/{mapName}.json", JsonConvert.SerializeObject(this));
        }

        /// <summary>
        /// Gets all stored maps from the maps file path
        /// </summary>
        /// <returns>A list of stored maps</returns>
        public static List<CustomMap> GetStoredMaps()
        {
            string[] files = Directory.GetFiles(SavePath);
            List<CustomMap> maps = new List<CustomMap>();

            foreach(var file in files)
            {
                string contents = File.ReadAllText(file);
                try
                {
                    CustomMap map = JsonConvert.DeserializeObject<CustomMap>(contents);
                    maps.Add(map);
                } catch 
                {
                    Debug.LogError($"Error parsing saved map: {file}");
                }
            }

            return maps;
        }

        [Serializable]
        /// <summary>
        /// A object used to save and load into maps.
        /// </summary>
        public class CustomMapObject
        {
            /// <summary>
            /// The unique identifier of the prefab this object represents. 
            /// Dont touch this, otherwise everything will probably break.
            /// </summary>
            public string prefabIdentifier;
            [SerializeField]
            /// <summary>
            /// The position of the object (from transform.position)
            /// </summary>
            public Vector3 position;
            [SerializeField]
            /// <summary>
            /// The rotation of the object (from transform.rotation)
            /// </summary>
            public Vector3 rotation;
            [SerializeField]
            /// <summary>
            /// The scale of the object (from transform.localScale)
            /// </summary>
            public Vector3 scale;
        }
    }
}
