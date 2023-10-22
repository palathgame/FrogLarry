using UnityEngine;
using UnityEngine.Tilemaps;


public class CameraController : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;          // Reference na Tilemap
    private float padding = 0.2f;        // Okraj mezi okrajem Tilemap a zábìrem kamery
    private float zoomSpeed = 0.001f; // Rychlost pøiblížení a oddálení kamery pomocí koleèka myši
    private float moveSpeed = 0.1f; // Rychlost ohybu camery do stran
    private float minOrthographicSize = 1f; // Minimální hodnota pro oddálení kamery
    private float maxOrthographicSize; // Maximální hodnota pro oddálení kamery


    private Camera cam;
    private Vector3 targetPosition;
    private float targetOrthographicSize;
    private bool changeview = true;

    public void SetChangeview(bool changeview) {

        this.changeview = changeview;
    }


    private void Start()
    {
        cam = GetComponent<Camera>();

    }

    public void CenterCam()
    {
        targetPosition = tilemap.localBounds.center;
        targetPosition.z = cam.transform.position.z;
        cam.transform.position = targetPosition;
        targetOrthographicSize = CalculateOrthographicSize();
        cam.orthographicSize = targetOrthographicSize;
        maxOrthographicSize = targetOrthographicSize;


    }

    private void Update()
    {
        if (changeview) { 

            if (Input.touchCount == 2)
            {
                // Dotyky na displeji
                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);

                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
                float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;
                float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
                targetOrthographicSize = Mathf.Clamp(cam.orthographicSize + deltaMagnitudeDiff * zoomSpeed, minOrthographicSize, maxOrthographicSize);


            if (targetOrthographicSize < maxOrthographicSize)
            {
                if (targetOrthographicSize > maxOrthographicSize - 0.5 && deltaMagnitudeDiff > 0) {
                    CenterCam();
                    return;
                }
                cam.orthographicSize = targetOrthographicSize;


                cam.transform.position = BorderMoveCam(cam.transform.position);
            }

        }
            else if (Input.touchCount == 1)
            {
                // Pohyb kamery pomoci jednoho prstu
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Moved && maxOrthographicSize> cam.orthographicSize)
                {

                    Vector2 deltaPosition = touch.deltaPosition;
                    Vector3 move = new Vector3(deltaPosition.x, deltaPosition.y, 0) * moveSpeed * Time.deltaTime;

                cam.transform.position = BorderMoveCam(cam.transform.position - move);
                }
            }
        }
    }


    private float CalculateOrthographicSize()
    {

        BoundsInt bounds = tilemap.cellBounds;
        Vector3 cellSize = tilemap.cellSize;

        float desiredHeight = bounds.size.y * cellSize.y;
        float desiredWidth = bounds.size.x * cellSize.x;

        float screenRatio = Screen.width / (float)Screen.height;
        float targetRatio = desiredWidth / desiredHeight;
        // Pøizpùsobení zábìru kamery na základì pomìru obrazovky a Tilemap
        if (screenRatio >= targetRatio)
        {
            return (desiredHeight / 2f) + padding;
        }
        else
        {
            float targetWidth = desiredHeight * screenRatio;
            return (targetWidth / (2f * cam.aspect)) + padding;
        }
    }

    private Vector3 BorderMoveCam(Vector3 newPosition) {


        float cameraHalfHeight = cam.orthographicSize;
        float cameraHalfWidth = cameraHalfHeight * cam.aspect;
        newPosition.x = Mathf.Clamp(newPosition.x, (tilemap.localBounds.min.x + cameraHalfWidth) * 0.85F, (tilemap.localBounds.max.x - cameraHalfWidth) * 1.15F);
        newPosition.y = Mathf.Clamp(newPosition.y, (tilemap.localBounds.min.y + cameraHalfHeight) * 0.85F, (tilemap.localBounds.max.y - cameraHalfHeight) * 1.15F);

        return newPosition;
    }
}