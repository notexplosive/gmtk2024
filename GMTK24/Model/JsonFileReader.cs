using System;
using System.IO;
using ExplogineCore;
using ExplogineMonoGame;
using GMTK24.Config;
using Newtonsoft.Json;

namespace GMTK24.Model;

public static class JsonFileReader
{
    public static StructurePlan ReadPlan(string planFileName)
    {
        var planFiles = Client.Debug.RepoFileSystem.GetDirectory("Resource/plans");
        return Read<StructurePlan>(planFiles, planFileName);
    }

    public static T Read<T>(IFileSystem files, string fileName)
    {
        var result = JsonConvert.DeserializeObject<T>(files.ReadFile(fileName + ".json"));

        if (result == null)
        {
            throw new Exception($"Deserialize failed for {Path.Join(files.GetCurrentDirectory(), fileName + ".json")}");
        }

        return result;
    }
    
    public static T ReadOrDefault<T>(IFileSystem files, string fileName) where T : new()
    {
        var result = JsonConvert.DeserializeObject<T>(files.ReadFile(fileName + ".json"));

        if (result == null)
        {
            return new T();
        }

        return result;
    }
}
