using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 5f;

    [Header("Bounds")]
    public bool useBounds = true;
    public Vector2 minPosition = new Vector2(-5f, -5f);
    public Vector2 maxPosition = new Vector2(5f, 5f);

    private void Update()
    {
        Vector3 input = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            input.y += 1f;
        }

        if (Input.GetKey(KeyCode.S))
        {
            input.y -= 1f;
        }

        if (Input.GetKey(KeyCode.A))
        {
            input.x -= 1f;
        }

        if (Input.GetKey(KeyCode.D))
        {
            input.x += 1f;
        }

        if (input.sqrMagnitude > 1f)
        {
            input.Normalize();
        }

        Vector3 nextPosition = transform.position + input * moveSpeed * Time.deltaTime;

        if (useBounds)
        {
            nextPosition.x = Mathf.Clamp(nextPosition.x, minPosition.x, maxPosition.x);
            nextPosition.y = Mathf.Clamp(nextPosition.y, minPosition.y, maxPosition.y);
        }

        transform.position = nextPosition;
    }
}