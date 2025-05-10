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

        // public SerializedDictionary<MyGrid, Vector2> MyGridsList;

        private Dictionary<Vector2Int, MyGrid> _gridLookup;

        /*[HideInInspector]*/ public ReactiveProperty<MyGrid> CurrentMouseSelectedMyGrid;
        /*[HideInInspector]*/ public CarPart CurrentMouseSelectedCarPart;

        [SerializeField] Transform MyGridParents;

        MyGrid[] debugPath;

        private void Awake()
        {
            GridManagerScript = this;
            _gridLookup = new Dictionary<Vector2Int, MyGrid>();

            foreach (Transform child in MyGridParents)
            {
                var grid = child.GetComponent<MyGrid>();
                // Sahnedeki pozisyonu tam sayıya yuvarla:
                var ix = Mathf.RoundToInt(child.position.x);
                var iy = Mathf.RoundToInt(child.position.z);
                grid.Index = new Vector2Int(ix, iy);
                _gridLookup[grid.Index] = grid;
            }

            CurrentMouseSelectedMyGrid
                .Where(g => g != null)
                .Subscribe(OnMyGridChanged);
        }

        //public MyGrid FindMyGridWithPos(Vector2 _inputPos) => MyGridsList.FirstOrDefault(SelectedMyGrid => SelectedMyGrid.Value == _inputPos).Key;

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
            Debug.Log("Grid changed");

            if (grid == null)
                return;

            if (CurrentMouseSelectedCarPart == null)
                CurrentMouseSelectedCarPart = grid.CurrentCarPartt;

            if (CurrentMouseSelectedCarPart == null || CurrentMouseSelectedCarPart.CurrentGrid == null)
                return;

            //print(CurrentMouseSelectedCarPart.CurrentGrid + "= StartGrid   " + CurrentMouseSelectedMyGrid.Value + "= EndGrid");
            var path = CreatePath(CurrentMouseSelectedCarPart.CurrentGrid, CurrentMouseSelectedMyGrid.Value);

            if (path != null)
            {
                CurrentMouseSelectedCarPart.StartPathMove(path);
                SetDebugPath(path);
            }
        }

        #region //PathFind
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
                Gizmos.DrawLine(debugPath[i].transform.position + Vector3.up * 0.2f,
                                debugPath[i + 1].transform.position + Vector3.up * 0.2f);
            }
        }

        private IEnumerable<MyGrid> GetNeighbors(MyGrid grid)
        {
            var dirs = new[]
            {
            new Vector2Int( 0,  1),
            new Vector2Int( 1,  0),
            new Vector2Int( 0, -1),
            new Vector2Int(-1,  0)
        };
            foreach (var d in dirs)
            {
                var ni = grid.Index + d;
                if (_gridLookup.TryGetValue(ni, out var neigh))
                    yield return neigh;
            }
        }

        public MyGrid[] CreatePath(MyGrid start, MyGrid target)
        {
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
                    if (neighbor.IsOccupied || visited.Contains(neighbor))
                        continue;
                    visited.Add(neighbor);
                    cameFrom[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
            return null;
        }

        MyGrid[] RetracePath(MyGrid start, MyGrid end, Dictionary<MyGrid, MyGrid> cameFrom)
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

        void Update()
        {
            if (Input.GetMouseButtonUp(0) && CurrentMouseSelectedCarPart != null)
                CurrentMouseSelectedCarPart = null;
        }
    }
}