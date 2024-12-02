using UnityEngine;

public class Grid2D<T>
{
    private readonly T[] _data;

    public Grid2D(Vector2Int size, Vector2Int offset)
    {
        Size = size;
        Offset = offset;

        _data = new T[size.x * size.y];
    }

    public Vector2Int Size { get; }
    public Vector2Int Offset { get; set; }

    public T this[int x, int y]
    {
        get => this[new Vector2Int(x, y)];
        set => this[new Vector2Int(x, y)] = value;
    }

    public T this[Vector2Int pos]
    {
        get
        {
            pos += Offset;
            return _data[GetIndex(pos)];
        }
        set
        {
            pos += Offset;
            _data[GetIndex(pos)] = value;
        }
    }

    public int GetIndex(Vector2Int pos)
    {
        return pos.x + Size.x * pos.y;
    }

    public bool InBounds(Vector2Int pos)
    {
        return new RectInt(Vector2Int.zero, Size).Contains(pos + Offset);
    }

    public T[][] GetAs2DArray()
    {
        var returnArray = new T[Size.x][];
        for (var i = 0; i < Size.x; i++)
        {
            returnArray[i] = new T[Size.y];
        }

        for (var i = 0; i < Size.x; i++)
        {
            for (var j = 0; j < Size.y; j++)
            {
                returnArray[i][j] = this[i, j];
            }
        }

        return returnArray;
    }
}