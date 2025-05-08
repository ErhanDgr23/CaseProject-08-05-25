using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using UniRx;
using System.Linq;

public class CarPart : MonoBehaviour {

    public MyGrid CurrentGrid;
    public bool IsHead, IsTail;

    /*[HideInInspector]*/ public ReactiveProperty<MyGrid> TargetGrid;

    [SerializeField] CarPart[] Followers;

    [SerializeField] private List<MyGrid> _pathGrid = new List<MyGrid>();
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private int _currentPathVal;

    private void Awake()
    {
        TargetGrid.Subscribe(TargetMyGridIsChanged);
    }

    private void Start()
    {
        _gridManager = GridManager.GridManagerScript;

        SetMyGridToCar(_gridManager.FindMyGridWithPos(new Vector2(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z))));
    }

    public void SetMyGridToCar(MyGrid _myGrid)
    {
        CurrentGrid = _myGrid;
        CurrentGrid.CurrentCarPart = this;
        CurrentGrid.IsOccupied = true;
    }

    public void StartPathMove(MyGrid[] pathGrids)
    {
        if (pathGrids.Length <= 0)
            return;

        _currentPathVal = 0;
        _pathGrid = pathGrids.ToList();
        MovingPath();
    }

    void MovingPath()
    {
        if (_currentPathVal >= _pathGrid.Count)
            return;

        StopCoroutine(WaitMove());
        StartCoroutine(WaitMove());
    }

    IEnumerator WaitMove()
    {
        TargetGrid.Value = _pathGrid[_currentPathVal];
        yield return new WaitForSeconds(0.1f);
    }

    void TargetMyGridIsChanged(MyGrid grid)
    {
        if(grid != null)
        {
            TargetGrid.Value = grid;
            MoveTheTargetGrid(grid);
        }
    }

    void MoveTheTargetGrid(MyGrid grid)
    {
        if (!IsHead)
            return;

        List<MyGrid> route = new List<MyGrid>();
        route.Add(CurrentGrid);
        for (int i = 0; i < Followers.Length; i++)
        {
            route.Add(Followers[i].CurrentGrid);
        }

        for (int i = 0; i < Followers.Length; i++)
        {
            Followers[i].GoTheGrid(route[i]);
        }

        //FollowerPart?.TargetMyGridIsChanged(CurrentGrid);

        GoTheGrid(grid);
    }

    public void GoTheGrid(MyGrid grid)
    {
        if (CurrentGrid != null)
        {
            CurrentGrid.CurrentCarPart = null;
            CurrentGrid.IsOccupied = false;
            CurrentGrid = null;
        }

        Vector3 targetDirection = _gridManager.CurrentMouseSelectedCarPart.IsTail == true ? (transform.position - grid.transform.position) : (grid.transform.position - transform.position);
        targetDirection.y = 0f;

        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.DORotateQuaternion(targetRotation, 0.1f);
        }

        transform.DOMove(new Vector3(grid.transform.position.x, 0f, grid.transform.position.z), 0.1f)
        .OnComplete(() => 
        { 
            SetMyGridToCar(grid);
            //_currentPathVal++;
            _pathGrid.Remove(grid);
            MovingPath();
        });
    }
}
