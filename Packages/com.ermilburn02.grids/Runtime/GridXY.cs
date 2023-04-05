using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ERmilburn02.Grids
{
    public class GridXY<TGridObject>
    {
        public class OnGridObjectChangedEventArgs : EventArgs
        {
            public int x;
            public int y;
        }
        public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;

        private readonly int _width;
        private readonly int _height;
        private readonly float _cellSize;
        private readonly Vector3 _origin;
        private readonly TGridObject[,] _gridArray;

        public GridXY(int width, int height, float cellSize, Vector3 origin, Func<GridXY<TGridObject>, int, int, TGridObject> createGridObject)
        {
            _width = width;
            _height = height;
            _cellSize = cellSize;
            _origin = origin;

            _gridArray = new TGridObject[width, height];

            for (int x = 0; x < _gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < _gridArray.GetLength(1);  y++)
                {
                    _gridArray[x, y] = createGridObject(this, x, y);
                }
            }
        }

        public int GetWidth() { return _width; }
        public int GetHeight() { return _height; }
        public float GetCellSize() { return _cellSize; }

        public Vector3 GetWorldPosition(int x, int y)
        {
            return new Vector3(x, y) * _cellSize + _origin;
        }

        public void GetXY(Vector3 worldPosition, out int x, out int y)
        {
            x = Mathf.FloorToInt((worldPosition - _origin).x / _cellSize);
            y = Mathf.FloorToInt((worldPosition - _origin).y / _cellSize);
        }

        public void TriggerGridObjectChanged(int x, int y)
        {
            OnGridObjectChanged?.Invoke(this, new OnGridObjectChangedEventArgs { x = x, y = y });
        }

        public void SetGridObject(int x, int y, TGridObject value)
        {
            if (IsValidGridPosition(x, y))
            {
                _gridArray[x, y] = value;
                TriggerGridObjectChanged(x, y);
            }
        }

        public void SetGridObject(Vector3 worldPosition,  TGridObject value)
        {
            GetXY(worldPosition, out int x, out int y);
            SetGridObject(x, y, value);
        }

        public TGridObject GetGridObject(int x, int y)
        {
            if (IsValidGridPosition(x, y))
            {
                return _gridArray[x, y];
            }
            else
            {
                return default(TGridObject);
            }
        }

        public TGridObject GetGridObject(Vector3 worldPosition)
        {
            GetXY(worldPosition, out int x, out int y);
            return GetGridObject(x, y);
        }

        public bool IsValidGridPosition(int x, int y)
        {
            return (x >= 0 && y >= 0 && x < _width && y < _height);
        }
    }
}
