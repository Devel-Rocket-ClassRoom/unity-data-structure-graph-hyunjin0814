using System.Collections.Generic;
using UnityEngine;

public class TileSearch
{
    public List<Tile> path = new List<Tile>();
    private Map map;

    public void Init(Map map)
    {
        this.map = map;
    }

    private int Heuristic(Tile a, Tile b)
    {
        int ax = a.id % map.cols;
        int ay = a.id / map.cols;

        int bx = b.id % map.cols;
        int by = b.id / map.cols;

        return Mathf.Abs(ax - bx) + Mathf.Abs(ay - by);
    }

    public bool AStar(Tile startTile, Tile endTile)
    {
        path.Clear();
        map.ResetNodePrevious();

        var priorityQueue = new PriorityQueue<Tile, int>();
        var distances = new Dictionary<Tile, int>();
        foreach (var tile in map.tiles) distances[tile] = int.MaxValue;

        distances[startTile] = 0;
        priorityQueue.Enqueue(startTile, 0);

        bool success = false;
        while (priorityQueue.Count > 0)
        {
            var currentTile = priorityQueue.Dequeue();

            if (currentTile == endTile)
            {
                success = true;
                break;
            }

            foreach (var adjacent in currentTile.adjacents)
            {
                if (adjacent == null || adjacent.autoTileId == (int)TileTypes.Empty || adjacent.autoTileId == (int)TileTypes.Mountains)
                    continue;

                int newCost = distances[currentTile] + adjacent.Weight;

                if (newCost < distances[adjacent])
                {
                    distances[adjacent] = newCost;
                    adjacent.previous = currentTile;
                    priorityQueue.Enqueue(adjacent, newCost + Heuristic(adjacent, endTile));
                }
            }
        }

        if (!success)
        {
            return false;
        }

        Tile step = endTile;
        while (step != null)
        {
            path.Add(step);
            step = step.previous;
        }

        path.Reverse();
        path.RemoveAt(0);
        return true;
    }

}
