using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ReaderWriter : MonoBehaviour
{
    public static void WriteSong(List<Note> Notes, string name, string path = @"\Assets\Songs\")
    {
        path = System.Environment.CurrentDirectory + path;

        File.Delete(path + name);

        using (StreamWriter writer = new StreamWriter( File.Open(path + name, FileMode.CreateNew) ) )
        {
            foreach (Note note in Notes)
            {
                writer.WriteLine(note.name + "," + note.duration + "," + note.yOffset + "," + note.actualTime);
            }
        }
    }
}
