using UnityEngine;
using DG.Tweening;
using UniRx;
using System.Collections;

public class CarPart : MonoBehaviour {

    public MyGrid CurrentGrid;
    public bool IsHead;

    /*[HideInInspector]*/ public ReactiveProperty<MyGrid> TargetGrid;

    [SerializeField] CarPart FollowerPart;

    private GridManager _gridManager;
    [SerializeField] private MyGrid[] _pathGrid;
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

        //if (TargetGrid.Value != null)
        //{
        //    StopCoroutine(WaitMove());
        //    StopMove();
        //}

        _currentPathVal = 0;
        _pathGrid = pathGrids;
        MovingPath();
    }

    void MovingPath()
    {
        if (_currentPathVal >= _pathGrid.Length)
            return;

        StopCoroutine(WaitMove());
        StartCoroutine(WaitMove());
    }

    IEnumerator WaitMove()
    {
        TargetGrid.Value = _pathGrid[_currentPathVal];
        yield return new WaitForSeconds(0.15f);
        _currentPathVal++;
        MovingPath();
    }

    void TargetMyGridIsChanged(MyGrid grid)
    {
        if(grid != null)
            MoveTheTargetGrid(grid);
    }

    void StopMove()
    {
        if (FollowerPart != null)
            FollowerPart.StopMove();

        transform.position = TargetGrid.Value.Position;

        Vector3 targetDirection = (TargetGrid.Value.transform.position - transform.position);
        targetDirection.y = 0f;

        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.DORotateQuaternion(targetRotation, 0f);
        }
    }

    void MoveTheTargetGrid(MyGrid grid)
    {
        FollowerPart?.TargetMyGridIsChanged(CurrentGrid);

        if (CurrentGrid != null)
        {
            CurrentGrid.IsOccupied = false;
            CurrentGrid.CurrentCarPart = null;
        }

        Vector3 targetDirection = (grid.transform.position - transform.position);
        targetDirection.y = 0f;

        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.DORotateQuaternion(targetRotation, 0.1f);
        }

        transform.DOMove(new Vector3(grid.transform.position.x, 0f, grid.transform.position.z), 0.1f)
        .OnComplete(() => { CurrentGrid.IsOccupied = false; CurrentGrid = null; SetMyGridToCar(grid); });
    }
}
