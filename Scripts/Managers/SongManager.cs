using System;
using System.Collections.Generic;
using System.Linq;
using Com.IsartDigital.SokoVolt;
using Com.IsartDigital.SokoVolt.Managers;
using Godot;
using static EnumSong;
// Author : Soukai William
namespace RobotnikSokoban.Scripts.Managers;

public partial class SongManager : Manager
{ 
    public static SongManager Instance { get; private set; }
    public Dictionary<AmbientSong, AudioStreamPlayer> ambientDict = new();
    public override void _Ready()
    {
        if (Instance != null) {
            Free();
            GD.Print($"{nameof(SongManager)} Instance already exist, destroying the last added.");
            return;
        }
        Instance = this;
        
        Utils.Random.Randomize();
        base._Ready();
    }



    public override void Init()
    {
        int i = 0;
        foreach (AudioStreamPlayer songAudioStream in GetChildren())
        {
            if (i < Enum.GetValues(typeof(AmbientSong)).Length)
            {
                ambientDict[(AmbientSong)i] = songAudioStream;
            }
            i++;
        }
    }

    private AudioStreamPlayer PlayRandomInListExcept(List<AmbientSong> allowedSongs, AmbientSong lastPlayed, Dictionary<AmbientSong, AudioStreamPlayer> pDict)
    {
        var filteredList = allowedSongs.Where(m => m != lastPlayed).ToList();

        if (filteredList.Count == 0)
        {
            GD.Print("Pas d'autres musiques disponibles sauf la dernière jouée.");
            return null;
        }


        int randomIndex = Utils.Random.RandiRange(0, filteredList.Count - 1);
        AmbientSong selected = filteredList[randomIndex];

        if (pDict.ContainsKey(selected))
        {
            pDict[selected].Play();
            GD.Print("Je joue : " + selected);
            return pDict[selected];
        }

        GD.Print("La musique " + selected + " n'est pas dans le dictionnaire.");
        return null;
    }




    public async void ResetSlowMoFX(AmbientSong musiqueType, Dictionary<AmbientSong, AudioStreamPlayer> pDict, float duration = 1.5f)
    {
        if (!pDict.ContainsKey(musiqueType)) return;

        AudioStreamPlayer player = pDict[musiqueType];

        float startPitch = player.PitchScale;
        float startVolumeDb = player.VolumeDb;

        float time = 0f;

        while (time < duration)
        {
            time += Engine.GetProcessFrames();
            float t = time / duration;

            player.PitchScale = Mathf.Lerp(startPitch, 1f, t);
            player.VolumeDb = Mathf.Lerp(startVolumeDb, 0f, t);

            await ToSignal(GetTree(), "process_frame");
        }

        player.PitchScale = 1f;
        player.VolumeDb = 0f;

        GD.Print("Reset SlowMo terminé !");
    }


    public void Crossfade(AmbientSong fromKey, AmbientSong toKey,float duration = 2f)
    {
        if (!ambientDict.ContainsKey(fromKey) || !ambientDict.ContainsKey(toKey))
        {
            GD.PrintErr("Une des musiques spécifiées n'existe pas dans le dictionnaire !");
            return;
        }

        AudioStreamPlayer from = ambientDict[fromKey];
        AudioStreamPlayer to = ambientDict[toKey];

        Tween tween = GetTree().CreateTween();

        to.VolumeDb = -30f;
        to.Play();

        tween.TweenProperty(from, "volume_db", -30f, duration); 
        tween.TweenProperty(to, "volume_db", 0f, duration); 

        tween.TweenCallback(Callable.From(() =>
        {
            from.Stop();
            from.VolumeDb = 0f; 
            GD.Print($"Crossfade terminé : {fromKey} ➤ {toKey}");
        }));
    }

}
