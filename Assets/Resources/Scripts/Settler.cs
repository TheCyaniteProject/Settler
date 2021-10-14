using System.Collections.Generic;
using UnityEngine;

public class Settler : MonoBehaviour
{
    [Header("Variables")]
    public int maxWidth = 100;
    public float spread = 1;
    [Range(0, 100)]
    public int shopCountChance = 1;
    public float minDistanceToCenter = 10;
    public float noiseIntensity = 1;
    public float noiseScale = 0.1f;
    public Vector2 noiseOffset = Vector2.zero;
    public bool doCollisionChecks = true;
    [Header("Modifiers")]
    public float pathspread = 1;
    public int pathCount = 2;
    public float pathSamples = 10;
    public float pathMinDistance = 1;
    [Range(0, 100)]
    public float roadProps = 0;
    [Range(0, 100)]
    public float props = 0;
    public float facePathDistance = 10;
    public float pathCutDistance = 10;
    [Range(0, 100)]
    public float fromPathCutChance = 0;
    public float wallDistance = 10;
    public float towerDistance = 10;
    public int towerCount = 10;
    public int wallCount = 10;
    [Range(0, 100)]
    public float fromWallCutChance = 0;
    [Header("Prefabs")]
    public LayerMask structureMask;
    public GameObject tower;
    public GameObject wall;
    public GameObject gate;
    public List<Structure> houses;
    public List<Structure> shops;
    public List<Structure> roadDeco;
    public List<Structure> deco;

    [Space]
    public bool debugGenerate = false;

    List<GameObject> structures = new List<GameObject>();

    [System.Serializable]
    public class Structure
    {
        public string name = "New Structure";
        [Range(0, 180)]
        public float maxRandomRotation = 0;
        public float maxRandomOffset = 0;
        public GameObject prefab;
        public Vector3 colliderOffset = Vector3.zero;
        public Vector3 colliderScale = Vector3.one;
    }

    private void Update()
    {
        if (debugGenerate)
        {
            debugGenerate = false;
            Generate();
        }
    }

    public void Generate()
    {
        foreach (GameObject item in structures)
        {
            DestroyImmediate(item);
        }
        structures.Clear();

        GenerateWalls();
        //return;

        List<Vector2> pathEnds = new List<Vector2>();
        List<Vector2> pathPositions = new List<Vector2>();

        for (int i = 0; i < pathCount; i++)
        {
            bool flip = Random.Range(0, 2) == 1;
            float x = 0;
            float y = 0;

            bool hits = true;
            while (hits)
            {
                if (flip)
                {
                    x = Random.Range(0, 2) == 0 ? -1 : 1;
                    y = Random.Range(-1f, 1f);
                }
                else
                {
                    y = Random.Range(0, 2) == 0 ? -1 : 1;
                    x = Random.Range(-1f, 1f);
                }

                hits = false;
                foreach (Vector2 path in pathEnds)
                {
                    if (Vector2.Distance(path, new Vector2(x * (maxWidth / 2), y * (maxWidth / 2))) <= pathspread)
                        hits = true;
                }

            }

            float points = 1 / (pathSamples);//% along the path

            Vector2 target = new Vector2(x * (maxWidth / 2), y * (maxWidth / 2));
            pathEnds.Add(target);


            for (int l = 1; l <= pathSamples; l++)
            {
                Vector2 pos = new Vector2(transform.position.x, transform.position.z) + (target - new Vector2(transform.position.x, transform.position.z)) * (points * l);
                pathPositions.Add(pos);
            }
        }


        for (int x = 0; x < maxWidth / spread; x++)
        {
            for (int y = 0; y < maxWidth / spread; y++)
            {
                Structure structure = houses[Random.Range(0, houses.Count)];
                Vector3 position = new Vector3((maxWidth / 2) - x * spread - (spread / 2), 0, (maxWidth / 2) - y * spread - (spread / 2));

                float flag;

                // if we're bordering a road
                if (Vector3.Distance(transform.position, position) < minDistanceToCenter + facePathDistance || Vector2.Distance(GetClosest(pathPositions, position), new Vector2(position.x, position.z)) < pathMinDistance + facePathDistance)
                {
                    if (Vector3.Distance(transform.position, position) < minDistanceToCenter + wallDistance)
                    {
                        flag = Random.Range(0, 101);
                        if (flag < shopCountChance)
                            structure = shops[Random.Range(0, shops.Count)];
                    }
                }

                // if we're on the road
                if (Vector3.Distance(transform.position, position) < minDistanceToCenter || Vector2.Distance(GetClosest(pathPositions, position), new Vector2(position.x, position.z)) < pathMinDistance)
                {
                    flag = Random.Range(0, 101);
                    if (flag < roadProps)
                        structure = roadDeco[Random.Range(0, roadDeco.Count)];
                    else
                        continue;
                }

                // if we're a distance from the road or town square
                if (Vector3.Distance(transform.position, position) > minDistanceToCenter + facePathDistance + pathCutDistance &&
                    Vector2.Distance(GetClosest(pathPositions, position), new Vector2(position.x, position.z)) > pathMinDistance + facePathDistance + pathCutDistance)
                {
                    flag = Random.Range(0, 101);
                    if (flag < fromPathCutChance)
                    {
                        flag = Random.Range(0, 101);
                        if (flag < props)
                            structure = deco[Random.Range(0, deco.Count)];
                        else
                            continue;
                    }
                }
                
                // if we're outside the walls
                if (Vector3.Distance(transform.position, position) > minDistanceToCenter + wallDistance)
                {
                    flag = Random.Range(0, 101);
                    if (flag <= fromWallCutChance)
                    {
                        continue;
                    }
                }

                

                // keep structure changes above here
                float rotation = Random.Range(-structure.maxRandomRotation, structure.maxRandomRotation);
                Quaternion quaternion = Quaternion.identity;
                quaternion.eulerAngles = new Vector3(0, rotation, 0);

                // if we're colliding with anything
                if (Physics.CheckBox(position + structure.colliderOffset, structure.colliderScale, quaternion, structureMask) && doCollisionChecks)
                {
                    continue;
                }

                float outerNoise = Mathf.PerlinNoise(x + noiseOffset.x * noiseScale, y + noiseOffset.y * noiseScale);

                if (Vector3.Distance(transform.position, position) < (maxWidth / 2 + (outerNoise * noiseIntensity))) {

                    GameObject _structure = Instantiate(structure.prefab, transform);
                    structures.Add(_structure);
                    _structure.transform.localPosition = position;
                    if (Vector3.Distance(transform.position, position) <= minDistanceToCenter + facePathDistance ||
                        Vector2.Distance(GetClosest(pathPositions, position), new Vector2(position.x, position.z)) <= pathMinDistance + facePathDistance)
                    {
                        Vector2 pathPosition = GetClosest(pathPositions, position);
                        _structure.transform.LookAt(new Vector3(pathPosition.x, _structure.transform.position.y, pathPosition.y));

                        _structure.transform.localEulerAngles = new Vector3(0, _structure.transform.localEulerAngles.y, 0);
                    }
                    else
                        _structure.transform.localEulerAngles = new Vector3(0, rotation, 0);
                    _structure.SetActive(true);
                }
            }
        }
    }

    private Vector2 GetClosest(List<Vector2> pathPositions, Vector3 target)
    {
        Vector2 closest = new Vector2(999,999);
        foreach(Vector2 position in pathPositions)
        {
            if (Vector2.Distance(position, new Vector2(target.x, target.z)) < Vector2.Distance(closest, new Vector2(target.x, target.z)))
            {
                closest = position;
            }
        }

        return closest;
    }

    private void GenerateWalls()
    {
        List<GameObject> towers = new List<GameObject>();
        float radius = wallDistance/2 * towerDistance;
        for (int i = 0; i < towerCount; i++)
        {
            float angle = i * Mathf.PI * 2f / towerCount;
            Vector3 newPos = new Vector3(Mathf.Cos(angle) * radius, 1, Mathf.Sin(angle) * radius);
            GameObject _tower = Instantiate(tower, transform);
            _tower.transform.localPosition = newPos;
            _tower.SetActive(true);
            towers.Add(_tower);
            structures.Add(_tower);
        }

        for (int i = 1; i <= towers.Count; i++)
        {
            float points = 1f / (wallCount); //% along the path
            Vector2 thisTower;
            Vector2 lastTower;

            if (i == towers.Count)
            {
                thisTower = new Vector2(towers[0].transform.position.x, towers[0].transform.position.z);
                lastTower = new Vector2(towers[i - 1].transform.position.x, towers[i - 1].transform.position.z);
                for (int l = 1; l <= wallCount; l++)
                {
                    Vector2 newPos = thisTower + (lastTower - thisTower) * (points * l);
                    newPos = newPos + ((thisTower - lastTower).normalized * points * (wallCount * 10));

                    Vector3 direction = towers[0].transform.position - towers[i - 1].transform.position;

                    GameObject _wall = Instantiate(wall, transform);
                    _wall.transform.localPosition = new Vector3(newPos.x, 0, newPos.y);

                    _wall.transform.localEulerAngles = new Vector3(0, Quaternion.FromToRotation(Vector3.up, direction).eulerAngles.y, 0);
                    _wall.SetActive(true);
                    structures.Add(_wall);

                    Debug.Log(Vector3.Distance(towers[0].transform.position, towers[i - 1].transform.position));
                }
            }
            else
            {
                thisTower = new Vector2(towers[i].transform.position.x, towers[i].transform.position.z);
                lastTower = new Vector2(towers[i - 1].transform.position.x, towers[i - 1].transform.position.z);
                for (int l = 1; l <= wallCount; l++)
                {
                    Vector2 newPos = thisTower + (lastTower - thisTower) * (points * l);
                    newPos = newPos + ((thisTower - lastTower).normalized * points * (wallCount * 10));

                    Vector3 direction = towers[i].transform.position - towers[i - 1].transform.position;

                    GameObject _wall = Instantiate(wall, transform);
                    _wall.transform.localPosition = new Vector3(newPos.x, 0, newPos.y);

                    _wall.transform.localEulerAngles = new Vector3(0, Quaternion.FromToRotation(Vector3.up, direction).eulerAngles.y, 0);
                    _wall.SetActive(true);
                    structures.Add(_wall);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        float height = 20;
        Gizmos.color = Color.black;
        Gizmos.DrawWireCube(transform.position + new Vector3(0, height / 2, 0), new Vector3(maxWidth, height, maxWidth));
    }
}
