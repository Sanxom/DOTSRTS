using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSelectionManager : MonoBehaviour
{

    #region Event Fields
    public event Action OnSelectionAreaStart;
    public event Action OnSelectionAreaEnd;
    #endregion

    #region Public Fields
    #endregion

    #region Serialized Private Fields
    [SerializeField] private Camera _mainCamera;
    #endregion

    #region Private Fields
    private Vector2 _selectedStartMousePosition;
    #endregion

    #region Public Properties
    public static UnitSelectionManager Instance { get; private set; }
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        if (Instance != this && Instance != null)
            Destroy(gameObject);
        else
            Instance = this;
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            _selectedStartMousePosition = MouseWorldPosition.Instance.MousePositionAction.ReadValue<Vector2>();
            OnSelectionAreaStart?.Invoke();
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            // Vector2 selectedEndMousePosition = MouseWorldPosition.Instance.MousePositionAction.ReadValue<Vector2>();

        // Deselect Units
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected>().Build(entityManager);
            NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
            NativeArray<Selected> selectedArray = entityQuery.ToComponentDataArray<Selected>(Allocator.Temp);
            for (int i = 0; i < entityArray.Length; i++)
            {
                entityManager.SetComponentEnabled<Selected>(entityArray[i], false);
                Selected selected = selectedArray[i];
                selected.onDeselected = true;
                entityManager.SetComponentData(entityArray[i], selected);
            }

            Rect selectionAreaRect = GetSelectionAreaRect();
            float selectionAreaSize = selectionAreaRect.width + selectionAreaRect.height;
            float multipleSelectionSizeMin = 40f;
            bool isMultipleSelection = selectionAreaSize > multipleSelectionSizeMin;

            if (isMultipleSelection)
            {
        // Multi-select Units
                entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<LocalTransform, Unit>().WithPresent<Selected>().Build(entityManager);
                entityArray = entityQuery.ToEntityArray(Allocator.Temp);
                NativeArray<LocalTransform> localTransformArray = entityQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
                for (int i = 0; i < localTransformArray.Length; i++)
                {
                    LocalTransform unitLocalTransform = localTransformArray[i];
                    Vector2 unitScreenPosition = _mainCamera.WorldToScreenPoint(unitLocalTransform.Position);
                    if (selectionAreaRect.Contains(unitScreenPosition))
                    {
                        entityManager.SetComponentEnabled<Selected>(entityArray[i], true);
                        Selected selected = entityManager.GetComponentData<Selected>(entityArray[i]);
                        selected.onSelected = true;
                        entityManager.SetComponentData(entityArray[i], selected);
                    }
                }
            }
            else
            {
        // Single Select Units
                entityQuery = entityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton));
                PhysicsWorldSingleton physicsWorldSingleton = entityQuery.GetSingleton<PhysicsWorldSingleton>();
                CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
                UnityEngine.Ray cameraRay = _mainCamera.ScreenPointToRay(MouseWorldPosition.Instance.MousePositionAction.ReadValue<Vector2>());
                int unitsLayer = 6;
                RaycastInput raycastInput = new()
                {
                    Start = cameraRay.GetPoint(0f),
                    End = cameraRay.GetPoint(9999f),
                    Filter = new CollisionFilter
                    {
                        BelongsTo = ~0u,
                        CollidesWith = 1u << unitsLayer,
                        GroupIndex = 0,
                    }
                };

                if (collisionWorld.CastRay(raycastInput, out Unity.Physics.RaycastHit hit))
                {
                    if (entityManager.HasComponent<Unit>(hit.Entity)){
                        // Hit a Unit
                        entityManager.SetComponentEnabled<Selected>(hit.Entity, true);
                        Selected selected = entityManager.GetComponentData<Selected>(hit.Entity);
                        selected.onSelected = true;
                        entityManager.SetComponentData(hit.Entity, selected);
                    }
                }
            }

                OnSelectionAreaEnd?.Invoke();
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            Vector3 mouseWorldPosition = MouseWorldPosition.Instance.GetRaycastArrayPosition();

            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<UnitMover, Selected>().Build(entityManager);

            NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
            NativeArray<UnitMover> unitMoverArray = entityQuery.ToComponentDataArray<UnitMover>(Allocator.Temp);

            NativeArray<float3> movePositionArray = GenerateMovePositionArray(mouseWorldPosition, entityArray.Length);

            for (int i = 0; i < unitMoverArray.Length; i++)
            {
                UnitMover unitMover = unitMoverArray[i];
                unitMover.targetPosition = movePositionArray[i];
                unitMoverArray[i] = unitMover;
            }
            entityQuery.CopyFromComponentDataArray(unitMoverArray);
        }
    }
    #endregion

    #region Public Methods
    public Rect GetSelectionAreaRect()
    {
        Vector2 selectedEndMousePosition = MouseWorldPosition.Instance.MousePositionAction.ReadValue<Vector2>();
        Vector2 lowerLeftCorner = new(
            Mathf.Min(_selectedStartMousePosition.x, selectedEndMousePosition.x), 
            Mathf.Min(_selectedStartMousePosition.y, selectedEndMousePosition.y)
            );
        Vector2 upperRightCorner = new(
            Mathf.Max(_selectedStartMousePosition.x, selectedEndMousePosition.x),
            Mathf.Max(_selectedStartMousePosition.y, selectedEndMousePosition.y)
            );
        return new Rect(
            lowerLeftCorner.x,
            lowerLeftCorner.y,
            upperRightCorner.x - lowerLeftCorner.x,
            upperRightCorner.y - lowerLeftCorner.y
        );
    }
    #endregion

    #region Private Methods
    private NativeArray<float3> GenerateMovePositionArray(float3 targetPosition, int positionCount)
    {
        NativeArray<float3> positionArray = new(positionCount, Allocator.Temp);
        if (positionCount == 0)
            return positionArray;

        positionArray[0] = targetPosition;
        if (positionCount == 1)
            return positionArray;

        float ringSize = 2.2f;
        int ring = 0;
        int positionIndex = 1;

        while(positionIndex < positionCount)
        {
            int ringPositionCount = 3 + ring * 2;

            for (int i = 0; i < ringPositionCount; i++)
            {
                float angle = i * (math.PI2 / ringPositionCount);
                float3 ringVector = math.rotate(quaternion.RotateY(angle),  new float3(ringSize * (ring + 1), 0, 0));
                float3 ringPosition = targetPosition + ringVector;

                positionArray[positionIndex] = ringPosition;
                positionIndex++;

                if (positionIndex >= positionCount)
                    break;
            }
            ring++;
        }
        return positionArray;
    }
    #endregion
}