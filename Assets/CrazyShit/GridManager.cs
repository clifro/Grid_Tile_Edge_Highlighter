using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge
{
    public void SetEdge(int Tile, Vector3 A, Vector3 B)
    {
        pointA = A;
        pointB = B;
        TileID = Tile;
    }

    public int TileID = -1;
    public Vector3 pointA;
    public Vector3 pointB;

    public bool edgeEquals(Edge e)
    {
        return pointA == e.pointA && pointB == e.pointB || pointA == e.pointB && pointB == e.pointA;
    }
}

public class GridManager : MonoBehaviour
{
    // Tweaks
    public int Rows;
    public int Columns;
    public float LineHeightOffset = 1f;
    public float CornerRadius = 20f;
    public float CornerSmoothPoints = 20f;

    // Prefabs / Refs
    public GameObject GridBlock;
    public LineRenderer LineRenderer;

    // Storage
    private List<Edge> edges = new List<Edge>();
    private List<Block> EdgeBlocks = new List<Block>();
    private bool gridDrawn = false;
    List<Vector3> points = new List<Vector3>();
    List<Vector3> arrangedPoints = new List<Vector3>();

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            Reset();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            DrawGrid();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            ArrangeEdges();
            DrawDebugLines();
        }
    }
    void DrawGrid()
    {
        if(!gridDrawn)
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    GameObject blockObject = Instantiate<GameObject>(GridBlock, new Vector3(j, 0, i), Quaternion.identity);
                    Block blockRef = blockObject.GetComponent<Block>();
                    blockRef.SetGridManager(this);
                    blockRef.SetColor(new Color(Random.Range(0.1f, 0.6f), Random.Range(0.1f, 1f), Random.Range(0.1f, 1f)), true);
                    blockRef.TileID = Columns * i + j;
                }
            }
        }

        gridDrawn = true;
    }

    private void Reset()
    {
        LineRenderer.enabled = false;
        LineRenderer.positionCount = 0;
        points = new List<Vector3>();
        arrangedPoints = new List<Vector3>();

        foreach (Block b in EdgeBlocks)
        {
            b.ResetColor();
        }

        EdgeBlocks.Clear();
        edges.Clear();
    }
    public void AddBlock(Block block)
    {
        if(!EdgeBlocks.Contains(block))
        {
            EdgeBlocks.Add(block);
            block.SetColor(Color.red);
        }

        ComputeEdge();
    }

    public void ComputeEdge()
    {
        edges.Clear();

        if (EdgeBlocks.Count > 0)
        {
            for (int i = 0; i < EdgeBlocks.Count; i++)
            {
                Vector3 point = EdgeBlocks[i].GetExtents();
                Vector3 LB = EdgeBlocks[i].transform.position + new Vector3(-point.x, LineHeightOffset, -point.z);
                Vector3 RB = EdgeBlocks[i].transform.position + new Vector3(point.x, LineHeightOffset, -point.z);
                Vector3 RT = EdgeBlocks[i].transform.position + new Vector3(point.x, LineHeightOffset, point.z);
                Vector3 LT = EdgeBlocks[i].transform.position + new Vector3(-point.x, LineHeightOffset, point.z);

                Edge edge = new Edge();
                edge.SetEdge(EdgeBlocks[i].TileID, LB, RB);
                AddEdgeUnique(edge);

                edge = new Edge();
                edge.SetEdge(EdgeBlocks[i].TileID, RB, RT);
                AddEdgeUnique(edge);

                edge = new Edge();
                edge.SetEdge(EdgeBlocks[i].TileID, RT, LT);
                AddEdgeUnique(edge);

                edge = new Edge();
                edge.SetEdge(EdgeBlocks[i].TileID, LT, LB);
                AddEdgeUnique(edge);
            }
        }
    }
    public void AddEdgeUnique(Edge e)
    {
        int indexToRemove = -1;

        for(int i = 0; i < edges.Count; i++)
        {
            if(edges[i].edgeEquals(e))
            {
                indexToRemove = i;
                break;
            }
        }

        if(indexToRemove > -1)
        {
            edges.RemoveAt(indexToRemove);
        }
        else
        {
            edges.Add(e);
        }
    }
    public void ArrangeEdges()
    {
        if (edges.Count > 0)
        {
            arrangedPoints.Add(edges[0].pointA);
            Vector3 startpoint = edges[0].pointB;
            Edge currentEdge = edges[0];
            edges.RemoveAt(0);
            List<Edge> multipleEdges = new List<Edge>();

            while (edges.Count > 0)
            {
                int indexToRemove = -1;
                multipleEdges.Clear();

                for (int i = 0; i < edges.Count; i++)
                {
                    if (startpoint == edges[i].pointA || startpoint == edges[i].pointB)
                    {
                        multipleEdges.Add(edges[i]);
                    }

                    if (multipleEdges.Count == 3) // skip the loop if max 3 edges found from a point 
                        break;
                }

                // for diagonal tiles case
                if(multipleEdges.Count > 1)
                {
                    for(int edgeIndex = 0; edgeIndex < multipleEdges.Count;)
                    {
                        if (multipleEdges[edgeIndex].TileID == currentEdge.TileID)
                        {
                            multipleEdges.RemoveAt(edgeIndex);
                            continue;
                        }

                        Vector3 currentEdgeVector = currentEdge.pointB - currentEdge.pointA;
                        Vector3 nextEdgeVector = multipleEdges[edgeIndex].pointB - multipleEdges[edgeIndex].pointA;

                        if (Vector3.Dot(currentEdgeVector, nextEdgeVector) != 0)
                        {
                            multipleEdges.RemoveAt(edgeIndex);
                            continue;
                        }

                        edgeIndex++;
                    }
                }

                if(multipleEdges.Count == 1)
                {
                    if (startpoint == multipleEdges[0].pointA)
                    {
                        arrangedPoints.Add(multipleEdges[0].pointA);
                        startpoint = multipleEdges[0].pointB;
                        currentEdge = multipleEdges[0];
                        edges.Remove(multipleEdges[0]);
                    }
                    else if (startpoint == multipleEdges[0].pointB)
                    {
                        arrangedPoints.Add(multipleEdges[0].pointB);
                        startpoint = multipleEdges[0].pointA;
                        currentEdge = multipleEdges[0];
                        edges.Remove(multipleEdges[0]);
                    }
                }
                else
                {
                    Debug.LogError("GRID - INVALID EDGE");
                    return;
                }

                if (indexToRemove > -1)
                    edges.RemoveAt(indexToRemove);
            }
        }
    }

    public void DrawDebugLines()
    {
        if(arrangedPoints.Count > 0)
        {
            if (LineRenderer != null)
            {
                for (int i = 0; i < arrangedPoints.Count; i++)
                {
                    if(((i + 1) % arrangedPoints.Count < arrangedPoints.Count) && ((i + 2) % arrangedPoints.Count < arrangedPoints.Count))
                    {
                        Vector3 line1 = arrangedPoints[(i + 1) % arrangedPoints.Count] - arrangedPoints[i];
                        Vector3 line2 = arrangedPoints[(i + 2) % arrangedPoints.Count] - arrangedPoints[(i + 1) % arrangedPoints.Count];

                        if (Vector3.Dot(line1, line2) != 0)
                        {
                            points.Add(arrangedPoints[(i + 1) % arrangedPoints.Count]);
                            continue;
                        }

                        Vector3 line1_unit = line1.normalized;
                        Vector3 line2_unit = line2.normalized;
                        Vector3 pointA = arrangedPoints[i] + line1_unit * (line1.magnitude * 0.5f + line1.magnitude * (100f - CornerRadius) * 0.5f / 100);
                        Vector3 circleCenter = pointA + line2_unit * line1.magnitude * CornerRadius * 0.5f / 100;
                        Vector3 pointB = circleCenter + line1_unit * line1.magnitude * CornerRadius * 0.5f / 100;

                        points.Add(pointA);

                        for (int circlePoint = 1; circlePoint < CornerSmoothPoints; circlePoint++)
                        {
                            float ratio = circlePoint / CornerSmoothPoints;
                            Vector3 pointOnCircle = circleCenter + Vector3.Slerp(pointA - circleCenter, pointB - circleCenter, ratio) * line1.magnitude;
                            points.Add(pointOnCircle);
                        }

                        points.Add(pointB);
                    }
                }

                LineRenderer.enabled = true;
                LineRenderer.positionCount = points.Count;

                for (int i = 0; i < points.Count; i++)
                {
                    LineRenderer.SetPosition(i, points[i]);
                }
            }
        }
    }
}
