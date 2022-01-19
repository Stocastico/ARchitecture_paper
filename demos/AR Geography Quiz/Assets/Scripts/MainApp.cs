using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using Michsky.UI.ModernUIPack;
using UnityEngine;
using OrkestraLib;
using OrkestraLib.Message;
using OrkestraLib.Plugins;
using static OrkestraLib.Utilities.ParserExtensions;
using Transform = UnityEngine.Transform;

public class MainApp : Orkestra
{
    public float MessageDispatchRate = 0.05f;
    public float Speed = 0.01f;
    public float smoothTime = 0.3f;

    public GameObject Prefab;
    public TextMeshProUGUI InfoHeader;
    public TextMeshProUGUI InfoUsers;
    public TextMeshProUGUI chat;
    public TextMeshProUGUI userType;
    public TextMeshProUGUI username;
    public TextMeshProUGUI roomname;
    public TextMeshProUGUI loginMessage;

    public TextMeshProUGUI WaitScreenText;
    public CustomDropdown chatUserList;
    public NotificationManager Notification;
    public ModalWindowManager Modal;
    public ProgressBar Points;
    public WindowManager Windows;
    public CustomDropdown UserList;

    public ModalWindowManager CorrectAnswerForm;
    public ModalWindowManager WaitScoreModal;

    public List<PushPin> pins;
    public Sprite defaultIcon;
    public GameObject RegainControl;
    public GameObject NewSessionBtn;
    public GameObject ChatBtn;

    public GameObject ShareBtnHome;
    public GameObject ShareBtnWait;

    public GameObject PlaceSceneHome;
    public GameObject PlaceSceneWait;

    public SwitchManager SharedHome;
    public SwitchManager SharedWait;

    public GameObject ScoreUI;
    public GameObject Welcome;

    private string studentId = "";
    private string activeUserId = "";
    private bool isPlaying = false;
    private Multiplatform platform;

    public Action OnLateUpdate;
    public Action OnPlaceScene;
    public Action<CameraTransform, bool> ApplyObjectPose;
    private bool IsSceneInPlace = false;

    public GameObject previewViewport;
    public Planet defaultSelectionObject;

    public Transform RemoteRotation;
    public Transform RemoteReferenceSystem;
    public GameObject ReferenceSystem;

    /// <summary>
    /// Dispatch camera messages at constant rate (fps): <code>60 / <see cref="MessageDispatchRate"/></code>
    /// </summary>
    public IEnumerator UpdateRemoteCameras()
    {
        for (; ; )
        {
            if (isPlaying)
            {
                string userId = GetUsername();
                if (platform != null && platform.SceneCamera != null)
                {
                    GameObject cam = platform.SceneCamera.gameObject;
                    CameraTransform pose = new CameraTransform(userId, cam, ReferenceSystem);
                    Dispatch(Channel.Application, pose);

                    if (platform.HasTarget())
                    {
                        Dispatch(Channel.Application, new ObjectTransform(userId, platform.Target().gameObject));
                    }
                }
            }
            yield return new WaitForSeconds(MessageDispatchRate);
        }
    }

    public bool IsHome()
    {
        if (Windows.currentWindowIndex >= 0)
            return Windows.windows[Windows.currentWindowIndex].windowName == "Home";
        return false;
    }

    void ConfigureRemoteCamera()
    {
        previewViewport.SetActive(!IsShared());
    }

    void Start()
    {
        GameObject mp = GameObject.Find("Multiplatform");
        if (mp != null)
        {
            platform = mp.GetComponent<Multiplatform>();
        }
        else
        {
            platform = gameObject.AddComponent<Multiplatform>();
            platform.SceneCamera = gameObject;
            Debug.LogError("You cannot play this scene. Instead open Desktop or Android");
            Quit();
        }

        SelectObject(defaultSelectionObject);
        ConfigureRemoteCamera();

#if UNITY_ANDROID
        PlaceSceneHome.SetActive(true);
        PlaceSceneWait.SetActive(true);
#endif

        // Disable message debugger
        EnableParserDebugger = false;

        // Orkestra SDK
        UserEvents += UserEventSubscriber;
        ApplicationEvents += AppEventSubscriber;

        // Register custom events to be used in the app
        RegisterEvents(new Type[]{
            typeof(ActiveUser),
            typeof(Question),
            typeof(Answer),
            typeof(Result),
            typeof(CameraTransform),
            typeof(ObjectTransform),
            typeof(SharedView),
            typeof(Notification),
            typeof(SelectObject),
        });

        // List of active pins
        pins = new List<PushPin>();

        // Start helper coroutine
        StartCoroutine("UpdateRemoteCameras");

        // Pre-requirement to Connect()
        OrkestraWithHSIO.Install(this, (graceful, message) =>
        {
            Events.Add(() =>
            {
                if (!graceful) loginMessage.text = message;
                else InfoHeader.text = message;
            });
        });

        // Default to desktop behaviour. 
        OnLateUpdate = () =>
        {
            if (UserInteractionTurn())
            {
                ShowShareButton();

                // Select object
                if (Input.GetMouseButtonDown(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        Planet planet = hit.collider.gameObject.GetComponent<Planet>();
                        if (planet) SelectObject(planet);
                    }
                }

                // Place/validate object
                if (Input.GetMouseButtonDown(1))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        if (IsStudent())
                        {
                            string sender = GetUsername();
                            Vector3 pos = PushPin.Position(Prefab, hit.collider.gameObject, hit.point);
                            Dispatch(Channel.Application, new Answer(sender, hit.collider.gameObject, pos));
                            Dispatch(Channel.Application, new ActiveUser(sender, "teacher"));
                        }
                        else
                        {
                            PushPin pp = hit.collider.gameObject.GetComponent<PushPin>();
                            if (pp != null)
                            {
                                studentId = pp.answer.sender;
                                CorrectAnswerForm.titleText = "Validate answer";
                                CorrectAnswerForm.descriptionText = "Is this answer correct ?";
                                CorrectAnswerForm.UpdateUI();
                                CorrectAnswerForm.OpenWindow();

                                pins.Remove(pp);
                                Destroy(pp.gameObject);
                            }
                        }
                    }
                }
            }
            else HideShareButton();
        };

        // Default to desktop behaviour. That is, we should do nothing
        OnPlaceScene = () => { };

        // Default to desktop behaviour. 
        ApplyObjectPose = (cv, isSharedView) =>
        {
            if (platform && platform.SceneCamera != null && cv.name != null && platform.HasTarget())
            {
                // Android
                if (Camera.main.name.Equals("ARCamera"))
                {
                    Transform from = platform.RemoteSceneCamera.transform;
                    Transform target = platform.Target().transform;
                    Transform to = Camera.main.transform;

                    // Update remote scene camera
                    cv.Update(from, RemoteReferenceSystem);
                    from.gameObject.SetActive(!isSharedView);

                    // Set relative to this reference system
                    from.localPosition += ReferenceSystem.transform.localPosition;
                    from.localPosition -= RemoteReferenceSystem.localPosition;

                    // Update Target rotation
                    target.rotation = RemoteRotation.rotation;
                    if (isSharedView)
                    {
                        target.LookAt(to.position);
                        Quaternion rot = Quaternion.Inverse(from.transform.rotation);
                        rot = new Quaternion(rot.x, -rot.y, rot.z, -rot.w);
                        target.rotation *= rot * Quaternion.Euler(0, 180f, 0);
                    }
                }

                // Desktop
                else
                {
                    Transform cam = platform.SceneCamera.transform;
                    Transform remoteCam = platform.RemoteSceneCamera.transform;

                    if (isSharedView)
                        cv.Update(cam, ReferenceSystem.transform);

                    cv.Update(remoteCam, ReferenceSystem.transform);

                    // Shift by user-rotation
                    if (platform.HasTarget())
                    {
                        platform.Target().gameObject.transform.rotation = RemoteRotation.rotation;
                    }
                }
            }
        };
    }

    public override void LateUpdate()
    {
        base.LateUpdate();
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Logout();
            Quit();
        }
        OnLateUpdate?.Invoke();
    }

    void OnApplicationQuit()
    {
        Debug.LogWarning("Application ending after " + Time.time + " seconds");
    }

    void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void Logout()
    {
        // If the teacher leaves, the application should stop the session.
        if (!IsStudent()) NewSession();
    }

    /// <summary>Connect to Orkestralib after login</summary>
    public void Login()
    {
        string username = GetUsername();
        string roomId = GetRoomname();
        if (username.Length > 2 && roomId.Length > 2)
        {
            // Use username as agentid
            agentId = username;

            // Use login room as orkestra room
            room = roomId;

            // Connect
            try
            {
                loginMessage.text = "Authenticating...";
                Connect(null, (loginError, message) =>
                {
                    Events.Add(() =>
                    {
                        if (!loginError) loginMessage.text = message;
                        else InfoHeader.text = message;
                    });
                });
            }
            catch (Exception e)
            {
                loginMessage.text = e.Message;
            }
        }
        else
        {
            loginMessage.text = "Please insert valid credentials";
        }
    }

    /// <summary>Get user type from login interface</summary>
    public string GetUserType() { return userType.text?.RemoveNonUnicodeLetters(); }

    /// <summary>Get username from login interface</summary>
    public string GetUsername() { return username.text?.RemoveNonUnicodeLetters(); }

    /// <summary>Get roomname from login interface</summary>
    public string GetRoomname() { return roomname.text?.RemoveNonUnicodeLetters(); }

    /// <summary>Checks if it is a student</summary>
    public bool IsStudent() { return GetUserType().Equals("Student"); }

    /// <summary> Check if the user is playing </summary>    
    public bool IsPlaying() { return isPlaying; }

    /// <summary>Updates list of users</summary>
    /// BUG not upodated
    public void UpdateUserList()
    {
        List<string> users = new List<string>(AppContext.GetRemoveUserIds());
        InfoUsers.text = "Room: " + GetRoomname() + ", Users: " + string.Join(",", users.ToArray());
        chatUserList.dropdownItems.Clear();
        foreach (string id in users)
        {
            // Excludes this user from the selectable list
            if (!id.Equals(GetUsername()))
            {
                chatUserList.SetItemTitle(id);
                chatUserList.SetItemIcon(defaultIcon);
                chatUserList.CreateNewItem();
            }
        }
    }

    /// <summary> Process user events </summary>
    /// <param name="orkestraLib">Reference to OrkestraLib instance</param>
    /// <param name="evt">User event received from the server</param>
    /// 
    /// @BUG: join event does not happen in some cases 
    void UserEventSubscriber(object orkestraLib, UserEvent evt)
    {
        Debug.Log("UserEventSubscriber(" + evt.ToJSON() + ") ");
        if (evt.IsPresenceEvent())
        {
            // Start session only when user is logged in
            if (evt.agentid.Equals(GetUsername()) && evt.evt.Equals("join"))
            {
                Events.Add(StartSession);
            }
            Events.Add(UpdateUserList);
        }
        else if (evt.IsEvent(typeof(Question)))
        {
            Question msg = new Question(evt.data.value.Replace("\\\\", "\""));
            Events.Add(() => AskQuestion(msg));
        }
        else if (evt.IsEvent(typeof(Result)))
        {
            Result result = new Result(evt.data.value);
            Events.Add(() =>
            {
                int p = int.Parse(result.value);
                if (p <= 100 && p >= 0)
                {
                    Points.currentPercent = p;
                    Points.textPercent.text = "" + p;
                }
                ShowNotification(new Notification(result.sender, "You got " + p + " points"));
            });
        }
    }

    bool IgnoreMessage(string value)
    {
        Message msg = new Message(value); // fix problems
        if (msg.sender.Equals(GetUsername()) || isPlaying) return true;
        return false;
    }

    /// <summary>Process app events</summary>
    /// <param name="evt">Application event received</param>
    /// <param name="sender"></param>
    void AppEventSubscriber(object sender, ApplicationEvent evt)
    {
        // Active user interface
        if (evt.IsEvent(typeof(ActiveUser)))
        {
            ActiveUser user = new ActiveUser(evt.value);

            activeUserId = user.username;
            Events.Add(() => WaitScreenText.text = GetUsername() + " is observing " + activeUserId + ".");

            // Set this teacher active
            if (!IsStudent() && activeUserId.Equals("teacher"))
            {
                isPlaying = true;
                Events.Add(PlayTurn);
            }
            // Set this student user active
            else
            {
                if (activeUserId.Equals(GetUsername()))
                {
                    isPlaying = true;
                    Events.Add(PlayTurn);
                }
                // Other users
                else
                {
                    isPlaying = false;
                    Events.Add(WaitTurn);
                }
            }
        }

        // Draw/Validate answers
        else if (evt.IsEvent(typeof(Answer)))
        {
            Message msg = new Message(evt.value); // fix problems
            Answer answer = new Answer(msg.Data());
            if (GetUsername().Equals(answer.sender)) Events.Add(() => SubmitAnswer(answer));
            else Events.Add(() => ValidateAnswer(answer));
        }

        // Active scene
        else if (evt.IsEvent(typeof(SelectObject)))
        {
            if (IgnoreMessage(evt.value)) return;
            SelectObject scene = new SelectObject(evt.value);
            Events.Add(() =>
            {
                GameObject targetPlanet = GameObject.Find(scene.name);
                if (targetPlanet)
                {
                    SelectObject(targetPlanet.GetComponent<Planet>());
                }
            });
        }

        // Update shared view
        else if (evt.IsEvent(typeof(SharedView)))
        {
            if (IgnoreMessage(evt.value)) return;
            SharedView sv = new SharedView(evt.value);
            Events.Add(() =>
            {
                if (sv.active) ActivateSharedView();
                else DisableSharedView();
            });
        }

        // Camera updates
        else if (evt.IsEvent(typeof(CameraTransform)))
        {
            if (IgnoreMessage(evt.value)) return;

            Message msg = new Message(evt.value);
            Events.Add(() =>
            {
                ApplyObjectPose?.Invoke(new CameraTransform(msg.Data()), IsShared());
            });
        }

        // Object updates
        else if (evt.IsEvent(typeof(ObjectTransform)))
        {
            if (IgnoreMessage(evt.value)) return;
            Message msg = new Message(evt.value);
            Events.Add(() =>
            {
                ObjectTransform cv = new ObjectTransform(msg.Data());
                if (cv.name.Equals("Earth")) cv.Update(RemoteRotation);
            });
        }

        // Notifications
        else if (evt.IsEvent(typeof(Notification)))
        {
            if (IgnoreMessage(evt.value)) return;
            Message msg = new Message(evt.value);
            Events.Add(() => ShowNotification(new Notification(msg.Data())));
        }
    }

    // @BUG: should not be called at every frame
    public void SelectObject(Planet target, bool webSync = true)
    {
        if (platform)
        {
            if (InfoHeader.color != Color.red)
                InfoHeader.text = string.Format("Viewing: {0}", target.name);

            platform.SetTarget(target);
            if (webSync && UserInteractionTurn())
            {
                string sender = GetUsername();
                Dispatch(Channel.Application, new SelectObject(sender, target.name));
            }
        }
    }

    /// <summary>Called on chat submit message</summary>
    public void SendChatMessage()
    {
        try
        {
            CloseChat();
            ResetAnswers();
            string user = chatUserList.selectedText.text;
            string message = chat.text;
            if (string.IsNullOrEmpty(message))
            {
                Notification.title = "Nothing sent!";
                Notification.description = "Your message was empty so nothing was sent.";
                Notification.UpdateUI();
                Notification.OpenNotification();
            }
            else if (string.IsNullOrEmpty(user) || user.Equals("None"))
            {
                Notification.title = "Nothing sent!";
                Notification.description = "You did not set the user that will receive the message.";
                Notification.UpdateUI();
                Notification.OpenNotification();
            }
            else if (!IsStudent())
            {
                string sender = GetUsername();

                // Remove previous answer from app state
                DispatchUnset(Channel.Application, typeof(Answer));

                // Set new state
                Dispatch(Channel.User, new Question(sender, message), user);
                Dispatch(Channel.Application, new ActiveUser(sender, user));
                Dispatch(Channel.Application, new Notification(sender, user + " doing task"));
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public void OpenChat()
    {
        platform.DisableControls();
        UpdateUserList();
        Windows.OpenPanel("Chat");
    }

    public void CloseChat()
    {
        platform.EnableControls();
        Windows.OpenPanel("Home");
        isPlaying = true;
    }

    public void ResetRemoteScene()
    {
        RemoteRotation.localPosition = Vector3.zero;
        RemoteRotation.localRotation = Quaternion.identity;
        RemoteReferenceSystem.localPosition = Vector3.zero;
        RemoteReferenceSystem.localRotation = Quaternion.identity;
        if (platform.HasTarget())
        {
            Transform p = platform.Target().transform;
            p.localPosition = Vector3.zero;
            p.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

    public void NewSession()
    {
        ResetAnswers();
        DispatchUnset(Channel.Application, typeof(Answer));
        DispatchUnset(Channel.Application, typeof(Question));
        DispatchUnset(Channel.Application, typeof(ActiveUser));
        DispatchUnset(Channel.Application, typeof(CameraTransform));
        DispatchUnset(Channel.Application, typeof(Notification));
        DispatchUnset(Channel.Application, typeof(Result));
        DispatchUnset(Channel.Application, typeof(SelectObject));
    }

    public void StartSession()
    {
        Welcome.SetActive(false);
        WaitScreenText.text = "Please wait for your teacher (#" + GetRoomname() + ")";

#if UNITY_ANDROID
        InfoHeader.text = "Tap to place your AR scene";
        WaitScreenText.text = "Tap to place your AR scene";
#endif

        // Enable professor interface, if it applies
        // Otherwise shows a wait notification (app ctx event)
        if (!IsStudent())
        {
            string sender = GetUsername();
            Dispatch(Channel.Application, new ActiveUser(sender, "teacher"));
            PlayTurn(); // First message doest not get processed
        }
        else
        {
            WaitTurn(); // First message does not get processed
        }
    }

    /// <summary>
    ///  When student presses "End turn" button
    /// </summary>
    public void EndTurn()
    {
        string sender = GetUsername();
        Dispatch(Channel.Application, new ActiveUser(sender, "teacher"));
    }

    /// <summary>
    /// Send event to switch to this user
    /// </summary>
    public void StartTurn()
    {
        if (!IsStudent())
        {
            string sender = GetUsername();
            Dispatch(Channel.Application, new ActiveUser(sender, "teacher"));
            ShowShareButton();
        }
    }

    public void PlayTurn()
    {
#if UNITY_ANDROID

#else
        platform.EnableControls();
#endif

        Windows.OpenPanel("Home");

        bool isTeacher = !IsStudent();
        ChatBtn.SetActive(isTeacher);
        NewSessionBtn.SetActive(isTeacher);
        RegainControl.SetActive(isTeacher);
        ScoreUI.SetActive(!isTeacher);
        ShowShareButton();
        ResetRemoteScene();
    }

    public void WaitTurn()
    {
#if UNITY_ANDROID

#else
        platform.DisableControls();
#endif
        Windows.OpenPanel("WaitTurn");
        ResetRemoteScene();
    }

    public void ValidateAnswer(Answer answer)
    {
        Notification.title = "User answer";
        Notification.description = "Answer received from " + answer.sender;
        Notification.UpdateUI();
        Notification.OpenNotification();
        DrawAnswer(answer);
    }

    public void SubmitAnswer(Answer answer)
    {
        WaitScoreModal.titleText = "Answer submited";
        WaitScoreModal.descriptionText = "Please wait for the teacher to score your answer ";
        WaitScoreModal.UpdateUI();
        WaitScoreModal.OpenWindow();
        DrawAnswer(answer);
    }

    private void DrawAnswer(Answer answer)
    {
        GameObject hitObject = GameObject.Find(answer.name);
        Vector3 position = answer.Location();
        Vector3 onPlanet = UnityEngine.Random.onUnitSphere * hitObject.transform.localScale.x;
        GameObject pushpin = Instantiate(Prefab, onPlanet, Quaternion.identity);
        PushPin pin = pushpin.GetComponent<PushPin>();
        pin.answer = answer;
        pin.PlaceLocal(hitObject, position, Color.yellow);
        pins.Add(pin);
    }

    public void IsAnswerCorrect(bool correct)
    {
        // Send score to user
        if (correct) Dispatch(Channel.User, new Result(GetUsername(), "70"), studentId);
        else Dispatch(Channel.User, new Result(GetUsername(), "0"), studentId);

        // Remove answer from server
        DispatchUnset(Channel.Application, typeof(Answer));
    }

    public void ResetAnswers()
    {
        foreach (var p in pins) Destroy(p.gameObject);
        pins.Clear();
    }

    /// <summary>Ask question received from teacher</summary>
    public void AskQuestion(Question msg)
    {
        Modal.titleText = "Assignment from " + msg.sender;
        Modal.descriptionText = msg.content;
        Modal.UpdateUI();
        Modal.OpenWindow();
        ResetAnswers();
    }

    public void AcceptQuestion(bool accept)
    {
        string sender = GetUsername();
        if (accept) Dispatch(Channel.Application, new Notification(sender, sender + " took question"));
        else Dispatch(Channel.Application, new Notification(sender, sender + " rejected question"));

        // Remove question from server
        DispatchUnset(Channel.User, typeof(Question), sender);
    }

    /// <summary>
    /// Show a new message popup
    /// </summary>
    public void ShowNotification(Notification msg)
    {
        Notification.title = msg.sender;
        Notification.description = msg.content;
        Notification.UpdateUI();
        Notification.OpenNotification();
    }

    /// <summary>
    /// Asks the user to place the 3D scene. Will call ResetPlacement()
    /// </summary>
    public void InvokePlaceScene()
    {
        OnPlaceScene?.Invoke();
    }

    /// <summary>
    /// Introduces a delay to avoid imediate scene placement
    /// </summary>
    public void PlaceScene()
    {
        Invoke("InvokePlaceScene", 0.05f);
    }

    public void ActivateSharedView()
    {
        SetShared(true);
        if (UserInteractionTurn()) Dispatch(Channel.Application, new SharedView(GetUsername(), true));
    }

    public void DisableSharedView()
    {
        SetShared(false);
        if (UserInteractionTurn()) Dispatch(Channel.Application, new SharedView(GetUsername(), false));
    }

    /// <summary>
    /// Pre-condition: the user is allowed to interact. 
    /// Returns true if the action is to place a marker or false if it is to validate a object
    /// </summary>
    public bool PlaceMarker()
    {
        return IsStudent();
    }

    /// <summary>
    /// The user is allow to interact if it is her/his turn and is viewing the 3D content
    /// </summary>
    public bool UserInteractionTurn()
    {
        return IsPlaying() && IsHome();
    }

    public bool AwaitsPlacement()
    {
        return !IsSceneInPlace;
    }

    public void OnPlacement()
    {
        IsSceneInPlace = true;
        InfoHeader.text = "Your AR space is now configured!";
        WaitScreenText.text = "Your AR space is now configured";
        InfoHeader.color = Color.white;
    }

    public void ResetPlacement()
    {
        IsSceneInPlace = false;
        InfoHeader.text = "Tap to place your AR scene!!!";
        WaitScreenText.text = "Tap to place your AR scene!!!";
        InfoHeader.color = Color.red;
    }

    /// <summary>
    /// Hide replace scene button in desktop
    /// </summary>
    public void ShowShareButton()
    {
        ShareBtnHome.SetActive(true);
        ShareBtnWait.SetActive(true);
    }
    /// <summary>
    /// Hide replace scene button in desktop
    /// </summary>
    public void HideShareButton()
    {
        ShareBtnHome.SetActive(false);
        ShareBtnWait.SetActive(false);
    }

    public void SetShared(bool shared)
    {
        SharedHome.isOn = shared;
        SharedWait.isOn = shared;
        previewViewport.SetActive(!shared);
    }

    public bool IsShared()
    {
        return SharedHome.isOn;
    }
}
