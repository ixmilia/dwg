using System;
using System.Collections.Generic;
using System.IO;

namespace IxMilia.Dwg
{
    public class DwgDrawing
    {
        public DwgFileHeader FileHeader { get; private set; }
        public DwgHeaderVariables Variables { get; private set; }
        public IList<DwgClassDefinition> Classes { get; private set; }

        internal DwgObjectMap ObjectMap { get; private set; }

        public DwgDrawing()
        {
            FileHeader = new DwgFileHeader(DwgVersionId.Default, 0, 0, 0);
            Variables = new DwgHeaderVariables();
            Classes = new List<DwgClassDefinition>();
            ObjectMap = new DwgObjectMap();
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
            drawing.ObjectMap = DwgObjectMap.Parse(reader.FromOffset(drawing.FileHeader.ObjectMapLocator.Pointer));
            // don't read the R13C3 and later unknown section
            drawing.FileHeader.ValidateSecondHeader(reader, drawing.Variables);

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

            var currentOffset = fileHeaderSize;

            // header variables
            byte[] variableData;
            using (var ms = new MemoryStream())
            {
                var variableWriter = new BitWriter(ms);
                Variables.Write(variableWriter, FileHeader.Version);
                variableData = variableWriter.AsBytes();
            }

            FileHeader.HeaderVariablesLocator = DwgFileHeader.DwgSectionLocator.HeaderVariablesLocator(currentOffset, variableData.Length);
            currentOffset += variableData.Length;

            // classes
            byte[] classData;
            using (var ms = new MemoryStream())
            {
                var classWriter = new BitWriter(ms);
                DwgClasses.Write(Classes, classWriter);
                classData = classWriter.AsBytes();
            }

            FileHeader.ClassSectionLocator = DwgFileHeader.DwgSectionLocator.ClassSectionLocator(currentOffset, classData.Length);
            currentOffset += classData.Length;

            // padding
            byte[] paddingData = new byte[0x200];
            // may contain the MEASUREMENT variable as the first 4 bytes, but not required
            FileHeader.UnknownSection_PaddingLocator = DwgFileHeader.DwgSectionLocator.UnknownSection_PaddingLocator(currentOffset, paddingData.Length);
            currentOffset += paddingData.Length;

            // object data
            byte[] objectData = new byte[0];
            // TODO
            currentOffset += objectData.Length;

            // object map
            byte[] objectMapData;
            using (var ms = new MemoryStream())
            {
                var objectMapWriter = new BitWriter(ms);
                ObjectMap.Write(objectMapWriter);
                objectMapData = objectMapWriter.AsBytes();
            }

            FileHeader.ObjectMapLocator = DwgFileHeader.DwgSectionLocator.ObjectMapLocator(currentOffset, objectMapData.Length);
            currentOffset += objectMapData.Length;

            // unknown section - R13C3 and later
            byte[] unknownSection_R13C3AndLaterData;
            using (var ms = new MemoryStream())
            {
                // unknown section has a value of `4` at offset 21
                var unknownWriter = new BitWriter(ms);
                for (int i = 0; i < 20; i++)
                {
                    unknownWriter.WriteByte(0);
                }

                unknownWriter.WriteInt(4);
                unknownSection_R13C3AndLaterData = unknownWriter.AsBytes();
            }

            FileHeader.UnknownSection_R13C3AndLaterLocator = DwgFileHeader.DwgSectionLocator.UnknownSection_R13C3AndLaterLocator(currentOffset, unknownSection_R13C3AndLaterData.Length);
            currentOffset += unknownSection_R13C3AndLaterData.Length;

            //
            // now actually write everything
            //
            var writer = new BitWriter(stream);
            FileHeader.Write(writer);
            writer.WriteBytes(variableData);
            writer.WriteBytes(classData);
            writer.WriteBytes(paddingData);
            writer.WriteBytes(objectData);
            writer.WriteBytes(objectMapData);
            writer.WriteBytes(unknownSection_R13C3AndLaterData);

            // second header
            byte[] secondHeaderData;
            using (var ms = new MemoryStream())
            {
                var secondHeaderWriter = new BitWriter(ms);
                FileHeader.WriteSecondHeader(secondHeaderWriter, Variables, (int)writer.BaseStream.Position, out var sizeOffset, out var crcOffset);
                secondHeaderData = secondHeaderWriter.AsBytes();

                // backfill the section size excluding sentinels
                var sizeBytes = BitConverter.GetBytes(secondHeaderData.Length - DwgFileHeader.SecondHeaderStartSentinel.Length - DwgFileHeader.SecondHeaderEndSentinel.Length);
                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(sizeBytes);
                }

                Array.Copy(sizeBytes, 0, secondHeaderData, sizeOffset, sizeBytes.Length);

                // re-compute the CRC excluding sentinels
                var dataLength = crcOffset - sizeOffset;
                var computedCrc = BitReaderExtensions.ComputeCRC(secondHeaderData, DwgFileHeader.SecondHeaderStartSentinel.Length, dataLength, DwgHeaderVariables.InitialCrcValue);
                var crcBytes = BitConverter.GetBytes(computedCrc);
                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(crcBytes);
                }

                Array.Copy(crcBytes, 0, secondHeaderData, crcOffset, crcBytes.Length);
            }

            currentOffset += secondHeaderData.Length;

            // image data
            byte[] imageData = new byte[0];
            // TODO
            FileHeader.ImagePointer = currentOffset;
            currentOffset += imageData.Length;

            //
            // continue writing
            //
            writer.WriteBytes(secondHeaderData);
            writer.WriteBytes(imageData);
        }
    }
}
