using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UserInput;
using UnityEngine.SceneManagement;



public class SpawnManager : MonoBehaviour
{
    public GameObject varcoObj;
    public GameObject city;
    public GameObject piazzaYenne_ref, bastione_ref;
    public GameObject crowdDensity; // an object to show the crowd density in the map by changing color 

    // valori di riferimento per la scala
    private readonly float x_bastione = 4340750.584f;
    private readonly float z_bastione = 510059.186f;
    private readonly float x_piazzaYenne = 4340915.275f;
    private readonly float z_piazzaYenne = 509799.823f;
    private float fattore_scala;
    private readonly LogFormat logFormat = new();
    private Coroutine spawnCoroutine;
    private Dictionary<string, GameObject> varchi = new();
    private List<Dictionary<string, object>> gaps = new();

    void Start()
    {
        float d_real = Distanza(new Vector2(x_bastione, z_bastione), new Vector2(x_piazzaYenne, z_piazzaYenne));

        float x1 = piazzaYenne_ref.transform.localPosition.x;
        float z1 = piazzaYenne_ref.transform.localPosition.z;

        float x2 = bastione_ref.transform.localPosition.x;
        float z2 = bastione_ref.transform.localPosition.z;

        float d_Unity = Distanza(new Vector2(x1, z1), new Vector2(x2, z2));

        fattore_scala = d_real / d_Unity;
        // Database should be already downloaded
        spawnCoroutine = StartCoroutine(SpawnGaps()); // start the coroutine to spawn the gaps
    }
    private IEnumerator SpawnGaps()
    {
        yield return new WaitForSeconds(1f); // wait for the Starts method to complete and the city to be loaded
        if (varcoObj == null || city == null)
        {
            Debug.LogError($"{logFormat.databaseErrorLog} Il varco o la città non sono stati assegnati correttamente");
            yield break; // if not, break the coroutine
        }
        if (!GetGapsFromDatabase()) // check if the gaps are available in the database
        {
            Debug.Log($"{logFormat.databaseErrorLog} Impossibile ottenere i varchi dal database");
            yield break; // if not, break the coroutine
        }
        yield return null; // wait for the next frame
        StartCoroutine(LoadGaps()); // load the gaps into the scene
    }
    
    private bool GetGapsFromDatabase()
    {
        DataUsage dataUsage = new();
        gaps = dataUsage.GetAllGapsWithLocation();
        if (gaps.Count == 0)
        {
            Debug.Log($"{logFormat.databaseErrorLog} Il database non contiene varchi");
            return false;
        }
        return true;
    }

    /// <summary>
    /// Load the gaps from the database into the map in the scene
    /// </summary>
    private IEnumerator LoadGaps()
    {
        // Get the scenes in the build settings, only for them the gaps are implemented
        // and can be instantiated
        // HashSet<string> scenesInBuild = GetScenesInBuild();
        // Check if the gap name corresponds to an actual scene
        foreach (Dictionary<string, object> gap in gaps)
        {
            // check if the gap name correspond to an actual scene
            string sceneName = gap["gap_name"].ToString().ToUpper();
            // if (!scenesInBuild.Contains(sceneName))
            // {
            //     Debug.Log($"{logFormat.databaseLog} Il varco {sceneName} non è implementato");
            //     continue;
            // }

            // spawn the gap in the scene
            double gap_z_d = (double)gap["utm_east"];
            double gap_x_d = (double)gap["utm_north"];
            float gap_z = (float)gap_z_d;
            float gap_x = (float)gap_x_d;
            float x = (gap_x - x_bastione) / fattore_scala;
            float z = (gap_z - z_bastione) / fattore_scala;
            Vector3 posizione_varco = new(x, -0.05f, -z);
            GameObject obj = Instantiate(varcoObj, varcoObj.transform.position, new Quaternion(0, 0, 0, 0));
            obj.transform.parent = city.transform; // set the parent to the city object
            // set the position and rotation of the varco to adjust for the parent object
            Quaternion rotation = Quaternion.Euler(0, 0, 0);
            obj.transform.SetLocalPositionAndRotation(posizione_varco, rotation); // final position and rotation with respect to the parent object
            // the new varco is named after the gap name in the list and only if it exists
            obj.name = gap["gap_name"].ToString().ToUpper();
            varchi.Add(obj.name, obj); // add the new varco to the dictionary
            yield return null; // wait for the next frame to avoid freezing the main thread
        }
    }

    /// <summary>
    /// Get a dictionary of gaps in the scene.
    /// If no gaps are found, it returns null.
    /// /// </summary>
    /// <returns>A dictionary with gap names as keys and their corresponding GameObjects as values.</returns>
    public Dictionary<string, GameObject> GetGapInScene()
    {
        if (varchi.Count == 0)
        {
            Debug.Log($"{logFormat.databaseErrorLog} Nessun varco trovato");
            return null; // return an empty array if no gaps are found
        }
        return varchi; // return the dictionary of gaps
    }

    /* Utilities */

    /// <summary>
    /// Calculate the Euclidean distance between two points
    /// </summary>
    /// <param name="n1"></param>
    /// <param name="n2"></param>
    /// <returns></returns>
    private float Distanza(Vector2 n1, Vector2 n2)
    {
        float one = Mathf.Pow(n1.x - n2.x, 2f);
        float two = Mathf.Pow(n1.y - n2.y, 2f);
        float d2 = one + two;
        float d = Mathf.Sqrt(d2);
        return d;
    }

    /// <summary>
    /// Get the scenes in the build settings
    /// </summary>
    /// <returns></returns>
    // private HashSet<string> GetScenesInBuild()
    // {
    //     HashSet<string> scenes = new();
    //     int sceneCount = SceneManager.sceneCountInBuildSettings;
    //     for (int i = 0; i < sceneCount; i++)
    //     {
    //         string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
    //         string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
    //         scenes.Add(sceneName);
    //     }
    //     return scenes;
    // }
}
