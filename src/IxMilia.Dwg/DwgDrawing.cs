using System.Collections.Generic;
using System.IO;

namespace IxMilia.Dwg
{
    public class DwgDrawing
    {
        public DwgFileHeader FileHeader { get; private set; }
        public DwgHeaderVariables Variables { get; private set; }
        public IList<DwgClassDefinition> Classes { get; private set; }

        public DwgDrawing()
        {
            FileHeader = new DwgFileHeader(DwgVersionId.Default, 0, 0, 0);
            Variables = new DwgHeaderVariables();
            Classes = new List<DwgClassDefinition>();
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
            drawing.Variables = DwgHeaderVariables.Parse(new BitReader(reader.Data, drawing.FileHeader.HeaderVariablesLocator.Pointer), drawing.FileHeader.Version);
            drawing.Classes = DwgClasses.Parse(new BitReader(reader.Data, drawing.FileHeader.ClassSectionLocator.Pointer), drawing.FileHeader.Version);

            return drawing;
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
            // create a dummy-write of the file header to compute its size
            int fileHeaderSize;
            using (var ms = new MemoryStream())
            {
                var headerWriter = new BitWriter(ms);
                FileHeader.Write(headerWriter);
                fileHeaderSize = headerWriter.AsBytes().Length;
            }

            //
            // write each section to memory so the offsets can be calculated
            //

            // header variables
            byte[] variableData;
            using (var ms = new MemoryStream())
            {
                var variableWriter = new BitWriter(ms);
                Variables.Write(variableWriter, FileHeader.Version);
                variableData = variableWriter.AsBytes();
            }

            FileHeader.HeaderVariablesLocator = DwgFileHeader.DwgSectionLocator.HeaderVariablesLocator(fileHeaderSize, variableData.Length);

            // classes
            byte[] classData;
            using (var ms = new MemoryStream())
            {
                var classWriter = new BitWriter(ms);
                DwgClasses.Write(Classes, classWriter);
                classData = classWriter.AsBytes();
            }

            FileHeader.ClassSectionLocator = DwgFileHeader.DwgSectionLocator.ClassSectionLocator(FileHeader.HeaderVariablesLocator.Pointer + variableData.Length, classData.Length);

            // TODO: object map
            // TODO: unknown 1
            // TODO: unknown 2
            // TODO: unknown 3

            //
            // now actually write everything
            //
            var writer = new BitWriter(stream);
            FileHeader.Write(writer);
            writer.WriteBytes(variableData);
            writer.WriteBytes(classData);
        }
    }
}
