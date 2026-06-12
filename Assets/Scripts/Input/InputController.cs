using UnityEngine;

public class InputController : MonoBehaviour
{
    public Camera targetCamera;
    public LayerMask hitLayerMask = ~0;

    private bool inputEnabled;
    private GameController gameController;

    public void Initialize(GameController controller)
    {
        gameController = controller;

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
    }

    private void Update()
    {
        if (!inputEnabled)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("鼠标按下");
            HandleMouseClick();
        }
    }

    private void HandleMouseClick()
    {
        if (targetCamera == null)
        {
            return;
        }

        Vector3 screenPosition = Input.mousePosition;
        Vector3 worldPosition = targetCamera.ScreenToWorldPoint(screenPosition);
        Vector2 worldPoint = new Vector2(worldPosition.x, worldPosition.y);

        Collider2D hit = Physics2D.OverlapPoint(worldPoint, hitLayerMask);

        if (hit == null)
        {
            Debug.Log("未点击到任何对象");
            return;
        }

        ArrowRoot arrowRoot = hit.transform.GetComponentInParent<ArrowRoot>();

        if (arrowRoot != null)
        {
            Debug.Log($"点击到箭头: {arrowRoot.name}");
            gameController.OnArrowClicked(arrowRoot);
        }
    }
}