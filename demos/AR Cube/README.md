
## About / Synopsis
* Simple example developed with Orkestralib-unity 
<p align="center">

<img src="https://user-images.githubusercontent.com/25354672/142847375-75ee9268-52e5-4220-9f68-582e278c6105.gif" alt="login" align="center">
 </p>
 
## Table of contents
  * [About / Synopsis](#about--synopsis)
  * [Table of contents](#table-of-contents)
  * [Minimum requirements](#minimum-requirements)
  * [Usage](#usage)
  * [Assets / Scenes](#assets--scenes)
  * [Assets / Scripts](#assets--scripts)
  * [Assets / Messages](#assets--messages)

## Minimum requirements
- Unity 2020.3.19f 
  - Android module / IOS module
- Android 7.0 (API level 24)  / IOS 11 

## Usage

Clone the project from the repository: 
```
 git clone  https://username@github.com/tv-vicomtech/ARETE.git
 git checkout origin/develop
```

Download the latest version of the OrkestraLib and H.Socket.IO package (SocketIOClient package is not necessary for this example) from the OrkestraLib-Unity [releases](https://github.com/tv-vicomtech/orkestralib-unity/releases/).

![Releases](https://user-images.githubusercontent.com/25354672/142617936-c0d2a0c1-6521-4c0d-849e-76a34e038d0a.PNG)

Open with Unity the ARETE/Unity3D/SimpleDemo folder.

Open the OrkestraLib.unitypackage and the H.Socket.IO.unitypackage.

![image](https://user-images.githubusercontent.com/25354672/142626321-3ee12ed1-83ee-404b-b7b4-bfcc0c3af402.png)

Everything is ready!

## Assets / Scenes

There are 2 scenes in the application. The first one is the one used to provide session data, while the other is the AR scene.

### EnterSession
Login scene with the fields required to start an Orkestra session.

<img src="https://user-images.githubusercontent.com/25354672/142848723-645b36af-d949-4314-8b0d-36dffc357b9a.jpg" alt="login" width="400" height="650">

### ARScene
Scene with AR Foundation.

<img src="https://user-images.githubusercontent.com/25354672/142848682-166f15e3-ebdf-4cf2-8123-32449b1d46aa.jpg" alt="arscene" width="400" height="650">

The **Agent** field represent a unique user identifier, the **URL** identifies the server (and it should stay as it is) and finally the **Room** field is used to identify the session. All the users that connect using the same value for Room will be sharing the same AR experience.


## Assets / Scripts
### [OrkestraImpl.cs](https://github.com/tv-vicomtech/ARETE/blob/develop/Unity3D/SimpleDemo/Assets/Scripts/OrkestraImpl.cs)
Creates the connection with the Orkestra Server and manage the info input/output.

The most important steps are the following:

##### Connection with Orkestra

```csharp
 public void ConnectOrkestra(string roomName, string agentID, string URL)
    {
        
        this.room = roomName;
        this.agentId= agentID;
        this.url = URL;

        // True to erase the events of the room when disconnecting
        ResetRoomAtDisconnect = true;    

        // Register the particular messages of the application 
        RegisterEvents(new Type[]{
                typeof(ColorNotification),
                typeof(RotationNotification)
            });

        // Use Orkestra SDK with HSocketIOClient
        OrkestraWithHSIO.Install(this);

        try
        {
            // Start Orkestra
            Connect(() =>
            {
                Debug.Log("All stuff is ready");                
            });

        }
        catch (ServiceException e)
        {
            Debug.LogError(e.Message);
        }

    }
```

##### Send information to all users through the Application channel.

```csharp
  public void SendRotation(Vector3 eulerAngles)
  {        
        Dispatch(Orkestra.Channel.Application, new RotationNotification(agentId, eulerAngles), "");        
  }
  public void SendColor(string color)
  {        
        Dispatch(Orkestra.Channel.Application, new ColorNotification(agentId, color), "");
  }

```

##### Send information to a specific user

It is very similar to what we do when sending data to all the users, you just have to specify the receiving when calling *dispatch*

```csharp
Dispatch(Orkestra.Channel.User, new RotationNotification(agentId, eulerAngles), receiver);
```

##### Subscribe to the Application Channel so the app will receive the events sent by other users.

```csharp
public OrkestraImpl(ARManager arManager)
{
        this.arManager = arManager;
        ApplicationEvents += AppEventSubscriber;
}

void AppEventSubscriber(object sender, ApplicationEvent evt)
    {
        if (evt.IsEvent(typeof(ColorNotification)))
        {
            ColorNotification colorNotification = new ColorNotification(evt.value);

            if (!colorNotification.sender.Equals(agentId))
            {                
                arManager.ColorReceive(colorNotification.content);
            }
        }
        else if (evt.IsEvent(typeof(RotationNotification)))
        {
            RotationNotification msg = new RotationNotification(evt.value);

            if (!msg.sender.Equals(agentId))
            {
                arManager.RotationReceive(msg.angle);
            }
        }
    }
```


### [ARManager.cs](https://github.com/tv-vicomtech/ARETE/blob/develop/Unity3D/SimpleDemo/Assets/Scripts/ARManager.cs)

This is the main script with the behaviour of the Augmented reality scene.

##### Starts and stops the connection with Orkestra, send message through the Orkestra implementation.

```csharp
private void Awake()
    {
        orkestra = new OrkestraImpl(this);
        
        
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
```

```csharp
 orkestra.ConnectOrkestra(PlayerPrefs.GetString("RoomUI"), PlayerPrefs.GetString("AgentUI") , PlayerPrefs.GetString("URL"));
```

```csharp
 private void OnDisable()
    {
        arTrackedImageManager.trackedImagesChanged -= ImageChanged;
        orkestra.Disconnect();
    }
```

##### Send Messages

By default the Rotation message dispatch rate is set to *0.01* seconds. Using higher values of *rotationMessageDispatchRate* (for example, 0.25 seconds) will make the application less smooth, but it will reduce the amount of data exchanged with the server.

```csharp
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
```

### [ARGameObject.cs](https://github.com/tv-vicomtech/ARETE/blob/develop/Unity3D/SimpleDemo/Assets/Scripts/ARGameObject.cs)

Encapsulate the functionality and attributes of the game object overlay on the marker.
Update the Color and Rotation of the AR game object.

## Assets / Messages
Store the information that the Application will send through Orkestra.
Messages should implement the OrkestraLib.Message interface.

### [ColorNotification.cs](https://github.com/tv-vicomtech/ARETE/blob/develop/Unity3D/SimpleDemo/Assets/Messages/ColorNotification.cs)

```csharp
[Serializable]
        public class ColorNotification  : Message
        {
            public string content;

            public ColorNotification(string json) : base(json) { }

            public ColorNotification(string sender, string content) :
              base(typeof(ColorNotification).Name, sender)
            {
                this.content = content;
            }

            public string GetContent()
            {
                return content;
            }

            public override string FriendlyName()
            {
                return typeof(ColorNotification).Name;
            }
        }
```


### [RotationNotification.cs](https://github.com/tv-vicomtech/ARETE/blob/develop/Unity3D/SimpleDemo/Assets/Messages/RotationNotification.cs)

```csharp
    [Serializable]
        public class RotationNotification : Message
        {
            public Vector3 angle;
        
            public RotationNotification(string json) : base(json) { }

            public RotationNotification(string sender, Vector3 angle) :
              base(typeof(RotationNotification).Name, sender)
            {
                this.angle = angle;
              
            }

            public Vector3 GetContent()
            {
                return angle;
            }

            public override string FriendlyName()
            {
                return typeof(RotationNotification).Name;
            }
        }
    }
```
