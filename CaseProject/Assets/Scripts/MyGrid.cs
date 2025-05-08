using UnityEngine;

public class MyGrid : MonoBehaviour {

    public CarPart CurrentCarPart;
    public bool IsOccupied;

    [HideInInspector] public MyGrid parent;
    [HideInInspector] public float gCost;
    [HideInInspector] public float hCost;
    public float fCost => gCost + hCost;

    public Vector2 Position => new Vector2(transform.position.x, transform.position.z);

    private GridManager _gridManager;

    private void Start()
    {
        _gridManager = GridManager.GridManagerScript;
    }

    private void OnMouseEnter()
    {
        if (Input.GetButton("Fire1"))
            SelectGrid(this);
    }

    private void OnMouseDown()
    {
        SelectGrid(this);
    }

    private void OnMouseExit()
    {
        if(_gridManager.CurrentMouseSelectedMyGrid.Value == this)
            SelectGrid(null);
    }

    private void OnMouseUp()
    {


        SelectGrid(null);
    }

    public void SelectGrid(MyGrid _grid)
    {
        _gridManager.CurrentMouseSelectedMyGrid.Value = _grid;
    }
}
