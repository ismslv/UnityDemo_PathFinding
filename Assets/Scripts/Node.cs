using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Node
{
    public enum ShapeType {SPHERE, CYLINDER, CUBE, CAPSULE}
    public ShapeType Shape = ShapeType.CUBE;
    public Vector2Int Coords;
    public bool isStart;
    public bool isFinish;
    public Transform transform;

    //PathFinding
    public List<Connection> Neighbors;
    public bool Visited = false;
    public float StraightLineDistanceToEnd;
    public float MinDistanceToStart = 0;
    public Node NearestToStart;
    
    public Node(string shape, int x, int y, bool isstart, bool isfinish) {
        if (System.Enum.TryParse(shape, out ShapeType _Shape))
            Shape = _Shape;
        Coords = new Vector2Int(x, y);
        Neighbors = new List<Connection>();
        isStart = isstart;
        isFinish = isfinish;
    }

    public Vector3 Coords3D {
        get {
            return new Vector3(Coords.x, 0, Coords.y);
        }
    }

    public float StraightLineDistanceTo(Node node) {
        return Vector3.Distance(Coords3D, node.Coords3D);
    }

    public Connection GetConnectionTo(Node to) {
        return Neighbors.First(n => n.To == to);
    }
}
