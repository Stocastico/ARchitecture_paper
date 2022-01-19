using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// Behaviour of the AR Scene
/// </summary>
[RequireComponent(typeof(ARTrackedImageManager))]
public class ARManager : MonoBehaviour
{
    // Prefab of the object to instiante over the Marker
    public GameObject gameObjectPrefab;
    
    // A reference to the particular orkestra implementation for the App
    private OrkestraImpl orkestra;

    ARTrackedImageManager arTrackedImageManager;   
    
    bool gameObjectMoving = false;

    // Store the rotation events recieve by other users
    private Queue<Action> RotationEvents = new Queue<Action>();
    
    private ARGameObject arCube;
    
    // UI Text field to change the dispatch rate of the rotations events
    public TextField RotationsMessageDispatchRate_field { get; private set; }
    
    // Default value in seconds of the dispatch rate
    public const float DEFAULT_ROTATIONS_MESSAGE_DISPATCH_RATE = 0.01f;

    /// <summary>
    /// Starts the connection with the orkestra Server and the coroutine to send the rotation Messages
    /// </summary>
    void Start()
    {            
            orkestra.ConnectOrkestra(PlayerPrefs.GetString("RoomUI"), PlayerPrefs.GetString("AgentUI") , PlayerPrefs.GetString("URL"));
            StartCoroutine("SendRotationRoutine");
    }


    /// <summary>
    /// If there is any rotation events the player won't be able to rotate the game object
    ///     - The rotation eventes receive will be invoke and the cube will update its rotation
    /// If there are no rotation events, the player can rotate the game object
    /// </summary>
    void Update()
    {
        while (RotationEvents.Count > 0)
        {
            // Be carefull if there is many events it could be an infinite loop
            RotationEvents.Dequeue().Invoke();
        }
        
        RotateWithTouch();
    }


    /// <summary>
    /// Instiante and get reference of orkestra, ARCube and AR Tracked Image Manager
    /// Add functionality to the UI elements
    /// </summary>
    private void Awake()
    {
        orkestra = new OrkestraImpl(this);
        arTrackedImageManager = FindObjectOfType<ARTrackedImageManager>();
        arCube = gameObject.AddComponent<ARGameObject>();
        arCube.CreateARGameObject(gameObjectPrefab);

        var uiDocument = GetComponent<UIDocument>().rootVisualElement;

        // When the button is pressed, update the color and send the info to other users
        uiDocument.Q<Button>("Button_Red").clicked += delegate { arCube.UpdateColor(ARGameObject.RED); orkestra.SendColor(ARGameObject.RED); };
        uiDocument.Q<Button>("Button_Blue").clicked += delegate { arCube.UpdateColor(ARGameObject.BLUE); orkestra.SendColor(ARGameObject.BLUE); };
        uiDocument.Q<Button>("Back").clicked += delegate { SceneManager.LoadScene("EnterSession"); };

        // Init rotation Message field with the default value
        RotationsMessageDispatchRate_field = uiDocument.Q<TextField>("MessageDispatchRate");
        RotationsMessageDispatchRate_field.value = DEFAULT_ROTATIONS_MESSAGE_DISPATCH_RATE.ToString();
    }

    /// <summary>
    /// Subscribe to the Ar tracked image change method
    /// </summary>
    private void OnEnable()
    {
        arTrackedImageManager.trackedImagesChanged += ImageChanged;
    }

    /// <summary>
    /// Unsubscribe to the tracked image change method and disconnect from orkestra
    /// </summary>
    private void OnDisable()
    {
        arTrackedImageManager.trackedImagesChanged -= ImageChanged;
        orkestra.Disconnect();
    }

    /// <summary>
    /// Coroutine to send the rotation events when the gameobject is being rotated by the player
    /// It waits the seconds determined by the dispatch rate
    /// </summary>
    /// <returns>It is essentially that coroutine function is declared with a return type of IEnumerator</returns>
    IEnumerator SendRotationRoutine()
    {
        float rotationMessageDispatchRate;
        while (true)
        {
            try
            {
                rotationMessageDispatchRate = float.Parse(RotationsMessageDispatchRate_field.value);
            }
            catch (Exception e)
            {
                Debug.LogError("PARSE ERROR");
                rotationMessageDispatchRate = DEFAULT_ROTATIONS_MESSAGE_DISPATCH_RATE;
            }
            if (IsTheGameObjectRotating())
            {
                orkestra.SendRotation(arCube.EulerAngles());
            }
            yield return new WaitForSecondsRealtime(rotationMessageDispatchRate);
        }
    }

    private void OnApplicationQuit()
    {
        orkestra.Disconnect();
    }

    /// <summary>
    /// Returns the value of gameObjectMoving
    /// </summary>
    /// <returns>
    /// true: the gameObject is rotating
    /// false: the gameObject isnt rotating
    /// </returns>
    public bool IsTheGameObjectRotating()
    {
        return gameObjectMoving;
    }

    /// <summary>
    /// Enqueue the Update rotation events
    /// </summary>
    /// <param name="angle">Vector with the rotation angle receive</param>
    internal void RotationReceive(Vector3 angle)
    {
        RotationEvents.Enqueue(() => arCube.UpdateARGameObjectRotation(angle));
    }
    
    /// <summary>
    /// Update color with the value receive
    /// </summary>
    /// <param name="color">String with the color value</param>
    internal void ColorReceive(string color)
    {
        arCube.UpdateColor(color);
    }

    /// <summary>
    /// When the user rotate the cube by touch, the game object its rotated 
    /// https://docs.unity3d.com/ScriptReference/TouchPhase.html
    /// </summary>
    private void RotateWithTouch()
    {
        if (Input.touchCount == 1)
        {
            Touch touch0 = Input.GetTouch(0);
            switch (touch0.phase)
            {
                case TouchPhase.Moved:
                    float rotateX = touch0.deltaPosition.x * 0.5f * -1;
                    if (Math.Abs(rotateX) > 1) // Condition so we can 
                    {
                        gameObjectMoving = true;
                        Debug.Log("MOVING");
                        arCube.UpdateArGameObjectRotation(rotateX);
                    }
                    else
                    {
                        gameObjectMoving = false;
                    }
                    break;
                case TouchPhase.Ended:
                    gameObjectMoving = false;
                    break;
            }
        }
    }


    /// <summary>
    /// When the tracked Image is detected/updated the update image method is called
    /// The game object active value is set to false when the tracked image isnt being the detected by the device
    /// </summary>
    /// <param name="eventArgs">Events of the AR Tracked image manager</param>
    void ImageChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {        
            foreach (ARTrackedImage trackedImage in eventArgs.added)
            {
                UpdateImage(trackedImage);
            }
            foreach (ARTrackedImage trackedImage in eventArgs.updated)
            {
                UpdateImage(trackedImage);
            }
            foreach (ARTrackedImage trackedImage in eventArgs.removed)
            {
                arCube.SetActive(false);
            }        
    }

    /// <summary>
    /// Update the rotation of the game object if the marker changes it's rotation
    /// </summary>
    /// <param name="trackedImage">marker tracked by the AR App</param>
    private void UpdateImage(ARTrackedImage trackedImage)
    {
        arCube.UpdateMarkerRotation(trackedImage.transform.position,trackedImage.transform.rotation);
    }

}
