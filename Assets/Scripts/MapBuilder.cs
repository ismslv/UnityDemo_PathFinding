using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MapBuilder : MonoBehaviour
{
    //Public
    [Header("Map")]
    public TextAsset map;
    [Header("Options")]
    [Tooltip("0: Node Simple\n1: Node Begin\n2: Node End\n3: Connection simple\n4: Connection route")]
    public Color[] colors;
    public int neighborsCount = 4;
    public PathFinding.Algorithm algorithm;
    [Header("Prefabs")]
    [Tooltip("0: SPHERE\n1: CYLINDER\n2: CUBE\n3: CAPSULE")]
    public Transform[] nodePrefabsSrc;
    public LineRenderer linePrefab;
    [Header("Objects")]
    public TextMesh labelStart;
    public TextMesh labelFinish;
    public TextMesh labelRoute;
    public Transform nodesNest;
    public Transform connectionsNest;

    //Inner
    Map mapList;
    Dictionary<Node.ShapeType, Transform> nodePrefabs;
    Dictionary<(Vector2Int, Vector2Int), LineRenderer> lines;

    void Start()
    {
        Init();
        ProcessMap(map);
    }

    #region Initialisation
    void Init() {
        PrepareNodePrefabs();
        labelStart.color = colors[1];
        labelFinish.color = colors[2];
        labelRoute.color = colors[4];
        lines = new Dictionary<(Vector2Int, Vector2Int), LineRenderer>();
    }

    void PrepareNodePrefabs() {
        var shapeTypes = System.Enum.GetValues(typeof(Node.ShapeType));
        if (nodePrefabsSrc.Length != shapeTypes.Length)
            throw new System.Exception("Not all node prefabs are defined.");
        nodePrefabs = new Dictionary<Node.ShapeType, Transform>();
        for (int i = 0; i < shapeTypes.Length; i++) {
            nodePrefabs[(Node.ShapeType)i] = nodePrefabsSrc[i];
        }
    }
    #endregion

    #region MapProcessing
    void ProcessMap(TextAsset _map) {
        var readMapTime = Diagnostics.WatchAction(() => {
            mapList = MapReader.ReadMap(_map);
        });
        Debug.Log("I've read the map '" + _map.name + "' in " + readMapTime + " s!");
        Debug.Log("We have " + mapList.Count + " nodes here.");

        var buildMapTime = Diagnostics.WatchAction(() => {
            BuildMap();
        });
        Debug.Log("I've built the map in " + buildMapTime + " s!");
        Debug.Log("And we've got " + mapList.Connections.Keys.Count + " connections");
        
        var route = new List<Node>();
        int visited = 0;
        var findRouteTime = Diagnostics.WatchAction(() => {
            FindRoute(algorithm, out route, out visited);
        });
        if (route.Count > 1) {
            Debug.Log("I've found the route in " + findRouteTime + " s!");
            Debug.Log("Using the " + (algorithm == PathFinding.Algorithm.Astar ? "A*" : "Dijkstra") + " algorithm, I've visited " + visited + " nodes");
            Debug.Log("The shortest route: " + route.Count + " nodes, " + GetRouteDistance(route).ToString("0.00") + " meters");
            DrawRoute(route);
        } else {
            Debug.Log("I could not find the route :(");
        }
    }

    void BuildMap() {
        mapList.EachNode(n => {
            CreateNode(n);
            //Expand the spiral from each node and stop when found 4 neighbors
            //Of course it will fail if there are less than 4 nodes in total
            var neighbors = mapList.FindNeighborsSpiral(n.Coords, neighborsCount);
            foreach (var nb in neighbors) {
                //For each neighbor check if there is already a connection and draw a new one if not
                if (!mapList.CheckConnectionsHash(n.Coords, nb)) {
                    lines[(n.Coords, nb)] = DrawLine(n, mapList.Nodes[nb.x, nb.y]);
                    mapList.AddConnection(n.Coords, nb);
                }
            }
        });
    }

    void FindRoute(PathFinding.Algorithm algorithm, out List<Node> route, out int visited) {
        route = mapList.GetShortestPath(out visited, algorithm);
    }
    #endregion

    #region Visualisation
    double GetRouteDistance(List<Node> route) {
        var d = 0.0;
        for (int i = 0; i < route.Count - 1; i++) {
            var c = route[i].GetConnectionTo(route[i + 1]);
            d += Mathf.Sqrt(c.Length);
        }
        return d;
    }

    void DrawRoute(List<Node> route) {
        for (int i = 0; i < route.Count - 1; i++) {
            var l = lines.First(
                c => c.Key == (route[i].Coords, route[i+1].Coords)
                  || c.Key == (route[i+1].Coords, route[i].Coords)
            ).Value;
            l.material.color = colors[4];
            l.sortingOrder = 1;
            var p0 = l.GetPosition(0);
            var p1 = l.GetPosition(1);
            p0.y += 0.1f;
            p1.y += 0.1f;
            l.SetPosition(0, p0);
            l.SetPosition(1, p1);
        }
    }

    void CreateNode(Node node) {
        node.transform = Instantiate(nodePrefabs[node.Shape]);
        node.transform.SetParent(nodesNest);
        node.transform.position = node.Coords3D;
        node.transform.GetComponent<MeshRenderer>().material.color = !node.isStart && !node.isFinish ? colors[0] : (node.isStart ? colors[1] : colors[2]);
    }

    LineRenderer DrawLine(Node nodeA, Node nodeB) {
        var line = Instantiate(linePrefab);
        line.transform.SetParent(connectionsNest);
        line.transform.position = nodeA.Coords3D;
        line.SetPosition(0, nodeA.Coords3D);
        line.SetPosition(1, nodeB.Coords3D);
        line.material.color = colors[3];
        return line;
    }
    #endregion
}