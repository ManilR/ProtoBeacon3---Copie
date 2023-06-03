using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Beacon
{
    public class InputManager : MonoBehaviour
    {
        [Header("Inputs")]
        [SerializeField] private InputAction LeftClick;
        [SerializeField] private InputAction RightClick;
        [SerializeField] private InputAction Escape;

        [Header("Cameras")]
        private bool UseMainCamera = true;
        private bool AllowClick = true;

        private Entity entity;
        private World world;

        public static InputManager instance;
        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(gameObject);
        }

        private void OnEnable()
        {
            LeftClick.started += LeftMouseClicked;
            LeftClick.Enable();

            RightClick.started += RightMouseClicked;
            RightClick.Enable();

            Escape.started += EscapePressed;
            Escape.Enable();

            EventManager.Instance.AddListener<GamePlayDayEvent>(onDayEvent);
            EventManager.Instance.AddListener<GamePlayNightEvent>(onNightEvent);

            world = World.DefaultGameObjectInjectionWorld;
        }

        private void OnDisable()
        {
            LeftClick.started -= LeftMouseClicked;
            LeftClick.Disable();

            RightClick.started -= RightMouseClicked;
            RightClick.Disable();

            Escape.started -= EscapePressed;
            Escape.Disable();

            EventManager.Instance.RemoveListener<GamePlayDayEvent>(onDayEvent);
            EventManager.Instance.RemoveListener<GamePlayNightEvent>(onNightEvent);

            if (world.IsCreated && world.EntityManager.Exists(entity))
                world.EntityManager.DestroyEntity(entity);
        }
        
        #region Input Callbacks
        private void LeftMouseClicked(InputAction.CallbackContext obj)
        {
            if (!AllowClick)
                return;

            Camera camera;

            if (UseMainCamera)
                camera = Camera.main;
            else
                camera = GameObject.Find("minimapCamera").GetComponent<Camera>();

            Vector2 screenPosition = obj.ReadValue<Vector2>();
            UnityEngine.Ray ray = camera.ScreenPointToRay(screenPosition);

            if (world.IsCreated && !world.EntityManager.Exists(entity))
            {
                entity = world.EntityManager.CreateEntity();
                world.EntityManager.AddBuffer<PlayerClickInput>(entity);
            }
            RaycastInput rcInput = new RaycastInput()
            {
                Start = ray.origin,
                Filter = CollisionFilter.Default,
                End = ray.GetPoint(camera.farClipPlane)
            };

            world.EntityManager.GetBuffer<PlayerClickInput>(entity).Add(new PlayerClickInput { Value = rcInput, clicktype = Clicktype.left});
        }
        private void RightMouseClicked(InputAction.CallbackContext obj)
        {
            if (!AllowClick)
                return;

            Camera camera;

            if (UseMainCamera)
                camera = Camera.main;
            else
                camera = GameObject.Find("minimapCamera").GetComponent<Camera>();

            Vector2 screenPosition = obj.ReadValue<Vector2>();
            UnityEngine.Ray ray = camera.ScreenPointToRay(screenPosition);

            if (world.IsCreated && !world.EntityManager.Exists(entity))
            {
                entity = world.EntityManager.CreateEntity();
                world.EntityManager.AddBuffer<PlayerClickInput>(entity);
            }
            RaycastInput rcInput = new RaycastInput()
            {
                Start = ray.origin,
                Filter = CollisionFilter.Default,
                End = ray.GetPoint(camera.farClipPlane)
            };

            world.EntityManager.GetBuffer<PlayerClickInput>(entity).Add(new PlayerClickInput { Value = rcInput, clicktype = Clicktype.right});
        }

        private void EscapePressed(InputAction.CallbackContext obj)
        {
            EventManager.Instance.Raise(new PauseButtonClickedEvent());
        }
        #endregion

        #region Event Callbacks
        public void onMinimapEnter(bool enter)
        {
            UseMainCamera = !enter;
            EventManager.Instance.Raise(new UpdateMinimapEvent()
            {
                value = enter
            });
        }

        public void onHUDEnter(bool enter)
        {
            AllowClick = !enter;
        }

        private void onDayEvent(GamePlayDayEvent e)
        {
            AllowClick = true;
        }
        private void onNightEvent(GamePlayNightEvent e)
        {
            AllowClick = true;
        }
        #endregion
    }

    public enum Clicktype { left, right, middle };
    public struct PlayerClickInput : IBufferElementData
    {
        public RaycastInput Value;
        public Clicktype clicktype;
    }
}