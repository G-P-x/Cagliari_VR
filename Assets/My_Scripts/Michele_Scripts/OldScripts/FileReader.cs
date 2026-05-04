//using System.IO;
//using System.Collections.Generic;
//using DataSpace;


//public class FileReader
//{
//    //l'idea  che ogni giorno venga creato un nuovo file corrispondente al gps (varco) che verr� nominato con il nome di quest'ultimo, seguito dalla data giornaliera,
//    //quindi ad esempio: bn220_270923
//    //In questo modo sar� possibile estrarre i dati relativi ad ogni giorno
//    //Il file da leggere conterr� 24 righe.
//    //Ciascuna riga dovr� corrispondere ad una registrazione dei dati di ogni ora, composta da: latitudine, longitudine, persone registrate in quell'ora.
//    //La prima riga corrisponder� a mezzanotte, la seconda a l'una del mattino e cos� via fino alle 23 di sera
    
//    public List<GapData> dataList = new();
//    private readonly string folderPath = @"C:\Users\giopa\OneDrive\Desktop\DATI";


//    public List<GapData> ReadFiles()
//    {
//        string[] fileEntries = Directory.GetFiles(folderPath, "*.txt");


//        foreach (string file in fileEntries)
//        {
//            string result = File.ReadAllText(file);
//            string[] file_info = Path.GetFileNameWithoutExtension(file).Split("_");
//            string name = file_info[0];
//            string date = file_info[1];

//            string[] results_array = result.Trim().Split('\n');

//            for (int i = 0; i < results_array.Length; i++)
//            {
//                string[] riga = results_array[i].Split('-');
//                //string time = riga[0];
//                string time_slot = i.ToString() + ":" + (i + 1).ToString(); //restituir� ad esempio 0:1 oppure 16:17
//                double latitude = double.Parse(riga[1].Replace(".", ","));
//                double longitude = double.Parse(riga[2].Replace(".", ","));
//                int people = int.Parse(riga[3]);

//                GapData data_gap = new(name, date, time_slot, latitude, longitude, people); // istanzio oggetto database

//                dataList.Add(data_gap);
//            }
//        }

//        return dataList;

//    }
//}
