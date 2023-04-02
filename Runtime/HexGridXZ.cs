using System;
using System.Collections.Generic;
using UnityEngine;

namespace ERmilburn02.Grids
{
    public class HexGridXZ<TGridObject>
    {
        private const float HEX_VERTICAL_GRID_OFFSET_MULTIPLIER = 0.75f;

        public class OnGridObjectChangedEventArgs : EventArgs
        {
            public int x;
            public int z;
        }
        public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;

        private readonly int _width;
        private readonly int _height;
        private readonly float _cellSize;
        private readonly Vector3 _origin;
        private readonly TGridObject[,] _gridArray;

        public HexGridXZ(int width, int height, float cellSize, Vector3 origin, Func<HexGridXZ<TGridObject>, int, int, TGridObject> createGridObject)
        {
            _width = width;
            _height = height;
            _cellSize = cellSize;
            _origin = origin;

            _gridArray = new TGridObject[width, height];

            for (int x = 0; x < _gridArray.GetLength(0); x++)
            {
                for (int y = 0;  y < _gridArray.GetLength(1); y++)
                {
                    _gridArray[x, y] = createGridObject(this, x, y);
                }
            }
        }

        public int GetWidth() { return _width; }

        public int GetHeight() { return _height; }

        public float GetCellSize() { return _cellSize; }

        public Vector3 GetWorldPosition(int x, int z)
        {
            return
                new Vector3(x, 0, 0) * _cellSize +
                _cellSize * HEX_VERTICAL_GRID_OFFSET_MULTIPLIER * new Vector3(0, 0, z) +
                ((Mathf.Abs(z) % 2) == 1 ? _cellSize * 0.5f * new Vector3(1, 0, 0) : Vector3.zero) +
                _origin;
        }

        public void GetXZ(Vector3 worldPosition, out int x, out int z)
        {
            int roughX = Mathf.RoundToInt((worldPosition - _origin).x / _cellSize);
            int roughZ = Mathf.RoundToInt((worldPosition - _origin).z / _cellSize / HEX_VERTICAL_GRID_OFFSET_MULTIPLIER);

            Vector3Int roughXZ = new Vector3Int(roughX, 0, roughZ);

            bool oddRow = roughZ % 2 == 1;

            List<Vector3Int> neighbourXZList = new List<Vector3Int>
            {
                roughXZ + new Vector3Int(-1, 0, 0),
                roughXZ + new Vector3Int(+1, 0, 0),
                roughXZ + new Vector3Int(oddRow ? +1 : -1, 0, +1),
                roughXZ + new Vector3Int(0, 0, +1),
                roughXZ + new Vector3Int(oddRow ? +1 : -1, 0, -1),
                roughXZ + new Vector3Int(0, 0, -1),
            };

            Vector3Int closestXZ = roughXZ;

            foreach (Vector3Int neighbourXZ in neighbourXZList)
            {
                if (Vector3.Distance(worldPosition, GetWorldPosition(neighbourXZ.x, neighbourXZ.z)) < Vector3.Distance(worldPosition, GetWorldPosition(closestXZ.x, closestXZ.z)))
                {
                    closestXZ = neighbourXZ;
                }
            }

            x = closestXZ.x;
            z = closestXZ.z;
        }

        public void SetGridObject(int x, int z, TGridObject value)
        {
            if (IsValidGridPosition(x, z))
            {
                _gridArray[x, z] = value;
                TriggerGridObjectChanged(x, z);
            }
        }

        public void TriggerGridObjectChanged(int x, int z)
        {
            OnGridObjectChanged?.Invoke(this, new OnGridObjectChangedEventArgs { x = x, z = z });
        }

        public void SetGridObject(Vector3 worldPosition, TGridObject value)
        {
            GetXZ(worldPosition, out int x, out int z);
            SetGridObject(x, z, value);
        }

        public TGridObject GetGridObject(int x, int z)
        {
            if (IsValidGridPosition(x, z))
            {
                return _gridArray[x, z];
            }
            else
            {
                return default(TGridObject);
            }
        }

        public TGridObject GetGridObject(Vector3 worldPosition)
        {
            GetXZ(worldPosition, out int x, out int z);
            return GetGridObject(x, z);
        }

        public Vector2Int ValidateGridPosition(Vector2Int gridPosition)
        {
            return new Vector2Int(
                Mathf.Clamp(gridPosition.x, 0, _width - 1),
                Mathf.Clamp(gridPosition.y, 0, _height - 1)
            );
        }

        public bool IsValidGridPosition(Vector2Int gridPosition)
        {
            int x = gridPosition.x;
            int z = gridPosition.y;

            return IsValidGridPosition(x, z);
        }

        public bool IsValidGridPosition(int x, int z)
        {
            return (x >= 0 && z >= 0 && x < _width && z < _height);
        }
    }
}
