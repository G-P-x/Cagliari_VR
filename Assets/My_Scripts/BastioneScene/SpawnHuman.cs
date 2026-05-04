using System.Collections.Generic;
using UnityEngine;

public class SpawnHuman : MonoBehaviour
{
    public GameObject humanInPrefab;

    public GameObject startPointIn;
    public GameObject startPointOut;

    private float timeIntervalIn = 1f;
    private float timeIntervalOut = 1f;
    
    public void SpawnHumanPrefabs(Dictionary<string, object> data)
    {
        int p_in = (int)data["people_entered"];
        int p_out = (int)data["people_left"];
        if(p_in == 0 && p_out != 0)
        {
            timeIntervalOut = 1f;
            InvokeRepeating(nameof(SpawnOut), 0f, timeIntervalOut);
            return;
        }
        if(p_in != 0 && p_out == 0)
        {
            timeIntervalIn = 1f;
            InvokeRepeating(nameof(SpawnIn), 0f, timeIntervalIn);
            return;
        }
        if(p_in == 0 && p_out == 0)
        {
            return;
        }
        if(p_in < p_out)
        {
            timeIntervalIn = 1f;
            timeIntervalOut = (float)p_out/p_in;
        }
        else
        {
            timeIntervalIn = 1f;
            timeIntervalOut = (float)p_in/p_out;
        }
        InvokeRepeating(nameof(SpawnIn), 0, timeIntervalIn);
        InvokeRepeating(nameof(SpawnOut), 0.2f, timeIntervalOut); 
    }
    private void SpawnIn()
    {
        Instantiate(humanInPrefab, startPointIn.transform.position, humanInPrefab.transform.rotation);
    }
    
    private void SpawnOut()
    {        
        Vector3 euler = humanInPrefab.transform.rotation.eulerAngles + new Vector3(0, 180f, 0);
        Quaternion rotation = Quaternion.Euler(euler);
        Instantiate(humanInPrefab, startPointOut.transform.position, rotation);
    }
    public void StopSpawn()
    {
        CancelInvoke(nameof(SpawnIn));
        CancelInvoke(nameof(SpawnOut));
        GameObject[] humans = GameObject.FindGameObjectsWithTag("Human");
        foreach (GameObject human in humans)
        {
            Destroy(human);
        }
    }
}
