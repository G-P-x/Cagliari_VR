using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UserInput;
using UnityEngine.SceneManagement;



[System.Serializable]
public class Gaps
{
    public string gap_name;
    public double utm_north;
    public double utm_east;
}

public class SpawnManager : MonoBehaviour
{
    public GameObject varcoObj;
    public GameObject city;
    public GameObject piazzaYenne_ref, bastione_ref;
    public GameObject crowdDensity; // an object to show the crowd density in the map by changing color 
    [Header("Static gaps (Inspector)")]
    public List<Gaps> staticGaps = new();

    // valori di riferimento per la scala
    private readonly float x_bastione = 4340750.584f;
    private readonly float z_bastione = 510059.186f;
    private readonly float x_piazzaYenne = 4340915.275f;
    private readonly float z_piazzaYenne = 509799.823f;
    private float fattore_scala; // conversion factor to convert real world coordinates to Unity coordinates
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
        // Database should be already downloaded (fallback). Start spawning.
        spawnCoroutine = StartCoroutine(SpawnGaps()); // start the coroutine to spawn the gaps
    }
    private IEnumerator SpawnGaps()
    {
        // small delay to allow scene objects to initialize
        yield return new WaitForSeconds(0.5f);

        if (varcoObj == null || city == null)
        {
            Debug.LogError($"{logFormat.databaseErrorLog} Il varco o la città non sono stati assegnati correttamente");
            yield break;
        }

        bool haveGaps = false;
        // Prefer inspector static list when available
        if (staticGaps != null && staticGaps.Count > 0)
        {
            haveGaps = LoadGapsFromStaticList();
        }

        // Fallback to database if static list is empty
        // if (!haveGaps)
        // {
        //     if (!GetGapsFromDatabase())
        //     {
        //         Debug.Log($"{logFormat.databaseErrorLog} Impossibile ottenere i varchi dal database o dalla lista statica");
        //         yield break;
        //     }
        // }

        // load the gaps into the scene
        yield return null;
        StartCoroutine(LoadGaps());
    }
    
    private bool LoadGapsFromStaticList()
    {
        if (staticGaps == null || staticGaps.Count == 0) return false;
        gaps.Clear();
        foreach (Gaps gi in staticGaps)
        {
            var dict = new Dictionary<string, object>();
            dict["gap_name"] = gi.gap_name ?? string.Empty;
            dict["utm_north"] = gi.utm_north;
            dict["utm_east"] = gi.utm_east;
            gaps.Add(dict);
        }
        return gaps.Count > 0;
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

            // instantiate with identity and parented to city with local transform
            GameObject obj = Instantiate(varcoObj, Vector3.zero, Quaternion.identity);
            obj.transform.SetParent(city.transform, false);
            obj.transform.localPosition = posizione_varco;
            obj.transform.localRotation = Quaternion.identity;

            // the new varco is named after the gap name
            string gapName = gap["gap_name"].ToString().ToUpperInvariant();
            obj.name = gapName;
            if (varchi.ContainsKey(obj.name))
            {
                Debug.LogWarning($"{logFormat.databaseLog} Varco duplicato ignorato: {obj.name}");
                Destroy(obj);
                continue;
            }
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
        }
        return varchi; // return the dictionary of gaps (may be empty)
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
