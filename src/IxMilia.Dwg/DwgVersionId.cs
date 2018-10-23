using System;

namespace IxMilia.Dwg
{
    public enum DwgVersionId
    {
        R13,
        R14
    }

    public static class DwgVersionIdExtensions
    {
        public const string R13String = "AC1012";
        public const string R14String = "AC1014";

        public static string VersionString(this DwgVersionId id)
        {
            switch (id)
            {
                case DwgVersionId.R13:
                    return R13String;
                case DwgVersionId.R14:
                    return R14String;
                default:
                    throw new NotSupportedException($"Unrecognized version {id}");
            }
        }

        public static DwgVersionId VersionIdFromString(string s)
        {
            switch (s)
            {
                case R13String:
                    return DwgVersionId.R13;
                case R14String:
                    return DwgVersionId.R14;
                default:
                    throw new NotSupportedException($"Unrecognized version {s}");
            }
        }
    }
}
