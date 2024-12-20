// <copyright file="HistoricalStartSystem.cs" company="algernon (K. Algernon A. Sheppard)">
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
        private EntityQuery _serviceQuery;
        private EntityQuery _milestoneQuery;
        private PrefabSystem _prefabSystem;

        /// <summary>
        /// Called every update.
        /// </summary>
        protected override void OnUpdate()
        {
            // Get current settings.
            ModSettings currentSettings = Mod.Instance.ActiveSettings;

            // Set unlocks.
            bool unlockBus = currentSettings.UnlockBus;
            bool unlockTrams = currentSettings.UnlockTrams;
            bool unlockTrains = currentSettings.UnlockTrains;
            bool unlockShips = currentSettings.UnlockShips;
            bool unlockFarming = currentSettings.UnlockFarming;
            bool unlockMining = currentSettings.UnlockMining;
            bool unlockOil = currentSettings.UnlockOil;
            bool unlockBasicHighways = currentSettings.UnlockBasicHighways | currentSettings.UnlockAllHighways;
            bool unlockAllHighways = currentSettings.UnlockAllHighways;
            bool unlockingProduction = unlockFarming | unlockMining | unlockOil;
            bool unlockingTransport = unlockBus | unlockTrams | unlockTrains | unlockShips;

            // Entity references.
            Entity transportationServiceEntity = Entity.Null;
            Entity milestone4Entity = Entity.Null;

            // Set up for Transport service unlock requirement replacement if we're unlocking the transportation budget.
            if (unlockingTransport)
            {
                // Get milestone 4 entity.
                foreach (Entity entity in _milestoneQuery.ToEntityArray(Allocator.Temp))
                {
                    if (EntityManager.TryGetComponent(entity, out MilestoneData milestoneData) && milestoneData.m_Index == 4)
                    {
                        _log.Debug($"Found milestone {milestoneData.m_Index} entity {entity}");
                        milestone4Entity = entity;
                        break;
                    }
                }

                // If we found the milestone entity, get (and unlock) the Transportation service entity.
                if (milestone4Entity != Entity.Null)
                {
                    foreach (Entity entity in _serviceQuery.ToEntityArray(Allocator.Temp))
                    {
                        if (EntityManager.TryGetComponent(entity, out ServiceData serviceData) && serviceData.m_Service == Game.City.CityService.Transportation)
                        {
                            _log.Debug($"Found {serviceData.m_Service} entity {entity}");
                            transportationServiceEntity = entity;
                            Unlock(entity);
                            break;
                        }
                    }
                }
            }

            // Determine if we're going ahead with Transport service unlock requriement replacement (requires having found both milestone and service entity).
            bool replacingEntities = unlockingTransport & transportationServiceEntity != Entity.Null & milestone4Entity != Entity.Null;

            // Now iterate through all enities in our main query.
            foreach (Entity entity in _lockedQuery.ToEntityArray(Allocator.Temp))
            {
                string entityPrefabName = GetEntityPrefab(entity);

                switch (entityPrefabName)
                {
                    // General transport-related prefabs (e.g. budgets, transport overview) if needed.
                    case "Budget":
                    case "City Budget":
                    case "Service Budgets":
                    case "Transportation Overview":
                        if (unlockingTransport)
                        {
                            _log.Info($"Unlocking Transportation service-related prefab {entityPrefabName} {entity}");
                            Unlock(entity);
                        }

                        continue;

                    // Advanced farming.
                    case "Agriculture Area Placeholder - Vegetable":
                    case "Agriculture Area Placeholder - Cotton":

                        if (unlockFarming)
                        {
                            _log.Info($"Unlocking farming prefab {entityPrefabName} {entity}");
                            Unlock(entity);
                        }

                        continue;

                    // Oil.
                    case "Oil Area Placeholder":
                        if (unlockOil)
                        {
                            _log.Info($"Unlocking oil prefab {entityPrefabName} {entity}");
                            Unlock(entity);
                        }

                        continue;

                    // Mining.
                    case "Ore Area Placeholder - Ore":
                    case "Ore Area Placeholder - Coal":
                        if (unlockMining)
                        {
                            _log.Info($"Unlocking mining prefab {entityPrefabName} {entity}");
                            Unlock(entity);
                        }

                        continue;

                    // General resource extraction.
                    case "Production Panel":
                        if (unlockingProduction)
                        {
                            _log.Info($"Unlocking production prefab {entityPrefabName} {entity}");
                            Unlock(entity);
                        }

                        continue;

                    case "Highway Twoway - 2 lanes":
                    case "Highway Twoway - 3 lanes":
                        if (unlockBasicHighways)
                        {
                            _log.Info($"Unlocking basic highway {entityPrefabName} {entity}");
                            Unlock(entity);
                        }

                        continue;
                }

                if (unlockingTransport | unlockAllHighways)
                {
                    // Get unlock requirements and iterate through.
                    if (EntityManager.TryGetBuffer(entity, true, out DynamicBuffer<UnlockRequirement> unlockRequirements))
                    {
                        for (int i = 0; i < unlockRequirements.Length; ++i)
                        {
                            UnlockRequirement requirement = unlockRequirements[i];

                            string requirementPrefabName = GetEntityPrefab(requirement.m_Prefab);

                            _log.Debug($"...... requirement name {requirementPrefabName}");

                            switch (requirementPrefabName)
                            {
                                // Buses.
                                case "BasicTransportationNode":
                                case "Bus Depot Built Req":
                                    if (unlockBus)
                                    {
                                        _log.Info($"Unlocking bus prefab {entityPrefabName} {entity}");
                                        Unlock(entity);
                                    }

                                    continue;

                                // Trams.
                                case "TransportationTram":
                                case "Tram Depot Built Req":
                                    if (unlockTrams)
                                    {
                                        _log.Info($"Unlocking tram prefab {entityPrefabName} {entity}");
                                        Unlock(entity);
                                    }

                                    continue;

                                // Rail.
                                case "TrainNode":
                                case "TransportationTrain":
                                case "Train Track Built Req":
                                case "Passenger Train Station Built Req":
                                case "Cargo Train Terminal Built Req":
                                    if (unlockTrains)
                                    {
                                        _log.Info($"Unlocking rail prefab {entityPrefabName} {entity}");
                                        Unlock(entity);
                                    }

                                    continue;

                                // Shipping.
                                case "TransportationWater":
                                case "HarborNode":
                                case "Passenger Harbor Built Req":
                                case "Cargo Harbor Built Req":
                                    if (unlockShips)
                                    {
                                        _log.Info($"Unlocking shipping prefab {entityPrefabName} {entity}");
                                        Unlock(entity);
                                    }

                                    continue;

                                case "RoadsHighways":
                                    if (unlockAllHighways)
                                    {
                                        _log.Info($"Unlocking highway prefab {entityPrefabName} {entity}");
                                        Unlock(entity);
                                    }

                                    continue;
                            }

                            // If not one of the above named prefabs, perform Transport service unlock requirement replacement if doing so.
                            if (replacingEntities)
                            {
                                if (requirement.m_Prefab == transportationServiceEntity)
                                {
                                    _log.Debug($"  Swapping transportation service requirement for {entityPrefabName} {entity}");
                                    requirement.m_Prefab = milestone4Entity;
                                    unlockRequirements[i] = requirement;
                                }
                            }
                        }
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

            // Get system references.
            _prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();

            // Set up queries.
            _lockedQuery = SystemAPI.QueryBuilder().WithAllRW<Locked>().WithAll<PrefabData>().Build();
            _serviceQuery = SystemAPI.QueryBuilder().WithAll<ServiceData>().Build();
            _milestoneQuery = SystemAPI.QueryBuilder().WithAll<MilestoneData>().Build();
            RequireForUpdate(_serviceQuery);
            RequireForUpdate(_milestoneQuery);
            RequireForUpdate(_lockedQuery);
        }

        /// <summary>
        /// Unlocks the given entity.
        /// </summary>
        /// <param name="entity">Entity to unlock.</param>
        private void Unlock(Entity entity)
        {
            // Unlock via unock event.
            EntityArchetype m_UnlockEventArchetype = EntityManager.CreateArchetype(ComponentType.ReadWrite<Game.Common.Event>(), ComponentType.ReadWrite<Unlock>());
            Entity e = EntityManager.CreateEntity(m_UnlockEventArchetype);
            EntityManager.AddComponentData(e, new Game.Prefabs.Unlock(entity));

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

        /// <summary>
        /// Attempts to get the prefab name associated with a given entity.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <returns>Entity prefab name, or <see cref="string.Empty"/> if the entity doesn't contain a prefab or the prefab reference was invalid.</returns>
        private string GetEntityPrefab(Entity entity)
        {
            if (EntityManager.TryGetComponent(entity, out PrefabData entityPrefabData)
                    && entityPrefabData.m_Index > 0
                    && _prefabSystem.GetPrefab<PrefabBase>(entityPrefabData) is PrefabBase entityPrefab
                    && !string.IsNullOrEmpty(entityPrefab.name))
            {
                return entityPrefab.name;
            }

            // If we got here, we failed.
            return string.Empty;
        }
    }
}
