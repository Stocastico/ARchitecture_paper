using UnityEngine;
using OrkestraLib;
using OrkestraLib.Message;
using OrkestraLib.Exceptions;
using System;
using OrkestraLib.Plugins;

/// <summary>
/// Interface for the Orkestra Package
/// </summary>
public class OrkestraImpl : Orkestra
{
    ARManager arManager;

    /// <summary>
    /// Create a reference to the app and subscribe to the Application events
    /// </summary>
    /// <param name="arManager"> Reference to the app script</param>
    public OrkestraImpl(ARManager arManager)
    {
        this.arManager = arManager;
        ApplicationEvents += AppEventSubscriber;
    }

    /// <summary>
    /// Create the connection with the orkestra server
    /// </summary>
    /// <param name="roomName">Name of the orkestra room</param>
    /// <param name="agentID">Identification of the agent</param>
    /// <param name="URL">url of the orkestra server</param>
    public void ConnectOrkestra(string roomName, string agentID, string URL)
    {
        
        this.room = roomName;
        this.agentId= agentID;
        this.url = URL;

        // true to erease the events of the room
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
    /// <summary>
    /// Send Message with the rotation info
    /// </summary>
    /// <param name="eulerAngles"></param>
    public void SendRotation(Vector3 eulerAngles)
    {        
        Dispatch(Orkestra.Channel.Application, new RotationNotification(agentId, eulerAngles), "");        
    }

    /// <summary>
    /// Send Message with the info of the color
    /// </summary>
    /// <param name="color">String with the color info</param>
    public void SendColor(string color)
    {        
        Dispatch(Orkestra.Channel.Application, new ColorNotification(agentId, color), "");
    }

    /// <summary>
    /// Receive messages from orkestra
    /// </summary>
    /// <param name="sender">Sender info</param>
    /// <param name="evt">Message info</param>
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

}
