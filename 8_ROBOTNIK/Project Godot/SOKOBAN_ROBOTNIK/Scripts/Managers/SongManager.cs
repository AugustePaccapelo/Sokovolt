using System;
using System.Collections.Generic;
using System.Linq;
using Com.IsartDigital.SokoVolt;
using Com.IsartDigital.SokoVolt.Managers;
using Godot;
using static EnumSong;
// Author : Soukai William


namespace RobotnikSokoban.Scripts.Managers;
/// <summary>
/// Manages all ambient songs in the game, including playback, crossfade transitions, and slow-motion FX.
/// Uses a singleton pattern to ensure a single instance.
/// </summary>
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

    /// <summary>
    /// Plays a random song from a provided list, excluding the last played one.
    /// </summary>
    /// <param name="pAllowedSongs">List of songs allowed to be played.</param>
    /// <param name="pLastPlayed">The last song that was played (will be excluded).</param>
    /// <param name="pDict">The dictionary of songs to search in.</param>
    /// <returns>The AudioStreamPlayer that was played, or null if none were valid.</returns>
    public AudioStreamPlayer PlayRandomInListExcept(List<AmbientSong> pAllowedSongs, AmbientSong pLastPlayed, Dictionary<AmbientSong, AudioStreamPlayer> pDict)
    {
        var filteredList = pAllowedSongs.Where(m => m != pLastPlayed).ToList();

        if (filteredList.Count == 0)
        {
            return null;
        }


        int randomIndex = Utils.Random.RandiRange(0, filteredList.Count - 1);
        AmbientSong selected = filteredList[randomIndex];

        if (pDict.ContainsKey(selected))
        {
            pDict[selected].Play();
            return pDict[selected];
        }

        return null;
    }


    /// <summary>
    /// Useful for slow-motion effects.
    /// Smoothly resets the pitch and volume of a song back to normal over time.
    /// </summary>
    /// <param name="pMusiqueType">The song to reset.</param>
    /// <param name="pDict">The song dictionary.</param>
    /// <param name="pDuration">Time in seconds for the reset to complete.</param>
    public async void ResetSlowMoFX(AmbientSong pMusiqueType, Dictionary<AmbientSong, AudioStreamPlayer> pDict, float pDuration = 1.5f)
    {
        if (!pDict.ContainsKey(pMusiqueType)) return;

        AudioStreamPlayer lPlayer = pDict[pMusiqueType];

        float lStartPitch = lPlayer.PitchScale;
        float lStartVolumeDb = lPlayer.VolumeDb;

        float lTime = 0f;

        while (lTime < pDuration)
        {
            lTime += Engine.GetProcessFrames();
            float lT = lTime / pDuration;

            lPlayer.PitchScale = Mathf.Lerp(lStartPitch, 1f, lT);
            lPlayer.VolumeDb = Mathf.Lerp(lStartVolumeDb, 0f, lT);

            await ToSignal(GetTree(), "process_frame");
        }

        lPlayer.PitchScale = 1f;
        lPlayer.VolumeDb = 0f;

        GD.Print("Reset SlowMo terminé !");
    }

    /// <summary>
    /// Fades out one song and fades in another over a set pDuration.
    /// </summary>
    /// <param name="pFromKey">The currently playing song to fade out.</param>
    /// <param name="pToKey">The target song to fade in.</param>
    /// <param name="pDuration">The pDuration of the crossfade in seconds.</param>
    public void Crossfade(AmbientSong pFromKey, AmbientSong pToKey,float pDuration = 2f)
    {
        if (!ambientDict.ContainsKey(pFromKey) || !ambientDict.ContainsKey(pToKey))
        {
            GD.PrintErr("One of the specified songs doesn't exist in the dictionary!");
            return;
        }

        AudioStreamPlayer lFrom = ambientDict[pFromKey];
        AudioStreamPlayer lTo = ambientDict[pToKey];

        Tween lTween = GetTree().CreateTween();

        lTo.VolumeDb = -30f;
        lTo.Play();

        lTween.TweenProperty(lFrom, "volume_db", -30f, pDuration); 
        lTween.TweenProperty(lTo, "volume_db", 0f, pDuration); 

        lTween.TweenCallback(Callable.From(() =>
        {
            lFrom.Stop();
            lFrom.VolumeDb = 0f; 
        }));
    }

}
