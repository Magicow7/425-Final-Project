
using System;
using System.Collections.Generic;
using UnityEngine;
using BlueRaja;

public class DungeonPathfinder2D {
    public class Node {
        public Vector2Int Position { get; private set; }
        public Node Previous { get; set; }
        public float Cost { get; set; }

        public Node(Vector2Int position) {
            Position = position;
        }
    }

    public struct PathCost {
        public bool traversable;
        public float cost;
    }

    static readonly Vector2Int[] _neighbors = {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
    };

    readonly Grid2D<Node> _grid;
    SimplePriorityQueue<Node, float> _queue;
    HashSet<Node> _closed;
    readonly Stack<Vector2Int> _stack;

    public DungeonPathfinder2D(Vector2Int size) {
        _grid = new Grid2D<Node>(size, Vector2Int.zero);

        _queue = new SimplePriorityQueue<Node, float>();
        _closed = new HashSet<Node>();
        _stack = new Stack<Vector2Int>();

        for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++) {
                    _grid[x, y] = new Node(new Vector2Int(x, y));
            }
        }
    }

    void ResetNodes() {
        var size = _grid.Size;

        for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++) {
                var node = _grid[x, y];
                node.Previous = null;
                node.Cost = float.PositiveInfinity;
            }
        }
    }

    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end, Func<Node, Node, PathCost> costFunction) {
        ResetNodes();
        _queue.Clear();
        _closed.Clear();

        _queue = new SimplePriorityQueue<Node, float>();
        _closed = new HashSet<Node>();

        _grid[start].Cost = 0;
        _queue.Enqueue(_grid[start], 0);

        while (_queue.Count > 0) {
            Node node = _queue.Dequeue();
            _closed.Add(node);

            if (node.Position == end) {
                return ReconstructPath(node);
            }

            foreach (var offset in _neighbors) {
                if (!_grid.InBounds(node.Position + offset)) continue;
                var neighbor = _grid[node.Position + offset];
                if (_closed.Contains(neighbor)) continue;

                var pathCost = costFunction(node, neighbor);
                if (!pathCost.traversable) continue;

                float newCost = node.Cost + pathCost.cost;

                if (newCost < neighbor.Cost) {
                    neighbor.Previous = node;
                    neighbor.Cost = newCost;

                    if (_queue.TryGetPriority(node, out float existingPriority)) {
                        _queue.UpdatePriority(node, newCost);
                    } else {
                        _queue.Enqueue(neighbor, neighbor.Cost);
                    }
                }
            }
        }

        return null;
    }

    List<Vector2Int> ReconstructPath(Node node) {
        List<Vector2Int> result = new List<Vector2Int>();

        while (node != null) {
            _stack.Push(node.Position);
            node = node.Previous;
        }

        while (_stack.Count > 0) {
            result.Add(_stack.Pop());
        }

        return result;
    }
}
