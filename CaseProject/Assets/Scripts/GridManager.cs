using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;
using System.Linq;
using UniRx;

public class GridManager : MonoBehaviour {

    public static GridManager GridManagerScript;

    public SerializedDictionary<MyGrid, Vector2> MyGridsList;

    /*[HideInInspector]*/ public ReactiveProperty<MyGrid> CurrentMouseSelectedMyGrid;
    /*[HideInInspector]*/ public CarPart CurrentMouseSelectedCarPart;

    [SerializeField] Transform MyGridParents;

    MyGrid[] debugPath;

    private void Awake()
    {
        GridManagerScript = this;

        for (int i = 0; i < MyGridParents.childCount; i++)
            MyGridsList.Add(MyGridParents.GetChild(i).GetComponent<MyGrid>(), 
                new Vector2(MyGridParents.GetChild(i).position.x, MyGridParents.GetChild(i).position.z));

        CurrentMouseSelectedMyGrid.Subscribe(OnMyGridChanged);
    }

    public MyGrid FindMyGridWithPos(Vector2 _inputPos) => MyGridsList.FirstOrDefault(SelectedMyGrid => SelectedMyGrid.Value == _inputPos).Key;

    void OnMyGridChanged(MyGrid grid)
    {
        if (grid == null)
            return;

        if (CurrentMouseSelectedCarPart == null)
            CurrentMouseSelectedCarPart = grid.CurrentCarPart;

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

    public MyGrid[] CreatePath(MyGrid startGrid, MyGrid targetGrid)
    {
        print("Path Creating");

        Queue<MyGrid> queue = new Queue<MyGrid>();
        Dictionary<MyGrid, MyGrid> cameFrom = new Dictionary<MyGrid, MyGrid>();
        HashSet<MyGrid> visited = new HashSet<MyGrid>();

        queue.Enqueue(startGrid);
        visited.Add(startGrid);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current == targetGrid)
            {
                return RetracePath(startGrid, targetGrid, cameFrom);
            }

            foreach (var neighbor in GetSortedNeighborsByTarget(current, targetGrid))
            {
                if (neighbor == null || neighbor.IsOccupied || visited.Contains(neighbor))
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
        List<MyGrid> path = new List<MyGrid>();
        MyGrid current = end;

        while (current != start)
        {
            path.Add(current);
            current = cameFrom[current];
        }

        path.Reverse();
        return path.ToArray();
    }

    List<MyGrid> GetSortedNeighborsByTarget(MyGrid current, MyGrid target)
    {
        List<(MyGrid grid, float distance)> neighbors = new List<(MyGrid, float)>();

        Vector2[] directions = new Vector2[]
        {
        Vector2.up,
        Vector2.down,
        Vector2.left,
        Vector2.right
        };

        foreach (var dir in directions)
        {
            Vector2 checkPos = current.Position + dir;
            MyGrid neighbor = FindMyGridWithPos(checkPos);

            if (neighbor != null)
            {
                float distToTarget = Vector2.Distance(neighbor.Position, target.Position);
                neighbors.Add((neighbor, distToTarget));
            }
        }

        return neighbors.OrderBy(n => n.distance).Select(n => n.grid).ToList();
    }
    #endregion

    void Update()
    {
        if (Input.GetMouseButtonUp(0) && CurrentMouseSelectedCarPart != null)
            CurrentMouseSelectedCarPart = null;
    }
}
