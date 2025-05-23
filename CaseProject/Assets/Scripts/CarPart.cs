﻿using System.Collections.Generic;
using System.Collections;
using _project.Interface;
using _project.Settings;
using _project.Enums;
using _project.Grid;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using UniRx;
using UnityEngine.UIElements;

namespace _project.Car
{
    public class CarPart : MonoBehaviour, IColorChanger
    {
        public bool IsHead, IsTail, IsStopped;
        public ColorEnum MyColor;

        [SerializeField] public CarTypeEnum MyType;
        [SerializeField] CarPart[] Followers;

        [Space(50)]
        [Header("Oto Full")]
        /*[HideInInspector]*/ public Transform LookFrontObj;
        /*[HideInInspector]*/ public Transform LookBakObj;
        /*[HideInInspector]*/ public int MaxPassengerVal;
        /*[HideInInspector]*/ public MyGrid CurrentGrid;
        /*[HideInInspector]*/ public MyGrid TargetGrid;

        [SerializeField] private List<MyGrid> _pathGrid = new List<MyGrid>();
        [SerializeField] private GridManager _gridManager;
        [SerializeField] private int _currentPathVal;

        SettingSO _setting;

        private void Awake()
        {
            transform.parent.GetComponent<CarCountainer>().PassengersFulled += DestroyAnim;
            //TargetGrid.Subscribe(TargetMyGridIsChanged);
        }

        private void Start()
        {
            _setting = SettingSO.Instance;
            _gridManager = GridManager.GridManagerScript;

            MaxPassengerVal = _setting.GetMaxPassengerWithType(MyType);
            SetMyGridToCar(_gridManager.FindMyGridWithPos(new Vector2(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z))));
        }

        public void SetMyGridToCar(MyGrid _myGrid)
        {
            CurrentGrid = _myGrid;
            CurrentGrid.CarPartChange(this);
            CurrentGrid.IsOccupied = true;
        }

        public void StartPathMove(MyGrid[] pathGrids)
        {
            StopCoroutine(WaitMove());

            foreach (var item in Followers)
                item._pathGrid.Clear();

            if (pathGrids.Length <= 0)
                return;

            _currentPathVal = 0;
            _pathGrid = pathGrids.ToList();
            TargetMyGridIsChanged(_pathGrid[0]);
        }

        void MovingPath()
        {
            if (_currentPathVal >= _pathGrid.Count)
                return;

            if (IsStopped)
            {
                StopTheCar();
                return;
            }

            StopCoroutine(WaitMove());
            StartCoroutine(WaitMove());
        }

        IEnumerator WaitMove()
        {
            TargetMyGridIsChanged(_pathGrid[_currentPathVal]);
            yield return new WaitForSeconds(0.125f);
        }

        void TargetMyGridIsChanged(MyGrid grid)
        {
            if (grid == CurrentGrid)
                return;

            if (IsStopped)
            {
                StopTheCar();
                return;
            }

            if (grid != null)
            {
                TargetGrid = grid;
                MoveTheTargetGrid(grid);
            }
        }

        void MoveTheTargetGrid(MyGrid grid)
        {
            if (!IsHead || grid == null)
                return;

            List<MyGrid> route = new List<MyGrid>();
            MyGrid previousGrid = CurrentGrid;

            for (int i = 0; i < Followers.Length; i++)
            {
                route.Add(previousGrid);
                previousGrid = Followers[i].CurrentGrid;
            }

            for (int i = 0; i < Followers.Length; i++)
                Followers[i].GoTheGrid(route[i]);

            GoTheGrid(grid);
        }

        public void GoTheGrid(MyGrid grid)
        {
            transform.DOKill(complete: false);

            if (!grid)
                return;

            if (CurrentGrid != null)
            {
                CurrentGrid.CarPartChange(null);
                CurrentGrid.IsOccupied = false;
                CurrentGrid = null;
            }

            Vector3 targetDirection = Vector3.zero;
            targetDirection = _gridManager.CurrentMouseSelectedCarPart.Value.IsTail == true ?
(transform.position - grid.transform.position) : (grid.transform.position - transform.position);

            if (targetDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                transform.DORotateQuaternion(targetRotation, 0.12f);
            }

            transform.DOMove(new Vector3(grid.transform.position.x, 0f, grid.transform.position.z), 0.12f)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                if (IsStopped || grid == null || _pathGrid == null)
                    return;

                SetMyGridToCar(grid);
                _pathGrid.Remove(grid);

                if (!IsStopped && _pathGrid.Count > 0)
                    MovingPath();
            });
        }

        private void LateUpdate()
        {
            if (_gridManager.CurrentMouseSelectedCarPart.Value == null)
                return;

            if (IsHead || IsTail)
                return;

            if (LookBakObj != null && _gridManager.CurrentMouseSelectedCarPart.Value.IsTail)
                transform.GetChild(0).transform.rotation = Quaternion.LookRotation(transform.GetChild(0).transform.position - LookBakObj.position);
            else if (LookFrontObj != null && !_gridManager.CurrentMouseSelectedCarPart.Value.IsTail)
                transform.GetChild(0).transform.rotation = Quaternion.LookRotation(LookFrontObj.position - transform.GetChild(0).transform.position);
            else
                transform.GetChild(0).transform.eulerAngles = Vector3.zero;
        }

        public void ColorChanged(ColorEnum color)
        {
            MyColor = color;
        }

        public void DestroyAnim()
        {
            CancelInvoke("DestroyInvoke");
            Invoke("DestroyInvoke", 0.3f);
        }

        void DestroyInvoke()
        {
            transform.DOKill(complete: false);
            transform.DOScale(Vector3.zero, 0.4f);
            CurrentGrid.CurrentCarPartt = null;
            CurrentGrid.IsOccupied = false;
        }

        public void StopTheCar()
        {
            _pathGrid.Clear();
            TargetGrid = null;
            IsStopped = true;
        }
    }
}