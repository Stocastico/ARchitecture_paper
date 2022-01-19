using UnityEngine;

/// <summary>
/// Encapsulate the functionality of the game object overlay on the marker
/// </summary>
public class ARGameObject : MonoBehaviour
{
	private GameObject arGameObject;
	private GameObject markerRotation;
	private GameObject otherUserRotation;

	public const string RED = "RED";
	public const string BLUE = "BLUE";

	/// <summary>
	/// Create the game objects needed for the AR Scene   
	/// </summary>
	/// <param name="arGOPrefab">Prefab of the game object that the app will overlay on the marker</param>
	public void CreateARGameObject(GameObject arGOPrefab)
	{
		markerRotation = new GameObject(); //This game object will encapsulate the marker rotations

		otherUserRotation = new GameObject();// This game Object will encapsulate the rotations recieve by other users
		otherUserRotation.transform.SetParent(markerRotation.transform);

		arGameObject = Instantiate(arGOPrefab, Vector3.zero, Quaternion.identity); // This game object will encapsulate the rotations made by the user
		arGameObject.transform.SetParent(otherUserRotation.transform);

		// Change the start color of the game object
		MaterialPropertyBlock props = new MaterialPropertyBlock();
		props.SetColor("_Color", Color.grey);
		arGameObject.GetComponent<Renderer>().SetPropertyBlock(props);
	}

	/// <summary>
	/// </summary>
	/// <returns> The rotation of the arGameObject converted to euler</returns>
	public Vector3 EulerAngles()
	{
		return arGameObject.transform.rotation.eulerAngles;
	}

	/// <summary>
	/// Change the color of the game object
	/// </summary>
	/// <param name="color_string">String with the color</param>
	public void UpdateColor(string color_string)
	{
		Color color = Color.black;

		if (color_string.Contains(RED))
		{
			color = Color.red;
		}
		else if (color_string.Contains(BLUE))
		{
			color = Color.blue;
		}
		else
		{
			Debug.LogError("COLOR NOT FOUND");
		}

		MaterialPropertyBlock props = new MaterialPropertyBlock();
		props.SetColor("_Color", color);
		arGameObject.GetComponent<Renderer>().SetPropertyBlock(props);
	}


	/// <summary>
	/// Method to rotate the UP axis of the ar Game Object  
	/// </summary>
	/// <param name="rotate">Angle of the rotation</param>
	public void UpdateArGameObjectRotation(float rotate)
	{
		//UP--> Green Axis, right --> red axis, forward --> blue axis       
		arGameObject.transform.RotateAround(arGameObject.transform.position, transform.up, rotate);
	}

	/// <summary>
	/// Updates the rotation of the AR Game Object with the info from the other users
	/// </summary>
	/// <param name="angle">Vector with the rotation recieve from other user</param>
	public void UpdateARGameObjectRotation(Vector3 angle)
	{
		otherUserRotation.transform.rotation = Quaternion.Euler(angle);
	}

	/// <summary>
	/// Updates the rotation of the game with the info from the marker 
	/// </summary>
	/// <param name="position">postion of the tracked image</param>
	/// <param name="rotation">rotation of the tracked image</param>
	internal void UpdateMarkerRotation(Vector3 position, Quaternion rotation)
	{
		markerRotation.transform.SetPositionAndRotation(position, rotation);
		//markerRotation.SetActive(true);
	}

	/// <summary>
	/// Make the Ar Game Objects appear or disappear from the scene
	/// </summary>
	/// <param name="value">true true appear, false to dissapear</param>
	internal void SetActive(bool value)
	{
		arGameObject.SetActive(value);
	}


}

