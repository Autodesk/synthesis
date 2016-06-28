using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Inventor;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using DSNodeServices;
using Dynamo.Models;
using Dynamo.Utilities;
using InventorLibrary.GeometryConversion;
using InventorServices.Persistence;

namespace InventorLibrary.API
{
    [IsVisibleInDynamoLibrary(false)]
    public class InvUnitsOfMeasure
    {
        #region Internal properties
        internal Inventor.UnitsOfMeasure InternalUnitsOfMeasure { get; set; }

        internal Object InternalParent
        {
            get { return UnitsOfMeasureInstance.Parent; }
        }

        internal InvObjectTypeEnum InternalType
        {
            get { return UnitsOfMeasureInstance.Type.As<InvObjectTypeEnum>(); }
        }


        internal int InternalAngleDisplayPrecision { get; set; }

        internal UnitsTypeEnum InternalAngleUnits { get; set; }

        internal int InternalLengthDisplayPrecision { get; set; }

        internal UnitsTypeEnum InternalLengthUnits { get; set; }

        internal UnitsTypeEnum InternalMassUnits { get; set; }

        internal UnitsTypeEnum InternalTimeUnits { get; set; }
        #endregion

        //internal UnitsOfMeasure InternalkNoOwnership
        //{
        //    get { return Inventor.UnitsOfMeasure.kNoOwnership; }
        //}
        //internal UnitsOfMeasure InternalkSaveOwnership
        //{
        //    get { return Inventor.UnitsOfMeasure.kSaveOwnership; }
        //}
        //internal UnitsOfMeasure InternalkExclusiveOwnership
        //{
        //    get { return Inventor.UnitsOfMeasure.kExclusiveOwnership; }
        //}
        #region Private constructors
        private InvUnitsOfMeasure(InvUnitsOfMeasure invUnitsOfMeasure)
        {
            InternalUnitsOfMeasure = invUnitsOfMeasure.InternalUnitsOfMeasure;
        }

        private InvUnitsOfMeasure(Inventor.UnitsOfMeasure invUnitsOfMeasure)
        {
            InternalUnitsOfMeasure = invUnitsOfMeasure;
        }
        #endregion

        #region Private methods
        private bool InternalCompatibleUnits(string expression1, Object unitsSpecifier1, string expression2, Object unitsSpecifier2)
        {
            return UnitsOfMeasureInstance.CompatibleUnits( expression1,  unitsSpecifier1,  expression2,  unitsSpecifier2);
        }

        private double InternalConvertUnits(double value, Object inputUnitsSpecifier, Object outputUnitsSpecifier)
        {
            return UnitsOfMeasureInstance.ConvertUnits( value,  inputUnitsSpecifier,  outputUnitsSpecifier);
        }

        private string InternalGetDatabaseUnitsFromExpression(string expression, Object unitsSpecifier)
        {
            return UnitsOfMeasureInstance.GetDatabaseUnitsFromExpression( expression,  unitsSpecifier);
        }

        private ParametersEnumerator InternalGetDrivingParameters(string expression)
        {
            return UnitsOfMeasureInstance.GetDrivingParameters( expression);
        }

        private string InternalGetLocaleCorrectedExpression(string expression, Object unitsSpecifier)
        {
            return UnitsOfMeasureInstance.GetLocaleCorrectedExpression( expression,  unitsSpecifier);
        }

        private string InternalGetPreciseStringFromValue(double value, Object unitsSpecifier)
        {
            return UnitsOfMeasureInstance.GetPreciseStringFromValue( value,  unitsSpecifier);
        }

        private string InternalGetStringFromType(UnitsTypeEnum unitsType)
        {
            return UnitsOfMeasureInstance.GetStringFromType( unitsType);
        }

        private string InternalGetStringFromValue(double value, Object unitsSpecifier)
        {
            return UnitsOfMeasureInstance.GetStringFromValue( value,  unitsSpecifier);
        }

        private UnitsTypeEnum InternalGetTypeFromString(string unitsString)
        {
            return UnitsOfMeasureInstance.GetTypeFromString( unitsString);
        }

        private Object InternalGetValueFromExpression(string expression, Object unitsSpecifier)
        {
            return UnitsOfMeasureInstance.GetValueFromExpression( expression,  unitsSpecifier);
        }

        private bool InternalIsExpressionValid(string expression, Object unitsSpecifier)
        {
            return UnitsOfMeasureInstance.IsExpressionValid( expression,  unitsSpecifier);
        }

        #endregion

        #region Public properties
        public Inventor.UnitsOfMeasure UnitsOfMeasureInstance
        {
            get { return InternalUnitsOfMeasure; }
            set { InternalUnitsOfMeasure = value; }
        }

        public Object Parent
        {
            get { return InternalParent; }
        }

        public InvObjectTypeEnum Type
        {
            get { return InternalType; }
        }

        public int AngleDisplayPrecision
        {
            get { return InternalAngleDisplayPrecision; }
            set { InternalAngleDisplayPrecision = value; }
        }

        //public InvUnitsTypeEnum AngleUnits
        //{
        //    get { return InternalAngleUnits; }
        //    set { InternalAngleUnits = value; }
        //}

        public int LengthDisplayPrecision
        {
            get { return InternalLengthDisplayPrecision; }
            set { InternalLengthDisplayPrecision = value; }
        }

        //public InvUnitsTypeEnum LengthUnits
        //{
        //    get { return InternalLengthUnits; }
        //    set { InternalLengthUnits = value; }
        //}

        //public InvUnitsTypeEnum MassUnits
        //{
        //    get { return InternalMassUnits; }
        //    set { InternalMassUnits = value; }
        //}

        //public InvUnitsTypeEnum TimeUnits
        //{
        //    get { return InternalTimeUnits; }
        //    set { InternalTimeUnits = value; }
        //}

        #endregion
        //public UnitsOfMeasure kNoOwnership
        //{
        //    get { return InternalkNoOwnership; }
        //}
        //public UnitsOfMeasure kSaveOwnership
        //{
        //    get { return InternalkSaveOwnership; }
        //}
        //public UnitsOfMeasure kExclusiveOwnership
        //{
        //    get { return InternalkExclusiveOwnership; }
        //}
        #region Public static constructors
        public static InvUnitsOfMeasure ByInvUnitsOfMeasure(InvUnitsOfMeasure invUnitsOfMeasure)
        {
            return new InvUnitsOfMeasure(invUnitsOfMeasure);
        }
        public static InvUnitsOfMeasure ByInvUnitsOfMeasure(Inventor.UnitsOfMeasure invUnitsOfMeasure)
        {
            return new InvUnitsOfMeasure(invUnitsOfMeasure);
        }
        #endregion

        #region Public methods
        public bool CompatibleUnits(string expression1, Object unitsSpecifier1, string expression2, Object unitsSpecifier2)
        {
            return InternalCompatibleUnits( expression1,  unitsSpecifier1,  expression2,  unitsSpecifier2);
        }

        public double ConvertUnits(double value, Object inputUnitsSpecifier, Object outputUnitsSpecifier)
        {
            return InternalConvertUnits( value,  inputUnitsSpecifier,  outputUnitsSpecifier);
        }

        public string GetDatabaseUnitsFromExpression(string expression, Object unitsSpecifier)
        {
            return InternalGetDatabaseUnitsFromExpression( expression,  unitsSpecifier);
        }

        public ParametersEnumerator GetDrivingParameters(string expression)
        {
            return InternalGetDrivingParameters( expression);
        }

        public string GetLocaleCorrectedExpression(string expression, Object unitsSpecifier)
        {
            return InternalGetLocaleCorrectedExpression( expression,  unitsSpecifier);
        }

        public string GetPreciseStringFromValue(double value, Object unitsSpecifier)
        {
            return InternalGetPreciseStringFromValue( value,  unitsSpecifier);
        }

        public string GetStringFromType(UnitsTypeEnum unitsType)
        {
            return InternalGetStringFromType( unitsType);
        }

        public string GetStringFromValue(double value, Object unitsSpecifier)
        {
            return InternalGetStringFromValue( value,  unitsSpecifier);
        }

        public UnitsTypeEnum GetTypeFromString(string unitsString)
        {
            return InternalGetTypeFromString( unitsString);
        }

        public Object GetValueFromExpression(string expression, Object unitsSpecifier)
        {
            return InternalGetValueFromExpression( expression,  unitsSpecifier);
        }

        public bool IsExpressionValid(string expression, Object unitsSpecifier)
        {
            return InternalIsExpressionValid( expression,  unitsSpecifier);
        }

        #endregion
    }
}
