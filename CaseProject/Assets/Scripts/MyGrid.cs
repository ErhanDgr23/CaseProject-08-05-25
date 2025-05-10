using _project.Car;
using System;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace _project.Grid
{
    public class MyGrid : MonoBehaviour
    {
        public event Action<CarPart> OnCarPartChanged;
        public bool IsOccupied;

        [HideInInspector] public CarPart CurrentCarPartt
        {
            get => _currentCarPartt;
            set
            {
                if (_currentCarPartt != value)
                {
                    _currentCarPartt = value;
                    // Burada event çağırabilirsin, örn:
                    OnCarPartChanged?.Invoke(_currentCarPartt);
                }
            }
        }
        [HideInInspector] public Vector2Int Index;

        public Vector2 Position => new Vector2(transform.position.x, transform.position.z);

        private CarPart _currentCarPartt;
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
            if (_gridManager.CurrentMouseSelectedMyGrid.Value == this)
                SelectGrid(null);
        }

        private void OnMouseUp()
        {
            SelectGrid(null);
        }

        public void CarPartChange(CarPart part)
        {
            CurrentCarPartt = part;
        }

        public void SelectGrid(MyGrid _grid)
        {
            _gridManager.CurrentMouseSelectedMyGrid.Value = _grid;
        }
    }
}