using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class UnitAuthoring : MonoBehaviour
{
    #region Event Fields
    #endregion

    #region Public Fields
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

    public class Baker : Baker<UnitAuthoring>
    {
        public override void Bake(UnitAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Unit());
        }
    }
}

public struct Unit : IComponentData
{

}