using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ErasableSprite : MonoBehaviour
{ 
    [SerializeField] private float _eraseRadius = 20f;

    private SpriteRenderer _spriteRenderer;
    private Texture2D _erasableTexture;
    private Sprite _originalSprite;
    private PolygonColliderGenerator _colliderGenerator;

    private Vector2 _lastErasePoint;
    private bool _isErasing;

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
    
        _erasableTexture = new Texture2D(_originalSprite.texture.width, _originalSprite.texture.height, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
    
        _erasableTexture.SetPixels(_originalSprite.texture.GetPixels());
        _erasableTexture.Apply();
    
        UpdateSprite();
    }
    
    private void HandleInput()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
            _isErasing = true;

        if (Input.GetMouseButtonUp(0))
        {
            _isErasing = false;
            _lastErasePoint = Vector2.zero;
        }
            

        if (_isErasing)
            EraseAtMousePosition();
#endif

#if UNITY_ANDROID || UNITY_IOS
    if (Input.touchCount > 0)
    {
        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
            _isErasing = true;

        if (touch.phase == TouchPhase.Ended)
        {
            _isErasing = false;
            _lastErasePoint = Vector2.zero;
        }

        if (_isErasing)
            EraseAtTouchPosition(touch.position);
    }
#endif
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
    }

    private void UpdateTexturePosition(Vector2 localPos)
    {
        Vector2 texturePos = new Vector2(
            Mathf.Clamp((localPos.x / _originalSprite.bounds.size.x + 0.5f) * _erasableTexture.width, 0, _erasableTexture.width - 1),
            Mathf.Clamp((localPos.y / _originalSprite.bounds.size.y + 0.5f) * _erasableTexture.height, 0, _erasableTexture.height - 1)
        );

        if (_lastErasePoint == Vector2.zero)
            _lastErasePoint = texturePos;

        EraseBetweenPoints(_lastErasePoint, texturePos);
        _lastErasePoint = texturePos;

        _erasableTexture.Apply();
        UpdateSprite();
        _colliderGenerator.RefreshPolygonCollider();
    }
    
    private void EraseBetweenPoints(Vector2 start, Vector2 end)
    {
        float distance = Vector2.Distance(start, end);
        int steps = Mathf.CeilToInt(distance);

        for (int i = 0; i <= steps; i++)
        {
            Vector2 position = Vector2.Lerp(start, end, i / (float)steps);
            EraseCircle(position);
        }
    }
    
    private void EraseCircle(Vector2 center)
    {
        int radius = Mathf.RoundToInt(_eraseRadius);
        int xStart = Mathf.Clamp((int)(center.x - radius), 0, _erasableTexture.width);
        int xEnd = Mathf.Clamp((int)(center.x + radius), 0, _erasableTexture.width);
        int yStart = Mathf.Clamp((int)(center.y - radius), 0, _erasableTexture.height);
        int yEnd = Mathf.Clamp((int)(center.y + radius), 0, _erasableTexture.height);

        for (int x = xStart; x < xEnd; x++)
        {
            for (int y = yStart; y < yEnd; y++)
            {
                if (Vector2.Distance(center, new Vector2(x, y)) < radius)
                {
                    _erasableTexture.SetPixel(x, y, Color.clear);
                }
            }
        }
    }
    
    private void UpdateSprite()
    {
        Rect rect = new Rect(0, 0, _erasableTexture.width, _erasableTexture.height);
        Sprite newSprite = Sprite.Create(_erasableTexture, rect, new Vector2(0.5f, 0.5f), 100);
        _spriteRenderer.sprite = newSprite;
    }
}