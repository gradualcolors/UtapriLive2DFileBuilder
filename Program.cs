using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Linq;

namespace UtapriLive2DFileBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            /*=========================== VARIABLES =========================== */
            //inputs
            string ModelFolderPath;
            string TextureFolderPath;

            //calculated
            string fileName;
            int fileSize;
            int fileCount = 0;

            //byte patterns to search
            byte[] moc_pattern = new byte[] {
                77,
                79,
                67,
                51
               }; //moc3 pattern

            bool[] wildcard = {
                false,
                false,
                false,
                false
               }; //wildcard pattern, set to none

            //outputs
            string outputModelFolder; //folder where mdl*.data files are
            string outputTextureFolder; //folder where texture files are


            Console.WriteLine("Utapri Live2D Model Creator");
            Console.WriteLine("Version: 1.0");
            Console.WriteLine("Author: @gradualcolours");
            Console.WriteLine("");

            /*=========================== INPUT =========================== */

            //input mdl*.dat folder, loop until a valid folderpath is inputted
            do
            {
                Console.WriteLine("Input folder path to mdl*.dat files: ");
                ModelFolderPath = Console.ReadLine();
                Console.WriteLine("");

                if (!Directory.Exists(ModelFolderPath))
                {
                    Console.WriteLine("Folder not found. Press enter to re-input.");
                    Console.WriteLine("");

                }
                else if (Directory.GetFiles(ModelFolderPath, "mdl*.dat").Length == 0)
                {
                    Console.WriteLine("Files not found. Press enter to re-input.");
                    Console.WriteLine("");

                }
            }

            while (!Directory.Exists(ModelFolderPath) ||
             Directory.GetFiles(ModelFolderPath, "mdl*.dat").Length == 0);


            //input texture folder, loop until a valid folderpath is inputted
            do
            {
                Console.WriteLine("Input folder path to texture files: ");
                TextureFolderPath = Console.ReadLine();
                Console.WriteLine("");

                if (!Directory.Exists(TextureFolderPath))
                {
                    Console.WriteLine("Folder not found. Press enter to re-input.");
                    Console.WriteLine("");

                }
                else if (Directory.GetFiles(TextureFolderPath, "tex_*.png").Length == 0)
                {
                    Console.WriteLine("Files not found. Press enter to re-input.");
                    Console.WriteLine("");

                }
            }

            while (!Directory.Exists(TextureFolderPath) ||
             Directory.GetFiles(TextureFolderPath, "tex_*.png").Length == 0);


            /*=========================== CREATE MODEL =========================== */


            foreach (string file in Directory.EnumerateFiles(ModelFolderPath, "mdl*.dat"))
            {
                fileName = Path.GetFileNameWithoutExtension(file); //removes *.dat extension
                byte[] inputFile = File.ReadAllBytes(file); //read file bytes
                int startPosition = FindPattern(inputFile, moc_pattern, wildcard, 0); //find position where moc3 pattern starts
                fileSize = inputFile.Length - startPosition; //get size of output file by subtracting the number of bytes before moc3 pattern starts

                byte[] outputFile = new byte[fileSize]; //initialize output file

                Array.Copy(inputFile, startPosition, outputFile, 0, fileSize); //copy range of bytes starting from new position from inputFile to outputFile


                //old code that read mdl*.txt line by line, thank god we are not using this anymore
                /*foreach (string file in Directory.EnumerateFiles(ModelFolderPath, "mdl*.txt"))

                {
                    List<byte> fileBytes = new List<byte>();

                    foreach (var myString in File.ReadLines(file))
                    {

                        Regex regexName = new Regex("m_Name");
                        Match matchName = regexName.Match(myString);

                        Regex regexSize = new Regex("size");
                        Match matchSize = regexSize.Match(myString);

                        Regex regexByte = new Regex("UInt8 data");
                        Match matchByte = regexByte.Match(myString);

                        if (matchName.Success)

                        {
                            MatchCollection col = Regex.Matches(myString, "\"([^\"]*)\"");

                            // Copy groups to a string array
                            string[] name = new string[col.Count];
                            for (int i = 0; i < name.Length; i++)
                            {
                                name[i] = col[i].Groups[1].Value;
                                fileName = name[0];
                            }
                            Console.WriteLine("Writing model " + fileName + "...");
                        }

                        else if (matchSize.Success)

                        {
                            fileSize = Int32.Parse(Regex.Match(myString, @"\d+").Value);



                        }

                                else if (matchByte.Success)

                                 {


                                     int decValue = Int32.Parse(Regex.Match(myString, @"\b\d+").Value);
                                     string hexString = Convert.ToString(decValue, 16);
                                     fileBytes.Add(Convert.ToByte(hexString, 16));

                                 }

                    }*/



                //outputFile = fileBytes.ToArray(); 

                string[] splitFileName = fileName.Split("_"); //split mdl_[char]_[outfit-id] into 3 parts
                string textureName = "tex_" + splitFileName[1] + "_" + splitFileName[2] + ".png"; //texture file name becomes tex_[char]_[outfit-id].png

                outputModelFolder = Path.Combine(ModelFolderPath, fileName); //output model folder becomes [ModelFolderPath]/mdl_[char]_[outfit-id]
                outputTextureFolder = Path.Combine(outputModelFolder, "textures"); //output texture folder becomes [outputModelFolder]/textures

                Directory.CreateDirectory(outputModelFolder); //create folder at outputModelFolder path
                Directory.CreateDirectory(outputTextureFolder); //create folder at outputTextureFolder path

                File.WriteAllBytes(outputModelFolder + "//" + fileName + ".moc3", outputFile); //write moc3 file to outputModelFolder

                if (File.Exists(Path.Combine(TextureFolderPath, textureName))) //find texture file at input texture folder path
                {
                    File.Copy(Path.Combine(TextureFolderPath, textureName), Path.Combine(outputTextureFolder, textureName), true); //copy texture file to output texture folder
                }
                else
                    Console.WriteLine("Did not find {0}. Texture was not added to folder.", textureName); //if texture file isn't found, write to log

                /*=========================== CREATE MODEL3.JSON =========================== */

                ModelFile model = new ModelFile(); //initilize ModelFile class
                ModelData modelData = new ModelData(); //initialize method
                model = modelData.CreateJsonFile(fileName, textureName); //call method with fileName and textureName as parameters

                string jsonPath = Path.Combine(outputModelFolder, fileName) + ".model3.json"; //create file path to [fileName].model3.json

                using (FileStream fs = File.Open(jsonPath, FileMode.CreateNew)) //create [fileName].model3.json
                using (StreamWriter sw = new StreamWriter(fs))
                using (JsonWriter jw = new JsonTextWriter(sw))
                {
                    jw.Formatting = Formatting.Indented;
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(jw, model);
                }

                Console.WriteLine("Model {0} outputted to {1}", fileName, outputModelFolder);
                fileCount++; //increment file count
            }

            Console.WriteLine("Created {0} models.", fileCount);
        }

        /*=========================== SEARCH BYTE PATTERN =========================== */

        public static int FindPattern(byte[] Body, byte[] Pattern, bool[] Wild, int start = 0)
        {
            int foundIndex = -1;
            bool match = false;

            if (Body.Length > 0 &&
             Pattern.Length > 0 &&
             start <= Body.Length - Pattern.Length && Pattern.Length <= Body.Length)
                for (int index = start; index <= Body.Length - Pattern.Length; index += 4)

                    if (Wild[0] || (Body[index] == Pattern[0]))
                    {
                        match = true;
                        for (int index2 = 1; index2 <= Pattern.Length - 1; index2++)
                        {
                            if (!Wild[index2] &&
                             (Body[index + index2] != Pattern[index2]))
                            {
                                match = false;
                                break;
                            }

                        }

                        if (match)
                        {
                            foundIndex = index;
                            break;
                        }
                    }

            return foundIndex;
        }

    }
}