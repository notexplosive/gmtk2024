using System;
using System.Collections.Generic;
using System.IO;
using ExplogineCore;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace GMTK24;

public class ResourceAssets
{
    private static ResourceAssets? instanceImpl;
    private readonly Dictionary<string, Canvas> _dynamicTextures = new();
    public Dictionary<string, Texture2D> Textures { get; } = new();
    public Dictionary<string, SpriteSheet> Sheets { get; } = new();
    public Dictionary<string, SoundEffectInstance> SoundInstances { get; set; } = new();
    public Dictionary<string, SoundEffect> SoundEffects { get; set; } = new();

    public static ResourceAssets Instance
    {
        get
        {
            if (instanceImpl == null)
            {
                instanceImpl = new ResourceAssets();
            }

            return instanceImpl;
        }
    }

    public IEnumerable<ILoadEvent> LoadEvents(Painter painter)
    {
        var resourceFiles = Client.Debug.RepoFileSystem.GetDirectory("Resource");

        yield return new VoidLoadEvent("Sound", () =>
        {
            foreach (var path in resourceFiles.GetFilesAt(".", "ogg"))
            {
                AddSound(resourceFiles, path.RemoveFileExtension());
            }
        });

        yield return new VoidLoadEvent("Textures", () =>
        {
            foreach (var path in resourceFiles.GetFilesAt(".", "png"))
            {
                var imageName = path.RemoveFileExtension();
                var texture = Texture2D.FromFile(Client.Graphics.Device,
                    Path.Join(resourceFiles.GetCurrentDirectory(), path));
                Textures.Add(imageName, texture);
            }
        });
    }

    public void Unload()
    {
        Unload(_dynamicTextures);
        Unload(SoundEffects);
        Unload(SoundInstances);
        Unload(Textures);
    }

    public void AddSound(IFileSystem resourceFiles, string path)
    {
        var vorbis = ReadOgg.ReadVorbis(Path.Join(resourceFiles.GetCurrentDirectory(), path + ".ogg"));
        var soundEffect = ReadOgg.ReadSoundEffect(vorbis);
        SoundInstances[path] = soundEffect.CreateInstance();
        SoundEffects[path] = soundEffect;
    }

    public void PlaySound(string key, SoundEffectSettings settings)
    {
        if (SoundInstances.TryGetValue(key, out var sound))
        {
            if (settings.Cached)
            {
                sound.Stop();
            }

            sound.Pan = settings.Pan;
            sound.Pitch = settings.Pitch;
            sound.Volume = settings.Volume;
            sound.IsLooped = settings.Loop;

            sound.Play();
        }
        else
        {
            Client.Debug.LogWarning($"Could not find sound `{key}`");
        }
    }

    private void Unload<T>(Dictionary<string, T> dictionary) where T : IDisposable
    {
        foreach (var sound in dictionary.Values)
        {
            sound.Dispose();
        }

        dictionary.Clear();
    }

    public static void Reset()
    {
        Instance.Unload();
        instanceImpl = null;
    }
}