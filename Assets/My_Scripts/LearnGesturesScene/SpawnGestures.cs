using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnGestures : MonoBehaviour
{
    public enum GestureType
    {
        SwipeLeft,
        SwipeRight,
        SwipeUp,
        SwipeDown,
        Tap,
        DoubleTap,
        LongPress
    }
    public GestureType gestureType;
    
}
