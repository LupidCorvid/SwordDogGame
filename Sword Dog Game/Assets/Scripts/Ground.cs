using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ground : MonoBehaviour
{
    public enum Type {
        GRASS, WOOD, ROCK, SAND
    }

    public Type type;
}
