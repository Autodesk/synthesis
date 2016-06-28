using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Text;

using Inventor;


namespace DynamoInventor
{
    /// <summary>
    /// This class holds static references that the application needs. 
    /// TODO Delete this class
    /// </summary>
    public class InventorSettings
    {
        //This is the name of the storage for Dynamo object bindings.
        private static string dynamoStorageName = "Dynamo";
        public static string DynamoStorageName
        {
            get { return dynamoStorageName; }
        }
    }
}
