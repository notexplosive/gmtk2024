using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Audio;

namespace GMTK24;

public class MusicPlayer
{
    private List<SoundEffectInstance> _tracks = new();

    public float[] Volumes()
    {
        return _tracks.Select(a=>a.Volume).ToArray();
    }
    
    public void Start()
    {
        _tracks = new List<SoundEffectInstance>()
        {
            ResourceAssets.Instance.SoundInstances["sounds/music_zoomin"],
            ResourceAssets.Instance.SoundInstances["sounds/music_neutral"],
            ResourceAssets.Instance.SoundInstances["sounds/music_zoomout"]
        };

        _tracks.ForEach(instance => instance.IsLooped = true);
        _tracks.ForEach(instance => instance.Volume = 0);
        _tracks.ForEach(instance => instance.Play());
    }

    public void Update(float percent)
    {
        if (percent > 0.5f)
        {
            var subPercent = (percent - 0.5f)*2f;
            _tracks[0].Volume = 0f;
            _tracks[1].Volume = 1-subPercent;
            _tracks[2].Volume = subPercent;
        }

        if (percent < 0.5f)
        {
            var quieter = percent * 2f;
            var louder = 1-quieter;
            _tracks[0].Volume = louder;
            _tracks[1].Volume = quieter;
            _tracks[2].Volume = 0;
        }
    }
}
