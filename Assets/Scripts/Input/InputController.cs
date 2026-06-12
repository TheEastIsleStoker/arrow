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
            return;
        }

        ArrowRoot arrowRoot = hit.transform.GetComponentInParent<ArrowRoot>();

        if (arrowRoot != null)
        {
            gameController.OnArrowClicked(arrowRoot);
        }
    }
}