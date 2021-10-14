using System.Collections.Generic;
using UnityEngine;

public class ObjectReference : MonoBehaviour
{
    public List<ObjectSpawner> childSpawners;
    public bool debugGenerate = false;

    public void Update()
    {
        if (debugGenerate)
        {
            debugGenerate = false;
            Generate();
        }
    }

    private void Awake()
    {
        Generate();
    }

    public void Generate()
    {
        foreach (ObjectSpawner child in childSpawners)
        {
            child.Generate();
        }
    }
}
