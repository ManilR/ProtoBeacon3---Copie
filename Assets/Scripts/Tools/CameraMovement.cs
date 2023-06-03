using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Beacon
{
    public class CameraMovement : MonoBehaviour
    {
        [Space(10)]
        [SerializeField] private float cameraSpeed = 20;
        [Space(10)]
        [Header("Camera FOV")]
        [SerializeField, Range(10f, 150f)] private float minFOV = 25f;
        [SerializeField, Range(10f, 150f)] private float maxFOV = 120f;
        [Header("Minimap Size")]
        [SerializeField, Range(10f, 150f)] private float minMinimapSize = 25f;
        [SerializeField, Range(10f, 150f)] private float maxMinimapSize = 150f;
        [Space(10)]
        [SerializeField] private bool inverseScroll = false;

        private Entity selectedBuilding;
        private Entity beacon;
        private EntityQuery clickedBuildingQuery;
        private EntityQuery beaconQuery;
        private EntityManager _entityManager;

        private LocalTransform building;

        private GameObject minimap;

        private bool UpdateMainCamera = true;

        private void OnEnable()
        {
            EventManager.Instance.AddListener<DataLoadedEvent>(onDataLoadedEvent);
            EventManager.Instance.AddListener<UpdateMinimapEvent>(onUpdateMinimapEvent);
        }

        private void OnDisable()
        {
            EventManager.Instance.RemoveListener<DataLoadedEvent>(onDataLoadedEvent);
            EventManager.Instance.RemoveListener<UpdateMinimapEvent>(onUpdateMinimapEvent);
        }

        private void Start()
        {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            clickedBuildingQuery = _entityManager.CreateEntityQuery(new ComponentType[] { typeof(Clicked), typeof(Building) });
            beaconQuery = _entityManager.CreateEntityQuery(new ComponentType[] { typeof(Beacon) });
        }

        private void Update()
        {
            if (!GameManager.instance.isPlaying)
                return;

            float hAxis;

            if (Input.GetMouseButton(2))
                hAxis = Input.GetAxis("Mouse X") * cameraSpeed;
            else
                hAxis = -Input.GetAxis("Horizontal") * 5;

            transform.RotateAround(new Vector3(0.0f, 10.0f, 0.0f), Vector3.up, hAxis * 5 * Time.fixedDeltaTime);

            foreach (var entity in beaconQuery.ToEntityArray(Allocator.Temp))
                beacon = entity;
            if (beacon != Entity.Null)
            {
                building = _entityManager.GetComponentData<LocalTransform>(beacon);
                Quaternion beaconQuat = building.Rotation;
                Quaternion cameraQuat = transform.rotation;
                Vector3 beaconVect = beaconQuat.eulerAngles;
                Vector3 cameraVect = cameraQuat.eulerAngles;
                beaconVect.y = cameraVect.y + 270;
                beaconQuat = Quaternion.Euler(beaconVect);
                building.Rotation = beaconQuat;
                _entityManager.SetComponentData<LocalTransform>(beacon, building);
            }

            minimap = GameObject.Find("minimapCamera");
            if (minimap)
            {
                Quaternion minimapQuat = minimap.transform.rotation;
                Quaternion cameraQuat = transform.rotation;
                Vector3 minimapVect = minimapQuat.eulerAngles;
                Vector3 cameraVect = cameraQuat.eulerAngles;
                minimapVect.y = cameraVect.y;
                minimapQuat = Quaternion.Euler(minimapVect);
                minimap.transform.rotation = minimapQuat;
            }

            float scroll;

            if (Input.mouseScrollDelta.y != 0)
                scroll = Input.GetAxis("Mouse ScrollWheel") * 3f * (inverseScroll ? -1 : 1);
            else
                scroll = -(Input.GetAxis("Vertical") * 0.1f);

            float zoomedFOV ;

            if (UpdateMainCamera)
            {
                zoomedFOV = Camera.main.fieldOfView + scroll;
                Camera.main.fieldOfView = Mathf.Clamp(zoomedFOV, minFOV, maxFOV);
            }
            else
            {
                zoomedFOV = minimap.GetComponent<Camera>().orthographicSize + scroll;
                minimap.GetComponent<Camera>().orthographicSize = Mathf.Clamp(zoomedFOV, minMinimapSize, maxMinimapSize);
            }
                

            if (hAxis != 0 || (scroll != 0 && UpdateMainCamera))
            {
                EventManager.Instance.Raise(new CameraMovedEvent()
                {
                    zoomLevel = Camera.main.fieldOfView,
                    position = transform.position,
                    rotation = transform.rotation
                });
            }
        }

        private void onDataLoadedEvent(DataLoadedEvent e)
        {
            transform.position = e.settings.cameraPos;
            transform.rotation = e.settings.cameraRot;
            Camera.main.fieldOfView = e.settings.zoomLevel;
        }

        public void onUpdateMinimapEvent(UpdateMinimapEvent e)
        {
            UpdateMainCamera = !e.value;
        }
    }
}