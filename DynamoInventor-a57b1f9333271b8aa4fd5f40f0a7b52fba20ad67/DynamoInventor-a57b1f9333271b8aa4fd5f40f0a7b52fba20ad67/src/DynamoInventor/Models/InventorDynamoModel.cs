using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text;

using Dynamo;
using Dynamo.Core;
using Dynamo.Models;
using Dynamo.Interfaces;
using Dynamo.Nodes;
using Dynamo.Utilities;

namespace DynamoInventor.Models
{
    public class InventorDynamoModel : DynamoModel
    {

        public new static InventorDynamoModel Start()
        {
            return InventorDynamoModel.Start(new StartConfiguration());
        }

        public new static InventorDynamoModel Start(StartConfiguration configuration)
        {
            if (string.IsNullOrEmpty(configuration.Context))
                configuration.Context = Dynamo.Core.Context.REVIT_2015;
            if (string.IsNullOrEmpty(configuration.DynamoCorePath))
            {
                var asmLocation = Assembly.GetExecutingAssembly().Location;
                configuration.DynamoCorePath = Path.GetDirectoryName(asmLocation);
            }

            if (configuration.Preferences == null)
                configuration.Preferences = new PreferenceSettings();

            return new InventorDynamoModel(configuration);
        }

        private InventorDynamoModel(StartConfiguration configuration) :
            base(configuration)
        {
            string context = configuration.Context;
            IPreferences preferences = configuration.Preferences;
            string corePath = configuration.DynamoCorePath;
            bool isTestMode = configuration.StartInTestMode;
        }
    }
}
