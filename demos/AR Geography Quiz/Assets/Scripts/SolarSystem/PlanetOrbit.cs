using UnityEngine;

[RequireComponent(typeof(Planet))]
public class PlanetOrbit : MonoBehaviour
{
    public float speed = 1f;
    private Planet planet;

    void Start()
    {
        planet = GetComponent<Planet>();
    }

    void Update()
    {
        transform.localPosition = GetPosition(Time.time * speed * planet.app.Speed);
    }

    private Vector3 GetPosition(float angle)
    {
        return new Vector3(planet.distanceSun * Mathf.Sin(angle), 0, planet.distanceSun * Mathf.Cos(angle));
    }
}