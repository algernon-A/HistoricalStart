// <copyright file="ModSettings.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache Licence, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace HistoricalStart
{
    using System.Xml.Serialization;
    using Colossal.IO.AssetDatabase;
    using Game.Modding;
    using Game.Settings;

    /// <summary>
    /// The mod's settings.
    /// </summary>
    [FileLocation(Mod.ModName)]
    public class ModSettings : ModSetting
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModSettings"/> class.
        /// </summary>
        /// <param name="mod"><see cref="IMod"/> instance.</param>
        public ModSettings(IMod mod)
            : base(mod)
        {
            SetDefaults();
        }

        /// <summary>
        /// Gets or sets a value indicating whether buses should be unlocked on load.
        /// </summary>
        public bool UnlockBus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether trams should be unlocked on load.
        /// </summary>
        public bool UnlockTrams { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether railways should be unlocked on load.
        /// </summary>
        public bool UnlockTrains { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether ships should be unlocked on load.
        /// </summary>
        public bool UnlockShips { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether advanced farming should be unlocked on load.
        /// </summary>
        public bool UnlockFarming { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether ore and coal extraction should be unlocked on load.
        /// </summary>
        public bool UnlockMining { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether oil extraction should be unlocked on load.
        /// </summary>
        public bool UnlockOil { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether basic highways should be unlocked on load.
        /// </summary>
        public bool UnlockBasicHighways { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether all highways should be unlocked on load.
        /// </summary>
        public bool UnlockAllHighways { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether districts should be unlocked on load.
        /// </summary>
        public bool UnlockDistricts { get; set; }

        /// <summary>
        /// Sets a value indicating whether the mod's settings should be reset.
        /// </summary>
        [XmlIgnore]
        [SettingsUIButton]
        [SettingsUISection("ResetModSettings")]
        [SettingsUIConfirmation]
        public bool ResetModSettings
        {
            set
            {
                // Apply defaults.
                SetDefaults();

                // Save.
                ApplyAndSave();
            }
        }

        /// <summary>
        /// Restores mod settings to default.
        /// </summary>
        public override void SetDefaults()
        {
            UnlockBus = false;
            UnlockTrams = false;
            UnlockTrains = true;
            UnlockShips = true;
            UnlockFarming = true;
            UnlockMining = false;
            UnlockOil = false;
            UnlockBasicHighways = true;
            UnlockAllHighways = false;
            UnlockDistricts = false;
        }
    }
}