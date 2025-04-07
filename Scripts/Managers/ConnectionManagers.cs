using Com.IsartDigital.SokoVolt.GameObjects.Movables;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Com.IsartDigital.SokoVolt.Managers;
using Godot.Collections;


// Author : Soukai William
namespace Com.IsartDigital.SokoVolt.GameObjects
{
    /// <summary>
    /// Manages all Tesla box connections and generator logic within the level.
    /// Handles connection propagation, disconnection, and cleanup.
    /// </summary>
    public partial class ConnectionManagers : Manager
    {
        public static List<BoxTesla> boxTeslasList = new List<BoxTesla>();
        public static List<Generator> generatorList = new List<Generator>();
        public  List<BoxTesla> TeslasConnected = new List<BoxTesla>();


        public override void _Ready()
        {
            base._Ready();
        }

        public override void Init()
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
            foreach (BoxTesla lBox in boxTeslasList)
                lBox.LineDeconnection();
            
            TeslasConnected.Clear();
        }

        private void clearTeslas()
        {
            boxTeslasList.Clear();
        }

        // Searches for Tesla boxes that can connect directly to generators.
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
        //Recursively connects Tesla boxes in a chain from already energized ones.
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



    #region Search
     /// <summary>
     /// Searches for the nearest unconnected Tesla box that can be connected to the given object.
     /// </summary>
     /// <param name="pObject">The GameObject (Generator or Tesla) to connect from.</param>
     /// <returns>The nearest connectable BoxTesla, or null if none are found.</returns>
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
        #endregion



    }
}
