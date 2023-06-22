using System;
using System.IO;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public static class SavCsv
{
    public static bool Save(string filename, List<List<float>> data) 
    {
		if (!filename.ToLower().EndsWith(".csv")) {
			filename += ".csv";
		}

		var filepath = Path.Combine(Application.persistentDataPath, filename);

		Debug.Log(filepath);

		// Make sure directory exists if user is saving to sub dir.
		Directory.CreateDirectory(Path.GetDirectoryName(filepath));

        File.WriteAllLines(filepath, data.Select(line => string.Join(",", line.ConvertAll<string>(x => x.ToString()).ToArray())));

		return true; // TODO: return false if there's a failure saving the file
	}

}