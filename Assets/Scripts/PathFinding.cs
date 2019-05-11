using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathFinding
{
    public enum Algorithm {Astar, Dijkstra};

    private static void BuildShortestPath(List<Node> list, Node node)
    {
        if (node.NearestToStart == null) return;
        list.Add(node.NearestToStart);
        BuildShortestPath(list, node.NearestToStart);
    }

    #region Dijkstra
    public static List<Node> GetShortestPathDijkstra(out int visited, Node from = null, Node to = null)
    {
        DijkstraSearch(from, to, out visited);
        var shortestPath = new List<Node>();
        shortestPath.Add(to);
        BuildShortestPath(shortestPath, to);
        shortestPath.Reverse();
        return shortestPath;
    }

    private static void DijkstraSearch(Node from, Node to, out int visited)
    {
        from.MinDistanceToStart = 0;
        var queue = new List<Node>();
        visited = 0;
        queue.Add(from);
        do {
            queue = queue.OrderBy(x => x.MinDistanceToStart).ToList();
            var node = queue.First();
            queue.Remove(node);
            visited++;
            foreach (var cnn in node.Neighbors.OrderBy(c => c.Length))
            {
                var childNode = cnn.To;
                if (childNode.Visited)
                    continue;
                if (childNode.MinDistanceToStart == 0 ||
                    node.MinDistanceToStart + cnn.Length < childNode.MinDistanceToStart)
                {
                    childNode.MinDistanceToStart = node.MinDistanceToStart + cnn.Length;
                    childNode.NearestToStart = node;
                    if (!queue.Contains(childNode))
                        queue.Add(childNode);
                }
            }
            node.Visited = true;
            if (node == to)
                return;
        } while (queue.Any());
    }
    #endregion

    #region A*
    public static List<Node> GetShortestPathAstar(out int visited, Node from = null, Node to = null)
    {
        AstarSearch(from, to, out visited);
        var shortestPath = new List<Node>();
        shortestPath.Add(to);
        BuildShortestPath(shortestPath, to);
        shortestPath.Reverse();
        return shortestPath;
    }

    private static void AstarSearch(Node from, Node to, out int visited)
    {
        from.MinDistanceToStart = 0;
        var queue = new List<Node>();
        visited = 0;
        queue.Add(from);
        do {
            queue = queue.OrderBy(x => x.MinDistanceToStart + x.StraightLineDistanceToEnd).ToList();
            var node = queue.First();
            queue.Remove(node);
            visited++;
            foreach (var cnn in node.Neighbors.OrderBy(c => c.Length))
            {
                var childNode = cnn.To;
                if (childNode.Visited)
                    continue;
                if (childNode.MinDistanceToStart == 0 ||
                    node.MinDistanceToStart + cnn.Length < childNode.MinDistanceToStart)
                {
                    childNode.MinDistanceToStart = node.MinDistanceToStart + cnn.Length;
                    childNode.NearestToStart = node;
                    if (!queue.Contains(childNode))
                        queue.Add(childNode);
                }
            }
            node.Visited = true;
            if (node == to)
                return;
        } while (queue.Any());
    }
    #endregion
}