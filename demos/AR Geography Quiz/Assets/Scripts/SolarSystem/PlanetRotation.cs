using UnityEngine;

public class PlanetRotation : MonoBehaviour
{
    public float speed = 120.0f;
    public MainApp app;

    void Update()
    {
        transform.Rotate(Vector3.up, Time.deltaTime * speed * app.Speed);
    }
}
