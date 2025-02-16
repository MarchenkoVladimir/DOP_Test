using System.Collections;
using UnityEngine;

public class ErasableSprite : MonoBehaviour
{ 
    [SerializeField] private float _eraseRadius = 20f;
    [SerializeField] private float _colliderUpdateDelay = 0.3f;

    private SpriteRenderer _spriteRenderer;
    private Texture2D _erasableTexture;
    private Sprite _originalSprite;
    private PolygonColliderGenerator _colliderGenerator;

    private Vector2 _lastErasePoint;
    private bool _isErasing;
    private Color32[] _texturePixels;

    private void Awake()
    {
        _colliderGenerator = GetComponent<PolygonColliderGenerator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        InitializeTexture();
    }

    private void Update()
    {
        HandleInput();
    }

    private void InitializeTexture()
    {
        _originalSprite = _spriteRenderer.sprite;
        var sourceTexture = _originalSprite.texture;

        _erasableTexture = new Texture2D(sourceTexture.width, sourceTexture.height, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        _texturePixels = sourceTexture.GetPixels32();
        _erasableTexture.SetPixels32(_texturePixels);
        _erasableTexture.Apply();
        
    }

    private void HandleInput()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0)) _isErasing = true;
        if (Input.GetMouseButtonUp(0)) StopErasing();
        if (_isErasing) EraseAtMousePosition();
#endif

#if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began) _isErasing = true;
            if (touch.phase == TouchPhase.Ended) StopErasing();
            if (_isErasing) EraseAtTouchPosition(touch.position);
        }
#endif
    }

    private void StopErasing()
    {
        _isErasing = false;
        _lastErasePoint = Vector2.zero;
    }


    private void EraseAtMousePosition()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 localPos = transform.InverseTransformPoint(mousePosition);
        UpdateTexturePosition(localPos);
    }

    private void EraseAtTouchPosition(Vector2 touchPosition)
    {
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(touchPosition);
        Vector2 localPos = transform.InverseTransformPoint(worldPosition);
        UpdateTexturePosition(localPos);
        _colliderGenerator.RefreshPolygonCollider();
    }

    private void UpdateTexturePosition(Vector2 localPos)
    {
        Vector2 texturePos = new Vector2(
            Mathf.Clamp((localPos.x / _originalSprite.bounds.size.x + 0.5f) * _erasableTexture.width, 0, _erasableTexture.width - 1),
            Mathf.Clamp((localPos.y / _originalSprite.bounds.size.y + 0.5f) * _erasableTexture.height, 0, _erasableTexture.height - 1)
        );

        if (_lastErasePoint == Vector2.zero) _lastErasePoint = texturePos;

        EraseBetweenPoints(_lastErasePoint, texturePos);
        _lastErasePoint = texturePos;

        ApplyTextureChanges();
    }

    private void EraseBetweenPoints(Vector2 start, Vector2 end)
    {
        int steps = Mathf.CeilToInt(Vector2.Distance(start, end));

        for (int i = 0; i <= steps; i++)
        {
            Vector2 position = Vector2.Lerp(start, end, i / (float)steps);
            EraseCircle(position);
        }
    }

    private void EraseCircle(Vector2 center)
    {
        int radius = Mathf.RoundToInt(_eraseRadius);
        int xStart = Mathf.Clamp((int)(center.x - radius), 0, _erasableTexture.width - 1);
        int xEnd = Mathf.Clamp((int)(center.x + radius), 0, _erasableTexture.width - 1);
        int yStart = Mathf.Clamp((int)(center.y - radius), 0, _erasableTexture.height - 1);
        int yEnd = Mathf.Clamp((int)(center.y + radius), 0, _erasableTexture.height - 1);

        for (int y = yStart; y <= yEnd; y++)
        {
            for (int x = xStart; x <= xEnd; x++)
            {
                if (Vector2.Distance(center, new Vector2(x, y)) < radius)
                {
                    _texturePixels[y * _erasableTexture.width + x] = Color.clear;
                }
            }
        }
    }

    private void ApplyTextureChanges()
    {
        _erasableTexture.SetPixels32(_texturePixels);
        _erasableTexture.Apply();
        
        Rect rect = new Rect(0, 0, _erasableTexture.width, _erasableTexture.height);
        _spriteRenderer.sprite = Sprite.Create(_erasableTexture, rect, new Vector2(0.5f, 0.5f), 100);
    }
}