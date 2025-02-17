using UnityEngine;
using UnityEngine.Serialization;

public class GridColliderManager : MonoBehaviour
{
    [SerializeField] private float _pixelsPerUnit = 100f;
    [SerializeField] private int _cellSize = 10; 
    [SerializeField] private float _brushSizeMultiplier = 7; 
    [SerializeField] private InputHandler _inputHandler; 

    private BoxCollider2D[,] _gridColliders;

    void Update()
    {
        if (_inputHandler.IsDrawing)
        {
            RemoveCollidersUnderBrush();
        }
    }

    public void GenerateGridColliders(Texture2D texture)
    {
        if (texture == null)
        {
            Debug.LogError("Texture not assigned!");
            return;
        }

        int width = texture.width;
        int height = texture.height;

        int gridWidth = Mathf.CeilToInt((float)width / _cellSize);
        int gridHeight = Mathf.CeilToInt((float)height / _cellSize);

        _gridColliders = new BoxCollider2D[gridWidth, gridHeight];

        for (int y = 0; y < height; y += _cellSize)
        {
            for (int x = 0; x < width; x += _cellSize)
            {
                if (HasOpaquePixels(texture, x, y, _cellSize))
                {
                    CreateColliderForCell(texture, x, y, _cellSize);
                }
            }
        }
    }

    private void CreateColliderForCell(Texture2D texture, int startX, int startY, int size)
    {
        float centerX = (startX + size / 2f - texture.width / 2f) / _pixelsPerUnit;
        float centerY = (startY + size / 2f - texture.height / 2f) / _pixelsPerUnit;

        GameObject colliderObject = new GameObject("GridCollider");
        colliderObject.transform.parent = transform;
        colliderObject.transform.localPosition = new Vector3(centerX, centerY, 0);

        BoxCollider2D boxCollider = colliderObject.AddComponent<BoxCollider2D>();
        boxCollider.size = new Vector2(size / _pixelsPerUnit, size / _pixelsPerUnit);

        int gridX = startX / _cellSize;
        int gridY = startY / _cellSize;
        _gridColliders[gridX, gridY] = boxCollider;
    }

    private bool HasOpaquePixels(Texture2D texture, int startX, int startY, int size)
    {
        int width = texture.width;
        int height = texture.height;

        for (int y = startY; y < startY + size && y < height; y++)
        {
            for (int x = startX; x < startX + size && x < width; x++)
            {
                Color pixelColor = texture.GetPixel(x, y);
               
                if (pixelColor.a > 0)
                {
                    return true; 
                }
            }
        }
        return false; 
    }

    private void RemoveCollidersUnderBrush()
    {
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(_inputHandler.ScreenPosition);
        float brushSizeWorld = _inputHandler.BrushSize * _brushSizeMultiplier;

        for (int x = 0; x < _gridColliders.GetLength(0); x++)
        {
            for (int y = 0; y < _gridColliders.GetLength(1); y++)
            {
                BoxCollider2D collider = _gridColliders[x, y];
                if (collider != null)
                {
                    if (Vector2.Distance(worldPosition, collider.transform.position) <= brushSizeWorld)
                    {
                        Destroy(collider.gameObject);
                        _gridColliders[x, y] = null;
                    }
                }
            }
        }
    }
}