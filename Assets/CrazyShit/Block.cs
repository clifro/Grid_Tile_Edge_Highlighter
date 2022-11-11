using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    private GridManager gridManager;
    private Renderer BRenderer;
    private Color setColor;
    // Start is called before the first frame update
    void Awake()
    {
        BRenderer = GetComponentInChildren<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetGridManager(GridManager manager)
    {
        gridManager = manager;
    }

    private void OnMouseUp()
    {
        if (gridManager != null)
        {
            gridManager.AddBlock(this);
        }
    }
    public void SetColor(Color color, bool cache = false)
    {
        if(cache)
            setColor = color;

        BRenderer.material.color = color;
    }

    public void ResetColor()
    {
        BRenderer.material.color = setColor;
    }

    public Vector3 GetExtents()
    {
        return BRenderer.bounds.extents;
    }
}
