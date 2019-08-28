using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node {
    public List<Node> adjacent;
    public int x { get; set; }
    public int z { get; set; }

    public Node() {
        adjacent = new List<Node>();
    }
}
