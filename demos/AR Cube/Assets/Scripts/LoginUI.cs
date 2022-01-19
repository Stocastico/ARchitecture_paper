using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class LoginUI:   MonoBehaviour
{
    // Default values for the UI elements
    private readonly string roomUI = "AR Test Room";
    private readonly string agentIDUI = "Agent_";
    private readonly string URL = "https://cloud.flexcontrol.net";

    // UI elements created with UIToolkit https://docs.unity3d.com/Manual/UIElements.html
    private TextField Room_field;
    private TextField AgentID_field;
    private TextField URL_field;
    private Button Connect_Button;

    /// <summary>
    /// Inits the login to Orkestra UI
    /// To init an Orkestra Session it's required an Agent ID, a room name and a URL with an Orkestra server deploy
    /// </summary>
    void Start()
    {
        var uiDocument = GetComponent<UIDocument>().rootVisualElement;
        Room_field = uiDocument.Q<TextField>("RoomField");
        AgentID_field = uiDocument.Q<TextField>("AgentField");
        URL_field = uiDocument.Q<TextField>("URLField");
        Connect_Button = uiDocument.Q<Button>("Connect_Button");

        Room_field.value = roomUI;
        AgentID_field.value = agentIDUI;
        URL_field.value = URL;
             
        Connect_Button.clicked += StartApp;
    }
  
    /// <summary>
    /// Inits the AR Scene and save the login values
    /// </summary>
    private void StartApp()
    {
        SceneManager.LoadScene("ARScene");
        PlayerPrefs.SetString("RoomUI", Room_field.value);
        PlayerPrefs.SetString("AgentUI", AgentID_field.value);
        PlayerPrefs.SetString("URL", URL_field.value);
    }
}
