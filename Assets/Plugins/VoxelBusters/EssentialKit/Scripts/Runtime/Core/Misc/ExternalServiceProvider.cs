﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelBusters.CoreLibrary;

namespace VoxelBusters.EssentialKit
{
    /// <summary>
    /// Provides an interface to connect external services used by the plugin.
    /// </summary>
    public static class ExternalServiceProvider
    {
        #region Static properties

        private     static  IJsonServiceProvider            s_jsonServiceProvider;

        private     static  ISaveServiceProvider            s_saveServiceProvider;

        private     static  ILocalisationServiceProvider    s_localisationServiceProvider;

        #endregion

        #region Static fields

        /// <summary>
        /// Gets or sets the JSON service provider.
        /// </summary>
        /// <value>The JSON service provider.</value>
        public static IJsonServiceProvider JsonServiceProvider
        {
            get
            {
                if (null == s_jsonServiceProvider)
                {
                    s_jsonServiceProvider   = new DefaultJsonServiceProvider();
                }

                return s_jsonServiceProvider;
            }
            set
            {
                // validate property
                Assertions.AssertIfPropertyIsNull(value, "value");

                // set value
                s_jsonServiceProvider       = value;
            }
        }

        /// <summary>
        /// Gets or sets the save service provider.
        /// </summary>
        /// <value>The save service provider.</value>
        public static ISaveServiceProvider SaveServiceProvider
        {
            get
            {
                if (null == s_saveServiceProvider)
                {
                    s_saveServiceProvider   = new DefaultSaveServiceProvider();
                }

                return s_saveServiceProvider;
            }
            set
            {
                // validate property
                Assertions.AssertIfPropertyIsNull(value, "value");

                // set value
                s_saveServiceProvider       = value;
            }
        }

        public static ILocalisationServiceProvider LocalisationServiceProvider
        {
            get
            {
                if (null == s_localisationServiceProvider)
                {
                    s_localisationServiceProvider   = new DefaultLocalisationServiceProvider();
                }

                return s_localisationServiceProvider;
            }
            set
            {
                // validate property
                Assertions.AssertIfPropertyIsNull(value, "value");

                // set value
                s_localisationServiceProvider       = value;
            }
        }

        #endregion
    }
}