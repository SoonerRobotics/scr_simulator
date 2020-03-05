using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ConfigFile
{
    [JsonIgnore]
    protected string fileName;
    [JsonIgnore]
    protected string configFolderPath = "";

    public ConfigFile(string file)
    {
        configFolderPath = $"{Application.persistentDataPath}/Config/";
        if (!Directory.Exists(configFolderPath))
        {
            Directory.CreateDirectory(configFolderPath);
        }
        fileName = file + ".json";
    }

    /// <summary>
    /// Populates the child with existing content
    /// </summary>
    protected void Populate(object child)
    {
        string path = configFolderPath + fileName;
        if (!File.Exists(path))
        {
            File.WriteAllText(path, "");
        }
        string content = File.ReadAllText(path);

        if (content != string.Empty)
            JsonConvert.PopulateObject(content, child);
    }

    protected bool Save(object child)
    {
        string path = configFolderPath + fileName;

        if (File.Exists(path))
        {
            string serializedContent = JsonConvert.SerializeObject(child, Formatting.Indented);
            File.WriteAllText(path, serializedContent);
            return true;
        }
        else
        {
            return false;
        }
    }
}
