using System.Collections.Generic;
using System.IO;
using IxMilia.Dwg.Objects;

namespace IxMilia.Dwg
{
    public class DwgDrawing
    {
        public DwgFileHeader FileHeader { get; private set; }
        public DwgHeaderVariables Variables { get; private set; }
        public IList<DwgClassDefinition> Classes { get; private set; }
        public DwgImageData ImageData { get; private set; }
        public DwgBlockControlObject BlockHeaders { get; private set; }
        public DwgLayerControlObject Layers { get; private set; }
        public DwgStyleControlObject Styles { get; private set; }
        public DwgLineTypeControlObject LineTypes { get; private set; }
        public DwgViewControlObject Views { get; private set; }
        public DwgUCSControlObject UCSs { get; private set; }
        public DwgViewPortControlObject ViewPorts { get; private set; }
        public DwgAppIdControlObject AppIds { get; private set; }
        public DwgDimStyleControlObject DimStyles { get; private set; }
        public DwgViewPortEntityHeaderControlObject ViewPortEntityHeaders { get; private set; }
        public DwgDictionary GroupDictionary { get; private set; }
        public DwgDictionary MLineStyleDictionary { get; private set; }
        public DwgDictionary NamedObjectDictionary { get; private set; }

        public DwgBlockHeader PaperSpaceBlockRecord { get; set; }
        public DwgBlockHeader ModelSpaceBlockRecord { get; set; }
        public DwgLineType ByLayerLineType { get; set; }
        public DwgLineType ByBlockLineType { get; set; }
        public DwgLineType ContinuousLineType { get; set; }
        public DwgViewPort CurrentViewPort { get; set; }
        public DwgLayer CurrentLayer { get; set; }
        public DwgStyle TextStyle { get; set; }
        public DwgLineType CurrentEntityLineType { get; set; }
        public DwgDimStyle DimensionStyle { get; set; }
        public DwgMLineStyle CurrentMultiLineStyle { get; set; }
        public DwgUCS PaperSpaceCurrentUCS { get; set; }
        public DwgUCS CurrentUCS { get; set; }
        public DwgStyle DimensionTextStyle { get; set; }

        public DwgDrawing()
        {
            FileHeader = new DwgFileHeader(DwgVersionId.Default, 0, 0, 30);
            Variables = new DwgHeaderVariables();
            Classes = new List<DwgClassDefinition>();

            var continuous = new DwgLineType("CONTINUOUS") { Description = "Solid line" };
            var defaultLayer = new DwgLayer("0") { LineType = continuous };
            var standardStyle = new DwgStyle("STANDARD");
            var standardMLineStyle = DwgMLineStyle.GetDefaultMLineStyle();

            BlockHeaders = new DwgBlockControlObject()
            {
                DwgBlockHeader.GetPaperSpaceBlockRecord(defaultLayer),
                DwgBlockHeader.GetModelSpaceBlockRecord(defaultLayer)
            };
            Layers = new DwgLayerControlObject
            {
                defaultLayer
            };
            Styles = new DwgStyleControlObject()
            {
                standardStyle
            };
            LineTypes = new DwgLineTypeControlObject()
            {
                new DwgLineType("BYLAYER"),
                new DwgLineType("BYBLOCK"),
                continuous
            };
            Views = new DwgViewControlObject();
            UCSs = new DwgUCSControlObject();
            ViewPorts = new DwgViewPortControlObject()
            {
                DwgViewPort.GetActiveViewPort()
            };
            AppIds = new DwgAppIdControlObject()
            {
                DwgAppId.GetAcadAppId(),
                DwgAppId.GetAcadMLeaderVersionAppId()
            };
            DimStyles = new DwgDimStyleControlObject()
            {
                DwgDimStyle.GetStandardDimStyle(standardStyle)
            };
            ViewPortEntityHeaders = new DwgViewPortEntityHeaderControlObject();
            GroupDictionary = new DwgDictionary();
            MLineStyleDictionary = new DwgDictionary()
            {
                { standardMLineStyle.Name, standardMLineStyle }
            };
            NamedObjectDictionary = new DwgDictionary()
            {
                { "ACAD_GROUP", new DwgDictionary() },
                { "ACAD_MLINESTYLE", new DwgDictionary() },
                { "ACAD_PLOTSETTINGS", new DwgDictionary() },
                { "ACAD_LAYOUT", new DwgDictionary() },
                { "ACAD_SCALELIST", new DwgDictionary() },
                { "ACAD_VISUALSTYLE", new DwgDictionary() },
                { "ACAD_MATERIAL", new DwgDictionary() },
                { "ACAD_TABLESTYLE", new DwgDictionary() },
                { "ACAD_MLEADERSTYLE", new DwgDictionary() },
                { "ACDBHEADERROUNDTRIPXREC", new DwgDictionary() },
                { "ACDBVARIABLEDICTIONARY", new DwgDictionary() },
            };

            PaperSpaceBlockRecord = BlockHeaders["*PAPER_SPACE"];
            ModelSpaceBlockRecord = BlockHeaders["*MODEL_SPACE"];
            ByLayerLineType = LineTypes["BYLAYER"];
            ByBlockLineType = LineTypes["BYBLOCK"];
            ContinuousLineType = LineTypes["CONTINUOUS"];
            CurrentLayer = Layers["0"];
            TextStyle = Styles["STANDARD"];
            CurrentEntityLineType = LineTypes["BYBLOCK"];
            DimensionStyle = DimStyles["STANDARD"];
            CurrentMultiLineStyle = (DwgMLineStyle)MLineStyleDictionary["Standard"];
            DimensionTextStyle = Styles["STANDARD"];
        }

#if HAS_FILESYSTEM_ACCESS
        public static DwgDrawing Load(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open))
            {
                return Load(stream);
            }
        }
#endif

        public static DwgDrawing Load(Stream stream)
        {
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            return Load(buffer);
        }

        public static DwgDrawing Load(byte[] data)
        {
            var reader = new BitReader(data);
            var drawing = new DwgDrawing();
            drawing.FileHeader = DwgFileHeader.Parse(reader);
            drawing.Variables = DwgHeaderVariables.Parse(reader.FromOffset(drawing.FileHeader.HeaderVariablesLocator.Pointer), drawing.FileHeader.Version);
            drawing.Classes = DwgClasses.Parse(reader.FromOffset(drawing.FileHeader.ClassSectionLocator.Pointer), drawing.FileHeader.Version);
            // don't read the R13C3 and later unknown section
            drawing.FileHeader.ValidateSecondHeader(reader, drawing.Variables);
            drawing.ImageData = DwgImageData.Parse(reader.FromOffset(drawing.FileHeader.ImagePointer));

            var objectCache = DwgObjectCache.Parse(reader.FromOffset(drawing.FileHeader.ObjectMapLocator.Pointer), drawing.FileHeader.Version);
            drawing.LoadObjects(reader, objectCache);

            var freeSpaceSection = DwgObjectFreeSpaceSection.Parse(reader.FromOffset(drawing.FileHeader.ObjectFreeSpaceLocator.Pointer));
            var objectOffset = objectCache.GetOffsetFromHandle(drawing.Variables.BlockControlObjectHandle.HandleOrOffset);
            // TODO: if (version > DwgVersionId.R14) TDUUPDATE else TDUPDATE
            freeSpaceSection.Validate((uint)objectCache.ObjectCount, drawing.Variables.UpdateDate, (uint)objectOffset);

            return drawing;
        }

        private void LoadObjects(BitReader reader, DwgObjectCache objectCache)
        {
            AssertHandleType(Variables.BlockControlObjectHandle, DwgHandleReferenceCode.SoftPointer, nameof(Variables.BlockControlObjectHandle));
            AssertHandleType(Variables.LayerControlObjectHandle, DwgHandleReferenceCode.SoftPointer, nameof(Variables.LayerControlObjectHandle));
            AssertHandleType(Variables.StyleObjectControlHandle, DwgHandleReferenceCode.SoftPointer, nameof(Variables.StyleObjectControlHandle));
            AssertHandleType(Variables.LineTypeObjectControlHandle, DwgHandleReferenceCode.SoftPointer, nameof(Variables.LineTypeObjectControlHandle));
            AssertHandleType(Variables.ViewControlObjectHandle, DwgHandleReferenceCode.SoftPointer, nameof(Variables.ViewControlObjectHandle));
            AssertHandleType(Variables.UcsControlObjectHandle, DwgHandleReferenceCode.SoftPointer, nameof(Variables.UcsControlObjectHandle));
            AssertHandleType(Variables.ViewPortControlObjectHandle, DwgHandleReferenceCode.SoftPointer, nameof(Variables.ViewPortControlObjectHandle));
            AssertHandleType(Variables.AppIdControlObjectHandle, DwgHandleReferenceCode.SoftPointer, nameof(Variables.AppIdControlObjectHandle));
            AssertHandleType(Variables.DimStyleControlObjectHandle, DwgHandleReferenceCode.SoftPointer, nameof(Variables.DimStyleControlObjectHandle));
            AssertHandleType(Variables.ViewPortEntityHeaderControlObjectHandle, DwgHandleReferenceCode.SoftPointer, nameof(Variables.ViewPortEntityHeaderControlObjectHandle));
            AssertHandleType(Variables.GroupDictionaryHandle, DwgHandleReferenceCode.SoftOwner, nameof(Variables.GroupDictionaryHandle));
            AssertHandleType(Variables.MLineStyleDictionaryHandle, DwgHandleReferenceCode.SoftOwner, nameof(Variables.MLineStyleDictionaryHandle));
            AssertHandleType(Variables.NamedObjectsDictionaryHandle, DwgHandleReferenceCode.SoftPointer, nameof(Variables.NamedObjectsDictionaryHandle));
            AssertHandleType(Variables.PaperSpaceBlockRecordHandle, DwgHandleReferenceCode.SoftOwner, nameof(Variables.PaperSpaceBlockRecordHandle));
            AssertHandleType(Variables.ModelSpaceBlockRecordHandle, DwgHandleReferenceCode.SoftOwner, nameof(Variables.ModelSpaceBlockRecordHandle));
            AssertHandleType(Variables.ByLayerLineTypeHandle, DwgHandleReferenceCode.SoftOwner, nameof(Variables.ByLayerLineTypeHandle));
            AssertHandleType(Variables.ByBlockLineTypeHandle, DwgHandleReferenceCode.SoftOwner, nameof(Variables.ByBlockLineTypeHandle));
            AssertHandleType(Variables.ContinuousLineTypeHandle, DwgHandleReferenceCode.SoftOwner, nameof(Variables.ContinuousLineTypeHandle));

            BlockHeaders = objectCache.GetObject<DwgBlockControlObject>(reader, Variables.BlockControlObjectHandle.HandleOrOffset);
            Layers = objectCache.GetObject<DwgLayerControlObject>(reader, Variables.LayerControlObjectHandle.HandleOrOffset);
            Styles = objectCache.GetObject<DwgStyleControlObject>(reader, Variables.StyleObjectControlHandle.HandleOrOffset);
            LineTypes = objectCache.GetObject<DwgLineTypeControlObject>(reader, Variables.LineTypeObjectControlHandle.HandleOrOffset);
            Views = objectCache.GetObject<DwgViewControlObject>(reader, Variables.ViewControlObjectHandle.HandleOrOffset);
            UCSs = objectCache.GetObject<DwgUCSControlObject>(reader, Variables.UcsControlObjectHandle.HandleOrOffset);
            ViewPorts = objectCache.GetObject<DwgViewPortControlObject>(reader, Variables.ViewPortControlObjectHandle.HandleOrOffset);
            AppIds = objectCache.GetObject<DwgAppIdControlObject>(reader, Variables.AppIdControlObjectHandle.HandleOrOffset);
            DimStyles = objectCache.GetObject<DwgDimStyleControlObject>(reader, Variables.DimStyleControlObjectHandle.HandleOrOffset);
            ViewPortEntityHeaders = objectCache.GetObject<DwgViewPortEntityHeaderControlObject>(reader, Variables.ViewPortEntityHeaderControlObjectHandle.HandleOrOffset);
            GroupDictionary = objectCache.GetObject<DwgDictionary>(reader, Variables.GroupDictionaryHandle.HandleOrOffset);
            MLineStyleDictionary = objectCache.GetObject<DwgDictionary>(reader, Variables.MLineStyleDictionaryHandle.HandleOrOffset);
            NamedObjectDictionary = objectCache.GetObject<DwgDictionary>(reader, Variables.NamedObjectsDictionaryHandle.HandleOrOffset);
            PaperSpaceBlockRecord = objectCache.GetObject<DwgBlockHeader>(reader, Variables.PaperSpaceBlockRecordHandle.HandleOrOffset);
            ModelSpaceBlockRecord = objectCache.GetObject<DwgBlockHeader>(reader, Variables.ModelSpaceBlockRecordHandle.HandleOrOffset);
            ByLayerLineType = objectCache.GetObject<DwgLineType>(reader, Variables.ByLayerLineTypeHandle.HandleOrOffset);
            ByBlockLineType = objectCache.GetObject<DwgLineType>(reader, Variables.ByBlockLineTypeHandle.HandleOrOffset);
            ContinuousLineType = objectCache.GetObject<DwgLineType>(reader, Variables.ContinuousLineTypeHandle.HandleOrOffset);
            CurrentViewPort = objectCache.GetObjectOrDefault<DwgViewPort>(reader, Variables.CurrentViewPortEntityHandle.HandleOrOffset);
            CurrentLayer = objectCache.GetObject<DwgLayer>(reader, Variables.CurrentLayerHandle.HandleOrOffset);
            TextStyle = objectCache.GetObject<DwgStyle>(reader, Variables.TextStyleHandle.HandleOrOffset);
            CurrentEntityLineType = objectCache.GetObject<DwgLineType>(reader, Variables.CurrentEntityLineTypeHandle.HandleOrOffset);
            DimensionStyle = objectCache.GetObject<DwgDimStyle>(reader, Variables.DimensionStyleHandle.HandleOrOffset);
            CurrentMultiLineStyle = objectCache.GetObject<DwgMLineStyle>(reader, Variables.CurrentMultiLineStyleHandle.HandleOrOffset);
            PaperSpaceCurrentUCS = objectCache.GetObjectOrDefault<DwgUCS>(reader, Variables.PaperSpaceCurrentUCSHandle.HandleOrOffset);
            CurrentUCS = objectCache.GetObjectOrDefault<DwgUCS>(reader, Variables.CurrentUCSHandle.HandleOrOffset);
            DimensionTextStyle = objectCache.GetObject<DwgStyle>(reader, Variables.DimensionTextStyleHandle.HandleOrOffset);
        }

        private static void AssertHandleType(DwgHandleReference handle, DwgHandleReferenceCode expectedHandleCode, string itemName)
        {
            if (handle.Code != expectedHandleCode)
            {
                throw new DwgReadException($"Invalid handle code for {itemName}.");
            }
        }

#if HAS_FILESYSTEM_ACCESS
        public void Save(string path)
        {
            using (var fs = new FileStream(path, FileMode.Create))
            {
                Save(fs);
            }
        }
#endif

        public void Save(Stream stream)
        {
            var objectMap = new DwgObjectMap();
            AssignHandles(objectMap);

            // write the file header; this will be re-written again once the pointers have been calculated
            var writer = new BitWriter(stream);
            var fileHeaderLocation = writer.Position;
            FileHeader.Write(writer);

            var variablesStart = writer.Position;
            Variables.Write(writer, FileHeader.Version);
            FileHeader.HeaderVariablesLocator = DwgFileHeader.DwgSectionLocator.HeaderVariablesLocator(variablesStart - fileHeaderLocation, writer.Position - variablesStart);

            var classesStart = writer.Position;
            DwgClasses.Write(Classes, writer);
            FileHeader.ClassSectionLocator = DwgFileHeader.DwgSectionLocator.ClassSectionLocator(classesStart - fileHeaderLocation, writer.Position - classesStart);

            var paddingStart = writer.Position;
            writer.WriteBytes(new byte[0x200]); // may contain the MEASUREMENT variable as the first 4 bytes, but not required
            FileHeader.UnknownSection_PaddingLocator = DwgFileHeader.DwgSectionLocator.UnknownSection_PaddingLocator(paddingStart - fileHeaderLocation, writer.Position - paddingStart);

            var objectDataStart = writer.Position;
            SaveObjects(writer, objectMap, objectDataStart);

            var objectMapStart = writer.Position;
            objectMap.Write(writer);
            FileHeader.ObjectMapLocator = DwgFileHeader.DwgSectionLocator.ObjectMapLocator(objectMapStart - fileHeaderLocation, writer.Position - objectMapStart);

            var objectFreeSpaceStart = writer.Position;
            // TODO: if (version > DwgVersionId.R14) TDUUPDATE else TDUPDATE
            var objectStart = objectMap.GetOffsetFromHandle(Variables.BlockControlObjectHandle.HandleOrOffset);
            var freeSpaceSection = new DwgObjectFreeSpaceSection((uint)objectMap.HandleCount, Variables.UpdateDate, (uint)objectStart);
            freeSpaceSection.Write(writer);
            FileHeader.ObjectFreeSpaceLocator = DwgFileHeader.DwgSectionLocator.ObjectFreeSpaceLocator(objectFreeSpaceStart - fileHeaderLocation, writer.Position - objectFreeSpaceStart);

            var secondHeaderStart = writer.Position;
            FileHeader.WriteSecondHeader(writer, Variables, secondHeaderStart - fileHeaderLocation);

            var imageDataStart = writer.Position;
            ImageData.Write(writer, imageDataStart - fileHeaderLocation);
            FileHeader.ImagePointer = imageDataStart - fileHeaderLocation;

            // re-write the file header now that the pointer values have been set
            var endPos = writer.Position;
            writer.BaseStream.Seek(fileHeaderLocation, SeekOrigin.Begin);
            FileHeader.Write(writer);
            writer.BaseStream.Seek(endPos, SeekOrigin.Begin);
        }

        private void AssignHandles(DwgObjectMap objectMap)
        {
            BlockHeaders.ClearHandles();
            Layers.ClearHandles();
            Styles.ClearHandles();
            LineTypes.ClearHandles();
            Views.ClearHandles();
            UCSs.ClearHandles();
            ViewPorts.ClearHandles();
            AppIds.ClearHandles();
            DimStyles.ClearHandles();
            ViewPortEntityHeaders.ClearHandles();
            GroupDictionary.ClearHandles();
            MLineStyleDictionary.ClearHandles();
            NamedObjectDictionary.ClearHandles();

            BlockHeaders.AssignHandles(objectMap);
            Layers.AssignHandles(objectMap);
            Styles.AssignHandles(objectMap);
            LineTypes.AssignHandles(objectMap);
            Views.AssignHandles(objectMap);
            UCSs.AssignHandles(objectMap);
            ViewPorts.AssignHandles(objectMap);
            AppIds.AssignHandles(objectMap);
            DimStyles.AssignHandles(objectMap);
            ViewPortEntityHeaders.AssignHandles(objectMap);
            GroupDictionary.AssignHandles(objectMap);
            MLineStyleDictionary.AssignHandles(objectMap);
            NamedObjectDictionary.AssignHandles(objectMap);

            Variables.BlockControlObjectHandle = new DwgHandleReference(DwgHandleReferenceCode.SoftPointer, BlockHeaders.Handle.HandleOrOffset);
            Variables.LayerControlObjectHandle = new DwgHandleReference(DwgHandleReferenceCode.SoftPointer, Layers.Handle.HandleOrOffset);
            Variables.StyleObjectControlHandle = new DwgHandleReference(DwgHandleReferenceCode.SoftPointer, Styles.Handle.HandleOrOffset);
            Variables.LineTypeObjectControlHandle = new DwgHandleReference(DwgHandleReferenceCode.SoftPointer, LineTypes.Handle.HandleOrOffset);
            Variables.ViewControlObjectHandle = new DwgHandleReference(DwgHandleReferenceCode.SoftPointer, Views.Handle.HandleOrOffset);
            Variables.UcsControlObjectHandle = new DwgHandleReference(DwgHandleReferenceCode.SoftPointer, UCSs.Handle.HandleOrOffset);
            Variables.ViewPortControlObjectHandle = new DwgHandleReference(DwgHandleReferenceCode.SoftPointer, ViewPorts.Handle.HandleOrOffset);
            Variables.AppIdControlObjectHandle = new DwgHandleReference(DwgHandleReferenceCode.SoftPointer, AppIds.Handle.HandleOrOffset);
            Variables.DimStyleControlObjectHandle = new DwgHandleReference(DwgHandleReferenceCode.SoftPointer, DimStyles.Handle.HandleOrOffset);
            Variables.ViewPortEntityHeaderControlObjectHandle = new DwgHandleReference(DwgHandleReferenceCode.SoftPointer, ViewPortEntityHeaders.Handle.HandleOrOffset);
            Variables.GroupDictionaryHandle = new DwgHandleReference(DwgHandleReferenceCode.SoftOwner, GroupDictionary.Handle.HandleOrOffset);
            Variables.MLineStyleDictionaryHandle = new DwgHandleReference(DwgHandleReferenceCode.SoftOwner, MLineStyleDictionary.Handle.HandleOrOffset);
            Variables.NamedObjectsDictionaryHandle = new DwgHandleReference(DwgHandleReferenceCode.SoftPointer, NamedObjectDictionary.Handle.HandleOrOffset);

            Variables.PaperSpaceBlockRecordHandle = new DwgHandleReference(DwgHandleReferenceCode.SoftOwner, PaperSpaceBlockRecord.Handle.HandleOrOffset);
            Variables.ModelSpaceBlockRecordHandle = new DwgHandleReference(DwgHandleReferenceCode.SoftOwner, ModelSpaceBlockRecord.Handle.HandleOrOffset);
            Variables.ByLayerLineTypeHandle = new DwgHandleReference(DwgHandleReferenceCode.SoftOwner, ByLayerLineType.Handle.HandleOrOffset);
            Variables.ByBlockLineTypeHandle = new DwgHandleReference(DwgHandleReferenceCode.SoftOwner, ByBlockLineType.Handle.HandleOrOffset);
            Variables.ContinuousLineTypeHandle = new DwgHandleReference(DwgHandleReferenceCode.SoftOwner, ContinuousLineType.Handle.HandleOrOffset);
            Variables.CurrentViewPortEntityHandle = new DwgHandleReference(DwgHandleReferenceCode.SoftOwner, CurrentViewPort?.Handle.HandleOrOffset ?? 0);
            Variables.CurrentLayerHandle = new DwgHandleReference(DwgHandleReferenceCode.SoftOwner, CurrentLayer.Handle.HandleOrOffset);
            Variables.TextStyleHandle = new DwgHandleReference(DwgHandleReferenceCode.SoftOwner, TextStyle.Handle.HandleOrOffset);
            Variables.CurrentEntityLineTypeHandle = new DwgHandleReference(DwgHandleReferenceCode.SoftOwner, CurrentEntityLineType.Handle.HandleOrOffset);
            Variables.DimensionStyleHandle = new DwgHandleReference(DwgHandleReferenceCode.SoftOwner, DimensionStyle.Handle.HandleOrOffset);
            Variables.CurrentMultiLineStyleHandle = new DwgHandleReference(DwgHandleReferenceCode.SoftOwner, CurrentMultiLineStyle.Handle.HandleOrOffset);
            Variables.PaperSpaceCurrentUCSHandle = new DwgHandleReference(DwgHandleReferenceCode.SoftOwner, PaperSpaceCurrentUCS?.Handle.HandleOrOffset ?? 0);
            Variables.CurrentUCSHandle = new DwgHandleReference(DwgHandleReferenceCode.SoftOwner, CurrentUCS?.Handle.HandleOrOffset ?? 0);
            Variables.DimensionTextStyleHandle = new DwgHandleReference(DwgHandleReferenceCode.SoftOwner, DimensionTextStyle.Handle.HandleOrOffset);

            objectMap.SetNextAvailableHandle(Variables);
        }

        private IEnumerable<DwgObject> TopLevelObjects
        {
            get
            {
                yield return BlockHeaders;
                yield return Layers;
                yield return Styles;
                yield return LineTypes;
                yield return Views;
                yield return UCSs;
                yield return ViewPorts;
                yield return AppIds;
                yield return DimStyles;
                yield return ViewPortEntityHeaders;
                yield return GroupDictionary;
                yield return MLineStyleDictionary;
                yield return NamedObjectDictionary;
            }
        }

        private void SaveObjects(BitWriter writer, DwgObjectMap objectMap, int pointerOffset)
        {
            var writtenHandles = new HashSet<int>();
            foreach (var groupObject in TopLevelObjects)
            {
                groupObject.Write(writer, objectMap, writtenHandles, pointerOffset, FileHeader.Version);
            }
        }
    }
}
