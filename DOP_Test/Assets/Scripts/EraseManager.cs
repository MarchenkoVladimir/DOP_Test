using UnityEngine;

public class EraseManager : MonoBehaviour
{
    [SerializeField] private InputHandler _inputHandler; 

    private GridColliderManager _colliderGenerator;
    private Material _eraseMaterial;
    private Camera _mainCamera;
    private SpriteRenderer _spriteRenderer;
    private Texture2D _maskTexture;
    private Vector2 _previousMousePos;
    private bool _isDrawing = false;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _colliderGenerator = GetComponent<GridColliderManager>();
        _eraseMaterial = new Material(_spriteRenderer.material);
        _spriteRenderer.material = _eraseMaterial;
    }

    void Start()
    {
        _mainCamera = Camera.main;
        Texture2D mainTexture = _spriteRenderer.sprite.texture;

        _maskTexture = new Texture2D(mainTexture.width, mainTexture.height, TextureFormat.R8, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        Color[] pixels = new Color[mainTexture.width * mainTexture.height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white; 
        }

        _maskTexture.SetPixels(pixels);
        _maskTexture.Apply();

        _eraseMaterial.SetTexture("_MaskTex", _maskTexture);
        _spriteRenderer.material = _eraseMaterial;
        _colliderGenerator.GenerateGridColliders(_spriteRenderer.sprite.texture);
    }
    
    void Update()
    {
        if (_inputHandler.IsDrawing)
        {
            HandleInput(_inputHandler.ScreenPosition);
        }
        else if (!_inputHandler.IsDrawing && _isDrawing)
        {
            _isDrawing = false;
        }
    }

    void HandleInput(Vector2 screenPosition)
    {
        Vector2 mouseWorldPos = _mainCamera.ScreenToWorldPoint(screenPosition);
        Vector2 spriteLocalPos = transform.InverseTransformPoint(mouseWorldPos);

        Vector2 uv = new Vector2(
            (spriteLocalPos.x / _spriteRenderer.bounds.size.x) + 0.5f,
            (spriteLocalPos.y / _spriteRenderer.bounds.size.y) + 0.5f
        );

        if (!_isDrawing)
        {
            _previousMousePos = uv;
            _isDrawing = true;
        }

        DrawLine(_previousMousePos, uv, _inputHandler.BrushSize);
        _previousMousePos = uv;
        _eraseMaterial.SetTexture("_MaskTex", _maskTexture);
        
        
    }

    void DrawLine(Vector2 start, Vector2 end, float brushSize)
    {
        int texWidth = _maskTexture.width;
        int texHeight = _maskTexture.height;

        int startX = (int)(start.x * texWidth);
        int startY = (int)(start.y * texHeight);
        int endX = (int)(end.x * texWidth);
        int endY = (int)(end.y * texHeight);

        int brushRadius = (int)(brushSize * Mathf.Max(texWidth, texHeight));

        int dx = Mathf.Abs(endX - startX);
        int dy = Mathf.Abs(endY - startY);
        int sx = startX < endX ? 1 : -1;
        int sy = startY < endY ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            DrawCircle(startX, startY, brushRadius);

            if (startX == endX && startY == endY) break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                startX += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                startY += sy;
            }
        }

        _maskTexture.Apply();
    }

    void DrawCircle(int centerX, int centerY, int radius)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    int px = centerX + x;
                    int py = centerY + y;

                    if (px >= 0 && px < _maskTexture.width && py >= 0 && py < _maskTexture.height)
                    {
                        _maskTexture.SetPixel(px, py, Color.black);
                    }
                }
            }
        }
    }

    void OnDestroy()
    {
        if (_maskTexture != null)
        {
            Destroy(_maskTexture);
        }
    }
}