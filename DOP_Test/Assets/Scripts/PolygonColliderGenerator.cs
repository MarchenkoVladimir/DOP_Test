using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PolygonCollider2D))]
public class PolygonColliderGenerator : MonoBehaviour
{
    private PolygonCollider2D polygonCollider;

    private void Awake()
    {
        polygonCollider = GetComponent<PolygonCollider2D>();
    }

    private void Start()
    {
        RefreshPolygonCollider();
    }

    public void RefreshPolygonCollider()
    {
      Destroy(polygonCollider);
      polygonCollider = gameObject.AddComponent<PolygonCollider2D>();
    }
}