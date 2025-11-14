#nullable enable

using System;
using System.Collections.Generic;

namespace IxMilia.Dwg.Objects
{
    public partial class DwgImage
    {
        internal DwgHandleReference _imageDefHandleReference;
        internal DwgHandleReference _imageDefReactorHandleReference;

        public DwgImageDefinition ImageDefinition { get; set; } = new DwgImageDefinition();

        public DwgImageDefinitionReactor ImageDefinitionReactor { get; set; } = new DwgImageDefinitionReactor();

        public DwgImage(string filePath, DwgPoint insertionPoint, int widthInPixels, int heightInPixels, double displayWidth, double displayHeight, double rotationAngleInRadians = 0.0)
            : this()
        {
            ImageDefinition.FilePath = filePath;
            ImageDefinition.ImageWidth = widthInPixels;
            ImageDefinition.ImageHeight = heightInPixels;
            ImageDefinition.PixelWidth = widthInPixels;
            ImageDefinition.PixelHeight = heightInPixels;
            ImageSize = new DwgVector(widthInPixels, heightInPixels, 0.0);
            InsertionPoint = insertionPoint;

            var uVectorLength = widthInPixels / displayWidth;
            var vVectorLength = heightInPixels / displayHeight;

            var sin = Math.Sin(rotationAngleInRadians);
            var cos = Math.Cos(rotationAngleInRadians);

            UVector = new DwgVector(cos / uVectorLength, sin / uVectorLength, 0.0);
            VVector = new DwgVector(-sin / vVectorLength, cos / vVectorLength, 0.0);
        }

        internal override IEnumerable<DwgObject> ChildItems
        {
            get
            {
                yield return ImageDefinition;
                yield return ImageDefinitionReactor;
            }
        }

        internal override void ReadPostData(BitReader reader, DwgVersionId version)
        {
            _imageDefHandleReference = reader.Read_H();
            if (_imageDefHandleReference.Code != DwgHandleReferenceCode.SoftOwner)
            {
                throw new DwgReadException("Incorrect image definition handle code");
            }

            _imageDefReactorHandleReference = reader.Read_H();
            if (_imageDefReactorHandleReference.Code != DwgHandleReferenceCode.SoftPointer)
            {
                throw new DwgReadException("Incorrect image definition reactor handle code");
            }
        }

        internal override void OnAfterEntityRead(BitReader reader, DwgObjectCache objectCache, DwgVersionId version)
        {
            ImageDefinition = objectCache.GetObject<DwgImageDefinition>(reader, ResolveHandleReference(_imageDefHandleReference));
            ImageDefinitionReactor = objectCache.GetObject<DwgImageDefinitionReactor>(reader, ResolveHandleReference(_imageDefReactorHandleReference));
        }

        internal override void OnBeforeEntityWrite(DwgVersionId version)
        {
            _imageDefHandleReference = ImageDefinition.MakeHandleReference(DwgHandleReferenceCode.SoftOwner);
            _imageDefReactorHandleReference = ImageDefinitionReactor.MakeHandleReference(DwgHandleReferenceCode.SoftPointer);

            ImageDefinition._reactorHandleReferences.Clear();
            ImageDefinition._reactorHandleReferences.Add(ImageDefinitionReactor.MakeHandleReference(DwgHandleReferenceCode.HardPointer));
        }

        internal override void WritePostData(BitWriter writer, DwgVersionId version)
        {
            writer.Write_H(_imageDefHandleReference);
            writer.Write_H(_imageDefReactorHandleReference);
        }
    }
}
