using Godot;
using System;
using System.Collections.Generic;

public partial class EnumSong 
{
    public static List<AmbientSong> popList = new List<AmbientSong>() {
        AmbientSong.pop1,
        AmbientSong.pop2,
        AmbientSong.pop3,
        AmbientSong.pop4,
        AmbientSong.pop5
    };
    public enum AmbientSong
    {
        Piece,
        RobloxDeath,
        click,
        spotLight,
        machineBackSound,
        machineBackSound2,
        mysteriousElectricity,
        heater,
        elevatorNoise,
        treadmill,
        arrowButton,
        pop1,
        pop2,
        pop3,
        pop4,
        pop5, 
        AmbianceMenumusic,
        AmbianceGamemusic,
        ButonSong,
        TVsong
    }

}

