﻿// <copyright file="HistoricalStartSystem.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache Licence, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// See LICENSE.txt file in the project root for full license information.
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
            foreach (Entity entity in _lockedQuery.ToEntityArray(Allocator.Temp))
            {
                // Train depot.
                if (EntityManager.TryGetComponent(entity, out TransportDepotData transportDepotData))
                {
                    if (transportDepotData.m_TransportType == TransportType.Train)
                    {
                        _log.Debug("unlocking train depot");
                        Unlock(entity);
                    }
                }

                // Train tracks.
                else if (EntityManager.TryGetComponent(entity, out TrackData trackData))
                {
                    // Only train tracks.
                    if (trackData.m_TrackType == Game.Net.TrackTypes.Train)
                    {
                        _log.Debug("unlocking train track");
                        Unlock(entity);
                    }
                }

                // Ship paths.
                else if (EntityManager.HasComponent<WaterwayData>(entity))
                {
                    _log.Debug("unlocking waterway");
                    Unlock(entity);
                }

                // Cargo transport stations.
                else if (EntityManager.HasComponent<CargoTransportStationData>(entity) && EntityManager.TryGetComponent(entity, out TransportStationData transportStationData))
                {
                    // Exclude airports.
                    if (transportStationData.m_AircraftRefuelTypes == Game.Vehicles.EnergyTypes.None)
                    {
                        _log.Debug("unlocking cargo transport station");
                        Unlock(entity);
                    }
                }

                // Specialized industry.
                else if (EntityManager.TryGetComponent(entity, out PlaceholderBuildingData placeholderData) && placeholderData.m_Type == BuildingType.ExtractorBuilding)
                {
                    _log.Debug("unlocking extractor");
                    Unlock(entity);
                }

                // Transport lines.
                else if (EntityManager.TryGetComponent(entity, out TransportLineData transportLineData))
                {
                    if (transportLineData.m_TransportType == TransportType.Train || transportLineData.m_TransportType == TransportType.Ship)
                    {
                        _log.Debug("unlocking transport line");
                        Unlock(entity);
                    }
                }

                // Specifically named prefabs.
                else if (EntityManager.TryGetComponent(entity, out PrefabData prefabData) && prefabData.m_Index > 0 && _prefabSystem.GetPrefab<PrefabBase>(prefabData) is PrefabBase prefab && !string.IsNullOrEmpty(prefab.name))
                {
                    switch (prefab.name)
                    {
                        case "Harbor01":
                        case "Highway Twoway - 2 lanes":
                        case "Highway Twoway - 3 lanes":
                        case "TrainStation01":
                        case "TrainStation02":
                        case "TrainStation03":
                        case "ZonesExtractors":
                        case "Budget":
                        case "City Budget":
                        case "Taxation":
                        case "Loans":
                        case "Service Budgets":
                        case "Production Panel":
                        case "Extractors":
                            _log.Debug($"unlocking named prefab {prefab.name}");
                            Unlock(entity);
                            break;
                        default:
                            _log.Debug($"Found unidentified prefab {prefab.name}");
                            break;
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
            _log = Mod.Instance.Log;
            _log.Info("OnCreate");

            // Get system references.
            _prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();

            // Set up query.
            _lockedQuery = SystemAPI.QueryBuilder().WithAllRW<Locked>().Build();
            RequireForUpdate(_lockedQuery);
        }

        /// <summary>
        /// Unlocks the given entity.
        /// </summary>
        /// <param name="entity">Entity to unlock.</param>
        private void Unlock(Entity entity)
        {
            // Unlock entity and remove unlock requirements.
            EntityManager.RemoveComponent<UnlockRequirement>(entity);
            EntityManager.RemoveComponent<Locked>(entity);

            // Reduce XP gain.
            if (EntityManager.TryGetComponent(entity, out PlaceableObjectData placeableData))
            {
                _log.Debug("Reducing XP from PlaceableObjectData");
                placeableData.m_XPReward /= 10;
                EntityManager.SetComponentData(entity, placeableData);
            }

            if (EntityManager.TryGetComponent(entity, out ServiceUpgradeData serviceData))
            {
                _log.Debug("Reducing XP from ServiceUpgradeData");
                placeableData.m_XPReward /= 10;
                EntityManager.SetComponentData(entity, serviceData);
            }
        }
    }
}
