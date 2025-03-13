using Com.IsartDigital.SokoVolt.GameObjects.Movables;
using Godot;
using System;
using System.Collections.Generic;
using Com.IsartDigital.SokoVolt.GameObjects;


// Author : Soukai William
namespace Com.IsartDigital.SokoVolt.GameObjects
{
    public partial class connectionManagers:Node2D
    {
        public static List<BoxTesla> boxTeslasList = new List<BoxTesla>();
        public static List<Generator> generatorList = new List<Generator>();
        public  List<BoxTesla> TeslasConnected = new List<BoxTesla>();
        static private connectionManagers instance;
        


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

        }

        private void DisconnectedAll()
        {
            foreach (var box in boxTeslasList)
            {
                box.LineDeconnection();
            }
        }


        private void rechercheGenerateur()
        {
            BoxTesla lTesla;
            float lLength;
            foreach (var Generator in generatorList)
            {
                foreach (var box in boxTeslasList)
                {
                    box.ConnectionSearch(Generator,out lTesla,out lLength );


                }
            }

        }

        private void BoxUpdated()
        {
           
        }

    }
}
