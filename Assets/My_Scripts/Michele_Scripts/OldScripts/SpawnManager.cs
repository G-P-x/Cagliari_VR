//using ProjNet.CoordinateSystems;
//using ProjNet.CoordinateSystems.Transformations;
//using UnityEngine;
//using TMPro;


//public class SpawnManager: MonoBehaviour
//{
//    [SerializeField] GameObject gapObj;
//    [SerializeField] GameObject caglariCityCentroBastione;
//    private readonly DataRequest dataRequest = new();

//    // riferimento
//    readonly double latitudeBastione = 39.2161; // Latitudine bastione
//    readonly double longitudeBastione = 9.1165; // Longitudine bastione

//    Vector3 reference_position = new();


//    void Start()
//    {
//         reference_position = Converter(latitudeBastione, longitudeBastione);
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
//        return new Vector3(utmNorth, caglariCityCentroBastione.transform.position.y, -utmEast);

//    }


//    public void SpawnGap(TextMeshPro text)
//    {      
//        string gap_name = text.text.Trim();
//        double[] coordinates = dataRequest.Get_GapCoordinates(gap_name);
//        Debug.Log($"ATTENZIONE: {coordinates[0]}--{coordinates[1]}");
//        Vector3 position = Converter(coordinates[0], coordinates[1]) - reference_position;
//        //GameObject obj = new GameObject();
//        GameObject obj = Instantiate(gapObj, caglariCityCentroBastione.transform, false);
//        obj.transform.localPosition = position;
//        obj.name = text.text;

//    }


//}
