using Monocle;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using Celeste.Mod.Mia.UtilsClass;
using Celeste.Mod.Mia.Settings;

using Celeste.Mod.Mia.InputAdder;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;

using NumSharp;

namespace Celeste.Mod.Mia
{

    public class Main : EverestModule
    {
        public static Main Instance;
        public override Type SettingsType => typeof(SettingsClass);
        public static SettingsClass Settings => (SettingsClass)Instance._Settings;
        public Main()
        {
            if (!Directory.Exists("Mia")) Directory.CreateDirectory("Mia");
            if (!Directory.Exists("Mia/Saves")) Directory.CreateDirectory("Mia/Saves");
            Instance = this;
            savedNumber = Directory.GetFiles("Mia/Saves").Length/2;
        }
        public static int savedNumber;
        private static bool doPlay = false;
        private static int planeIndex = 0;
        private static NDArray threedimarray = np.zeros(10000, 20, 20);
        private static NDArray twodimarray = np.zeros(10000, 7);
        private static bool record;
        int index = 0;
        bool[] movements = new bool[7];
        public int score = 0;
        public int[] b;
        public static string batchTrainPath = @"Mods\\Mia-master\\Code\\Module\\train.bat";
        public static string batchPlayPath = @"Mods\\Mia-master\\Code\\Module\\play.bat";
        public static ProcessStartInfo startInfo = new ProcessStartInfo("cmd.exe", $"/c \"{batchPlayPath}\"")
        {
            CreateNoWindow = true, // change this settings if you want the cmd linked to the python code to appear when making mia play
            UseShellExecute = false,
        };

        [Command("play", "Make Mia Play")]
        public static void PlayCommand()
        {
            if (!doPlay){
                np.Save(new int[54], "Mia/output.npy");
                np.Save(new int[] {1}, "Mia/bool1.npy");
                np.Save(new bool[] {true}, "Mia/bool2.npy");
                Process.Start(startInfo);
                record = false;
            }
            doPlay = !doPlay;
            Engine.Commands.Log($"Mia should start to play (except if the first prediction is to do nothing)");
        }

        [Command("record", "Record Command")]
        public static void RecordCommand()
        {   
            if (record)
            {
                SaveFiles();
            }
            record = !record;
            if (record) doPlay = false;
            Engine.Commands.Log($"Recording set to {record}");
        }

        [Command("train", "To train the neural network")]
        public static void TrainCommand()
        {   
            Process.Start("cmd.exe", $"/c \"{batchTrainPath}\"");
            Engine.Commands.Log("A cmd has been opened,");
            Engine.Commands.Log("you can now interact with it to create and train neural networks");
        }

        public override void Load()
        {
            On.Celeste.Player.Update += ModPlayerUpdate;
            Everest.Events.Level.OnLoadLevel += LoadLevel;
            Everest.Events.Level.OnExit += ExitLevel;
        }

        public override void Unload()
        {
            Utils.Print("Mod unloaded");
            On.Celeste.Player.Update -= ModPlayerUpdate;
            Everest.Events.Level.OnLoadLevel -= LoadLevel;
            if (record)
            {
                SaveFiles();
            }
            Everest.Events.Level.OnExit -= ExitLevel;
        }

        public static void SaveFiles(){
            np.Save((Array)threedimarray[$":{planeIndex},:,:"], $"Mia/Saves/ArraySaved_{savedNumber}.npy");
            np.Save((Array)twodimarray[$":{planeIndex},:,:"], $"Mia/Saves/InputSaved_{savedNumber}.npy");
        }
        private void ExitLevel(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow)
        {
            if(record) 
            {
                record = false;
                SaveFiles();
            }
        }

        private void LoadLevel(Level level, Player.IntroTypes playerIntro, bool isFromLoader) 
        {
            if (isFromLoader && !Settings.GetTiles)
            {
                Utils.Print("Tiles won't be retreived. Change the option in the settings to make able to retrieve level, Mia will not work otherwise.");
            }
            // there are issues with screen transition, so for now we will train the IA only on map where there is only a single screen
            // if (level.Session.Area.ID < mainRoomsPerChapter.Count)
            // {
            //     if (mainRoomsPerChapter[level.Session.Area.ID].IndexOf(level.Session.LevelData.Name) != -1)
            //     {
            //         score++;
            //     }
            // }
        }
        private void ModPlayerUpdate(On.Celeste.Player.orig_Update orig, Player self)
        {
            orig(self);
            GameWindow window = Engine.Instance.Window;
            if (Engine.Scene is Level level)
            {
                if (record && self.Position != self.PreviousPosition) 
                {   
                    threedimarray[planeIndex] = TileManager.TileManager.FusedArrays(level, level.SolidsData.ToArray(), self);
                    twodimarray[planeIndex] = Utils.GetInputs();
                    ++planeIndex;
                    if (planeIndex == 10000)
                    {
                        Console.WriteLine("Creating New File");
                        SaveFiles();
                        savedNumber++;
                        planeIndex = 0;
                        threedimarray = np.zeros(10000, 20, 20);
                        twodimarray = np.zeros(10000, 7);
                    }
                }
                if (doPlay) { 
                    try{
                        b = np.load("Mia/bool1.npy").ToArray<int>();
                    }
                    catch(System.IO.IOException){}
                    if (b[0] == 1){
                        var input = TileManager.TileManager.FusedArrays(level, level.SolidsData.ToArray(), self);
                        np.Save(input,"Mia/input.npy");
                        np.Save(new int[] {3},"Mia/bool1.npy");
                    }
                    else if (b[0] == 2){
                        var output = np.load("Mia/output.npy");
                        movements = Utils.GetWhatThingToMove(output);
                        np.Save(new int[] {1},"Mia/bool1.npy");
                    }  
                    Inputting.MoveAsync(movements); 
                }
                if (MInput.Keyboard.Check(Keys.A))
                {
                    doPlay = false;
                    np.Save(new bool[] {false}, "Mia/bool2.npy");
                }
                if (Settings.GetTiles) window.Title = "Celeste.exe/Mia enabled";
                else window.Title = "Celeste.exe/Mia not enabled";
            }
        }
    }
}