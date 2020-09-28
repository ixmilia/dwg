using System;
using System.IO;
using Xunit;

namespace IxMilia.Dwg.Test
{
    public class FileHeaderTests : AbstractReaderTests
    {
        [Fact]
        public void ParseSimpleFileHeader()
        {
            var data = new byte[]
            {
                (byte)'A', (byte)'C', (byte)'1', (byte)'0', (byte)'1', (byte)'4',
                0, 0, 0, 0, 0,
                42, // maintver
                1,
                0, 0, 0, 0, // image seeker
                0, 0, // unknown
                0x00, 0x01, // codepage
                0, 0, 0, 0, // no section locators
                0xF8, 0x63, // CRC
                0x95, 0xA0, 0x4E, 0x28, 0x99, 0x82, 0x1A, 0xE5, // sentinel
                0x5E, 0x41, 0xE0, 0x5F, 0x9D, 0x3A, 0x4D, 0x00,
            };
            var reader = new BitReader(data);
            var fileHeader = DwgFileHeader.Parse(reader);
            Assert.Equal(DwgVersionId.R14, fileHeader.Version);
            Assert.Equal(42, fileHeader.MaintenenceVersion);
            Assert.Equal(256, fileHeader.CodePage);
        }

        [Fact]
        public void WriteSimpleFileHeader()
        {
            var expected = new byte[]
            {
                (byte)'A', (byte)'C', (byte)'1', (byte)'0', (byte)'1', (byte)'4',
                0, 0, 0, 0, 0,
                42, // maintver
                1,
                0x78, 0x56, 0x34, 0x12, // image seeker
                0, 0, // unknown
                0x00, 0x01, // codepage
                5, 0, 0, 0, // 5 section locators
                0, 0, 0, 0, 0, 0, 0, 0, 0, // header locator
                0, 0, 0, 0, 0, 0, 0, 0, 0, // class section locator
                0, 0, 0, 0, 0, 0, 0, 0, 0, // object map locator
                0, 0, 0, 0, 0, 0, 0, 0, 0, // unknown R13C3 and later locator
                0, 0, 0, 0, 0, 0, 0, 0, 0, // unknown padding locator
                0x63, 0xF1, // CRC
                0x95, 0xA0, 0x4E, 0x28, 0x99, 0x82, 0x1A, 0xE5, // sentinel
                0x5E, 0x41, 0xE0, 0x5F, 0x9D, 0x3A, 0x4D, 0x00,
            };

            var fileHeader = new DwgFileHeader(DwgVersionId.R14, 42, 0x12345678, 256);
            using (var ms = new MemoryStream())
            {
                var writer = new BitWriter(ms);
                fileHeader.Write(writer);
                var actual = writer.AsBytes();
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void ReadFullRawHeader_R14()
        {
            var reader = Bits(
                0x41, 0x43, 0x31, 0x30, 0x31, 0x34, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x01, 0x3F, 0x0C, 0x00,
                0x00, 0x00, 0x00, 0x1E, 0x00, 0x05, 0x00, 0x00,
                0x00, 0x00, 0x58, 0x00, 0x00, 0x00, 0xED, 0x01,
                0x00, 0x00, 0x01, 0x45, 0x02, 0x00, 0x00, 0x26,
                0x00, 0x00, 0x00, 0x02, 0x27, 0x0B, 0x00, 0x00,
                0x50, 0x00, 0x00, 0x00, 0x03, 0x77, 0x0B, 0x00,
                0x00, 0x35, 0x00, 0x00, 0x00, 0x04, 0x3B, 0x0C,
                0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x2D, 0x5C,
                0x95, 0xA0, 0x4E, 0x28, 0x99, 0x82, 0x1A, 0xE5,
                0x5E, 0x41, 0xE0, 0x5F, 0x9D, 0x3A, 0x4D, 0x00,
                0xCF, 0x7B, 0x1F, 0x23, 0xFD, 0xDE, 0x38, 0xA9,
                0x5F, 0x7C, 0x68, 0xB8, 0x4E, 0x6D, 0x33, 0x5F,
                0xC7, 0x01, 0x00, 0x00, 0x00, 0x00, 0x07, 0x00,
                0x1F, 0xBF, 0x55, 0xD0, 0x95, 0x40, 0x5B, 0x6A,
                0x51, 0xA9, 0x43, 0x1A, 0x65, 0xAC, 0x40, 0x50,
                0x23, 0x30, 0x2D, 0x02, 0x41, 0x2A, 0x40, 0x50,
                0x19, 0x01, 0xAA, 0x90, 0x84, 0x19, 0x06, 0x41,
                0x90, 0x64, 0x19, 0x06, 0x40, 0xD4, 0x69, 0x30,
                0x41, 0x24, 0xC9, 0x26, 0xA6, 0x66, 0x66, 0x66,
                0x66, 0x72, 0x4F, 0xC9, 0xA9, 0x99, 0x99, 0x99,
                0x99, 0x9A, 0x93, 0xF2, 0x6A, 0x66, 0x66, 0x66,
                0x66, 0x66, 0xE4, 0xFC, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0xE0, 0x3F, 0xAA, 0xAA, 0x80, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x0E, 0x03, 0xF0, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x03, 0x80, 0xFD, 0x80,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x0E, 0x03, 0xF5,
                0x40, 0x4B, 0x8B, 0x56, 0x52, 0x50, 0x02, 0xD1,
                0xA6, 0x00, 0x08, 0xB5, 0x65, 0x25, 0x00, 0x20,
                0x29, 0xE0, 0x00, 0xA3, 0x30, 0xF4, 0x00, 0x02,
                0x33, 0x0F, 0x40, 0x00, 0x30, 0x14, 0xD5, 0x10,
                0xF5, 0x11, 0x05, 0x11, 0x45, 0x11, 0xD5, 0x11,
                0xCA, 0x84, 0x08, 0xCB, 0x57, 0x81, 0xDA, 0xF1,
                0x54, 0x41, 0x02, 0x32, 0xD5, 0xE0, 0x76, 0xBC,
                0x55, 0x10, 0x40, 0x8C, 0xB5, 0x78, 0x1D, 0xAF,
                0x15, 0x44, 0x10, 0x23, 0x2D, 0x5E, 0x07, 0x6B,
                0xC5, 0x71, 0x04, 0x08, 0xCB, 0x57, 0x81, 0xDA,
                0xF1, 0x5C, 0x41, 0x02, 0x32, 0xD5, 0xE0, 0x76,
                0xBC, 0x57, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0xA1, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x89, 0x02, 0xA9, 0xA9, 0x94, 0x2A, 0x10,
                0x23, 0x2D, 0x5E, 0x07, 0x6B, 0xC5, 0x51, 0x04,
                0x08, 0xCB, 0x57, 0x81, 0xDA, 0xF1, 0x54, 0x41,
                0x02, 0x32, 0xD5, 0xE0, 0x76, 0xBC, 0x55, 0x10,
                0x40, 0x8C, 0xB5, 0x78, 0x1D, 0xAF, 0x15, 0xC4,
                0x10, 0x23, 0x2D, 0x5E, 0x07, 0x6B, 0xC5, 0x71,
                0x04, 0x08, 0xCB, 0x57, 0x81, 0xDA, 0xF1, 0x5C,
                0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x84,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x24,
                0x0A, 0xA6, 0xA6, 0x50, 0x30, 0x00, 0x40, 0x00,
                0x08, 0x00, 0x18, 0x00, 0x00, 0x00, 0x01, 0x02,
                0x90, 0x44, 0x11, 0x02, 0x40, 0x94, 0x44, 0x10,
                0x2B, 0x5E, 0x8D, 0xC0, 0xF4, 0x2B, 0x1C, 0xFC,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xB0, 0x3F,
                0x14, 0xAE, 0x07, 0xA1, 0x7A, 0xD4, 0x76, 0x0F,
                0xC0, 0xAD, 0x7A, 0x37, 0x03, 0xD0, 0xAC, 0x73,
                0xFA, 0xA0, 0x2B, 0x5E, 0x8D, 0xC0, 0xF4, 0x2B,
                0x1C, 0xFC, 0x0A, 0xD7, 0xA3, 0x70, 0x3D, 0x0A,
                0xB7, 0x3F, 0x86, 0x66, 0x66, 0x66, 0x66, 0x66,
                0x63, 0x94, 0x06, 0x40, 0xAD, 0x7A, 0x37, 0x03,
                0xD0, 0xAB, 0x73, 0xFA, 0xAA, 0xA3, 0x10, 0x13,
                0x10, 0x23, 0x10, 0x33, 0x10, 0x53, 0x10, 0x63,
                0x10, 0x73, 0x10, 0x83, 0x10, 0x93, 0x10, 0xA3,
                0x10, 0xB5, 0x10, 0xD5, 0x10, 0xE3, 0x10, 0xC5,
                0x11, 0x65, 0x11, 0x95, 0x11, 0x45, 0x11, 0x35,
                0x11, 0x51, 0xD5, 0x58, 0xD4, 0xA0, 0x34, 0x26,
                0x4B, 0x76, 0xE0, 0x5B, 0x27, 0x30, 0x84, 0xE0,
                0xDC, 0x02, 0x21, 0xC7, 0x56, 0xA0, 0x83, 0x97,
                0x47, 0xB1, 0x92, 0xCC, 0xA0
            );

            var fileHeader = DwgFileHeader.Parse(reader);
            Assert.Equal(DwgVersionId.R14, fileHeader.Version);
            Assert.Equal(0, fileHeader.MaintenenceVersion);
            Assert.Equal(3135, fileHeader.ImagePointer);
            Assert.Equal(30, fileHeader.CodePage);
            Assert.Equal(88, fileHeader.HeaderVariablesLocator.Pointer);
            Assert.Equal(493, fileHeader.HeaderVariablesLocator.Length);
            Assert.Equal(581, fileHeader.ClassSectionLocator.Pointer);
            Assert.Equal(38, fileHeader.ClassSectionLocator.Length);
            Assert.Equal(2855, fileHeader.ObjectMapLocator.Pointer);
            Assert.Equal(80, fileHeader.ObjectMapLocator.Length);
            Assert.Equal(2935, fileHeader.ObjectFreeSpaceLocator.Pointer);
            Assert.Equal(53, fileHeader.ObjectFreeSpaceLocator.Length);
            Assert.Equal(3131, fileHeader.UnknownSection_PaddingLocator.Pointer);
            Assert.Equal(4, fileHeader.UnknownSection_PaddingLocator.Length);

            var variables = DwgHeaderVariables.Parse(reader.FromOffset(fileHeader.HeaderVariablesLocator.Pointer), fileHeader.Version);
            Assert.Equal(new DwgHandleReference(5, 0), variables.CurrentViewPortEntityHandle);
            Assert.True(variables.CreateAssociativeDimensioning);
            Assert.True(variables.RecomputeDimensionsWhileDragging);
            Assert.False(variables.IsPolylineContinuousAroundVerticies);
            Assert.False(variables.DrawOrthoganalLines);
            Assert.True(variables.UseRegenMode);
            Assert.True(variables.FillModeOn);
            Assert.False(variables.UseQuickTextMode);
            Assert.True(variables.ScaleLineTypesInPaperSpace);
            Assert.False(variables.UseLimitsChecking);
            Assert.False(variables.BlipMode);
            Assert.True(variables.UserTimerOn);
            Assert.Equal(DwgPolySketchMode.SketchLines, variables.PolylineSketchMode);
            Assert.Equal(DwgAngleDirection.CounterClockwise, variables.AngleDirection);
            Assert.False(variables.DisplaySplinePolygonControl);
            Assert.True(variables.PromptForAttributeOnInsert);
            Assert.False(variables.ShowAttributeEntryDialogs);
            Assert.True(variables.MirrorText);
            Assert.True(variables.SetUCSToWCSInDViewOrVPoint);
            Assert.True(variables.PreviousReleaseTileCompatability);
            Assert.False(variables.LimitCheckingInPaperSpace);
            Assert.True(variables.RetainXRefDependentVisibilitySettings);
            Assert.True(variables.RetainDeletedObjects);
            Assert.False(variables.DisplaySilhouetteCurvesInWireframeMode);
            Assert.True(variables.SaveProxyGraphics);
            Assert.Equal(DwgDragMode.Auto, variables.DragMode);
            Assert.Equal(3020, variables.SpacialIndexMaxDepth);
            Assert.Equal(DwgUnitFormat.Decimal, variables.UnitFormat);
            Assert.Equal(4, variables.UnitPrecision);
            Assert.Equal(DwgAngleFormat.DecimalDegrees, variables.AngleUnitFormat);
            Assert.Equal(0, variables.AngleUnitPrecision);
            Assert.Equal(0, variables.ObjectSnapFlags);
            Assert.Equal(DwgAttributeVisibility.Normal, variables.AttributeVisibility);
            Assert.Equal(DwgCoordinateDisplay.ContinuousUpdate, variables.CoordinateDisplay);
            Assert.Equal(0, variables.PointDisplayMode);
            Assert.Equal(DwgPickStyle.Group, variables.PickStyle);
            Assert.Equal(0, variables.UserInt1);
            Assert.Equal(0, variables.UserInt2);
            Assert.Equal(0, variables.UserInt3);
            Assert.Equal(0, variables.UserInt4);
            Assert.Equal(0, variables.UserInt5);
            Assert.Equal(8, variables.LineSegmentsPerSplinePatch);
            Assert.Equal(6, variables.PEditSmoothMDensity);
            Assert.Equal(6, variables.PEditSmoothNDensity);
            Assert.Equal(DwgPolylineCurvedAndSmoothSurfaceType.CubicBSpline, variables.PEditSmoothSurfaceType);
            Assert.Equal(6, variables.MeshTabulationsInFirstDirection);
            Assert.Equal(6, variables.MeshTabulationsInSecondDirection);
            Assert.Equal(DwgPolylineCurvedAndSmoothSurfaceType.CubicBSpline, variables.PEditSplineCurveType);
            Assert.Equal(DwgShadeEdgeMode.FacesInEntityColorEdgesInBlack, variables.EdgeShading);
            Assert.Equal(70, variables.PercentAmbientToDiffuse);
            Assert.False(variables.DisplayFractionsInInput);
            Assert.Equal(48, variables.MaximumActiveViewports);
            Assert.Equal(DwgJustification.Top, variables.CurrentMultilineJustification);
            Assert.Equal(1.0, variables.LineTypeScale);
            Assert.Equal(0.2, variables.DefaultTextHeight);
            Assert.Equal(0.05, variables.TraceWidth);
            Assert.Equal(0.1, variables.SketchRecordIncrement);
            Assert.Equal(0.5, variables.FilletRadius);
            Assert.Equal(0.0, variables.Thickness);
            Assert.Equal(0.0, variables.AngleZeroDirection);
            Assert.Equal(0.0, variables.PointDisplaySize);
            Assert.Equal(0.0, variables.DefaultPolylineWidth);
            Assert.Equal(0.0, variables.UserReal1);
            Assert.Equal(0.0, variables.UserReal2);
            Assert.Equal(0.0, variables.UserReal3);
            Assert.Equal(0.0, variables.UserReal4);
            Assert.Equal(0.0, variables.UserReal5);
            Assert.Equal(0.5, variables.FirstChamferDistance);
            Assert.Equal(0.5, variables.SecondChamferDistance);
            Assert.Equal(1.0, variables.ChamferLength);
            Assert.Equal(0.0, variables.ChamferAngle);
            Assert.Equal(1.0, variables.CurrentMultilineScale);
            Assert.Equal(1.0, variables.CurrentEntityLineTypeScale);
            Assert.Equal(".", variables.FileName);
            Assert.Equal(DateTime.Parse("1998-02-24T11:39:30.1000000"), variables.CreationDate);
            Assert.Equal(DateTime.Parse("1998-02-24T11:39:45.9200000"), variables.UpdateDate);
            Assert.Equal(TimeSpan.Parse("00:00:15.8200000"), variables.TimeInDrawing);
            Assert.Equal(TimeSpan.Parse("00:00:15.8200000"), variables.UserElapsedTimer);
            Assert.Equal(DwgColor.ByLayer, variables.CurrentEntityColor);
            Assert.Equal(new DwgHandleReference(0, 0x4D), variables.NextAvailableHandle);
            Assert.Equal(new DwgHandleReference(5, 0x0F), variables.CurrentLayerHandle);
            Assert.Equal(new DwgHandleReference(5, 0x10), variables.TextStyleHandle);
            Assert.Equal(new DwgHandleReference(5, 0x14), variables.CurrentEntityLineTypeHandle);
            Assert.Equal(new DwgHandleReference(5, 0x1D), variables.DimensionStyleHandle);
            Assert.Equal(new DwgHandleReference(5, 0x1C), variables.CurrentMultiLineStyleHandle);
            Assert.Equal(DwgPoint.Origin, variables.PaperSpaceInsertionBase);
            Assert.Equal(new DwgPoint(1E+20, 1E+20, 1E+20), variables.PaperSpaceMinimumDrawingExtents);
            Assert.Equal(new DwgPoint(-1E+20, -1E+20, -1E+20), variables.PaperSpaceMaximumDrawingExtents);
            Assert.Equal(DwgPoint.Origin, variables.PaperSpaceMinimumDrawingLimits);
            Assert.Equal(new DwgPoint(12.0, 9.0, 0.0), variables.PaperSpaceMaximumDrawingLimits);
            Assert.Equal(0.0, variables.PaperSpaceElevation);
            Assert.Equal(DwgPoint.Origin, variables.PaperSpaceUCSOrigin);
            Assert.Equal(DwgVector.XAxis, variables.PaperSpaceUCSXAxis);
            Assert.Equal(DwgVector.YAxis, variables.PaperSpaceUCSYAxis);
            Assert.Equal(new DwgHandleReference(5, 0x00), variables.PaperSpaceCurrentUCSHandle);
            Assert.Equal(DwgPoint.Origin, variables.InsertionBase);
            Assert.Equal(new DwgPoint(1E+20, 1E+20, 1E+20), variables.MinimumDrawingExtents);
            Assert.Equal(new DwgPoint(-1E+20, -1E+20, -1E+20), variables.MaximumDrawingExtents);
            Assert.Equal(DwgPoint.Origin, variables.MinimumDrawingLimits);
            Assert.Equal(new DwgPoint(12.0, 9.0, 0.0), variables.MaximumDrawingLimits);
            Assert.Equal(0.0, variables.Elevation);
            Assert.Equal(DwgPoint.Origin, variables.UCSOrigin);
            Assert.Equal(DwgVector.XAxis, variables.UCSXAxis);
            Assert.Equal(DwgVector.YAxis, variables.UCSYAxis);
            Assert.Equal(new DwgHandleReference(5, 0x00), variables.CurrentUCSHandle);
            Assert.False(variables.GenerateDimensionTolerances);
            Assert.False(variables.GenerateDimensionLimits);
            Assert.True(variables.DimensionTextInsideHorizontal);
            Assert.True(variables.DimensionTextOutsideHorizontal);
            Assert.False(variables.SuppressFirstDimensionExtensionLine);
            Assert.False(variables.SuppressSecondDimensionExtensionLine);
            Assert.False(variables.UseAlternateDimensioning);
            Assert.False(variables.ForceDimensionLineExtensionsOutsideIfTextIs);
            Assert.False(variables.UseSeparateArrowBlocksForDimensions);
            Assert.False(variables.ForceDimensionTextInsideExtensions);
            Assert.False(variables.SuppressOutsideExtensionDimensionLines);
            Assert.Equal(2, variables.AlternateDimensioningDecimalPlaces);
            Assert.Equal(DwgUnitZeroSuppression.SuppressZeroFeetAndZeroInches, variables.DimensionUnitZeroSuppression);
            Assert.Equal(DwgJustification.Middle, variables.DimensionToleranceVerticalJustification);
            Assert.Equal(DwgDimensionTextJustification.AboveLineCenter, variables.DimensionTextJustification);
            Assert.Equal(DwgDimensionFit.MoveEitherForBestFit, variables.DimensionTextAndArrowPlacement);
            Assert.False(variables.DimensionCursorControlsTextPosition);
            Assert.Equal(DwgUnitZeroSuppression.SuppressZeroFeetAndZeroInches, variables.DimensionToleranceZeroSuppression);
            Assert.Equal(DwgUnitZeroSuppression.SuppressZeroFeetAndZeroInches, variables.AlternateDimensioningZeroSupression);
            Assert.Equal(DwgUnitZeroSuppression.SuppressZeroFeetAndZeroInches, variables.AlternateDimensioningToleranceZeroSupression);
            Assert.False(variables.TextAboveDimensionLine);
            Assert.Equal(DwgUnitFormat.Decimal, variables.DimensionUnitFormat);
            Assert.Equal(DwgAngleFormat.DecimalDegrees, variables.DimensioningAngleFormat);
            Assert.Equal(4, variables.DimensionUnitToleranceDecimalPlaces);
            Assert.Equal(4, variables.DimensionToleranceDecimalPlaces);
            Assert.Equal(DwgUnitFormat.Decimal, variables.AlternateDimensioningUnits);
            Assert.Equal(2, variables.AlternateDimensioningToleranceDecimalPlaces);
            Assert.Equal(new DwgHandleReference(5, 0x10), variables.DimensionTextStyleHandle);
            Assert.Equal(1.0, variables.DimensioningScaleFactor);
            Assert.Equal(0.18, variables.DimensioningArrowSize);
            Assert.Equal(0.0625, variables.DimensionExtensionLineOffset);
            Assert.Equal(0.38, variables.DimensionLineIncrement);
            Assert.Equal(0.18, variables.DimensionExtensionLineExtension);
            Assert.Equal(0.0, variables.DimensionDistanceRoundingValue);
            Assert.Equal(0.0, variables.DimensionLineExtension);
            Assert.Equal(0.0, variables.DimensionPlusTolerance);
            Assert.Equal(0.0, variables.DimensionMinusTolerance);
            Assert.Equal(0.18, variables.DimensioningTextHeight);
            Assert.Equal(0.09, variables.CenterMarkSize);
            Assert.Equal(0.0, variables.DimensioningTickSize);
            Assert.Equal(25.4, variables.AlternateDimensioningScaleFactor);
            Assert.Equal(1.0, variables.DimensionLinearMeasurementsScaleFactor);
            Assert.Equal(0.0, variables.DimensionVerticalTextPosition);
            Assert.Equal(1.0, variables.DimensionToleranceDisplayScaleFactor);
            Assert.Equal(0.09, variables.DimensionLineGap);
            Assert.Equal("", variables.DimensioningSuffix);
            Assert.Equal("", variables.AlternateDimensioningSuffix);
            Assert.Equal("", variables.ArrowBlockName);
            Assert.Equal("", variables.FirstArrowBlockName);
            Assert.Equal("", variables.SecondArrowBlockName);
            Assert.Equal(DwgColor.ByBlock, variables.DimensionLineColor);
            Assert.Equal(DwgColor.ByBlock, variables.DimensionExtensionLineColor);
            Assert.Equal(DwgColor.ByBlock, variables.DimensionTextColor);
            Assert.Equal(new DwgHandleReference(3, 0x01), variables.BlockControlObjectHandle);
            Assert.Equal(new DwgHandleReference(3, 0x02), variables.LayerControlObjectHandle);
            Assert.Equal(new DwgHandleReference(3, 0x03), variables.StyleObjectControlHandle);
            Assert.Equal(new DwgHandleReference(3, 0x05), variables.LineTypeObjectControlHandle);
            Assert.Equal(new DwgHandleReference(3, 0x06), variables.ViewControlObjectHandle);
            Assert.Equal(new DwgHandleReference(3, 0x07), variables.UcsControlObjectHandle);
            Assert.Equal(new DwgHandleReference(3, 0x08), variables.ViewPortControlObjectHandle);
            Assert.Equal(new DwgHandleReference(3, 0x09), variables.AppIdControlObjectHandle);
            Assert.Equal(new DwgHandleReference(3, 0x0A), variables.DimStyleControlObjectHandle);
            Assert.Equal(new DwgHandleReference(3, 0x0B), variables.ViewPortEntityHeaderControlObjectHandle);
            Assert.Equal(new DwgHandleReference(5, 0x0D), variables.GroupDictionaryHandle);
            Assert.Equal(new DwgHandleReference(5, 0x0E), variables.MLineStyleDictionaryHandle);
            Assert.Equal(new DwgHandleReference(3, 0x0C), variables.NamedObjectsDictionaryHandle);
            Assert.Equal(new DwgHandleReference(5, 0x16), variables.PaperSpaceBlockRecordHandle);
            Assert.Equal(new DwgHandleReference(5, 0x19), variables.ModelSpaceBlockRecordHandle);
            Assert.Equal(new DwgHandleReference(5, 0x14), variables.ByLayerLineTypeHandle);
            Assert.Equal(new DwgHandleReference(5, 0x13), variables.ByBlockLineTypeHandle);
            Assert.Equal(new DwgHandleReference(5, 0x15), variables.ContinuousLineTypeHandle);
        }
    }
}
