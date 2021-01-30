using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class CSVLogger
{
    string filePath;
    int columns;

    // Writes the header of the CSV file using a string array as the header
    public CSVLogger(string[] headerArray, string path)
    {
        filePath = path;
        columns = headerArray.Length;
        var header = String.Join(",", headerArray) + Environment.NewLine;
        File.WriteAllText(filePath, header);
    }

    // Writes a line to the CSV file
    public void Write(string[] row)
    {
        if(row.Length == columns)
        {
            File.AppendAllText(filePath, String.Join(",", row) + Environment.NewLine);
        } else
        {
            throw new Exception("Incorrect number of columns");
        }
    }
}
