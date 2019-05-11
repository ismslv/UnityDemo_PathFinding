using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Connection
{
    public Node To;
    public float Length;

    public Connection(Node to, float length) {
        To = to;
        Length = length;
    }
}