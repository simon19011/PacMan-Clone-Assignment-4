using System.Collections;
using UnityEngine;

public class CerryController : MonoBehaviour
{
    [Header("References")]
    public GameObject cherryPrefab;
    public Camera gameCamera;
    public Transform levelCenter;

    [Header("Movement")]
    public float movementSpeed = 5f;
    public float spawnDelay = 5f;
    public float outsideScreenDistance = 1f;

    private GameObject currentCherry;
    void Start()
    {
        if (gameCamera == null)
            gameCamera = Camera.main;

        StartCoroutine(SpawnCherryAfterDelay());
    }

    private IEnumerator SpawnCherryAfterDelay()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnDelay);

            if (cherryPrefab == null || gameCamera == null)
            {
                Debug.LogWarning(
                    "CherryController needs a Cherry Prefab and Camera."
                );
                yield break;
            }

            yield return MoveCherryAcrossScreen();
        }
    }

    private IEnumerator MoveCherryAcrossScreen()
    {
        Vector3 center = levelCenter != null ? levelCenter.position : GetCameraCenter();
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f).normalized;

        GetCameraBounds(out Vector3 min, out Vector3 max);

        float distanceToEdge = GetDistanceToCameraEdge(center, direction, min, max);

        Vector3 startPosition =center - direction * (distanceToEdge + outsideScreenDistance);
        Vector3 endPosition =center + direction * (distanceToEdge + outsideScreenDistance);

        currentCherry = Instantiate(cherryPrefab, startPosition, Quaternion.identity);

        SpriteRenderer spriteRenderer = currentCherry.GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 100;
            spriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        }

        float distance = Vector3.Distance(startPosition, endPosition);
        float duration = distance / Mathf.Max(movementSpeed, 0.01f);
        float elapsed = 0f;

        while (elapsed < duration && currentCherry != null)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / duration);

            currentCherry.transform.position = Vector3.Lerp(startPosition, endPosition, progress);

            yield return null;
        }

        if (currentCherry != null)
        {
            Destroy(currentCherry);
            currentCherry = null;
        }
    }

    private Vector3 GetCameraCenter()
    {
        Vector3 center = gameCamera.transform.position;
        center.z = 0f;
        return center;
    }

    private void GetCameraBounds(out Vector3 min, out Vector3 max)
    {
        float zDistance = Mathf.Abs(gameCamera.transform.position.z);
        Vector3 bottomLeft = gameCamera.ViewportToWorldPoint(new Vector3(0f, 0f, zDistance));
        Vector3 topRight = gameCamera.ViewportToWorldPoint(new Vector3(1f, 1f, zDistance));

        min = bottomLeft;
        max = topRight;
        min.z = 0f;
        max.z = 0f;
    }

    private float GetDistanceToCameraEdge(Vector3 center, Vector3 direction, Vector3 min, Vector3 max)
    {
        float distanceX = float.PositiveInfinity;
        float distanceY = float.PositiveInfinity;

        if (direction.x > 0f)
        {
            distanceX = (max.x - center.x) / direction.x;
        }

        else if (direction.x < 0f)
        {
            distanceX = (min.x - center.x) / direction.x;
        }

        if (direction.y > 0f)
        {
            distanceY = (max.y - center.y) / direction.y;
        }

        else if (direction.y < 0f)
        {
            distanceY = (min.y - center.y) / direction.y;
        }

        return Mathf.Min(distanceX, distanceY);
    }

    private void OnDisable()
    {
        if (currentCherry != null)
        {
            Destroy(currentCherry);
        }

        currentCherry = null;
    }
}
