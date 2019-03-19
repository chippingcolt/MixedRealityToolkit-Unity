﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Core.Definitions;
using Microsoft.MixedReality.Toolkit.Core.Interfaces;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Core.Services
{
    /// <summary>
    /// The base extension service implements <see cref="Interfaces.IMixedRealityExtensionService"/> and provides default properties for all extension services.
    /// </summary>
    /// <remarks>
    /// Empty, but reserved for future use, in case additional <see cref="Interfaces.IMixedRealityExtensionService"/> properties or methods are assigned.
    /// </remarks>
    public abstract class BaseExtensionService : BaseService, Interfaces.IMixedRealityExtensionService
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="registrar">The <see cref="IMixedRealityServiceRegistrar"/> instance that loaded the service.</param>
        /// <param name="name">The friendly name of the service.</param>
        /// <param name="priority">The registration priority of the service.</param>
        /// <param name="profile">The configuration profile for the service.</param>
        public BaseExtensionService(
            IMixedRealityServiceRegistrar registrar, 
            string name = null, 
            uint priority = DefaultPriority, 
            BaseMixedRealityProfile profile = null) : base()
        {
            Registrar = registrar;
            Name = name;
            Priority = priority;
            ConfigurationProfile = profile;
        }

        /// <summary>
        /// The service registrar instance that registered this service.
        /// </summary>
        protected IMixedRealityServiceRegistrar Registrar { get; set; } = null;

        /// <summary>
        /// Configuration Profile
        /// </summary>
        protected BaseMixedRealityProfile ConfigurationProfile { get; set; } = null;
    }
}
