using Godot;
using System;
using System.IO;
using FileAccess = Godot.FileAccess;

// Author : Ferlat Thibaud 

namespace Com.IsartDigital.SokoVolt {


	public static class JsonTool 
	{
		static public string ReadFileContents(string pFilePath)
        {
			if(!FileAccess.FileExists(pFilePath))
				return ""; 

			using var lFile = FileAccess.Open(pFilePath, FileAccess.ModeFlags.Read);
    			return lFile.GetAsText();
        }


        static public void WriteToFile(string pFilePath, string pContent)
        {
           if(!FileAccess.FileExists(pFilePath))
				return; 

			using var lFile = FileAccess.Open(pFilePath, FileAccess.ModeFlags.Write); 
			lFile.StoreLine(pContent);
        }

     	public static bool TryParseJson(string pContent, out Godot.Collections.Dictionary pRootDict)
		{
			pRootDict = null;
			var lResult = Json.ParseString(pContent);

			if (lResult.VariantType is Variant.Type.Dictionary)
			{
				pRootDict = (Godot.Collections.Dictionary)lResult;
				return true;
			}

			return false;
		}


        public static string SaveTextToFile(string pPath, string pFileName, string pData)
        {

            if (!Directory.Exists(pPath))
            {
                Directory.CreateDirectory(pPath);
            }

            pPath = Path.Combine(pPath, pFileName);
            if (File.Exists(pPath))
            {
                return pPath;
            }
            try
            {
                File.WriteAllText(pPath, pData);
            }
            catch (Exception lErr)
            {
                GD.PrintErr(lErr);
                throw;
            }
            return pPath;
        }


	}

}
