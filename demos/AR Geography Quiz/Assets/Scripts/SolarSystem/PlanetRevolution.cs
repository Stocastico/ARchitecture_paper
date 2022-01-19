using UnityEngine;

[RequireComponent(typeof(Planet))]
public class PlanetRevolution : MonoBehaviour
{
    public float speed = 0.1f;
    private Planet planet;

    void Start()
    {
        planet = GetComponent<Planet>();
    }

    void Update()
    {
        transform.localPosition = GetPosition(Time.time * speed * planet.app.Speed * Mathf.PI / 180.0f);
    }

    private Vector3 GetPosition(float angle)
    {
        var x = planet.distanceSun * Mathf.Sin(angle);
        var z = planet.distanceSun * Mathf.Cos(angle);
        return new Vector3(x, 0, z);
    }
}
