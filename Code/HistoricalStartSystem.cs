// <copyright file="HistoricalStartSystem.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// </copyright>

namespace HistoricalStart
{
    using Colossal.Entities;
    using Colossal.Logging;
    using Game;
    using Game.Prefabs;
    using Unity.Collections;
    using Unity.Entities;

    /// <summary>
    /// The historical start mod system.
    /// </summary>
    internal sealed partial class HistoricalStartSystem : GameSystemBase
    {
        // References.
        private ILog _log;
        private EntityQuery _lockedQuery;
        private PrefabSystem _prefabSystem;

        /// <summary>
        /// Called every update.
        /// </summary>
        protected override void OnUpdate()
        {
            Mod.Log.Info("OnUpdate");

            foreach (Entity entity in _lockedQuery.ToEntityArray(Allocator.Temp))
            {
                // Train depot.
                if (EntityManager.TryGetComponent(entity, out TransportDepotData transportDepotData))
                {
                    if (transportDepotData.m_TransportType == TransportType.Train)
                    {
                        Mod.Log.Info("unlocking train depot");
                        EntityManager.RemoveComponent<Locked>(entity);
                        EntityManager.RemoveComponent<UnlockRequirement>(entity);
                    }
                }

                // Train tracks.
                else if (EntityManager.TryGetComponent(entity, out TrackData trackData))
                {
                    // Only train tracks.
                    if (trackData.m_TrackType == Game.Net.TrackTypes.Train)
                    {
                        Mod.Log.Info("unlocking train track");
                        EntityManager.RemoveComponent<Locked>(entity);
                        EntityManager.RemoveComponent<UnlockRequirement>(entity);
                    }
                }

                // Ship paths.
                else if (EntityManager.HasComponent<WaterwayData>(entity))
                {
                    Mod.Log.Info("unlocking waterway");
                    EntityManager.RemoveComponent<Locked>(entity);
                    EntityManager.RemoveComponent<UnlockRequirement>(entity);
                }

                // Cargo transport stations.
                else if (EntityManager.HasComponent<CargoTransportStationData>(entity) && EntityManager.TryGetComponent(entity, out TransportStationData transportStationData))
                {
                    // Exclude airports.
                    if (transportStationData.m_AircraftRefuelTypes == Game.Vehicles.EnergyTypes.None)
                    {
                        Mod.Log.Info("unlocking cargo transport station");
                        EntityManager.RemoveComponent<Locked>(entity);
                        EntityManager.RemoveComponent<UnlockRequirement>(entity);
                    }
                }

                // Specialized industry.
                else if (EntityManager.TryGetComponent(entity, out PlaceholderBuildingData placeholderData) && placeholderData.m_Type == BuildingType.ExtractorBuilding)
                {
                    _log.Info("unlocking extractor");
                    if (EntityManager.TryGetComponent(entity, out PrefabData prefabData) && _prefabSystem.GetPrefab<PrefabBase>(prefabData) is PrefabBase prefab)
                    {
                        _log.Info("extractor prefab name is " + prefab.name);
                        EntityManager.RemoveComponent<UnlockRequirement>(entity);
                        EntityManager.RemoveComponent<Locked>(entity);
                    }
                }

                // Specifically named prefabs.
                else if (EntityManager.TryGetComponent(entity, out PrefabData prefabData) && _prefabSystem.GetPrefab<PrefabBase>(prefabData) is PrefabBase prefab)
                {
                    if (prefab.name.Equals("TransportationTrain") || prefab.name.Equals("TrainStation01") || prefab.name.Equals("ZonesExtractors") || prefab.name.Equals("TransportationWater") || prefab.name.Equals("WaterTransportationGroup")
                        || prefab.name.Equals("TransportationGroup") || prefab.name.Equals("Harbor01"))
                    {
                        Mod.Log.Info("unlocking named prefab " + prefab.name);
                        EntityManager.RemoveComponent<UnlockRequirement>(entity);
                        EntityManager.RemoveComponent<Locked>(entity);
                    }
                }
            }
        }

        /// <summary>
        /// Called when the system is created.
        /// </summary>
        protected override void OnCreate()
        {
            base.OnCreate();

            // Set log.
            _log = Mod.Log;
            _log.Info("OnCreate");

            // Get system references.
            _prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();

            // Set up query.
            _lockedQuery = GetEntityQuery(ComponentType.ReadWrite<Locked>());
            RequireForUpdate(_lockedQuery);
        }
    }
}
