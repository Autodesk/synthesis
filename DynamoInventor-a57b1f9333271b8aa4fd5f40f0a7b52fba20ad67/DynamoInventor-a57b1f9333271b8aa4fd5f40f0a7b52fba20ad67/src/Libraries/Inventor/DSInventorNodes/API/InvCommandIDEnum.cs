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
    public class InvCommandIDEnum
    {
        #region Internal properties
        internal Inventor.CommandIDEnum InternalCommandIDEnum { get; set; }

        #endregion

        internal CommandIDEnum InternalkDefaultCommand
        {
            get { return Inventor.CommandIDEnum.kDefaultCommand; }
        }
        internal CommandIDEnum InternalkCreateSketchCommand
        {
            get { return Inventor.CommandIDEnum.kCreateSketchCommand; }
        }
        internal CommandIDEnum InternalkCreateExtrudeCommand
        {
            get { return Inventor.CommandIDEnum.kCreateExtrudeCommand; }
        }
        internal CommandIDEnum InternalkCreateRevolveCommand
        {
            get { return Inventor.CommandIDEnum.kCreateRevolveCommand; }
        }
        internal CommandIDEnum InternalkCreateSweepCommand
        {
            get { return Inventor.CommandIDEnum.kCreateSweepCommand; }
        }
        internal CommandIDEnum InternalkCreateLoftCommand
        {
            get { return Inventor.CommandIDEnum.kCreateLoftCommand; }
        }
        internal CommandIDEnum InternalkPlaceComponentCommand
        {
            get { return Inventor.CommandIDEnum.kPlaceComponentCommand; }
        }
        internal CommandIDEnum InternalkRebuildPartCommand
        {
            get { return Inventor.CommandIDEnum.kRebuildPartCommand; }
        }
        internal CommandIDEnum InternalkFileSaveCopyAsCommand
        {
            get { return Inventor.CommandIDEnum.kFileSaveCopyAsCommand; }
        }
        internal CommandIDEnum InternalkFileOpenCommand
        {
            get { return Inventor.CommandIDEnum.kFileOpenCommand; }
        }
        internal CommandIDEnum InternalkMeasureCommand
        {
            get { return Inventor.CommandIDEnum.kMeasureCommand; }
        }
        internal CommandIDEnum InternalkAddPolygonCommand
        {
            get { return Inventor.CommandIDEnum.kAddPolygonCommand; }
        }
        internal CommandIDEnum InternalkCreateSketchMirrorCommand
        {
            get { return Inventor.CommandIDEnum.kCreateSketchMirrorCommand; }
        }
        internal CommandIDEnum InternalkCreateEquationParamCommand
        {
            get { return Inventor.CommandIDEnum.kCreateEquationParamCommand; }
        }
        internal CommandIDEnum InternalkCreateTextStylesCommand
        {
            get { return Inventor.CommandIDEnum.kCreateTextStylesCommand; }
        }
        internal CommandIDEnum InternalkCreateRibCommand
        {
            get { return Inventor.CommandIDEnum.kCreateRibCommand; }
        }
        internal CommandIDEnum InternalkEditSketchCoordCommand
        {
            get { return Inventor.CommandIDEnum.kEditSketchCoordCommand; }
        }
        internal CommandIDEnum InternalkLineCommand
        {
            get { return Inventor.CommandIDEnum.kLineCommand; }
        }
        internal CommandIDEnum InternalkSplineCommand
        {
            get { return Inventor.CommandIDEnum.kSplineCommand; }
        }
        internal CommandIDEnum InternalkCenterPointCircleCommand
        {
            get { return Inventor.CommandIDEnum.kCenterPointCircleCommand; }
        }
        internal CommandIDEnum InternalkTangentCircleCommand
        {
            get { return Inventor.CommandIDEnum.kTangentCircleCommand; }
        }
        internal CommandIDEnum InternalkEllipseCommand
        {
            get { return Inventor.CommandIDEnum.kEllipseCommand; }
        }
        internal CommandIDEnum InternalkThreePointArcCommand
        {
            get { return Inventor.CommandIDEnum.kThreePointArcCommand; }
        }
        internal CommandIDEnum InternalkTangentArcCommand
        {
            get { return Inventor.CommandIDEnum.kTangentArcCommand; }
        }
        internal CommandIDEnum InternalkCenterPointArcCommand
        {
            get { return Inventor.CommandIDEnum.kCenterPointArcCommand; }
        }
        internal CommandIDEnum InternalkInsertImportCommand
        {
            get { return Inventor.CommandIDEnum.kInsertImportCommand; }
        }
        internal CommandIDEnum InternalkCreatePunchCommand
        {
            get { return Inventor.CommandIDEnum.kCreatePunchCommand; }
        }
        internal CommandIDEnum InternalkCreateBrokenViewCommand
        {
            get { return Inventor.CommandIDEnum.kCreateBrokenViewCommand; }
        }
        internal CommandIDEnum InternalkCreateHoleTableFromViewCommand
        {
            get { return Inventor.CommandIDEnum.kCreateHoleTableFromViewCommand; }
        }
        internal CommandIDEnum InternalkCreateMateConstraintCommand
        {
            get { return Inventor.CommandIDEnum.kCreateMateConstraintCommand; }
        }
        internal CommandIDEnum InternalkCreateBaselineDimensionCommand
        {
            get { return Inventor.CommandIDEnum.kCreateBaselineDimensionCommand; }
        }
        internal CommandIDEnum InternalkAddPointCommand
        {
            get { return Inventor.CommandIDEnum.kAddPointCommand; }
        }
        internal CommandIDEnum InternalkTwoPointRectangleCommand
        {
            get { return Inventor.CommandIDEnum.kTwoPointRectangleCommand; }
        }
        internal CommandIDEnum InternalkThreePointRectangleCommand
        {
            get { return Inventor.CommandIDEnum.kThreePointRectangleCommand; }
        }
        internal CommandIDEnum InternalkSketchFilletCommand
        {
            get { return Inventor.CommandIDEnum.kSketchFilletCommand; }
        }
        internal CommandIDEnum InternalkSketchExtendCommand
        {
            get { return Inventor.CommandIDEnum.kSketchExtendCommand; }
        }
        internal CommandIDEnum InternalkSketchTrimCommand
        {
            get { return Inventor.CommandIDEnum.kSketchTrimCommand; }
        }
        internal CommandIDEnum InternalkSketchOffsetCommand
        {
            get { return Inventor.CommandIDEnum.kSketchOffsetCommand; }
        }
        internal CommandIDEnum InternalkSketchGeneralDimCommand
        {
            get { return Inventor.CommandIDEnum.kSketchGeneralDimCommand; }
        }
        internal CommandIDEnum InternalkConsPerpendicularCommand
        {
            get { return Inventor.CommandIDEnum.kConsPerpendicularCommand; }
        }
        internal CommandIDEnum InternalkConsParallelCommand
        {
            get { return Inventor.CommandIDEnum.kConsParallelCommand; }
        }
        internal CommandIDEnum InternalkDeleteFaceCommand
        {
            get { return Inventor.CommandIDEnum.kDeleteFaceCommand; }
        }
        internal CommandIDEnum InternalkReplaceFaceCommand
        {
            get { return Inventor.CommandIDEnum.kReplaceFaceCommand; }
        }
        internal CommandIDEnum InternalkCreateKnitSurfaceCommand
        {
            get { return Inventor.CommandIDEnum.kCreateKnitSurfaceCommand; }
        }
        internal CommandIDEnum InternalkThickenCommand
        {
            get { return Inventor.CommandIDEnum.kThickenCommand; }
        }
        internal CommandIDEnum InternalkCreateEmbossCommand
        {
            get { return Inventor.CommandIDEnum.kCreateEmbossCommand; }
        }
        internal CommandIDEnum InternalkCreateGroundedWorkPointCommand
        {
            get { return Inventor.CommandIDEnum.kCreateGroundedWorkPointCommand; }
        }
        internal CommandIDEnum InternalkConsTangentCommand
        {
            get { return Inventor.CommandIDEnum.kConsTangentCommand; }
        }
        internal CommandIDEnum InternalkConsCoincidentCommand
        {
            get { return Inventor.CommandIDEnum.kConsCoincidentCommand; }
        }
        internal CommandIDEnum InternalkConsConcentricCommand
        {
            get { return Inventor.CommandIDEnum.kConsConcentricCommand; }
        }
        internal CommandIDEnum InternalkConsCollinearCommand
        {
            get { return Inventor.CommandIDEnum.kConsCollinearCommand; }
        }
        internal CommandIDEnum InternalkConsHorizontalCommand
        {
            get { return Inventor.CommandIDEnum.kConsHorizontalCommand; }
        }
        internal CommandIDEnum InternalkConsVerticalCommand
        {
            get { return Inventor.CommandIDEnum.kConsVerticalCommand; }
        }
        internal CommandIDEnum InternalkConsEqualCommand
        {
            get { return Inventor.CommandIDEnum.kConsEqualCommand; }
        }
        internal CommandIDEnum InternalkConsFixCommand
        {
            get { return Inventor.CommandIDEnum.kConsFixCommand; }
        }
        internal CommandIDEnum InternalkShowConstraintsCommand
        {
            get { return Inventor.CommandIDEnum.kShowConstraintsCommand; }
        }
        internal CommandIDEnum InternalkProjectGeometryCommand
        {
            get { return Inventor.CommandIDEnum.kProjectGeometryCommand; }
        }
        internal CommandIDEnum InternalkCreateTextCommand
        {
            get { return Inventor.CommandIDEnum.kCreateTextCommand; }
        }
        internal CommandIDEnum InternalkCreateDecalCommand
        {
            get { return Inventor.CommandIDEnum.kCreateDecalCommand; }
        }
        internal CommandIDEnum InternalkCreateAreaFillCommand
        {
            get { return Inventor.CommandIDEnum.kCreateAreaFillCommand; }
        }
        internal CommandIDEnum InternalkInsertImageCommand
        {
            get { return Inventor.CommandIDEnum.kInsertImageCommand; }
        }
        internal CommandIDEnum InternalkProjectViewEdgesCommand
        {
            get { return Inventor.CommandIDEnum.kProjectViewEdgesCommand; }
        }
        internal CommandIDEnum InternalkCreateCaterpillarCommand
        {
            get { return Inventor.CommandIDEnum.kCreateCaterpillarCommand; }
        }
        internal CommandIDEnum InternalkProjectCutEdgesCommand
        {
            get { return Inventor.CommandIDEnum.kProjectCutEdgesCommand; }
        }
        internal CommandIDEnum InternalkCreateHoleCommand
        {
            get { return Inventor.CommandIDEnum.kCreateHoleCommand; }
        }
        internal CommandIDEnum InternalkCreateShellCommand
        {
            get { return Inventor.CommandIDEnum.kCreateShellCommand; }
        }
        internal CommandIDEnum InternalkCreateCoilCommand
        {
            get { return Inventor.CommandIDEnum.kCreateCoilCommand; }
        }
        internal CommandIDEnum InternalkCreateFilletCommand
        {
            get { return Inventor.CommandIDEnum.kCreateFilletCommand; }
        }
        internal CommandIDEnum InternalkCreateChamferCommand
        {
            get { return Inventor.CommandIDEnum.kCreateChamferCommand; }
        }
        internal CommandIDEnum InternalkCreateFaceDraftCommand
        {
            get { return Inventor.CommandIDEnum.kCreateFaceDraftCommand; }
        }
        internal CommandIDEnum InternalkCreateSplitCommand
        {
            get { return Inventor.CommandIDEnum.kCreateSplitCommand; }
        }
        internal CommandIDEnum InternalkViewCatalogCommand
        {
            get { return Inventor.CommandIDEnum.kViewCatalogCommand; }
        }
        internal CommandIDEnum InternalkCreateDesignElementCommand
        {
            get { return Inventor.CommandIDEnum.kCreateDesignElementCommand; }
        }
        internal CommandIDEnum InternalkCreateBreakoutViewCommand
        {
            get { return Inventor.CommandIDEnum.kCreateBreakoutViewCommand; }
        }
        internal CommandIDEnum InternalkCreateEndTreatmentCommand
        {
            get { return Inventor.CommandIDEnum.kCreateEndTreatmentCommand; }
        }
        internal CommandIDEnum InternalkEditViewCommand
        {
            get { return Inventor.CommandIDEnum.kEditViewCommand; }
        }
        internal CommandIDEnum InternalkCreateRevisionTableCommand
        {
            get { return Inventor.CommandIDEnum.kCreateRevisionTableCommand; }
        }
        internal CommandIDEnum InternalkCreateRevisionTagCommand
        {
            get { return Inventor.CommandIDEnum.kCreateRevisionTagCommand; }
        }
        internal CommandIDEnum InternalkEditFixedWorkPointCommand
        {
            get { return Inventor.CommandIDEnum.kEditFixedWorkPointCommand; }
        }
        internal CommandIDEnum InternalkInsertDesignElementCommand
        {
            get { return Inventor.CommandIDEnum.kInsertDesignElementCommand; }
        }
        internal CommandIDEnum InternalkCreateDerivedPartCommand
        {
            get { return Inventor.CommandIDEnum.kCreateDerivedPartCommand; }
        }
        internal CommandIDEnum InternalkCreateRectPatternCommand
        {
            get { return Inventor.CommandIDEnum.kCreateRectPatternCommand; }
        }
        internal CommandIDEnum InternalkCreateCircularPatternCommand
        {
            get { return Inventor.CommandIDEnum.kCreateCircularPatternCommand; }
        }
        internal CommandIDEnum InternalkCreateMirrorCommand
        {
            get { return Inventor.CommandIDEnum.kCreateMirrorCommand; }
        }
        internal CommandIDEnum InternalkCreateWorkPlaneCommand
        {
            get { return Inventor.CommandIDEnum.kCreateWorkPlaneCommand; }
        }
        internal CommandIDEnum InternalkCreateWorkAxisCommand
        {
            get { return Inventor.CommandIDEnum.kCreateWorkAxisCommand; }
        }
        internal CommandIDEnum InternalkCreateWorkPointCommand
        {
            get { return Inventor.CommandIDEnum.kCreateWorkPointCommand; }
        }
        internal CommandIDEnum InternalkMoveFaceCommand
        {
            get { return Inventor.CommandIDEnum.kMoveFaceCommand; }
        }
        internal CommandIDEnum InternalkExtendBodyCommand
        {
            get { return Inventor.CommandIDEnum.kExtendBodyCommand; }
        }
        internal CommandIDEnum InternalkRefreshViewCommand
        {
            get { return Inventor.CommandIDEnum.kRefreshViewCommand; }
        }
        internal CommandIDEnum InternalkPushBackLocalBOMItemNumbersCommand
        {
            get { return Inventor.CommandIDEnum.kPushBackLocalBOMItemNumbersCommand; }
        }
        internal CommandIDEnum InternalkSheetMetalSettingsCommand
        {
            get { return Inventor.CommandIDEnum.kSheetMetalSettingsCommand; }
        }
        internal CommandIDEnum InternalkCreateFlatPatternCommand
        {
            get { return Inventor.CommandIDEnum.kCreateFlatPatternCommand; }
        }
        internal CommandIDEnum InternalkCreateFaceCommand
        {
            get { return Inventor.CommandIDEnum.kCreateFaceCommand; }
        }
        internal CommandIDEnum InternalkCreateCutCommand
        {
            get { return Inventor.CommandIDEnum.kCreateCutCommand; }
        }
        internal CommandIDEnum InternalkCreateFlangeCommand
        {
            get { return Inventor.CommandIDEnum.kCreateFlangeCommand; }
        }
        internal CommandIDEnum InternalkCreateCornerSeamCommand
        {
            get { return Inventor.CommandIDEnum.kCreateCornerSeamCommand; }
        }
        internal CommandIDEnum InternalkCreateBendCommand
        {
            get { return Inventor.CommandIDEnum.kCreateBendCommand; }
        }
        internal CommandIDEnum InternalkCreateCornerRoundCommand
        {
            get { return Inventor.CommandIDEnum.kCreateCornerRoundCommand; }
        }
        internal CommandIDEnum InternalkCreateCornerChamferCommand
        {
            get { return Inventor.CommandIDEnum.kCreateCornerChamferCommand; }
        }
        internal CommandIDEnum InternalkCreateHemCommand
        {
            get { return Inventor.CommandIDEnum.kCreateHemCommand; }
        }
        internal CommandIDEnum InternalkCreateFoldCommand
        {
            get { return Inventor.CommandIDEnum.kCreateFoldCommand; }
        }
        internal CommandIDEnum InternalkCreateContourFlangeCommand
        {
            get { return Inventor.CommandIDEnum.kCreateContourFlangeCommand; }
        }
        internal CommandIDEnum InternalkCreateAutoDimCommand
        {
            get { return Inventor.CommandIDEnum.kCreateAutoDimCommand; }
        }
        internal CommandIDEnum InternalkCreateSketchChamferCommand
        {
            get { return Inventor.CommandIDEnum.kCreateSketchChamferCommand; }
        }
        internal CommandIDEnum InternalkCreateThreadCommand
        {
            get { return Inventor.CommandIDEnum.kCreateThreadCommand; }
        }
        internal CommandIDEnum InternalkEditThreadCommand
        {
            get { return Inventor.CommandIDEnum.kEditThreadCommand; }
        }
        internal CommandIDEnum InternalkFillHatchSketchRegionCommand
        {
            get { return Inventor.CommandIDEnum.kFillHatchSketchRegionCommand; }
        }
        internal CommandIDEnum InternalkCreate3DSketchCommand
        {
            get { return Inventor.CommandIDEnum.kCreate3DSketchCommand; }
        }
        internal CommandIDEnum InternalkCreateComponentCommand
        {
            get { return Inventor.CommandIDEnum.kCreateComponentCommand; }
        }
        internal CommandIDEnum InternalkPatternComponentCommand
        {
            get { return Inventor.CommandIDEnum.kPatternComponentCommand; }
        }
        internal CommandIDEnum InternalkReplaceAllComponentsCommand
        {
            get { return Inventor.CommandIDEnum.kReplaceAllComponentsCommand; }
        }
        internal CommandIDEnum InternalkPlaceConstraintCommand
        {
            get { return Inventor.CommandIDEnum.kPlaceConstraintCommand; }
        }
        internal CommandIDEnum InternalkReplaceComponentCommand
        {
            get { return Inventor.CommandIDEnum.kReplaceComponentCommand; }
        }
        internal CommandIDEnum InternalkCreateInterfaceCommand
        {
            get { return Inventor.CommandIDEnum.kCreateInterfaceCommand; }
        }
        internal CommandIDEnum InternalkMoveComponentCommand
        {
            get { return Inventor.CommandIDEnum.kMoveComponentCommand; }
        }
        internal CommandIDEnum InternalkRotateComponentCommand
        {
            get { return Inventor.CommandIDEnum.kRotateComponentCommand; }
        }
        internal CommandIDEnum InternalkCreateQuarterSectionViewCommand
        {
            get { return Inventor.CommandIDEnum.kCreateQuarterSectionViewCommand; }
        }
        internal CommandIDEnum InternalkCreateHalfSectionViewCommand
        {
            get { return Inventor.CommandIDEnum.kCreateHalfSectionViewCommand; }
        }
        internal CommandIDEnum InternalkCreate3QuarterSectionViewCommand
        {
            get { return Inventor.CommandIDEnum.kCreate3QuarterSectionViewCommand; }
        }
        internal CommandIDEnum InternalkCreateUnsectionedViewCommand
        {
            get { return Inventor.CommandIDEnum.kCreateUnsectionedViewCommand; }
        }
        internal CommandIDEnum InternalkPopupBrowserFiltersCommand
        {
            get { return Inventor.CommandIDEnum.kPopupBrowserFiltersCommand; }
        }
        internal CommandIDEnum InternalkDesignViewsCommand
        {
            get { return Inventor.CommandIDEnum.kDesignViewsCommand; }
        }
        internal CommandIDEnum InternalkCreatePresentationViewCommand
        {
            get { return Inventor.CommandIDEnum.kCreatePresentationViewCommand; }
        }
        internal CommandIDEnum InternalkTweakComponentsCommand
        {
            get { return Inventor.CommandIDEnum.kTweakComponentsCommand; }
        }
        internal CommandIDEnum InternalkPreciseViewRotationCommand
        {
            get { return Inventor.CommandIDEnum.kPreciseViewRotationCommand; }
        }
        internal CommandIDEnum InternalkAnimateCommand
        {
            get { return Inventor.CommandIDEnum.kAnimateCommand; }
        }
        internal CommandIDEnum InternalkCreateDrawingViewCommand
        {
            get { return Inventor.CommandIDEnum.kCreateDrawingViewCommand; }
        }
        internal CommandIDEnum InternalkCreateProjectedViewCommand
        {
            get { return Inventor.CommandIDEnum.kCreateProjectedViewCommand; }
        }
        internal CommandIDEnum InternalkCreateAuxiliaryViewCommand
        {
            get { return Inventor.CommandIDEnum.kCreateAuxiliaryViewCommand; }
        }
        internal CommandIDEnum InternalkCreateSectionViewCommand
        {
            get { return Inventor.CommandIDEnum.kCreateSectionViewCommand; }
        }
        internal CommandIDEnum InternalkCreateDetailViewCommand
        {
            get { return Inventor.CommandIDEnum.kCreateDetailViewCommand; }
        }
        internal CommandIDEnum InternalkCreateHoleTableGroupCommand
        {
            get { return Inventor.CommandIDEnum.kCreateHoleTableGroupCommand; }
        }
        internal CommandIDEnum InternalkCreateSelectedHoleTableCommand
        {
            get { return Inventor.CommandIDEnum.kCreateSelectedHoleTableCommand; }
        }
        internal CommandIDEnum InternalkModifyStandardSettingsCommand
        {
            get { return Inventor.CommandIDEnum.kModifyStandardSettingsCommand; }
        }
        internal CommandIDEnum InternalkCreateDraftViewCommand
        {
            get { return Inventor.CommandIDEnum.kCreateDraftViewCommand; }
        }
        internal CommandIDEnum InternalkCreateNewSheetCommand
        {
            get { return Inventor.CommandIDEnum.kCreateNewSheetCommand; }
        }
        internal CommandIDEnum InternalkCreateGeneralDimensionCommand
        {
            get { return Inventor.CommandIDEnum.kCreateGeneralDimensionCommand; }
        }
        internal CommandIDEnum InternalkCreateOrdinateDimensionCommand
        {
            get { return Inventor.CommandIDEnum.kCreateOrdinateDimensionCommand; }
        }
        internal CommandIDEnum InternalkInsertHoleNotesCommand
        {
            get { return Inventor.CommandIDEnum.kInsertHoleNotesCommand; }
        }
        internal CommandIDEnum InternalkCreateCenterMarkCommand
        {
            get { return Inventor.CommandIDEnum.kCreateCenterMarkCommand; }
        }
        internal CommandIDEnum InternalkCreateCenterLineCommand
        {
            get { return Inventor.CommandIDEnum.kCreateCenterLineCommand; }
        }
        internal CommandIDEnum InternalkRotateViewCommand
        {
            get { return Inventor.CommandIDEnum.kRotateViewCommand; }
        }
        internal CommandIDEnum InternalkEditTweaksCommand
        {
            get { return Inventor.CommandIDEnum.kEditTweaksCommand; }
        }
        internal CommandIDEnum InternalkCreateCenterLineBisectorCommand
        {
            get { return Inventor.CommandIDEnum.kCreateCenterLineBisectorCommand; }
        }
        internal CommandIDEnum InternalkCreateCenteredPatternCommand
        {
            get { return Inventor.CommandIDEnum.kCreateCenteredPatternCommand; }
        }
        internal CommandIDEnum InternalkInsertSurfaceTextureCommand
        {
            get { return Inventor.CommandIDEnum.kInsertSurfaceTextureCommand; }
        }
        internal CommandIDEnum InternalkCreateWeldSymbolCommand
        {
            get { return Inventor.CommandIDEnum.kCreateWeldSymbolCommand; }
        }
        internal CommandIDEnum InternalkInsertFeatureControlFrameCommand
        {
            get { return Inventor.CommandIDEnum.kInsertFeatureControlFrameCommand; }
        }
        internal CommandIDEnum InternalkInsertFeatureIdentifierCommand
        {
            get { return Inventor.CommandIDEnum.kInsertFeatureIdentifierCommand; }
        }
        internal CommandIDEnum InternalkInsertDatumIdentifierCommand
        {
            get { return Inventor.CommandIDEnum.kInsertDatumIdentifierCommand; }
        }
        internal CommandIDEnum InternalkInsertDatumTargetLeaderCommand
        {
            get { return Inventor.CommandIDEnum.kInsertDatumTargetLeaderCommand; }
        }
        internal CommandIDEnum InternalkInsertDatumTargetCircleCommand
        {
            get { return Inventor.CommandIDEnum.kInsertDatumTargetCircleCommand; }
        }
        internal CommandIDEnum InternalkInsertDatumTargetLineCommand
        {
            get { return Inventor.CommandIDEnum.kInsertDatumTargetLineCommand; }
        }
        internal CommandIDEnum InternalkInsertDatumTargetPointCommand
        {
            get { return Inventor.CommandIDEnum.kInsertDatumTargetPointCommand; }
        }
        internal CommandIDEnum InternalkInsertDatumTargetRectCommand
        {
            get { return Inventor.CommandIDEnum.kInsertDatumTargetRectCommand; }
        }
        internal CommandIDEnum InternalkInsertTextCommand
        {
            get { return Inventor.CommandIDEnum.kInsertTextCommand; }
        }
        internal CommandIDEnum InternalkInsertLeaderTextCommand
        {
            get { return Inventor.CommandIDEnum.kInsertLeaderTextCommand; }
        }
        internal CommandIDEnum InternalkInsertBalloonCommand
        {
            get { return Inventor.CommandIDEnum.kInsertBalloonCommand; }
        }
        internal CommandIDEnum InternalkInsertBalloonAllCommand
        {
            get { return Inventor.CommandIDEnum.kInsertBalloonAllCommand; }
        }
        internal CommandIDEnum InternalkInsertPartsListCommand
        {
            get { return Inventor.CommandIDEnum.kInsertPartsListCommand; }
        }
        internal CommandIDEnum InternalkAddParametersCommand
        {
            get { return Inventor.CommandIDEnum.kAddParametersCommand; }
        }
        internal CommandIDEnum InternalkZoomCommand
        {
            get { return Inventor.CommandIDEnum.kZoomCommand; }
        }
        internal CommandIDEnum InternalkZoomAllCommand
        {
            get { return Inventor.CommandIDEnum.kZoomAllCommand; }
        }
        internal CommandIDEnum InternalkZoomWindowCommand
        {
            get { return Inventor.CommandIDEnum.kZoomWindowCommand; }
        }
        internal CommandIDEnum InternalkZoomSelectCommand
        {
            get { return Inventor.CommandIDEnum.kZoomSelectCommand; }
        }
        internal CommandIDEnum InternalkPanCommand
        {
            get { return Inventor.CommandIDEnum.kPanCommand; }
        }
        internal CommandIDEnum InternalkRotateCommand
        {
            get { return Inventor.CommandIDEnum.kRotateCommand; }
        }
        internal CommandIDEnum InternalkLookAtViewCommand
        {
            get { return Inventor.CommandIDEnum.kLookAtViewCommand; }
        }
        internal CommandIDEnum InternalkViewWireframeCommand
        {
            get { return Inventor.CommandIDEnum.kViewWireframeCommand; }
        }
        internal CommandIDEnum InternalkViewHiddenEdgeCommand
        {
            get { return Inventor.CommandIDEnum.kViewHiddenEdgeCommand; }
        }
        internal CommandIDEnum InternalkViewShadedCommand
        {
            get { return Inventor.CommandIDEnum.kViewShadedCommand; }
        }
        internal CommandIDEnum InternalkDeselAuthorCommand
        {
            get { return Inventor.CommandIDEnum.kDeselAuthorCommand; }
        }
        internal CommandIDEnum InternalkCreate3DLineCommand
        {
            get { return Inventor.CommandIDEnum.kCreate3DLineCommand; }
        }
        internal CommandIDEnum InternalkCreate3DBendCommand
        {
            get { return Inventor.CommandIDEnum.kCreate3DBendCommand; }
        }
        internal CommandIDEnum InternalkIncludeGeometry3DCommand
        {
            get { return Inventor.CommandIDEnum.kIncludeGeometry3DCommand; }
        }
        internal CommandIDEnum InternalkCreateSingleOrdinateDimCommand
        {
            get { return Inventor.CommandIDEnum.kCreateSingleOrdinateDimCommand; }
        }
        internal CommandIDEnum InternalkFormatDimStylesCommand
        {
            get { return Inventor.CommandIDEnum.kFormatDimStylesCommand; }
        }
        internal CommandIDEnum InternalkConsSymmetricCommand
        {
            get { return Inventor.CommandIDEnum.kConsSymmetricCommand; }
        }
        internal CommandIDEnum InternalkCreateIntersectionCurveCommand
        {
            get { return Inventor.CommandIDEnum.kCreateIntersectionCurveCommand; }
        }
        internal CommandIDEnum InternalkInsertTableCommand
        {
            get { return Inventor.CommandIDEnum.kInsertTableCommand; }
        }
        #region Private constructors
        private InvCommandIDEnum(InvCommandIDEnum invCommandIDEnum)
        {
            InternalCommandIDEnum = invCommandIDEnum.InternalCommandIDEnum;
        }

        private InvCommandIDEnum(Inventor.CommandIDEnum invCommandIDEnum)
        {
            InternalCommandIDEnum = invCommandIDEnum;
        }
        #endregion

        #region Private methods
        #endregion

        #region Public properties
        public Inventor.CommandIDEnum CommandIDEnumInstance
        {
            get { return InternalCommandIDEnum; }
            set { InternalCommandIDEnum = value; }
        }

        #endregion
        public CommandIDEnum kDefaultCommand
        {
            get { return InternalkDefaultCommand; }
        }
        public CommandIDEnum kCreateSketchCommand
        {
            get { return InternalkCreateSketchCommand; }
        }
        public CommandIDEnum kCreateExtrudeCommand
        {
            get { return InternalkCreateExtrudeCommand; }
        }
        public CommandIDEnum kCreateRevolveCommand
        {
            get { return InternalkCreateRevolveCommand; }
        }
        public CommandIDEnum kCreateSweepCommand
        {
            get { return InternalkCreateSweepCommand; }
        }
        public CommandIDEnum kCreateLoftCommand
        {
            get { return InternalkCreateLoftCommand; }
        }
        public CommandIDEnum kPlaceComponentCommand
        {
            get { return InternalkPlaceComponentCommand; }
        }
        public CommandIDEnum kRebuildPartCommand
        {
            get { return InternalkRebuildPartCommand; }
        }
        public CommandIDEnum kFileSaveCopyAsCommand
        {
            get { return InternalkFileSaveCopyAsCommand; }
        }
        public CommandIDEnum kFileOpenCommand
        {
            get { return InternalkFileOpenCommand; }
        }
        public CommandIDEnum kMeasureCommand
        {
            get { return InternalkMeasureCommand; }
        }
        public CommandIDEnum kAddPolygonCommand
        {
            get { return InternalkAddPolygonCommand; }
        }
        public CommandIDEnum kCreateSketchMirrorCommand
        {
            get { return InternalkCreateSketchMirrorCommand; }
        }
        public CommandIDEnum kCreateEquationParamCommand
        {
            get { return InternalkCreateEquationParamCommand; }
        }
        public CommandIDEnum kCreateTextStylesCommand
        {
            get { return InternalkCreateTextStylesCommand; }
        }
        public CommandIDEnum kCreateRibCommand
        {
            get { return InternalkCreateRibCommand; }
        }
        public CommandIDEnum kEditSketchCoordCommand
        {
            get { return InternalkEditSketchCoordCommand; }
        }
        public CommandIDEnum kLineCommand
        {
            get { return InternalkLineCommand; }
        }
        public CommandIDEnum kSplineCommand
        {
            get { return InternalkSplineCommand; }
        }
        public CommandIDEnum kCenterPointCircleCommand
        {
            get { return InternalkCenterPointCircleCommand; }
        }
        public CommandIDEnum kTangentCircleCommand
        {
            get { return InternalkTangentCircleCommand; }
        }
        public CommandIDEnum kEllipseCommand
        {
            get { return InternalkEllipseCommand; }
        }
        public CommandIDEnum kThreePointArcCommand
        {
            get { return InternalkThreePointArcCommand; }
        }
        public CommandIDEnum kTangentArcCommand
        {
            get { return InternalkTangentArcCommand; }
        }
        public CommandIDEnum kCenterPointArcCommand
        {
            get { return InternalkCenterPointArcCommand; }
        }
        public CommandIDEnum kInsertImportCommand
        {
            get { return InternalkInsertImportCommand; }
        }
        public CommandIDEnum kCreatePunchCommand
        {
            get { return InternalkCreatePunchCommand; }
        }
        public CommandIDEnum kCreateBrokenViewCommand
        {
            get { return InternalkCreateBrokenViewCommand; }
        }
        public CommandIDEnum kCreateHoleTableFromViewCommand
        {
            get { return InternalkCreateHoleTableFromViewCommand; }
        }
        public CommandIDEnum kCreateMateConstraintCommand
        {
            get { return InternalkCreateMateConstraintCommand; }
        }
        public CommandIDEnum kCreateBaselineDimensionCommand
        {
            get { return InternalkCreateBaselineDimensionCommand; }
        }
        public CommandIDEnum kAddPointCommand
        {
            get { return InternalkAddPointCommand; }
        }
        public CommandIDEnum kTwoPointRectangleCommand
        {
            get { return InternalkTwoPointRectangleCommand; }
        }
        public CommandIDEnum kThreePointRectangleCommand
        {
            get { return InternalkThreePointRectangleCommand; }
        }
        public CommandIDEnum kSketchFilletCommand
        {
            get { return InternalkSketchFilletCommand; }
        }
        public CommandIDEnum kSketchExtendCommand
        {
            get { return InternalkSketchExtendCommand; }
        }
        public CommandIDEnum kSketchTrimCommand
        {
            get { return InternalkSketchTrimCommand; }
        }
        public CommandIDEnum kSketchOffsetCommand
        {
            get { return InternalkSketchOffsetCommand; }
        }
        public CommandIDEnum kSketchGeneralDimCommand
        {
            get { return InternalkSketchGeneralDimCommand; }
        }
        public CommandIDEnum kConsPerpendicularCommand
        {
            get { return InternalkConsPerpendicularCommand; }
        }
        public CommandIDEnum kConsParallelCommand
        {
            get { return InternalkConsParallelCommand; }
        }
        public CommandIDEnum kDeleteFaceCommand
        {
            get { return InternalkDeleteFaceCommand; }
        }
        public CommandIDEnum kReplaceFaceCommand
        {
            get { return InternalkReplaceFaceCommand; }
        }
        public CommandIDEnum kCreateKnitSurfaceCommand
        {
            get { return InternalkCreateKnitSurfaceCommand; }
        }
        public CommandIDEnum kThickenCommand
        {
            get { return InternalkThickenCommand; }
        }
        public CommandIDEnum kCreateEmbossCommand
        {
            get { return InternalkCreateEmbossCommand; }
        }
        public CommandIDEnum kCreateGroundedWorkPointCommand
        {
            get { return InternalkCreateGroundedWorkPointCommand; }
        }
        public CommandIDEnum kConsTangentCommand
        {
            get { return InternalkConsTangentCommand; }
        }
        public CommandIDEnum kConsCoincidentCommand
        {
            get { return InternalkConsCoincidentCommand; }
        }
        public CommandIDEnum kConsConcentricCommand
        {
            get { return InternalkConsConcentricCommand; }
        }
        public CommandIDEnum kConsCollinearCommand
        {
            get { return InternalkConsCollinearCommand; }
        }
        public CommandIDEnum kConsHorizontalCommand
        {
            get { return InternalkConsHorizontalCommand; }
        }
        public CommandIDEnum kConsVerticalCommand
        {
            get { return InternalkConsVerticalCommand; }
        }
        public CommandIDEnum kConsEqualCommand
        {
            get { return InternalkConsEqualCommand; }
        }
        public CommandIDEnum kConsFixCommand
        {
            get { return InternalkConsFixCommand; }
        }
        public CommandIDEnum kShowConstraintsCommand
        {
            get { return InternalkShowConstraintsCommand; }
        }
        public CommandIDEnum kProjectGeometryCommand
        {
            get { return InternalkProjectGeometryCommand; }
        }
        public CommandIDEnum kCreateTextCommand
        {
            get { return InternalkCreateTextCommand; }
        }
        public CommandIDEnum kCreateDecalCommand
        {
            get { return InternalkCreateDecalCommand; }
        }
        public CommandIDEnum kCreateAreaFillCommand
        {
            get { return InternalkCreateAreaFillCommand; }
        }
        public CommandIDEnum kInsertImageCommand
        {
            get { return InternalkInsertImageCommand; }
        }
        public CommandIDEnum kProjectViewEdgesCommand
        {
            get { return InternalkProjectViewEdgesCommand; }
        }
        public CommandIDEnum kCreateCaterpillarCommand
        {
            get { return InternalkCreateCaterpillarCommand; }
        }
        public CommandIDEnum kProjectCutEdgesCommand
        {
            get { return InternalkProjectCutEdgesCommand; }
        }
        public CommandIDEnum kCreateHoleCommand
        {
            get { return InternalkCreateHoleCommand; }
        }
        public CommandIDEnum kCreateShellCommand
        {
            get { return InternalkCreateShellCommand; }
        }
        public CommandIDEnum kCreateCoilCommand
        {
            get { return InternalkCreateCoilCommand; }
        }
        public CommandIDEnum kCreateFilletCommand
        {
            get { return InternalkCreateFilletCommand; }
        }
        public CommandIDEnum kCreateChamferCommand
        {
            get { return InternalkCreateChamferCommand; }
        }
        public CommandIDEnum kCreateFaceDraftCommand
        {
            get { return InternalkCreateFaceDraftCommand; }
        }
        public CommandIDEnum kCreateSplitCommand
        {
            get { return InternalkCreateSplitCommand; }
        }
        public CommandIDEnum kViewCatalogCommand
        {
            get { return InternalkViewCatalogCommand; }
        }
        public CommandIDEnum kCreateDesignElementCommand
        {
            get { return InternalkCreateDesignElementCommand; }
        }
        public CommandIDEnum kCreateBreakoutViewCommand
        {
            get { return InternalkCreateBreakoutViewCommand; }
        }
        public CommandIDEnum kCreateEndTreatmentCommand
        {
            get { return InternalkCreateEndTreatmentCommand; }
        }
        public CommandIDEnum kEditViewCommand
        {
            get { return InternalkEditViewCommand; }
        }
        public CommandIDEnum kCreateRevisionTableCommand
        {
            get { return InternalkCreateRevisionTableCommand; }
        }
        public CommandIDEnum kCreateRevisionTagCommand
        {
            get { return InternalkCreateRevisionTagCommand; }
        }
        public CommandIDEnum kEditFixedWorkPointCommand
        {
            get { return InternalkEditFixedWorkPointCommand; }
        }
        public CommandIDEnum kInsertDesignElementCommand
        {
            get { return InternalkInsertDesignElementCommand; }
        }
        public CommandIDEnum kCreateDerivedPartCommand
        {
            get { return InternalkCreateDerivedPartCommand; }
        }
        public CommandIDEnum kCreateRectPatternCommand
        {
            get { return InternalkCreateRectPatternCommand; }
        }
        public CommandIDEnum kCreateCircularPatternCommand
        {
            get { return InternalkCreateCircularPatternCommand; }
        }
        public CommandIDEnum kCreateMirrorCommand
        {
            get { return InternalkCreateMirrorCommand; }
        }
        public CommandIDEnum kCreateWorkPlaneCommand
        {
            get { return InternalkCreateWorkPlaneCommand; }
        }
        public CommandIDEnum kCreateWorkAxisCommand
        {
            get { return InternalkCreateWorkAxisCommand; }
        }
        public CommandIDEnum kCreateWorkPointCommand
        {
            get { return InternalkCreateWorkPointCommand; }
        }
        public CommandIDEnum kMoveFaceCommand
        {
            get { return InternalkMoveFaceCommand; }
        }
        public CommandIDEnum kExtendBodyCommand
        {
            get { return InternalkExtendBodyCommand; }
        }
        public CommandIDEnum kRefreshViewCommand
        {
            get { return InternalkRefreshViewCommand; }
        }
        public CommandIDEnum kPushBackLocalBOMItemNumbersCommand
        {
            get { return InternalkPushBackLocalBOMItemNumbersCommand; }
        }
        public CommandIDEnum kSheetMetalSettingsCommand
        {
            get { return InternalkSheetMetalSettingsCommand; }
        }
        public CommandIDEnum kCreateFlatPatternCommand
        {
            get { return InternalkCreateFlatPatternCommand; }
        }
        public CommandIDEnum kCreateFaceCommand
        {
            get { return InternalkCreateFaceCommand; }
        }
        public CommandIDEnum kCreateCutCommand
        {
            get { return InternalkCreateCutCommand; }
        }
        public CommandIDEnum kCreateFlangeCommand
        {
            get { return InternalkCreateFlangeCommand; }
        }
        public CommandIDEnum kCreateCornerSeamCommand
        {
            get { return InternalkCreateCornerSeamCommand; }
        }
        public CommandIDEnum kCreateBendCommand
        {
            get { return InternalkCreateBendCommand; }
        }
        public CommandIDEnum kCreateCornerRoundCommand
        {
            get { return InternalkCreateCornerRoundCommand; }
        }
        public CommandIDEnum kCreateCornerChamferCommand
        {
            get { return InternalkCreateCornerChamferCommand; }
        }
        public CommandIDEnum kCreateHemCommand
        {
            get { return InternalkCreateHemCommand; }
        }
        public CommandIDEnum kCreateFoldCommand
        {
            get { return InternalkCreateFoldCommand; }
        }
        public CommandIDEnum kCreateContourFlangeCommand
        {
            get { return InternalkCreateContourFlangeCommand; }
        }
        public CommandIDEnum kCreateAutoDimCommand
        {
            get { return InternalkCreateAutoDimCommand; }
        }
        public CommandIDEnum kCreateSketchChamferCommand
        {
            get { return InternalkCreateSketchChamferCommand; }
        }
        public CommandIDEnum kCreateThreadCommand
        {
            get { return InternalkCreateThreadCommand; }
        }
        public CommandIDEnum kEditThreadCommand
        {
            get { return InternalkEditThreadCommand; }
        }
        public CommandIDEnum kFillHatchSketchRegionCommand
        {
            get { return InternalkFillHatchSketchRegionCommand; }
        }
        public CommandIDEnum kCreate3DSketchCommand
        {
            get { return InternalkCreate3DSketchCommand; }
        }
        public CommandIDEnum kCreateComponentCommand
        {
            get { return InternalkCreateComponentCommand; }
        }
        public CommandIDEnum kPatternComponentCommand
        {
            get { return InternalkPatternComponentCommand; }
        }
        public CommandIDEnum kReplaceAllComponentsCommand
        {
            get { return InternalkReplaceAllComponentsCommand; }
        }
        public CommandIDEnum kPlaceConstraintCommand
        {
            get { return InternalkPlaceConstraintCommand; }
        }
        public CommandIDEnum kReplaceComponentCommand
        {
            get { return InternalkReplaceComponentCommand; }
        }
        public CommandIDEnum kCreateInterfaceCommand
        {
            get { return InternalkCreateInterfaceCommand; }
        }
        public CommandIDEnum kMoveComponentCommand
        {
            get { return InternalkMoveComponentCommand; }
        }
        public CommandIDEnum kRotateComponentCommand
        {
            get { return InternalkRotateComponentCommand; }
        }
        public CommandIDEnum kCreateQuarterSectionViewCommand
        {
            get { return InternalkCreateQuarterSectionViewCommand; }
        }
        public CommandIDEnum kCreateHalfSectionViewCommand
        {
            get { return InternalkCreateHalfSectionViewCommand; }
        }
        public CommandIDEnum kCreate3QuarterSectionViewCommand
        {
            get { return InternalkCreate3QuarterSectionViewCommand; }
        }
        public CommandIDEnum kCreateUnsectionedViewCommand
        {
            get { return InternalkCreateUnsectionedViewCommand; }
        }
        public CommandIDEnum kPopupBrowserFiltersCommand
        {
            get { return InternalkPopupBrowserFiltersCommand; }
        }
        public CommandIDEnum kDesignViewsCommand
        {
            get { return InternalkDesignViewsCommand; }
        }
        public CommandIDEnum kCreatePresentationViewCommand
        {
            get { return InternalkCreatePresentationViewCommand; }
        }
        public CommandIDEnum kTweakComponentsCommand
        {
            get { return InternalkTweakComponentsCommand; }
        }
        public CommandIDEnum kPreciseViewRotationCommand
        {
            get { return InternalkPreciseViewRotationCommand; }
        }
        public CommandIDEnum kAnimateCommand
        {
            get { return InternalkAnimateCommand; }
        }
        public CommandIDEnum kCreateDrawingViewCommand
        {
            get { return InternalkCreateDrawingViewCommand; }
        }
        public CommandIDEnum kCreateProjectedViewCommand
        {
            get { return InternalkCreateProjectedViewCommand; }
        }
        public CommandIDEnum kCreateAuxiliaryViewCommand
        {
            get { return InternalkCreateAuxiliaryViewCommand; }
        }
        public CommandIDEnum kCreateSectionViewCommand
        {
            get { return InternalkCreateSectionViewCommand; }
        }
        public CommandIDEnum kCreateDetailViewCommand
        {
            get { return InternalkCreateDetailViewCommand; }
        }
        public CommandIDEnum kCreateHoleTableGroupCommand
        {
            get { return InternalkCreateHoleTableGroupCommand; }
        }
        public CommandIDEnum kCreateSelectedHoleTableCommand
        {
            get { return InternalkCreateSelectedHoleTableCommand; }
        }
        public CommandIDEnum kModifyStandardSettingsCommand
        {
            get { return InternalkModifyStandardSettingsCommand; }
        }
        public CommandIDEnum kCreateDraftViewCommand
        {
            get { return InternalkCreateDraftViewCommand; }
        }
        public CommandIDEnum kCreateNewSheetCommand
        {
            get { return InternalkCreateNewSheetCommand; }
        }
        public CommandIDEnum kCreateGeneralDimensionCommand
        {
            get { return InternalkCreateGeneralDimensionCommand; }
        }
        public CommandIDEnum kCreateOrdinateDimensionCommand
        {
            get { return InternalkCreateOrdinateDimensionCommand; }
        }
        public CommandIDEnum kInsertHoleNotesCommand
        {
            get { return InternalkInsertHoleNotesCommand; }
        }
        public CommandIDEnum kCreateCenterMarkCommand
        {
            get { return InternalkCreateCenterMarkCommand; }
        }
        public CommandIDEnum kCreateCenterLineCommand
        {
            get { return InternalkCreateCenterLineCommand; }
        }
        public CommandIDEnum kRotateViewCommand
        {
            get { return InternalkRotateViewCommand; }
        }
        public CommandIDEnum kEditTweaksCommand
        {
            get { return InternalkEditTweaksCommand; }
        }
        public CommandIDEnum kCreateCenterLineBisectorCommand
        {
            get { return InternalkCreateCenterLineBisectorCommand; }
        }
        public CommandIDEnum kCreateCenteredPatternCommand
        {
            get { return InternalkCreateCenteredPatternCommand; }
        }
        public CommandIDEnum kInsertSurfaceTextureCommand
        {
            get { return InternalkInsertSurfaceTextureCommand; }
        }
        public CommandIDEnum kCreateWeldSymbolCommand
        {
            get { return InternalkCreateWeldSymbolCommand; }
        }
        public CommandIDEnum kInsertFeatureControlFrameCommand
        {
            get { return InternalkInsertFeatureControlFrameCommand; }
        }
        public CommandIDEnum kInsertFeatureIdentifierCommand
        {
            get { return InternalkInsertFeatureIdentifierCommand; }
        }
        public CommandIDEnum kInsertDatumIdentifierCommand
        {
            get { return InternalkInsertDatumIdentifierCommand; }
        }
        public CommandIDEnum kInsertDatumTargetLeaderCommand
        {
            get { return InternalkInsertDatumTargetLeaderCommand; }
        }
        public CommandIDEnum kInsertDatumTargetCircleCommand
        {
            get { return InternalkInsertDatumTargetCircleCommand; }
        }
        public CommandIDEnum kInsertDatumTargetLineCommand
        {
            get { return InternalkInsertDatumTargetLineCommand; }
        }
        public CommandIDEnum kInsertDatumTargetPointCommand
        {
            get { return InternalkInsertDatumTargetPointCommand; }
        }
        public CommandIDEnum kInsertDatumTargetRectCommand
        {
            get { return InternalkInsertDatumTargetRectCommand; }
        }
        public CommandIDEnum kInsertTextCommand
        {
            get { return InternalkInsertTextCommand; }
        }
        public CommandIDEnum kInsertLeaderTextCommand
        {
            get { return InternalkInsertLeaderTextCommand; }
        }
        public CommandIDEnum kInsertBalloonCommand
        {
            get { return InternalkInsertBalloonCommand; }
        }
        public CommandIDEnum kInsertBalloonAllCommand
        {
            get { return InternalkInsertBalloonAllCommand; }
        }
        public CommandIDEnum kInsertPartsListCommand
        {
            get { return InternalkInsertPartsListCommand; }
        }
        public CommandIDEnum kAddParametersCommand
        {
            get { return InternalkAddParametersCommand; }
        }
        public CommandIDEnum kZoomCommand
        {
            get { return InternalkZoomCommand; }
        }
        public CommandIDEnum kZoomAllCommand
        {
            get { return InternalkZoomAllCommand; }
        }
        public CommandIDEnum kZoomWindowCommand
        {
            get { return InternalkZoomWindowCommand; }
        }
        public CommandIDEnum kZoomSelectCommand
        {
            get { return InternalkZoomSelectCommand; }
        }
        public CommandIDEnum kPanCommand
        {
            get { return InternalkPanCommand; }
        }
        public CommandIDEnum kRotateCommand
        {
            get { return InternalkRotateCommand; }
        }
        public CommandIDEnum kLookAtViewCommand
        {
            get { return InternalkLookAtViewCommand; }
        }
        public CommandIDEnum kViewWireframeCommand
        {
            get { return InternalkViewWireframeCommand; }
        }
        public CommandIDEnum kViewHiddenEdgeCommand
        {
            get { return InternalkViewHiddenEdgeCommand; }
        }
        public CommandIDEnum kViewShadedCommand
        {
            get { return InternalkViewShadedCommand; }
        }
        public CommandIDEnum kDeselAuthorCommand
        {
            get { return InternalkDeselAuthorCommand; }
        }
        public CommandIDEnum kCreate3DLineCommand
        {
            get { return InternalkCreate3DLineCommand; }
        }
        public CommandIDEnum kCreate3DBendCommand
        {
            get { return InternalkCreate3DBendCommand; }
        }
        public CommandIDEnum kIncludeGeometry3DCommand
        {
            get { return InternalkIncludeGeometry3DCommand; }
        }
        public CommandIDEnum kCreateSingleOrdinateDimCommand
        {
            get { return InternalkCreateSingleOrdinateDimCommand; }
        }
        public CommandIDEnum kFormatDimStylesCommand
        {
            get { return InternalkFormatDimStylesCommand; }
        }
        public CommandIDEnum kConsSymmetricCommand
        {
            get { return InternalkConsSymmetricCommand; }
        }
        public CommandIDEnum kCreateIntersectionCurveCommand
        {
            get { return InternalkCreateIntersectionCurveCommand; }
        }
        public CommandIDEnum kInsertTableCommand
        {
            get { return InternalkInsertTableCommand; }
        }
        #region Public static constructors
        public static InvCommandIDEnum ByInvCommandIDEnum(InvCommandIDEnum invCommandIDEnum)
        {
            return new InvCommandIDEnum(invCommandIDEnum);
        }
        public static InvCommandIDEnum ByInvCommandIDEnum(Inventor.CommandIDEnum invCommandIDEnum)
        {
            return new InvCommandIDEnum(invCommandIDEnum);
        }
        #endregion

        #region Public methods
        #endregion
    }
}
