// <copyright file="Mod.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// </copyright>

namespace HistoricalStart
{
    using System.Reflection;
    using Colossal.Logging;
    using Game;
    using Game.Modding;

    /// <summary>
    /// The base mod class for instantiation by the game.
    /// </summary>
    public sealed class Mod : IMod
    {
        /// <summary>
        /// The mod's default name.
        /// </summary>
        public const string ModName = "Historical Start";

        /// <summary>
        /// Gets the active instance reference.
        /// </summary>
        public static Mod Instance { get; private set; }

        /// <summary>
        /// Gets the mod's active log.
        /// </summary>
        internal ILog Log { get; private set; }

        /// <summary>
        /// Called by the game when the mod is loaded.
        /// </summary>
        /// <param name="updateSystem">Game update system.</param>
        public void OnLoad(UpdateSystem updateSystem)
        {
            // Set instance reference.
            Instance = this;

            // Initialize logger.
            Log = LogManager.GetLogger(ModName);
#if DEBUG
            Log.Info("setting logging level to Debug");
            Log.effectivenessLevel = Level.Debug;
#endif
            Log.Info($"loading {ModName} version {Assembly.GetExecutingAssembly().GetName().Version}");

            // Enable system.
            updateSystem.UpdateAfter<HistoricalStartSystem>(SystemUpdatePhase.Deserialize);
        }

        /// <summary>
        /// Called by the game when the mod is disposed of.
        /// </summary>
        public void OnDispose()
        {
            Log.Info("disposing");
        }
    }
}
