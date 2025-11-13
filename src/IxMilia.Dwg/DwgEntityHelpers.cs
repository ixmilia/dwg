#nullable enable

using System.Collections.Generic;
using IxMilia.Dwg.Objects;

namespace IxMilia.Dwg
{
    internal static class DwgEntityHelpers
    {
        public static List<DwgObject> FlattenAndAssignPointersForWrite(IList<DwgEntity> entities)
        {
            var seenHandles = new HashSet<DwgHandle>();
            var flatList = new List<DwgObject>();
            for (int i = 0; i < entities.Count; i++)
            {
                var currentEntity = entities[i];
                AddChildItemsToList(currentEntity, seenHandles, flatList);

                var previousEntity = i == 0
                    ? null
                    : entities[i - 1];
                var nextEntity = i == entities.Count - 1
                    ? null
                    : entities[i + 1];
                currentEntity.PreviousEntityHandle = currentEntity.GetHandleToObject(previousEntity, DwgHandleReferenceCode.HardPointer);
                currentEntity.NextEntityHandle = currentEntity.GetHandleToObject(nextEntity, DwgHandleReferenceCode.HardPointer);
            }

            return flatList;
        }

        private static void AddChildItemsToList(DwgObject obj, HashSet<DwgHandle> seenHandles, IList<DwgObject> objects)
        {
            if (seenHandles.Add(obj.Handle))
            {
                objects.Add(obj);
                foreach (var child in obj.ChildItems)
                {
                    if (seenHandles.Add(child.Handle))
                    {
                        objects.Add(child);
                        AddChildItemsToList(child, seenHandles, objects);
                    }
                }
            }
        }

        public static bool HasFlag(int flags, int mask)
        {
            return (flags & mask) == mask;
        }

        public static int WithFlag(bool value, int flags, int mask)
        {
            if (value)
            {
                return flags | mask;
            }
            else
            {
                return flags & ~mask;
            }
        }

        public static IEnumerable<TEntity?> EntitiesFromHandlePointer<TEntity>(DwgObjectCache objectCache, BitReader reader, DwgHandle initialHandle, DwgHandleReference startHandleReference)
            where TEntity : DwgEntity
        {
            var result = new List<TEntity?>();
            var currentEntityHandle = initialHandle.ResolveHandleReference(startHandleReference);
            while (!currentEntityHandle.IsNull)
            {
                var entity = objectCache.GetObject<TEntity>(reader, currentEntityHandle);
                result.Add(entity);
                if (entity is not null)
                {
                    currentEntityHandle = entity.ResolveHandleReference(entity.NextEntityHandle);
                }
            }

            return result;
        }

        public static void PopulateEntityPointers<TEntity>(IList<TEntity> entities, ref DwgHandleReference firstEntityHandle, ref DwgHandleReference lastEntityHandle, DwgLayer? layerToAssign = null)
            where TEntity : DwgEntity
        {
            for (int i = 0; i < entities.Count; i++)
            {
                var currentEntity = entities[i];
                if (layerToAssign != null)
                {
                    currentEntity.Layer = layerToAssign;
                }

                var previousEntity = i == 0
                    ? null
                    : entities[i - 1];
                var nextEntity = i == entities.Count - 1
                    ? null
                    : entities[i + 1];
                currentEntity.PreviousEntityHandle = currentEntity.GetHandleToObject(previousEntity, DwgHandleReferenceCode.HardPointer);
                currentEntity.NextEntityHandle = currentEntity.GetHandleToObject(nextEntity, DwgHandleReferenceCode.HardPointer);
            }

            if (entities.Count == 0)
            {
                firstEntityHandle = new DwgHandleReference(DwgHandleReferenceCode.HardPointer, 0);
                lastEntityHandle = new DwgHandleReference(DwgHandleReferenceCode.HardPointer, 0);
            }
            else
            {
                firstEntityHandle = entities[0].MakeHandleReference(DwgHandleReferenceCode.HardPointer);
                lastEntityHandle = entities[entities.Count - 1].MakeHandleReference(DwgHandleReferenceCode.HardPointer);
            }
        }
    }
}
