using OrkestraLib.Message;
using UnityEngine;

public class PushPin : MonoBehaviour
{
    public Answer answer;

    public void Place(GameObject target, Vector3 pos, Color color, float scale = 0.002f)
    {
        transform.position = pos;
        transform.SetParent(target.transform, true);
        transform.localScale = new Vector3(scale, scale, scale);
        transform.LookAt(target.transform.position);
        transform.rotation = transform.rotation * Quaternion.Euler(180, 0, 0);
        SetColor(color);
    }

    public void PlaceLocal(GameObject target, Vector3 pos, Color color, float scale = 0.002f)
    {
        transform.SetParent(target.transform, false);
        transform.localPosition = pos;
        transform.localScale = new Vector3(scale, scale, scale);
        transform.LookAt(target.transform.position);
        transform.rotation = transform.rotation * Quaternion.Euler(180, 0, 0);
        SetColor(color);
    }

    public void SetColor(Color color)
    {
        GetComponent<Renderer>().material.color = color;
    }

    public static Vector3 Position(GameObject prefab, GameObject hitObject, Vector3 hitPoint)
    {
        Vector3 onPlanet = Random.onUnitSphere * hitObject.transform.localScale.x;
        GameObject pushpin = Instantiate(prefab, onPlanet, Quaternion.identity);
        PushPin pin = pushpin.GetComponent<PushPin>();
        pin.Place(hitObject, hitPoint, Color.yellow);
        Vector3 pos = pin.transform.localPosition;
        Destroy(pin);
        return pos;
    }

}
