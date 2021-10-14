using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    
    [Range(0, 180)]
    [Tooltip("Value is doubled")]
    public float maxHorizontalRotation = 0;
    [Range(0, 180)]
    [Tooltip("Value is doubled")]
    public float maxVerticalRotation = 0;
    public Vector2 maxPositionOffset = Vector3.zero;
    public bool canSpawnNothing = true;
    public List<GameObject> objects;
    [Header("Visual Only")]
    public Vector3 gizmoSize = Vector3.one;
    public Color gizmoColor = Color.blue;

    public bool debugGenerate = false;
    public bool drawGizmos = true;

    // private
    GameObject object_instance;

    public void Update()
    {
        if (debugGenerate)
        {
            debugGenerate = false;
            Generate();
        }
    }

    public void Generate()
    {
        if (object_instance) Destroy(object_instance);
        if (objects.Count == 0) return;
        int index = Random.Range(canSpawnNothing?-1:0, objects.Count);

        if (index < 0) return;

        object_instance = Instantiate(objects[index], transform);

        object_instance.transform.localEulerAngles = new Vector3(Random.Range(-maxVerticalRotation, maxVerticalRotation), Random.Range(-maxHorizontalRotation, maxHorizontalRotation), 0);

        object_instance.transform.localPosition = new Vector3(Random.Range(-maxPositionOffset.x, maxPositionOffset.x), 0, Random.Range(-maxPositionOffset.y, maxPositionOffset.y));
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        Gizmos.matrix = rotationMatrix;

        Gizmos.color = gizmoColor;
        Gizmos.DrawWireCube(new Vector3(0, (gizmoSize.y / 2), 0), gizmoSize);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(0, (gizmoSize.y / 2), 0), new Vector3(0, (gizmoSize.y / 2), 0) + Vector3.forward);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(new Vector3(0, 0, 0), new Vector3(maxPositionOffset.x, 0, maxPositionOffset.y));
    }
}
