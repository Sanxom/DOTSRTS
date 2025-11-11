using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class UnitMoverAuthoring : MonoBehaviour
{
    #region Event Fields
    #endregion

    #region Public Fields
    public float moveSpeed;
    public float rotateSpeed;
    #endregion

    #region Serialized Private Fields
    #endregion

    #region Private Fields
    #endregion

    #region Public Properties
    #endregion

    #region Unity Callbacks
    #endregion

    #region Public Methods
    #endregion

    #region Private Methods
    #endregion

    public class Baker : Baker<UnitMoverAuthoring>
    {
        public override void Bake(UnitMoverAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new UnitMover
            {
                moveSpeed = authoring.moveSpeed,
                rotateSpeed = authoring.rotateSpeed,
            });
        }
    }
}

public struct UnitMover : IComponentData
{
    public float moveSpeed;
    public float rotateSpeed;
    public float3 targetPosition;
}