using System;
using Inventor;
using System.Collections.Generic;
using System.ComponentModel;

using Autodesk.DesignScript.Runtime;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace InventorLibrary.ModulePlacement
{
    [IsVisibleInDynamoLibrary(false)]
	internal class OccurrenceList
	{
        List<ComponentOccurrence> occurrencesList = new List<ComponentOccurrence>();
        AssemblyDocument templateAssemblyDoc;

		public OccurrenceList(AssemblyDocument assDoc)
		{
            templateAssemblyDoc = assDoc;
			ComponentOccurrences topLevelOccurrences = templateAssemblyDoc.ComponentDefinition.Occurrences; 
			EvaluateOccurrences(topLevelOccurrences);
		}

		public void EvaluateOccurrences(ComponentOccurrences componentOccurrences)
		{
            for (int i = 0; i < componentOccurrences.Count; i++)
            {
                if (componentOccurrences[i + 1].DefinitionDocumentType == DocumentTypeEnum.kAssemblyDocumentObject)
                {
                    occurrencesList.Add(componentOccurrences[i + 1]);
                    EvaluateOccurrences((ComponentOccurrences)componentOccurrences[i + 1].SubOccurrences);
				}

                else if (componentOccurrences[i + 1].DefinitionDocumentType != DocumentTypeEnum.kAssemblyDocumentObject)
                {
                    occurrencesList.Add(componentOccurrences[i + 1]);
				}
				
				else
                {
					continue;
				}
			}
		}

		public List<ComponentOccurrence> Items
		{
			get { return occurrencesList; }
		}

		public AssemblyDocument TargetAssembly
		{
			get { return templateAssemblyDoc; }
		}

		public void CloseTargetAssembly()
		{
	        templateAssemblyDoc.Close();	
		}
	}
}
