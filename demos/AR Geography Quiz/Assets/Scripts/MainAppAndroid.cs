namespace AreteAR
{
#if UNITY_ANDROID

    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;
    using OrkestraLib.Message;
    using static OrkestraLib.Orkestra;

    [RequireComponent(typeof(ARRaycastManager))]
    public class MainAppAndroid : MonoBehaviour
    {
        private static readonly List<ARRaycastHit> rayHits = new List<ARRaycastHit>();
        private ARRaycastManager raycastManager;
        private ARPlaneManager planeManager;
        public Multiplatform platform;

        // First touch
        private Vector2 lastTouchPosition;
        private int touchId = -1;
        private readonly float touchSpeed = 6f;

        // Viewport for remote user
        public Camera removeUserCamera;


        // Raycase framelimit
        private readonly float cooldownRaycast = 0.250f;
        private float timeRaycast = 0.0f;
        private bool PlaneVisible = true;

        private float timePressed = 0.0f;
        private float timeLastPress = 0.0f;
        public float timeDelayThreshold = 2.0f;

        void Awake()
        {
            raycastManager = GetComponent<ARRaycastManager>();
            planeManager = GetComponent<ARPlaneManager>();
        }

        void Start()
        {
            platform = GameObject.Find("Multiplatform").GetComponent<Multiplatform>();
            platform.OnSceneLoaded = (app) =>
            {
                planeManager.planesChanged += (e) =>
                {
                    foreach (ARPlane plane in e.added)
                    {
                        plane.gameObject.SetActive(PlaneVisible);
                    }
                };

                app.OnPlaceScene = () =>
                {
                    app.HideShareButton();
                    app.ResetPlacement();
                    PlaneVisibility(true);
                };

                app.OnLateUpdate = () =>
                {
                    timeRaycast += Time.deltaTime;

                    if (app.UserInteractionTurn())
                        app.ShowShareButton();
                    else
                        app.HideShareButton();

                    // Block all interactions until the player's turn 
                    if (timeRaycast > cooldownRaycast && TryGetTouchPosition(out Vector2 touchPosition))
                    {
                        if (app.AwaitsPlacement())
                        {
                            app.ReferenceSystem.SetActive(false);

                            // Place scene
                            if (raycastManager.Raycast(touchPosition, rayHits, TrackableType.PlaneWithinPolygon))
                            {
                                timeRaycast = 0.0f;
                                Pose firstHitPose = rayHits[0].pose;
                                //app.ReferenceSystem.transform.rotation = firstHitPose.rotation;
                                app.ReferenceSystem.transform.position = firstHitPose.position;
                                app.ReferenceSystem.SetActive(true);
                                app.OnPlacement();
                                PlaneVisibility(false);
                            }
                        }
                        else if (app.UserInteractionTurn())
                        {
                            // We are dragging
                            if (Input.touchCount > 1)
                            {
                                foreach (Touch t in Input.touches)
                                {
                                    // Select finger for drag
                                    if (touchId == -1)
                                    {
                                        if (t.phase == TouchPhase.Began)
                                        {
                                            lastTouchPosition = t.position;
                                            touchId = t.fingerId;
                                        }
                                    }
                                    // Use pre-selected finder for drag
                                    else
                                    {
                                        if (t.fingerId != touchId) continue;
                                        if (t.phase == TouchPhase.Moved)
                                        {
                                            Vector3 offset = t.position - lastTouchPosition;
                                            lastTouchPosition = t.position;
                                            float rotX = offset.x * touchSpeed * Mathf.Deg2Rad;
                                            float rotY = offset.y * touchSpeed * Mathf.Deg2Rad;
                                            if (platform.HasTarget())
                                            {
                                                platform.Target().transform.Rotate(Vector3.up, -rotX);
                                                platform.Target().transform.Rotate(Vector3.right, rotY);
                                                // check
                                                app.Dispatch(Channel.Application, new ObjectTransform(app.GetUsername(), platform.Target().gameObject));
                                            }
                                        }
                                        else if (t.phase == TouchPhase.Ended)
                                        {
                                            touchId = -1;
                                        }
                                    }
                                }
                            }

                            else if (Input.touchCount == 1)
                            {
                                if (Input.touches[0].phase == TouchPhase.Began)
                                    timeLastPress = Time.time; 

                                if (Input.touches[0].phase == TouchPhase.Ended)
                                {
                                    timePressed = Time.time - timeLastPress;
                                    if (timePressed > timeDelayThreshold)
                                    {
                                        timeRaycast = 0.0f;
                                        Ray ray = Camera.main.ScreenPointToRay(touchPosition);
                                        if (Physics.Raycast(ray, out RaycastHit hit))
                                        {
                                            Planet obj = hit.collider.gameObject.GetComponent<Planet>();
                                              if (obj)
                                            {
                                                // Select this object if not in focus
                                                app.SelectObject(obj);

                                                // We want to place a marker
                                                if (app.PlaceMarker()) CreateNewPin(hit);

                                                // We want to validate a marker
                                                else RemovePin(hit);
                                            }
                                        }
                                    }
                                }   
                            }
                        }
                    }
                };
            };
        }

        private void PlaneVisibility(bool visibility)
        {
            PlaneVisible = visibility;
            foreach (ARPlane plane in planeManager.trackables)
            {
                plane.gameObject.SetActive(PlaneVisible);
            }
        }

        private bool TryGetTouchPosition(out Vector2 touchPosition)
        {
            if (Input.touchCount > 0)
            {
                touchPosition = Input.GetTouch(0).position;
                return true;
            }

            touchPosition = default;
            return false;
        }

        private void CreateNewPin(RaycastHit hit)
        {
            timeRaycast = 0.0f;
            string sender = platform.App.GetUsername();
            Vector3 pos = PushPin.Position(platform.App.Prefab, hit.collider.gameObject, hit.point);
            Answer tempAnsw = new Answer(sender, hit.collider.gameObject, pos);
            platform.App.Dispatch(Channel.Application, tempAnsw);
            platform.App.Dispatch(Channel.Application, new ActiveUser(sender, "teacher"));
            platform.App.SubmitAnswer(tempAnsw);
        }

        private void RemovePin(RaycastHit hit)
        {
            PushPin pp = hit.collider.gameObject.GetComponent<PushPin>();
            if (pp != null)
            {
                platform.App.CorrectAnswerForm.titleText = "Validate answer";
                platform.App.CorrectAnswerForm.descriptionText = "Is this answer correct ?";
                platform.App.CorrectAnswerForm.UpdateUI();
                platform.App.CorrectAnswerForm.OpenWindow();
                platform.App.pins.Remove(pp);
                Destroy(pp.gameObject);
            }
        }
    }
#endif
}