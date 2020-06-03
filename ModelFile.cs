using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;


namespace UtapriLive2DFileBuilder
{
    //based on Perfare's CubismModel3Json class
    public class ModelFile
    {
        public int Version = 3;
        public FileReference FileReferences;
        public Group[] Groups { get; set; }
    }

    
    public class FileReference
    {
        public string Moc { get; set; }
        public string[] Textures { get; set; }
        public string Physics { get; set; }
        public JObject Motions { get; set; }

    }

    public class Group
    {
        public string Target;
        public string Name;
        public string[] Ids;
    }

    public class ModelData
    {
        public ModelFile CreateJsonFile(string fileName, string textureName)
        {
            ModelFile ModelObj = new ModelFile();

            FileReference fileRef = new FileReference();


            ModelObj.FileReferences = fileRef;

            fileRef.Moc = fileName + ".moc3";

            fileRef.Textures = new[] { "textures/" + textureName };

            fileRef.Physics = "";

            var groups = new List<Group>(); //empty no groups for now.
            var motions = new List<string>(); //empty no motions for now.

            var jObject = new JObject();
            var jarray = new JArray();
            foreach (var motion in motions)
            {
                var tempjob = new JObject();
                tempjob["File"] = motion;
                jarray.Add(tempjob);
            }
            jObject[""] = jarray;

            fileRef.Motions = jObject;

            ModelObj.Groups = groups.ToArray();

            return ModelObj;
        }

    }
}
