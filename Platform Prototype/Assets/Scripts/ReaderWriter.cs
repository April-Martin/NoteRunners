using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ReaderWriter : MonoBehaviour
{
    public static void WriteSong(List<Note> Notes, string name, float bpm, string path = @"\Assets\Songs\")
    {
        path = System.Environment.CurrentDirectory + path;

        File.Delete(path + name);

        using (StreamWriter writer = new StreamWriter(File.Open(path + name, FileMode.CreateNew)))
        {
            writer.WriteLine(bpm);
            foreach (Note note in Notes)
            {
                //                writer.WriteLine(note.name + "," + note.duration + "," + note.yOffset + "," + note.actualTime);
                writer.WriteLine(note.name + "," + note.duration);
            }
        }
        return;
    }

    public static void ReadSong(ref List<Note> Song, string name, ref float bpm)
    {
        string path = Application.dataPath + "/Songs/";
        string line = "";
        string[] vals = new string[2];

        using (StreamReader reader = new StreamReader(File.Open( path + name, FileMode.Open)))
        {
            bpm = float.Parse(reader.ReadLine());
            // Discard initial rest
            reader.ReadLine();
            while ((line = reader.ReadLine()) != null)
            {
                vals = line.Split(',');
                Song.Add( new Note(vals[0], float.Parse(vals[1])) );
            }
        }

        return;
    }
}
