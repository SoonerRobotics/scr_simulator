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
        if(!Directory.Exists(SavePath))
        {
            Directory.CreateDirectory(SavePath);
        }

        Prefabs = Resources.LoadAll<PrefabScriptableObject>("Map Editor").ToList();
        foreach(var prefab in Prefabs)
        {
            Debug.Log($"Loaded Custom Prefab -> {prefab.Name}:{prefab.Identifier}");
        }

        var maps = CustomMap.GetStoredMaps();
        activeMap = maps[0];

        if(!activeMap.mapName.Equals(string.Empty)) // This means we have a map!
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
            File.WriteAllText($"{SavePath}/{mapName}.json", JsonConvert.SerializeObject(this, Formatting.Indented)); // To save file space, change this back to regular formatting.

            Debug.Log("Saving to: " + SavePath + "/" + fileName);
        }

        public void UpdateObject(GameObject obj)
        {
            if (obj == null)
            {
                return;
            }

            ObjectInfo info = obj.GetComponent<ObjectInfo>();
            if (info == null)
                return;

            var entity = mapObjects.Where(y => y.uniqueIdentifier.Equals(info.uniqueIdentifier)).FirstOrDefault();
            if (entity == null)
                return;

            entity.position = new CustomMapObject.CustomMapVector3(obj.transform.position);
            entity.rotation = new CustomMapObject.CustomMapVector3(obj.transform.rotation.eulerAngles);
            entity.scale = new CustomMapObject.CustomMapVector3(obj.transform.localScale);
        }

        private void RemoveObject(GameObject obj, ObjectInfo info)
        {
            var entity = mapObjects.Where(y => y.uniqueIdentifier.Equals(info.uniqueIdentifier)).FirstOrDefault();
            if (entity == null)
                return;

            Destroy(obj);
            mapObjects.Remove(entity);
        }

        public void AddObject(GameObject obj, ObjectInfo info)
        {
            CustomMapObject newObj = new CustomMapObject();
            newObj.position = new CustomMapObject.CustomMapVector3(obj.transform.position);
            newObj.rotation = new CustomMapObject.CustomMapVector3(obj.transform.rotation.eulerAngles);
            newObj.scale = new CustomMapObject.CustomMapVector3(obj.transform.localScale);
            newObj.prefabIdentifier = info.prefab.Identifier;
            newObj.uniqueIdentifier = StringUtilities.RandomString(16);
            info.uniqueIdentifier = newObj.uniqueIdentifier;

            Debug.Log("Adding Object: " + JsonConvert.SerializeObject(newObj));
            mapObjects.Add(newObj);
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
            /// <summary>
            /// A unique identifier tied to the object used for various tasks.
            /// Dont touch this either, its randomly generated
            /// </summary>
            public string uniqueIdentifier;
            [SerializeField]
            /// <summary>
            /// The position of the object (from transform.position)
            /// </summary>
            public CustomMapVector3 position;
            [SerializeField]
            /// <summary>
            /// The rotation of the object (from transform.rotation)
            /// </summary>
            public CustomMapVector3 rotation;
            [SerializeField]
            /// <summary>
            /// The scale of the object (from transform.localScale)
            /// </summary>
            public CustomMapVector3 scale;

            [Serializable]
            public class CustomMapVector3
            {
                public float x, y, z;
                
                public Vector3 GetVector3()
                {
                    return new Vector3(x, y, z);
                }

                public CustomMapVector3(Vector3 vec)
                {
                    x = vec.x;
                    y = vec.y;
                    z = vec.z;
                }
            }
        }
    }
}
