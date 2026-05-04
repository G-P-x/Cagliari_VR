using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ServerConfig", menuName = "Config/Shared Data")]
public class SharedData : ScriptableObject
{
    public short UserID { get; set; }
}
