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
    public partial class connectionManagers:Node2D
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
            
            CustomSignals.GetInstance().StartRecherche += startConnection;
        }


        private void startConnection()
        {
            DisconnectedAll();
            RechercheGenerateur();
            
        }

        private void DisconnectedAll()
        {
            foreach (var box in boxTeslasList)
            {
                box.LineDeconnection();
            }
            TeslasConnected.Clear();
        }


        private void RechercheGenerateur()
        {
            BoxTesla lBox = null;
            foreach (var Generator in generatorList)
            {
               lBox=Recherche(Generator);
               if (lBox!= null)
               {
                   lBox.LineConnection(Generator);
                   TeslasConnected.Add(lBox);
                   RechercheTesla();
               }
            }

        }

    private void RechercheTesla()
    {
         int timer = 0;
        BoxTesla lBox = Recherche(TeslasConnected.Last());
        while (lBox!=null )
        {
            lBox.LineConnection(TeslasConnected.Last());
            TeslasConnected.Add(lBox);

            lBox = Recherche(TeslasConnected.Last());
        }
        CustomSignals.GetInstance().EmitSignal(CustomSignals.SignalName.BoxTeslaCalculsDone);
        
    }
        

        private BoxTesla Recherche(GameObject pObject)
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

            pritlist();
            return lShortBox;
        }


        private void pritlist()
        {
            foreach (var VARIABLE in TeslasConnected)
            {
                GD.Print("je suis"+ VARIABLE.energize);
            }
        }

    }
}
