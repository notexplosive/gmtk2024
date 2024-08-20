using System;
using System.Collections.Generic;
using System.Linq;
using ExplogineMonoGame;
using ExTween;
using GMTK24.Model;
using Microsoft.Xna.Framework.Audio;

namespace GMTK24;

public class MusicPlayer
{
    private List<SoundEffectInstance> _tracks = new();
    private SequenceTween _tween = new();
    public TweenableFloat MainGameMusicFader { get; } = new(0);

    public float[] Volumes()
    {
        return _tracks.Select(a=>a.Volume).ToArray();
    }
    
    public void Startup()
    {
        _tracks = new List<SoundEffectInstance>()
        {
            ResourceAssets.Instance.SoundInstances["sounds/music_zoomin"], // 0
            ResourceAssets.Instance.SoundInstances["sounds/music_neutral"], // 1
            ResourceAssets.Instance.SoundInstances["sounds/music_zoomout"], // 2
            ResourceAssets.Instance.SoundInstances["sounds/ambient_birds"], // 3
            ResourceAssets.Instance.SoundInstances["sounds/ambient_ocean"], // 4
        };

        _tracks.ForEach(instance => instance.IsLooped = true);
        _tracks.ForEach(instance => instance.Volume = 0);
        _tracks.ForEach(instance => instance.Play());
    }

    public void Update(float dt, float zoomPercent, Dictionary<string, float> ambientPercentages)
    {
        _tween.Update(dt);

        if (_tween.IsDone())
        {
            _tween.Clear();
        }
        
        if (zoomPercent > 0.5f)
        {
            var louderZoomedOut = (zoomPercent - 0.5f)*2f;
            var quieterZoomedOut = 1-louderZoomedOut;
            
            _tracks[0].Volume = 0f;
            _tracks[1].Volume = quieterZoomedOut * MainGameMusicFader;
            _tracks[2].Volume = louderZoomedOut * MainGameMusicFader;
        }
        
        // ocean
        _tracks[4].Volume = zoomPercent * 0.3f;

        if (zoomPercent < 0.5f)
        {
            var quieterZoomedIn = zoomPercent * 2f;
            var louderZoomedIn = 1-quieterZoomedIn;
            
            _tracks[0].Volume = louderZoomedIn * MainGameMusicFader;
            _tracks[1].Volume = quieterZoomedIn * MainGameMusicFader;
            _tracks[2].Volume = 0;
            
            // birds
            _tracks[3].Volume = quieterZoomedIn;

            foreach (var (ambientSound, percentage) in ambientPercentages)
            {
                var sound = ResourceAssets.Instance.SoundInstances["sounds/"+ambientSound];
                if (sound.State == SoundState.Stopped && percentage != 0)
                {
                    sound.Play();
                }

                if (sound.State == SoundState.Playing && percentage == 0)
                {
                    sound.Stop();
                }
                    
                sound.Volume = Math.Clamp(percentage * 2, 0,1);
            }
        }
    }

    public void FadeIn()
    {
        _tween.Add(MainGameMusicFader.TweenTo(1f, 3f, Ease.QuadSlowFast));
    }

    public void FadeOut()
    {
        _tween.Add(MainGameMusicFader.TweenTo(0f, 3f, Ease.QuadSlowFast));
    }
    
    public void FadeToVolume(float percent)
    {
        _tween.Add(MainGameMusicFader.TweenTo(percent, 1f, Ease.QuadSlowFast));
    }
}
