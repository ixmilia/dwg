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

        public DwgDrawing()
        {
            FileHeader = new DwgFileHeader(DwgVersionId.Default, 0, 0, 0);
            Variables = new DwgHeaderVariables();
            Classes = new List<DwgClassDefinition>();

            var continuous = new DwgLineType("CONTINUOUS") { Description = "Solid line" };
            var standardStyle = new DwgStyle("STANDARD");

            BlockHeaders = new DwgBlockControlObject();
            Layers = new DwgLayerControlObject
            {
                new DwgLayer("0") { LineType = continuous }
            };
            Styles = new DwgStyleControlObject()
            {
                standardStyle
            };
            LineTypes = new DwgLineTypeControlObject()
            {
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
            MLineStyleDictionary = new DwgDictionary();
            NamedObjectDictionary = new DwgDictionary();
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

            return drawing;
        }

        private void LoadObjects(BitReader reader, DwgObjectCache objectCache)
        {
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

            objectCache.LoadEntities(reader, this);
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

            var unknownR13C3Start = writer.Position;
            DwgUnknownSectionR13C3.Write(writer);
            FileHeader.UnknownSection_R13C3AndLaterLocator = DwgFileHeader.DwgSectionLocator.UnknownSection_R13C3AndLaterLocator(unknownR13C3Start - fileHeaderLocation, writer.Position - unknownR13C3Start);

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

            Variables.BlockControlObjectHandle = BlockHeaders.Handle;
            Variables.LayerControlObjectHandle = Layers.Handle;
            Variables.StyleObjectControlHandle = Styles.Handle;
            Variables.LineTypeObjectControlHandle = LineTypes.Handle;
            Variables.ViewControlObjectHandle = Views.Handle;
            Variables.UcsControlObjectHandle = UCSs.Handle;
            Variables.ViewPortControlObjectHandle = ViewPorts.Handle;
            Variables.AppIdControlObjectHandle = AppIds.Handle;
            Variables.DimStyleControlObjectHandle = DimStyles.Handle;
            Variables.ViewPortEntityHeaderControlObjectHandle = ViewPortEntityHeaders.Handle;
            Variables.GroupDictionaryHandle = GroupDictionary.Handle;
            Variables.MLineStyleDictionaryHandle = MLineStyleDictionary.Handle;
            Variables.NamedObjectsDictionaryHandle = NamedObjectDictionary.Handle;

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
