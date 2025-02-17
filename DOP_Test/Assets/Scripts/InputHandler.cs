using UnityEngine;

public class InputHandler : MonoBehaviour
{
    [SerializeField] [Range(0f, 0.1f)] private float _brushSize = 0.04f;

    public float BrushSize => _brushSize; // Свойство для получения размера кисти
    public Vector2 ScreenPosition { get; private set; } // Свойство для получения позиции ввода
    public bool IsDrawing { get; private set; } // Свойство для проверки, рисуем ли мы

    private void Update()
    {
        // Обработка для редактора и ПК
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButton(0))
        {
            ScreenPosition = Input.mousePosition;
            IsDrawing = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            IsDrawing = false;
        }
#endif

        // Обработка для мобильных устройств
#if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0); // Получаем первое касание

            if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                ScreenPosition = touch.position;
                IsDrawing = true;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                IsDrawing = false;
            }
        }
#endif
    }
}