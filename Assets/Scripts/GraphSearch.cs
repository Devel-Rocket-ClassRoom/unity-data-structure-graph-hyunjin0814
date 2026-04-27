using System.Collections.Generic;
using UnityEngine;

public class GraphSearch
{
    private Graph graph;
    public List<GraphNode> path = new List<GraphNode>();

    public void Init(Graph graph)
    {
        this.graph = graph;
    }

    public void DFS(GraphNode node)
    {
        path.Clear();

        var visited = new HashSet<GraphNode>();
        var stack = new Stack<GraphNode>();

        stack.Push(node);
        visited.Add(node);

        while (stack.Count > 0)
        {
            var currentNode = stack.Pop();
            path.Add(currentNode);

            foreach (var adjacent in currentNode.adjacents)
            {
                if (!adjacent.CanVisit || visited.Contains(adjacent))
                    continue;

                visited.Add(adjacent);
                stack.Push(adjacent);
            }
        }
    }

    public void BFS(GraphNode node)
    {
        path.Clear();

        var visited = new HashSet<GraphNode>();
        var queue = new Queue<GraphNode>();

        queue.Enqueue(node);
        visited.Add(node);

        while (queue.Count > 0)
        {
            var currentNode = queue.Dequeue();
            path.Add(currentNode);

            foreach (var adjacent in currentNode.adjacents)
            {
                if (!adjacent.CanVisit || visited.Contains(adjacent))
                    continue;

                visited.Add(adjacent);
                queue.Enqueue(adjacent);
            }
        }
    }

    public void DFSRecursive(GraphNode node)
    {
        path.Clear();
        var visited = new HashSet<GraphNode>();

        //visited.Add(node);

        DFSRecursive(node, visited);
    }

    protected void DFSRecursive(GraphNode node, HashSet<GraphNode> visited)
    {
        path.Add(node);
        visited.Add(node);

        //List<GraphNode> validAdjacents = new List<GraphNode>();

        //for (int i = node.adjacents.Count - 1; i >= 0; i--)
        //{
        //    var adj = node.adjacents[i];
        //    if (adj.CanVisit && !visited.Contains(adj))
        //    {
        //        visited.Add(adj);
        //        validAdjacents.Add(adj);
        //    }
            
        //}

        //foreach (var adj in validAdjacents)
        //{
        //    DFSRecursive(adj, visited);
        //}

        foreach (var adjacent in node.adjacents)
        {
            if (!adjacent.CanVisit || visited.Contains(adjacent))
                continue;

            DFSRecursive(adjacent, visited);
        }
    }

    public bool PathFindingBFS(GraphNode startNode, GraphNode endNode)
    {
        path.Clear();
        graph.ResetNodePrevious();

        var visited = new HashSet<GraphNode>();
        var queue = new Queue<GraphNode>();

        queue.Enqueue(startNode);
        visited.Add(startNode);

        bool success = false;
        while (queue.Count > 0)
        {
            var currentNode = queue.Dequeue();

            if (currentNode == endNode)
            {
                success = true;
                break;
            }

            foreach (var adjacent in currentNode.adjacents)
            {
                if (!adjacent.CanVisit || visited.Contains(adjacent))
                    continue;

                adjacent.previous = currentNode;
                visited.Add(adjacent);
                queue.Enqueue(adjacent);
            }
        }

        if (!success)
        {
            return false;
        }

        GraphNode step = endNode;
        while (step != null)
        {
            path.Add(step);
            step = step.previous;
        }

        path.Reverse();
        return true;
    }

    public bool Dijkstra(GraphNode startNode, GraphNode endNode)
    {
        path.Clear();
        graph.ResetNodePrevious();

        var visited = new HashSet<GraphNode>();
        var pq = new PriorityQueue<GraphNode, int>();
        var distances = new int[graph.nodes.Length];
        for (int i = 0; i < distances.Length; i++)
        {
            distances[i] = int.MaxValue;
        }

        distances[startNode.id] = 0;
        pq.Enqueue(startNode, distances[startNode.id]);

        bool success = false;
        while (pq.Count > 0)
        {
            var currentNode = pq.Dequeue();

            if (visited.Contains(currentNode))
                continue;

            visited.Add(currentNode);

            if (currentNode == endNode)
            {
                success = true;
                break;
            }

            foreach (var adjacent in currentNode.adjacents)
            {
                if (!adjacent.CanVisit || visited.Contains(adjacent))
                    continue;

                int newDistance = distances[currentNode.id] + adjacent.weight;

                if (distances[adjacent.id] > newDistance)
                {
                    distances[adjacent.id] = newDistance; 
                    adjacent.previous = currentNode; 
                    pq.Enqueue(adjacent, distances[adjacent.id]);
                }
            }
        }

        if (!success)
        {
            return false;
        }

        GraphNode step = endNode;
        while (step != null)
        {
            path.Add(step);
            step = step.previous;
        }

        path.Reverse();
        return true;
    }
    //public bool Dijkstra(GraphNode startNode, GraphNode endNode)
    //{
    //    path.Clear();
    //    graph.ResetNodePrevious();

    //    var priorityQueue = new PriorityQueue<GraphNode, int>();
    //    var distances = new Dictionary<GraphNode, int>();
    //    foreach (var node in graph.nodes) distances[node] = int.MaxValue;

    //    distances[startNode] = 0;
    //    priorityQueue.Enqueue(startNode, 0);

    //    bool success = false;
    //    while (priorityQueue.Count > 0)
    //    {
    //        var currentNode = priorityQueue.Dequeue();

    //        if (currentNode == endNode)
    //        {
    //            success = true;
    //            break;
    //        }

    //        foreach (var adjacent in currentNode.adjacents)
    //        {
    //            if (!adjacent.CanVisit)
    //                continue;

    //            int newCost = distances[currentNode] + adjacent.weight;

    //            if (newCost < distances[adjacent])
    //            {
    //                distances[adjacent] = newCost; 
    //                adjacent.previous = currentNode; 
    //                priorityQueue.Enqueue(adjacent, newCost);
    //            }
    //        }
    //    }

    //    if (!success)
    //    {
    //        return false;
    //    }

    //    GraphNode step = endNode;
    //    while (step != null)
    //    {
    //        path.Add(step);
    //        step = step.previous;
    //    }

    //    path.Reverse();
    //    return true;
    //}

    private int Heuristic(GraphNode a, GraphNode b)
    {
        int ax = a.id % graph.cols;
        int ay = a.id / graph.cols;

        int bx = b.id % graph.cols;
        int by = b.id / graph.cols;

        return Mathf.Abs(ax - bx) + Mathf.Abs(ay - by);
    }

    public bool AStar(GraphNode startNode, GraphNode endNode)
    {
        path.Clear();
        graph.ResetNodePrevious();

        var priorityQueue = new PriorityQueue<GraphNode, int>();
        var distances = new Dictionary<GraphNode, int>();
        foreach (var node in graph.nodes) distances[node] = int.MaxValue;

        distances[startNode] = 0;
        priorityQueue.Enqueue(startNode, 0);

        bool success = false;
        while (priorityQueue.Count > 0)
        {
            var currentNode = priorityQueue.Dequeue();

            if (currentNode == endNode)
            {
                success = true;
                break;
            }

            foreach (var adjacent in currentNode.adjacents)
            {
                if (!adjacent.CanVisit)
                    continue;

                int newCost = distances[currentNode] + adjacent.weight;

                if (newCost < distances[adjacent])
                {
                    distances[adjacent] = newCost;
                    adjacent.previous = currentNode;
                    priorityQueue.Enqueue(adjacent, newCost + Heuristic(adjacent, endNode));
                }
            }
        }

        if (!success)
        {
            return false;
        }

        GraphNode step = endNode;
        while (step != null)
        {
            path.Add(step);
            step = step.previous;
        }

        path.Reverse();
        return true;
    }

    //public void PathFindingBFS(GraphNode start, GraphNode end)
    //{
    //    path.Clear();

    //    graph.ResetNodePrevious();

    //    var visited = new HashSet<GraphNode>();
    //    var queue = new Queue<GraphNode>();

    //    queue.Enqueue(start);
    //    visited.Add(start);

    //    bool isFound = false;

    //    while (queue.Count > 0)
    //    {
    //        var currentNode = queue.Dequeue();

    //        if (currentNode == end)
    //        {
    //            isFound = true;
    //            break;
    //        }

    //        foreach (var adjacent in currentNode.adjacents)
    //        {
    //            if (!adjacent.CanVisit || visited.Contains(adjacent))
    //                continue;

    //            adjacent.previous = currentNode;

    //            visited.Add(adjacent);
    //            queue.Enqueue(adjacent);
    //        }
    //    }

    //    if (isFound)
    //    {
    //        GraphNode temp = end;

    //        while (temp != null)
    //        {
    //            path.Add(temp);
    //            temp = temp.previous;
    //        }

    //        path.Reverse();
    //    }
    //}
}
