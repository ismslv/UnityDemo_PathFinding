using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.IO;
using System.Linq;

public class MapReader
{
    public static Map ReadMap(TextAsset asset) {
        if (asset == null)
            throw new System.Exception("Asset " + asset.name + " does not exist!");
        var list = new List<Node>();
        XmlTextReader reader = new XmlTextReader(new StringReader(asset.text));
        var doc = XDocument.Load(reader);
        var nodes = from n in doc.Descendants("Node")
                select new {
                   X = (int)n.Element("X"),
                   Y = (int)n.Element("Y"),
                   Shape = (string)n.Element("Shape"),
                   Path = (string)n.Element("Path")
                };
        
        int xMax = 0;
        int yMax = 0;
        foreach(var node in nodes) {
            list.Add(new Node(node.Shape, node.X, node.Y, node.Path == "BEGIN", node.Path == "END"));
            xMax = Mathf.Max(xMax, node.X);
            yMax = Mathf.Max(yMax, node.Y);
        }
        var map = new Map(xMax, yMax);
        foreach (var n in list)
            map.Add(n);
        return map;
    }
}