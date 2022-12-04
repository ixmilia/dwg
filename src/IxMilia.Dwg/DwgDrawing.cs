using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public DwgBlockHeader PaperSpaceBlockRecord { get => BlockHeaders.PaperSpace; set => BlockHeaders.PaperSpace = value; }
        public DwgBlockHeader ModelSpaceBlockRecord { get => BlockHeaders.ModelSpace; set => BlockHeaders.ModelSpace = value; }
        public DwgLineType ByLayerLineType { get => LineTypes.ByLayer; set => LineTypes.ByLayer = value; }
        public DwgLineType ByBlockLineType { get => LineTypes.ByBlock; set => LineTypes.ByBlock = value; }
        public DwgLineType ContinuousLineType { get => LineTypes.Continuous; set => LineTypes.Continuous = value; }
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
            Classes = new List<DwgClassDefinition>()
            {
                new DwgClassDefinition(0, 0, "ObjectDBX Classes", "AcDbDictionaryWithDefault", "ACDBDICTIONARYWDFLT", false, false),
                new DwgClassDefinition(0, 1153, "ObjectDBX Classes", "AcDbScale", "SCALE", false, false),
                new DwgClassDefinition(0, 4095, "ObjectDBX Classes", "AcDbVisualStyle", "VISUALSTYLE", false, false),
                new DwgClassDefinition(0, 1153, "ObjectDBX Classes", "AcDbMaterial", "MATERIAL", false, false),
                new DwgClassDefinition(0, 4095, "ObjectDBX Classes", "AcDbTableStyle", "TABLESTYLE", false, false),
                new DwgClassDefinition(0, 4095, "ACDB_MLEADERSTYLE_CLASS", "AcDbMLeaderStyle", "MLEADERSTYLE", false, false),
                new DwgClassDefinition(0, 1153, "SCENEOE", "AcDbSun", "SUN", false, false),
                new DwgClassDefinition(0, 0, "ObjectDBX Classes", "AcDbDictionaryVar", "DICTIONARYVAR", false, false),
                new DwgClassDefinition(0, 1152, "ObjectDBX Classes", "AcDbCellStyleMap", "CELLSTYLEMAP", false, false),
                new DwgClassDefinition(0, 0, "ObjectDBX Classes", "AcDbXrecord", "XRECORD", false, false),
                DwgObjectTypeExtensions.GetClassDefinitionForObjectType(DwgObjectType.LwPolyline),
                new DwgClassDefinition(0, 0, "ObjectDBX Classes", "AcDbHatch", "HATCH", false, true),
                new DwgClassDefinition(0, 0, "ObjectDBX Classes", "AcDbPlaceHolder", "ACDBPLACEHOLDER", false, false),
                new DwgClassDefinition(0, 0, "ObjectDBX Classes", "AcDbLayout", "LAYOUT", false, false),
            };

            var standardStyle = new DwgStyle("STANDARD");

            LineTypes = DwgLineTypeControlObject.Create();
            var defaultLayer = new DwgLayer("0") { LineType = LineTypes.Continuous };
            BlockHeaders = DwgBlockControlObject.Create(defaultLayer);
            Layers = new DwgLayerControlObject
            {
                defaultLayer
            };
            Styles = new DwgStyleControlObject()
            {
                standardStyle
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
            GroupDictionary = DefaultGroupDictionary;
            MLineStyleDictionary = DefaultMLineStyleDictionary;
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

            CurrentLayer = Layers["0"];
            TextStyle = Styles["STANDARD"];
            CurrentEntityLineType = LineTypes.ByBlock;
            DimensionStyle = DimStyles["STANDARD"];
            CurrentMultiLineStyle = GetDefaultMLineStyle(MLineStyleDictionary);
            DimensionTextStyle = Styles["STANDARD"];
        }

        public static DwgDrawing Load(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open))
            {
                return Load(stream);
            }
        }

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

            var objectCache = DwgObjectCache.Parse(reader.FromOffset(drawing.FileHeader.ObjectMapLocator.Pointer), drawing.FileHeader.Version, drawing.Classes);
            drawing.LoadObjects(reader, objectCache);
            objectCache.ResolveLazyObjects();

            // TODO: one final pass of the objects now that everything has been resolved

            if (drawing.FileHeader.ObjectFreeSpaceLocator.Pointer != 0)
            {
                var freeSpaceSection = DwgObjectFreeSpaceSection.Parse(reader.FromOffset(drawing.FileHeader.ObjectFreeSpaceLocator.Pointer));
                var objectOffset = objectCache.GetOffsetFromHandle(drawing.Variables.BlockControlObjectHandle.AsAbsoluteHandle());
                // TODO: if (version > DwgVersionId.R14) TDUUPDATE else TDUPDATE
                freeSpaceSection.Validate((uint)objectCache.ObjectCount, drawing.Variables.UpdateDate, (uint)objectOffset);
            }

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

            BlockHeaders = objectCache.GetObject<DwgBlockControlObject>(reader, Variables.BlockControlObjectHandle.AsAbsoluteHandle());
            Layers = objectCache.GetObject<DwgLayerControlObject>(reader, Variables.LayerControlObjectHandle.AsAbsoluteHandle());
            Styles = objectCache.GetObject<DwgStyleControlObject>(reader, Variables.StyleObjectControlHandle.AsAbsoluteHandle());
            LineTypes = objectCache.GetObject<DwgLineTypeControlObject>(reader, Variables.LineTypeObjectControlHandle.AsAbsoluteHandle());
            Views = objectCache.GetObject<DwgViewControlObject>(reader, Variables.ViewControlObjectHandle.AsAbsoluteHandle());
            UCSs = objectCache.GetObject<DwgUCSControlObject>(reader, Variables.UcsControlObjectHandle.AsAbsoluteHandle());
            ViewPorts = objectCache.GetObject<DwgViewPortControlObject>(reader, Variables.ViewPortControlObjectHandle.AsAbsoluteHandle());
            AppIds = objectCache.GetObject<DwgAppIdControlObject>(reader, Variables.AppIdControlObjectHandle.AsAbsoluteHandle());
            DimStyles = objectCache.GetObject<DwgDimStyleControlObject>(reader, Variables.DimStyleControlObjectHandle.AsAbsoluteHandle());
            ViewPortEntityHeaders = objectCache.GetObject<DwgViewPortEntityHeaderControlObject>(reader, Variables.ViewPortEntityHeaderControlObjectHandle.AsAbsoluteHandle());
            GroupDictionary = objectCache.GetObject<DwgDictionary>(reader, Variables.GroupDictionaryHandle.AsAbsoluteHandle(), allowNull: true) ?? DefaultGroupDictionary;
            MLineStyleDictionary = objectCache.GetObject<DwgDictionary>(reader, Variables.MLineStyleDictionaryHandle.AsAbsoluteHandle(), allowNull: true) ?? DefaultMLineStyleDictionary;
            NamedObjectDictionary = objectCache.GetObject<DwgDictionary>(reader, Variables.NamedObjectsDictionaryHandle.AsAbsoluteHandle());
            PaperSpaceBlockRecord = objectCache.GetObject<DwgBlockHeader>(reader, Variables.PaperSpaceBlockRecordHandle.AsAbsoluteHandle());
            ModelSpaceBlockRecord = objectCache.GetObject<DwgBlockHeader>(reader, Variables.ModelSpaceBlockRecordHandle.AsAbsoluteHandle());
            ByLayerLineType = objectCache.GetObject<DwgLineType>(reader, Variables.ByLayerLineTypeHandle.AsAbsoluteHandle());
            ByBlockLineType = objectCache.GetObject<DwgLineType>(reader, Variables.ByBlockLineTypeHandle.AsAbsoluteHandle());
            ContinuousLineType = objectCache.GetObject<DwgLineType>(reader, Variables.ContinuousLineTypeHandle.AsAbsoluteHandle());
            CurrentViewPort = objectCache.GetObjectOrDefault<DwgViewPort>(reader, Variables.CurrentViewPortEntityHandle.AsAbsoluteHandle());
            CurrentLayer = objectCache.GetObject<DwgLayer>(reader, Variables.CurrentLayerHandle.AsAbsoluteHandle());
            TextStyle = objectCache.GetObject<DwgStyle>(reader, Variables.TextStyleHandle.AsAbsoluteHandle());
            CurrentEntityLineType = objectCache.GetObject<DwgLineType>(reader, Variables.CurrentEntityLineTypeHandle.AsAbsoluteHandle());
            DimensionStyle = objectCache.GetObject<DwgDimStyle>(reader, Variables.DimensionStyleHandle.AsAbsoluteHandle());
            CurrentMultiLineStyle = objectCache.GetObject<DwgMLineStyle>(reader, Variables.CurrentMultiLineStyleHandle.AsAbsoluteHandle(), allowNull: true) ?? GetDefaultMLineStyle(MLineStyleDictionary);
            PaperSpaceCurrentUCS = objectCache.GetObjectOrDefault<DwgUCS>(reader, Variables.PaperSpaceCurrentUCSHandle.AsAbsoluteHandle());
            CurrentUCS = objectCache.GetObjectOrDefault<DwgUCS>(reader, Variables.CurrentUCSHandle.AsAbsoluteHandle());
            DimensionTextStyle = objectCache.GetObject<DwgStyle>(reader, Variables.DimensionTextStyleHandle.AsAbsoluteHandle());
        }

        private static DwgDictionary DefaultGroupDictionary => new DwgDictionary();
        private static DwgDictionary DefaultMLineStyleDictionary
        {
            get
            {
                var standardMLineStyle = DwgMLineStyle.GetDefaultMLineStyle();
                return new DwgDictionary()
                {
                    { standardMLineStyle.Name, standardMLineStyle }
                };
            }
        }
        private static DwgMLineStyle GetDefaultMLineStyle(DwgDictionary mlineStyleDictionary) => (DwgMLineStyle)mlineStyleDictionary["Standard"];

        private static void AssertHandleType(DwgHandleReference handle, DwgHandleReferenceCode expectedHandleCode, string itemName)
        {
            if (handle.Code != expectedHandleCode)
            {
                throw new DwgReadException($"Invalid handle code for {itemName}.");
            }
        }

        public void Save(string path)
        {
            using (var fs = new FileStream(path, FileMode.Create))
            {
                Save(fs);
            }
        }

        public void Save(Stream stream)
        {
            EnsureObjectMemberships();
            EnsureClasses();

            var objectMap = new DwgObjectMap();
            AssignHandles(objectMap);
            AssignOwnershipHandles();

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

            var _objectDataStart = writer.Position;
            SaveObjects(writer, objectMap);

            var objectMapStart = writer.Position;
            objectMap.Write(writer);
            FileHeader.ObjectMapLocator = DwgFileHeader.DwgSectionLocator.ObjectMapLocator(objectMapStart - fileHeaderLocation, writer.Position - objectMapStart);

            var objectFreeSpaceStart = writer.Position;
            // TODO: if (version > DwgVersionId.R14) TDUUPDATE else TDUPDATE
            var objectStart = objectMap.GetOffsetFromHandle(Variables.BlockControlObjectHandle.AsAbsoluteHandle());
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

            Variables.BlockControlObjectHandle = BlockHeaders.MakeHandleReference(DwgHandleReferenceCode.SoftPointer);
            Variables.LayerControlObjectHandle = Layers.MakeHandleReference(DwgHandleReferenceCode.SoftPointer);
            Variables.StyleObjectControlHandle = Styles.MakeHandleReference(DwgHandleReferenceCode.SoftPointer);
            Variables.LineTypeObjectControlHandle = LineTypes.MakeHandleReference(DwgHandleReferenceCode.SoftPointer);
            Variables.ViewControlObjectHandle = Views.MakeHandleReference(DwgHandleReferenceCode.SoftPointer);
            Variables.UcsControlObjectHandle = UCSs.MakeHandleReference(DwgHandleReferenceCode.SoftPointer);
            Variables.ViewPortControlObjectHandle = ViewPorts.MakeHandleReference(DwgHandleReferenceCode.SoftPointer);
            Variables.AppIdControlObjectHandle = AppIds.MakeHandleReference(DwgHandleReferenceCode.SoftPointer);
            Variables.DimStyleControlObjectHandle = DimStyles.MakeHandleReference(DwgHandleReferenceCode.SoftPointer);
            Variables.ViewPortEntityHeaderControlObjectHandle = ViewPortEntityHeaders.MakeHandleReference(DwgHandleReferenceCode.SoftPointer);
            Variables.GroupDictionaryHandle = GroupDictionary.MakeHandleReference(DwgHandleReferenceCode.SoftOwner);
            Variables.MLineStyleDictionaryHandle = MLineStyleDictionary.MakeHandleReference(DwgHandleReferenceCode.SoftOwner);
            Variables.NamedObjectsDictionaryHandle = NamedObjectDictionary.MakeHandleReference(DwgHandleReferenceCode.SoftPointer);

            Variables.PaperSpaceBlockRecordHandle = PaperSpaceBlockRecord.MakeHandleReference(DwgHandleReferenceCode.SoftOwner);
            Variables.ModelSpaceBlockRecordHandle = ModelSpaceBlockRecord.MakeHandleReference(DwgHandleReferenceCode.SoftOwner);
            Variables.ByLayerLineTypeHandle = ByLayerLineType.MakeHandleReference(DwgHandleReferenceCode.SoftOwner);
            Variables.ByBlockLineTypeHandle = ByBlockLineType.MakeHandleReference(DwgHandleReferenceCode.SoftOwner);
            Variables.ContinuousLineTypeHandle = ContinuousLineType.MakeHandleReference(DwgHandleReferenceCode.SoftOwner);
            Variables.CurrentViewPortEntityHandle = (CurrentViewPort?.Handle ?? new DwgHandle()).MakeHandleReference(DwgHandleReferenceCode.SoftOwner);
            Variables.CurrentLayerHandle = CurrentLayer.MakeHandleReference(DwgHandleReferenceCode.SoftOwner);
            Variables.TextStyleHandle = TextStyle.MakeHandleReference(DwgHandleReferenceCode.SoftOwner);
            Variables.CurrentEntityLineTypeHandle = CurrentEntityLineType.MakeHandleReference(DwgHandleReferenceCode.SoftOwner);
            Variables.DimensionStyleHandle = DimensionStyle.MakeHandleReference(DwgHandleReferenceCode.SoftOwner);
            Variables.CurrentMultiLineStyleHandle = CurrentMultiLineStyle.MakeHandleReference(DwgHandleReferenceCode.SoftOwner);
            Variables.PaperSpaceCurrentUCSHandle = (PaperSpaceCurrentUCS?.Handle ?? new DwgHandle()).MakeHandleReference(DwgHandleReferenceCode.SoftOwner);
            Variables.CurrentUCSHandle = (CurrentUCS?.Handle ?? new DwgHandle()).MakeHandleReference(DwgHandleReferenceCode.SoftOwner);
            Variables.DimensionTextStyleHandle = (DimensionTextStyle?.Handle ?? new DwgHandle()).MakeHandleReference(DwgHandleReferenceCode.SoftOwner);

            objectMap.SetNextAvailableHandle(Variables);
        }

        private void AssignOwnershipHandles()
        {
            foreach (var lt in LineTypes.Values)
            {
                lt.LineTypeControlHandleReference = LineTypes.Handle.MakeHandleReference(DwgHandleReferenceCode.HardPointer);
            }
        }

        private void EnsureObjectMemberships()
        {
            EnsureCollectionContains(BlockHeaders, nameof(BlockHeaders),
                Tuple.Create(PaperSpaceBlockRecord, nameof(PaperSpaceBlockRecord)),
                Tuple.Create(ModelSpaceBlockRecord, nameof(ModelSpaceBlockRecord)));
            EnsureCollectionContains(LineTypes, nameof(LineTypes),
                Tuple.Create(ByLayerLineType, nameof(ByLayerLineType)),
                Tuple.Create(ByBlockLineType, nameof(ByBlockLineType)),
                Tuple.Create(ContinuousLineType, nameof(ContinuousLineType)),
                Tuple.Create(CurrentEntityLineType, nameof(CurrentEntityLineType)));
            EnsureCollectionContains(ViewPorts, nameof(ViewPorts), CurrentViewPort, nameof(CurrentViewPort), allowNull: true);
            EnsureCollectionContains(Layers, nameof(Layers), CurrentLayer, nameof(CurrentLayer));
            EnsureCollectionContains(Styles, nameof(Styles),
                Tuple.Create(TextStyle, nameof(TextStyle)),
                Tuple.Create(DimensionTextStyle, nameof(DimensionTextStyle)));
            EnsureCollectionContains(DimStyles, nameof(DimStyles), DimensionStyle, nameof(DimensionStyle));
            EnsureCollectionContains(MLineStyleDictionary, nameof(MLineStyleDictionary), CurrentMultiLineStyle, nameof(CurrentMultiLineStyle));
            EnsureCollectionContains(UCSs, nameof(UCSs), PaperSpaceCurrentUCS, nameof(PaperSpaceCurrentUCS), allowNull: true);
            EnsureCollectionContains(UCSs, nameof(UCSs), CurrentUCS, nameof(CurrentUCS), allowNull: true);
        }

        private void EnsureClasses()
        {
            var classNames = new HashSet<string>(Classes.Select(c => c.DxfClassName));
            foreach (var entity in ModelSpaceBlockRecord.Entities)
            {
                var expectedClassName = DwgObjectTypeExtensions.ClassNameFromTypeCode(entity.Type);
                if (expectedClassName is not null)
                {
                    if (!classNames.Contains(expectedClassName))
                    {
                        var classDefinition = DwgObjectTypeExtensions.GetClassDefinitionForObjectType(entity.Type);
                        if (classDefinition is null)
                        {
                            throw new InvalidOperationException($"Unable to create class definition for object type {entity.Type}.");
                        }

                        Classes.Add(classDefinition);
                        classNames.Add(expectedClassName);
                    }
                }
            }
        }

        private static void EnsureCollectionContains<TKey, TValue>(IDictionary<TKey, TValue> collection, string collectionName, params Tuple<TValue, string>[] items)
        {
            foreach (var item in items)
            {
                EnsureCollectionContains(collection, collectionName, item.Item1, item.Item2);
            }
        }

        private static void EnsureCollectionContains<TKey, TValue>(IDictionary<TKey, TValue> collection, string collectionName, TValue item, string itemName, bool allowNull = false)
        {
            if (!allowNull)
            {
                if (item == null)
                {
                    throw new InvalidOperationException($"The item '{itemName}' is not allowed to be null.");
                }

                if (!collection.Values.Any(v => ReferenceEquals(v, item)))
                {
                    throw new InvalidOperationException($"The item '{itemName}' is not a member of the collection '{collectionName}'.");
                }
            }
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

        private void SaveObjects(BitWriter writer, DwgObjectMap objectMap)
        {
            var classMap = new Dictionary<string, short>();
            for (int i = 0; i < Classes.Count; i++)
            {
                classMap.Add(Classes[i].DxfClassName.ToUpperInvariant(), Classes[i].Number);
            }

            var appIdMap = AppIds.ToDictionary(appId => appId.Key, appId => appId.Value.Handle);

            var writtenHandles = new HashSet<DwgHandle>();
            foreach (var groupObject in TopLevelObjects)
            {
                groupObject.Write(writer, objectMap, writtenHandles, FileHeader.Version, classMap, appIdMap);
            }
        }
    }
}
