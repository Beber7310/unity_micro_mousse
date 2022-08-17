using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using UnityEngine;

public class EnvGenerator : MonoBehaviour
{
    public GameObject wall;
    public GameObject floor;

    // Start is called before the first frame update
    void Start()
    {
        var maze = new Maze(20, 20, true);
        InstantiateMaze(maze);         
    }

    public void InstantiateMaze(Maze maze)
    {
        float wallWitdh = wall.transform.localScale.z;
        float wallHeight = wall.transform.localScale.y;
        float floorWitdh = floor.transform.localScale.z;

        GameObject tmp;
        var firstLine = string.Empty;
        for (var y = 0; y < maze._height; y++)
        {
            for (var x = 0; x < maze._width; x++)
            {

                if (maze[x, y].HasFlag(CellState.Top))
                {
                    tmp=Instantiate(wall, new Vector3(x + wallWitdh / 2, wallHeight / 2, y), transform.rotation * Quaternion.Euler(0, 90, 0));
                    tmp.transform.parent = gameObject.transform;
                }
                if (maze[x, y].HasFlag(CellState.Left))
                {
                    tmp = Instantiate(wall, new Vector3(x, wallHeight / 2, y + wallWitdh / 2), transform.rotation * Quaternion.Euler(0, 0, 0));
                    tmp.transform.parent = gameObject.transform;
                }

            }
            tmp = Instantiate(wall, new Vector3(maze._width, wallHeight / 2, y + wallWitdh / 2), transform.rotation * Quaternion.Euler(0, 0, 0));
            tmp.transform.parent = gameObject.transform;
        }
        for (var x = 0; x < maze._width; x++)
        {
            tmp = Instantiate(wall, new Vector3(x + wallWitdh / 2, wallHeight / 2, maze._height), transform.rotation * Quaternion.Euler(0, 90, 0));
            tmp.transform.parent = gameObject.transform;
        }


        for (var y = 0; y < maze._height; y++)
        {
            for (var x = 0; x < maze._width; x++)
            {
                tmp = Instantiate(floor, new Vector3(x + floorWitdh / 2, 0, y + floorWitdh / 2), Quaternion.identity);
                tmp.transform.parent = gameObject.transform;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}

public static class Extensions
{
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, System.Random rng)
    {
        var e = source.ToArray();
        for (var i = e.Length - 1; i >= 0; i--)
        {
            var swapIndex = rng.Next(i + 1);
            yield return e[swapIndex];
            e[swapIndex] = e[i];
        }
    }

    public static CellState OppositeWall(this CellState orig)
    {
        return (CellState)(((int)orig >> 2) | ((int)orig << 2)) & CellState.Initial;
    }

    public static bool HasFlag(this CellState cs, CellState flag)
    {
        return ((int)cs & (int)flag) != 0;
    }
}

[Flags]
public enum CellState
{
    Top = 1,
    Right = 2,
    Bottom = 4,
    Left = 8,
    Visited = 128,
    Initial = Top | Right | Bottom | Left,
}

public struct RemoveWallAction
{
    public Point Neighbour;
    public CellState Wall;
}

public class Maze
{
    private readonly CellState[,] _cells;
    public readonly int _width;
    public readonly int _height;
    private readonly System.Random _rng;

    public Maze(int width, int height, bool center)
    {
        _width = width;
        _height = height;
        _cells = new CellState[width, height];
        for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
                _cells[x, y] = CellState.Initial;
        _rng = new System.Random();
        if (center)
        {
            _cells[(width / 2), (height / 2)] |= CellState.Visited;
            _cells[(width / 2) - 1, (height / 2)] |= CellState.Visited;
            _cells[(width / 2), (height / 2) - 1] |= CellState.Visited;
            _cells[(width / 2) - 1, (height / 2) - 1] |= CellState.Visited;

            _cells[(width / 2) - 1, (height / 2)] -= CellState.Top;
            _cells[(width / 2), (height / 2)] -= CellState.Top;

            _cells[(width / 2) , (height / 2)] -= CellState.Left;
            _cells[(width / 2) , (height / 2)-1] -= CellState.Left;

            VisitCell((width / 2) - _rng.Next(0,2), (height / 2) - _rng.Next(0, 2));
        }
        else
        {
            VisitCell(_rng.Next(width), _rng.Next(height));
        }
    }

    public CellState this[int x, int y]
    {
        get { return _cells[x, y]; }
        set { _cells[x, y] = value; }
    }

    public IEnumerable<RemoveWallAction> GetNeighbours(Point p)
    {
        if (p.X > 0) yield return new RemoveWallAction { Neighbour = new Point(p.X - 1, p.Y), Wall = CellState.Left };
        if (p.Y > 0) yield return new RemoveWallAction { Neighbour = new Point(p.X, p.Y - 1), Wall = CellState.Top };
        if (p.X < _width - 1) yield return new RemoveWallAction { Neighbour = new Point(p.X + 1, p.Y), Wall = CellState.Right };
        if (p.Y < _height - 1) yield return new RemoveWallAction { Neighbour = new Point(p.X, p.Y + 1), Wall = CellState.Bottom };
    }

    public void VisitCell(int x, int y)
    {
        this[x, y] |= CellState.Visited;
        foreach (var p in GetNeighbours(new Point(x, y)).Shuffle(_rng).Where(z => !(this[z.Neighbour.X, z.Neighbour.Y].HasFlag(CellState.Visited))))
        {
            this[x, y] -= p.Wall;
            this[p.Neighbour.X, p.Neighbour.Y] -= p.Wall.OppositeWall();
            VisitCell(p.Neighbour.X, p.Neighbour.Y);
        }
    }



    public void Display()
    {
        var firstLine = string.Empty;
        for (var y = 0; y < _height; y++)
        {
            var sbTop = new StringBuilder();
            var sbMid = new StringBuilder();
            for (var x = 0; x < _width; x++)
            {
                sbTop.Append(this[x, y].HasFlag(CellState.Top) ? "+---" : "+   ");
                sbMid.Append(this[x, y].HasFlag(CellState.Left) ? "|   " : "    ");
            }
            if (firstLine == string.Empty)
                firstLine = "   " + sbTop.ToString();
            Debug.Log("   " + sbTop + "+");
            Debug.Log("   " + sbMid + "|");
        }
        Debug.Log(firstLine);
    }
}
