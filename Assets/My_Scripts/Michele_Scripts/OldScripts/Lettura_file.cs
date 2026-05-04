//using ProjNet.CoordinateSystems;
//using ProjNet.CoordinateSystems.Transformations;
//using UnityEngine;
//using System;
//using System.IO;
//using System.Collections;
//using System.Collections.ObjectModel;
//using System.Collections.Generic;


//public class Lettura_file : MonoBehaviour
//{
//    [SerializeField] GameObject provino;

    
//    double latitude_bastione = 39.2161; // Latitudine bastione
//    double longitude_bastione = 9.1165; // Longitudine bastione

//    public Dictionary<string, object> data = new Dictionary<string, object>();

//    void Start()
//    {
//        Vector3 reference_position = Converter(latitude_bastione, longitude_bastione);

//        string folderPath = @"C:\Users\giopa\OneDrive\Desktop\WORK\PROGETTO VARCHI\Raspberry_Python_Files_GPS\gps\Varchi\Dati_Varchi";
//        string[] fileEntries = Directory.GetFiles(folderPath, "*.txt");

//        foreach(string file in fileEntries) 
//        {
//            string passage_name = Path.GetFileNameWithoutExtension(file);

//            string[] content = File.ReadAllText(file).Split('\n'); // legge e separa i dati nel file riga per riga

//            string[] coordinates = content[0].Split('-'); // primo dato gps utile ( time-latitude-longitude )

//            double latitudine = double.Parse(coordinates[1].Replace(".", ","));

//            double longitudine = double.Parse(coordinates[2].Replace(".", ","));

//            Vector3 position = Converter(latitudine, longitudine) - reference_position;

//            GameObject obj = new GameObject();
//            obj = Instantiate(provino, gameObject.transform, false);
//            obj.transform.localPosition = position;
//            obj.name = passage_name;   

//        }

//        /*
//        for (int j = 0; j < contenuto_array.Length; j++)
//        {

//            contenuto_array[j].Trim();

//            string[] temp = contenuto_array[j].Split("-");

//            double latitudine = double.Parse(temp[1].Replace(".", ","));

//            double longitudine = double.Parse(temp[2].Replace(".", ","));

//            Vector3 position = Converter(latitudine, longitudine) - reference_position;

//            GameObject obj = new GameObject();

//            GameObject[] objs = new GameObject[contenuto_array.Length];

//            objs[j] = Instantiate(gapObj, gameObject.transform, false);
//            objs[j].transform.localPosition = position;
//            objs[j].name = $"varco {j}";

//        }
//        */
       

//    }



//    Vector3 Converter(double latitude, double longitude)
//    {

//        // Crea un oggetto CoordinateTransformationFactory per ottenere una trasformazione tra le coordinate
//        CoordinateTransformationFactory ctFactory = new CoordinateTransformationFactory();


//        // Crea la trasformazione tra i sistemi di coordinate
//        CoordinateTransformation transformation = (CoordinateTransformation)ctFactory.CreateFromCoordinateSystems(GeographicCoordinateSystem.WGS84, ProjectedCoordinateSystem.WGS84_UTM(32, true));
//        // CoordinateTransformation transformation = ctFactory.CreateFromCoordinateSystems(GeographicCoordinateSystem.WGS84, ProjectedCoordinateSystem.WGS84_UTM(32, true));



//        // Crea un array per le coordinate di input (latitudine e longitudine)
//        double[] input = new double[] { longitude, latitude };



//        // Effettua la conversione delle coordinate geografiche in coordinate UTM
//        double[] output = transformation.MathTransform.Transform(input);



//        // Ora "output" conterr� le coordinate UTM risultanti (nord, est) relative a New York City
//        float utmEast = (float)output[0];
//        float utmNorth = (float)output[1];

//        // Debug.Log($"est {utmEast} -- nord {utmNorth}");

//        // Nel sistema di riferimento locale della citt�:
//        //      x = NORD
//        //      z = OVEST, poich� gli UTM forniscono EST, il valore assegnato in z deve essere posto col segno negativo ( - )
//        // Debug.Log($"nord: {utmNorth}, est:{utmEast}");
//        return new Vector3(utmNorth, gameObject.transform.position.y, -utmEast);

//    } 


//}
