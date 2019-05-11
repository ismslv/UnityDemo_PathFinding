using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Map
{
    public Node[,] Nodes;
    public Vector2Int NodeStart;
    public Vector2Int NodeFinish;
    public int[,] Grid;
    public Vector2Int Size;
    public int Count;
    public Dictionary<(Vector2Int, Vector2Int), Connection> Connections;

    //Inner
    HashSet<(Vector2Int, Vector2Int)> ConnectionsHash;

    public Map(int sizeX, int sizeY) {
        Size = new Vector2Int(sizeX + 1, sizeY + 1);
        Nodes = new Node[Size.x, Size.y];
        Grid = new int[Size.x, Size.y];
        for (int x = 0; x < Size.x; x++) {
            for (int y = 0; y < Size.y; y++) {
                Grid[x, y] = 0;
            }
        }
        ConnectionsHash = new HashSet<(Vector2Int, Vector2Int)>();
        Connections = new Dictionary<(Vector2Int, Vector2Int), Connection>();
    }

    #region Spiral
    public List<Vector2Int> FindNeighborsSpiral(Vector2Int coords, int count) {
        var list = new List<Vector2Int>();
        Spiral(coords, c => {
            if (CoordsHasNode(c)) {
                list.Add(c);
            }
            return list.Count < count;
        });
        return list;
    }

    void Spiral(Vector2Int center, System.Func<Vector2Int, bool> action) {
        var dir = Dir.N; //start dir
        var l = 1;       //start distance
        var c = 0;       //counter for rotation
        var coords = center;
        var go = true;
        while (go) {
            for (int i = 0; i < l; i++) {
                coords = NeighborCoords(coords, dir);
                if (CoordsInBounds(coords)) {
                    go = action.Invoke(coords);
                    if (!go) break;
                }
            }
            dir = DirRotate(dir);
            if (c == 1) {
                c = 0;
                l++;
            } else {
                c++;
            }
        }
    }
    #endregion
    
    #region Coords
    enum Dir {N, E, S, W};

    Vector2Int VectorDir(Dir dir) {
        switch (dir) {
            case Dir.N:
                return new Vector2Int(0, 1);
            case Dir.E:
                return new Vector2Int(1, 0);
            case Dir.S:
                return new Vector2Int(0, -1);
            case Dir.W:
                return new Vector2Int(-1, 0);
            default:
                return Vector2Int.zero;
        }
    }

    Dir DirRotate(Dir dir, bool clockwise = true) {
        int direction = clockwise ? 1 : -1;
        int newdir = (int)dir + direction;
        if (newdir < 0) newdir = 3;
        if (newdir > 3) newdir = 0;
        return (Dir)newdir;
    }

    Vector2Int NeighborCoords(Vector2Int coords, Dir dir) {
        return coords + VectorDir(dir);
    }

    bool CoordsInBounds(Vector2Int coords) {
        return coords.x >= 0 && coords.y >= 0 && coords.x < Size.x && coords.y < Size.y;
    }

    bool CoordsHasNode(Vector2Int coords) {
        return Grid[coords.x, coords.y] == 1;
    }
#endregion

    #region Connections
    public bool CheckConnectionsHash(Vector2Int A, Vector2Int B) {
        return ConnectionsHash.Contains((A, B)) || ConnectionsHash.Contains((B, A));
    }

    public void AddConnection(Vector2Int A, Vector2Int B) {
        //Because hash is fastest
        ConnectionsHash.Add((A, B));
        //int distance = EuclideanDistance(A.x, B.x, A.y, B.y);
        var distance = Vector3.Distance(GetNode(A).Coords3D, GetNode(B).Coords3D);
        GetNode(A).Neighbors.Add(new Connection(GetNode(B), distance));
        GetNode(B).Neighbors.Add(new Connection(GetNode(A), distance));
    }
    #endregion

    #region Nodes
    public void Add(Node node) {
        Nodes[node.Coords.x, node.Coords.y] = node;
        Grid[node.Coords.x, node.Coords.y] = 1;
        Count++;
        if (node.isStart) NodeStart = node.Coords;
        if (node.isFinish) NodeFinish = node.Coords;
    }

    public void EachNode(System.Action<Node> action) {
        if (action == null) return;
        for (int x = 0; x < Size.x; x++) {
            for (int y = 0; y < Size.y; y++) {
                if (Nodes[x, y] != null)
                    action.Invoke(Nodes[x, y]);
            }
        }
    }

    public Node GetNode(Vector2Int coords) {
        if (Nodes[coords.x, coords.y] != null) {
            return Nodes[coords.x, coords.y];
        } else {
            throw new System.Exception("No node at " + coords.ToString());
        }
    }

    public Node NodeFrom {
        get {
            return GetNode(NodeStart);
        }
    }

    public Node NodeTo {
        get {
            return GetNode(NodeFinish);
        }
    }
    #endregion

    #region Path
        public int EuclideanDistance(Node A, Node B) {
            return EuclideanDistance(A.Coords.x, B.Coords.x, A.Coords.y, B.Coords.y);
        }

        public int EuclideanDistance(int x1, int x2, int y1, int y2)
        {
            int square = (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);
            return square;
        }

        public List<Node> GetShortestPath(out int visited, PathFinding.Algorithm algorithm, Node from = null, Node to = null) {
            if (from == null) from = NodeFrom;
            if (to == null) to = NodeTo;

            if (algorithm == PathFinding.Algorithm.Astar) {
                //Cache straight line distances
                EachNode(n => {
                    n.StraightLineDistanceToEnd = n.StraightLineDistanceTo(to);
                });
                return PathFinding.GetShortestPathAstar(out visited, from, to);
            } else {
                return PathFinding.GetShortestPathDijkstra(out visited, from, to);
            }
        }
    #endregion

}
