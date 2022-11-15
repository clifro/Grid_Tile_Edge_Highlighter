using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private List<Edge> mEdges = new List<Edge>();
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
            arrangedPoints = GetArrangedVertices(mEdges);
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
        mEdges.Clear();
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
        mEdges.Clear();

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

        for(int i = 0; i < mEdges.Count; i++)
        {
            if(mEdges[i].edgeEquals(e))
            {
                indexToRemove = i;
                break;
            }
        }

        if(indexToRemove > -1)
        {
            mEdges.RemoveAt(indexToRemove);
        }
        else
        {
            mEdges.Add(e);
        }
    }

    public List<Vector3> GetArrangedVertices(List<Edge> gridEdges)
    {
        if(gridEdges == null || gridEdges.Count == 0)
        {
            return null;
        }

        List<Vector3> arrangedVertices = new List<Vector3>();

        List<Edge> edges = new List<Edge>(gridEdges);

        Edge currentEdge = edges.First();
		arrangedVertices.Add(currentEdge.pointA);
		Vector3 currentVertex = currentEdge.pointB;
		edges.RemoveAt(0);

		while (edges.Count > 0)
		{
            int maxVertexConnectedEdges = 3; // excluding the current edge
            int? nextEdgeIndex = null;

			for (int i = 0; i < edges.Count; i++)
			{
				if (currentVertex == edges[i].pointA || currentVertex == edges[i].pointB)
				{
					Edge edge = edges[i];

                    if(nextEdgeIndex == null)
                    {
                        nextEdgeIndex = i;
                    }
                    else if(edge.TileID != currentEdge.TileID)
                    {
                        Vector3 currentEdgeDirection = currentEdge.pointB - currentEdge.pointA;
                        Vector3 nextEdgeDirection = edge.pointB - edge.pointA;

                        if (Vector3.Dot(currentEdgeDirection, nextEdgeDirection) == 0)
                        {
                            nextEdgeIndex = i;
                            break;
                        }
                    }

                    maxVertexConnectedEdges--;

                    if(maxVertexConnectedEdges <= 0)
                    {
                        break;
                    }
				}
			}

            Edge nextEdge = edges[nextEdgeIndex.Value];

            if (currentVertex == nextEdge.pointA)
			{
                currentEdge = nextEdge;
				arrangedVertices.Add(currentEdge.pointA);
				currentVertex = currentEdge.pointB;
			}
			else if (currentVertex == nextEdge.pointB)
			{
                currentEdge = nextEdge;
				arrangedVertices.Add(currentEdge.pointB);
				currentVertex = currentEdge.pointA;
			}

			edges.RemoveAt(nextEdgeIndex.Value);
		}

        return arrangedVertices;
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
