using UnityEngine.Rendering;
using UnityEngine;
using System.Linq;
using UniRx;
using System.Collections.Generic;
using System;

public class GridManager : MonoBehaviour {

    public static GridManager GridManagerScript;

    public SerializedDictionary<MyGrid, Vector2> MyGridsList;

    /*[HideInInspector]*/ public ReactiveProperty<MyGrid> CurrentMouseSelectedMyGrid;
    /*[HideInInspector]*/ public CarPart CurrentMouseSelectedCarPart;

    [SerializeField] Transform MyGridParents;

    MyGrid[] debugPath;

    CarPart _selectedCar;
    Camera _camera;
    Ray _ray;

    private void Awake()
    {
        GridManagerScript = this;
        _camera = Camera.main;

        for (int i = 0; i < MyGridParents.childCount; i++)
            MyGridsList.Add(MyGridParents.GetChild(i).GetComponent<MyGrid>(), 
                new Vector2(MyGridParents.GetChild(i).position.x, MyGridParents.GetChild(i).position.z));

        CurrentMouseSelectedMyGrid.Subscribe(OnMyGridChanged);
    }

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

    public MyGrid FindMyGridWithPos(Vector2 _inputPos) => MyGridsList.FirstOrDefault(SelectedMyGrid => SelectedMyGrid.Value == _inputPos).Key;

    void OnMyGridChanged(MyGrid grid)
    {
        if (grid == null)
            return;

        if (CurrentMouseSelectedCarPart == null)
            CurrentMouseSelectedCarPart = grid.CurrentCarPart;

        if (CurrentMouseSelectedCarPart == null || CurrentMouseSelectedCarPart.CurrentGrid == null)
            return;

        print(CurrentMouseSelectedCarPart.CurrentGrid + "= StartGrid   " + CurrentMouseSelectedMyGrid.Value + "= EndGrid");
        var path = CreatePath(CurrentMouseSelectedCarPart.CurrentGrid, CurrentMouseSelectedMyGrid.Value);

        if (path != null)
        {
            CurrentMouseSelectedCarPart.StartPathMove(path);
            SetDebugPath(path);
        }
    }

    public MyGrid[] CreatePath(MyGrid startGrid, MyGrid targetGrid)
    {
        print("Path Creating");

        var openSet = new List<MyGrid> { startGrid };
        var closedSet = new HashSet<MyGrid>();

        foreach (var kv in MyGridsList)
        {
            kv.Key.gCost = float.MaxValue;
            kv.Key.parent = null;
        }

        startGrid.gCost = 0;
        startGrid.hCost = Vector2.Distance(startGrid.Position, targetGrid.Position);

        while (openSet.Count > 0)
        {
            var currentGrid = openSet.OrderBy(n => n.fCost).ThenBy(n => n.hCost).First();

            if (currentGrid == targetGrid)
                return RetracePath(startGrid, targetGrid);

            openSet.Remove(currentGrid);
            closedSet.Add(currentGrid);

            foreach (var neighbor in GetNeighbors(currentGrid))
            {
                if (neighbor == null || neighbor.IsOccupied || closedSet.Contains(neighbor))
                    continue;

                float newGCost = currentGrid.gCost + Vector2.Distance(currentGrid.Position, neighbor.Position);
                if (newGCost < neighbor.gCost)
                {
                    neighbor.gCost = newGCost;
                    neighbor.hCost = Vector2.Distance(neighbor.Position, targetGrid.Position);
                    neighbor.parent = currentGrid;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return null;
    }

    List<MyGrid> GetNeighbors(MyGrid grid)
    {
        List<MyGrid> neighbors = new List<MyGrid>();

        Vector2[] directions = new Vector2[]
        {
        Vector2.up,
        Vector2.down,
        Vector2.left,
        Vector2.right
        };

        foreach (Vector2 dir in directions)
        {
            Vector2 neighborPos = grid.Position + dir;
            MyGrid neighbor = FindMyGridWithPos(neighborPos);
            if (neighbor != null)
                neighbors.Add(neighbor);
        }

        return neighbors;
    }

    MyGrid[] RetracePath(MyGrid startGrid, MyGrid endGrid)
    {
        List<MyGrid> path = new List<MyGrid>();
        MyGrid current = endGrid;

        while (current != startGrid)
        {
            path.Add(current);
            current = current.parent;
        }

        path.Reverse();
        return path.ToArray();
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0) && CurrentMouseSelectedCarPart != null)
            CurrentMouseSelectedCarPart = null;
    }
}
