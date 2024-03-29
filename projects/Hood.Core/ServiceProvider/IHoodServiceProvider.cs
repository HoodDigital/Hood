﻿using System;
using System.Collections.Generic;
using System.Linq;
using Hood.Enums;
using Hood.Extensions;
using Hood.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hood.Core
{
    /// <summary>
    /// Classes implementing this interface can serve as a portal for the various services composing the Nop engine. 
    /// Edit functionality, modules and implementations access most Nop functionality through this interface.
    /// </summary>
    public interface IHoodServiceProvider
    {
        List<StartupException> StartupExceptions { get; set; }

        bool Installed { get; }

        List<StartupException> GetStartupExceptionsByType(StartupError type);

        /// <summary>
        /// Initialize engine
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        void Initialize(IServiceCollection services);

        /// <summary>
        /// Add and configure services
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="configuration">Configuration root of the application</param>
        /// <returns>Service provider</returns>
        IServiceProvider ConfigureServices(IServiceCollection services, IConfiguration configuration);

        /// <summary>
        /// Resolve dependency
        /// </summary>
        /// <typeparam name="T">Type of resolved service</typeparam>
        /// <returns>Resolved service</returns>
        T Resolve<T>() where T : class;

        /// <summary>
        /// Resolve dependency
        /// </summary>
        /// <param name="type">Type of resolved service</param>
        /// <returns>Resolved service</returns>
        object Resolve(Type type);

        /// <summary>
        /// Resolve dependencies
        /// </summary>
        /// <typeparam name="T">Type of resolved services</typeparam>
        /// <returns>Collection of resolved services</returns>
        IEnumerable<T> ResolveAll<T>();

        /// <summary>
        /// Resolve unregistered service
        /// </summary>
        /// <param name="type">Type of service</param>
        /// <returns>Resolved service</returns>
        object ResolveUnregistered(Type type);
    }
}
