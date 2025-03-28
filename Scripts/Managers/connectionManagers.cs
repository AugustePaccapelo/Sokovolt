using Com.IsartDigital.SokoVolt.GameObjects.Movables;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Com.IsartDigital.SokoVolt.GameObjects;
using Godot.Collections;


// Author : Soukai William
namespace Com.IsartDigital.SokoVolt.GameObjects
{
    public partial class ConnectionManagers:Node2D
    {
        public static List<BoxTesla> boxTeslasList = new List<BoxTesla>();
        public static List<Generator> generatorList = new List<Generator>();
        public  List<BoxTesla> TeslasConnected = new List<BoxTesla>();


        public override void _Ready()
        {

            Init();
        }

        private void Init()
        {
            CustomSignals.GetInstance().UnLoadLevel+= clearTeslas;
            CustomSignals.GetInstance().StartRecherche += StartConnection;
        }


        private void StartConnection()
        {
            DisconnectedAll();
            SearchGenerator();

        }

        private void DisconnectedAll()
        {
            TeslasConnected.ForEach(lBox => lBox.LineDeconnection());
            TeslasConnected.Clear();
        }

        private void clearTeslas()
        {
            boxTeslasList.Clear();
        }

        private void SearchGenerator()
        {
            foreach (Generator lGenerator in generatorList)
            {
                BoxTesla lBox = Search(lGenerator);
                if (lBox != null && !TeslasConnected.Contains(lBox))
                {
                    lBox.LineConnection(lGenerator);
                    TeslasConnected.Add(lBox);
                }
            }
            SearchTesla();

        }

    private void SearchTesla()
    {
        if (TeslasConnected.Count == 0)return;
        while (true)
        {
            BoxTesla lBox = Search(TeslasConnected.Last());
            if (lBox == null) break;

            lBox.LineConnection(TeslasConnected.Last());
            TeslasConnected.Add(lBox);
        }

        CustomSignals.GetInstance()?.EmitSignal(CustomSignals.SignalName.BoxTeslaCalculsDone);

    }


        private BoxTesla Search(GameObject pObject)
        {

            float lLength;
            float lShortLength = Single.MaxValue;
            BoxTesla lShortBox = null;
            foreach (var box in boxTeslasList)
            {
                if (TeslasConnected.Contains(box)) continue;
                lLength=box.ConnectionSearch(pObject);
                if (lLength !=-1 && lLength<lShortLength)
                {

                    lShortLength = lLength;
                    lShortBox = box;
                }

            }

            return lShortBox;
        }




    }
}
