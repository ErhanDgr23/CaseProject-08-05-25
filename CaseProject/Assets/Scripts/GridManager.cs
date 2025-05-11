using System.Collections.Generic;
using _project.Car;
using UnityEngine;
using System.Linq;
using UniRx;

namespace _project.Grid
{
    public class GridManager : MonoBehaviour
    {
        public static GridManager GridManagerScript;

        private Dictionary<Vector2Int, MyGrid> _gridLookup;

        public ReactiveProperty<MyGrid> CurrentMouseSelectedMyGrid;
        public ReactiveProperty<CarPart> CurrentMouseSelectedCarPart;

        [SerializeField] Transform MyGridParents;

        MyGrid[] debugPath;

        private void Awake()
        {
            GridManagerScript = this;
            _gridLookup = new Dictionary<Vector2Int, MyGrid>();

            foreach (Transform child in MyGridParents)
            {
                var grid = child.GetComponent<MyGrid>();
                var ix = Mathf.RoundToInt(child.position.x);
                var iy = Mathf.RoundToInt(child.position.z);
                grid.Index = new Vector2Int(ix, iy);
                _gridLookup[grid.Index] = grid;
            }

            CurrentMouseSelectedMyGrid
                .Where(g => g != null)
                .Subscribe(OnMyGridChanged);
        }

        public MyGrid FindMyGridWithPos(Vector2 worldPos)
        {
            var key = new Vector2Int(
                Mathf.RoundToInt(worldPos.x),
                Mathf.RoundToInt(worldPos.y)
            );
            _gridLookup.TryGetValue(key, out var grid);
            return grid;
        }

        void OnMyGridChanged(MyGrid grid)
        {
            if (grid == null)
                return;

            if (CurrentMouseSelectedCarPart.Value == null)
                CurrentMouseSelectedCarPart.Value = grid.CurrentCarPartt;

            if (CurrentMouseSelectedCarPart.Value == null || CurrentMouseSelectedCarPart.Value.CurrentGrid == null)
                return;

            var path = CreatePath(CurrentMouseSelectedCarPart.Value.CurrentGrid, CurrentMouseSelectedMyGrid.Value);

            if (path != null)
            {
                CurrentMouseSelectedCarPart.Value.StartPathMove(path);
                SetDebugPath(path);
            }
            else
            {
                Debug.LogWarning($"[GridManager] No path from {CurrentMouseSelectedCarPart.Value.CurrentGrid.Index} to {CurrentMouseSelectedMyGrid.Value.Index}");
            }
        }

        #region PathFind
        public void SetDebugPath(MyGrid[] path)
        {
            debugPath = path;
        }

        private void OnDrawGizmos()
        {
            if (debugPath == null) return;

            Gizmos.color = Color.green;
            for (int i = 0; i < debugPath.Length - 1; i++)
            {
                Gizmos.DrawLine(
                    debugPath[i].transform.position + Vector3.up * 0.2f,
                    debugPath[i + 1].transform.position + Vector3.up * 0.2f
                );
            }
        }

        private IEnumerable<MyGrid> GetNeighbors(MyGrid grid)
        {
            // Dört yönlü komşuluk, index farkı üzerinden kontrol:
            foreach (var kv in _gridLookup)
            {
                var other = kv.Value;
                var dx = Mathf.Abs(kv.Key.x - grid.Index.x);
                var dy = Mathf.Abs(kv.Key.y - grid.Index.y);

                // Yalnızca yatay/dikey komşular: |dx|+|dy| == 1
                if (dx + dy == 1 && !other.IsOccupied)
                    yield return other;
            }
        }

        public MyGrid[] CreatePath(MyGrid start, MyGrid target)
        {
            var dx = Mathf.Abs(start.Index.x - target.Index.x);
            var dy = Mathf.Abs(start.Index.y - target.Index.y);
            if (dx + dy == 1 && !target.IsOccupied)
            {
                return new[] { target };
            }

            var queue = new Queue<MyGrid>();
            var cameFrom = new Dictionary<MyGrid, MyGrid>();
            var visited = new HashSet<MyGrid>();

            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current == target)
                    return RetracePath(start, target, cameFrom);

                foreach (var neighbor in GetNeighbors(current))
                {
                    if (visited.Contains(neighbor))
                        continue;
                    visited.Add(neighbor);
                    cameFrom[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }

            return null;
        }


        private MyGrid[] RetracePath(MyGrid start, MyGrid end, Dictionary<MyGrid, MyGrid> cameFrom)
        {
            var path = new List<MyGrid>();
            var cur = end;
            while (cur != start)
            {
                path.Add(cur);
                cur = cameFrom[cur];
            }
            path.Reverse();
            return path.ToArray();
        }
        #endregion

        private void Update()
        {
            if (Input.GetMouseButtonUp(0) && CurrentMouseSelectedCarPart.Value != null)
                CurrentMouseSelectedCarPart.Value = null;
        }
    }
}
