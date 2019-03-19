using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Polybrush;

#if PROBUILDER_4_0_OR_NEWER
using UnityEditor.ProBuilder;
using UnityEngine.ProBuilder;
#endif

namespace UnityEditor.Polybrush
{
    /// <summary>
    /// Base class for brush modes that modify the mesh.
    /// </summary>
    [System.Serializable]
	internal abstract class BrushModeMesh : BrushMode
	{
		// All meshes that have ever been modified, ever.  Kept around to refresh mesh vertices
		// on Undo/Redo since Unity doesn't.
		private HashSet<PolyMesh> modifiedMeshes = new HashSet<PolyMesh>();
#if PROBUILDER_4_0_OR_NEWER
        private HashSet<ProBuilderMesh> modifiedPbMeshes = new HashSet<ProBuilderMesh>();
#endif

        internal override void OnBrushBeginApply(BrushTarget brushTarget, BrushSettings brushSettings)
		{
            base.OnBrushBeginApply(brushTarget, brushSettings);
		}

		internal override void OnBrushApply(BrushTarget brushTarget, BrushSettings brushSettings)
		{
			// false means no ToMesh or Refresh, true does.  Optional addl bool runs pb_Object.Optimize()
			brushTarget.editableObject.Apply(true);

#if PROBUILDER_4_0_OR_NEWER
            ProBuilderEditor.Refresh(false);
#endif
            UpdateTempComponent(brushTarget, brushSettings);
		}

		internal override void RegisterUndo(BrushTarget brushTarget)
		{
#if PROBUILDER_4_0_OR_NEWER
            ProBuilderMesh pbMesh = brushTarget.gameObject.GetComponent<ProBuilderMesh>();
            if (pbMesh != null)
			{
				Undo.RegisterCompleteObjectUndo(pbMesh, UndoMessage);
				modifiedPbMeshes.Add(pbMesh);
			}
            else
            {
				Undo.RegisterCompleteObjectUndo(brushTarget.editableObject.polybrushMesh, UndoMessage);
				modifiedMeshes.Add(brushTarget.editableObject.polybrushMesh.polyMesh);
			}
#else
			Undo.RegisterCompleteObjectUndo(brushTarget.editableObject.polybrushMesh, UndoMessage);
			modifiedMeshes.Add(brushTarget.editableObject.polybrushMesh.polyMesh);
#endif

            brushTarget.editableObject.isDirty = true;
		}

		internal override void UndoRedoPerformed(List<GameObject> modified)
		{
			modifiedMeshes = new HashSet<PolyMesh>(modifiedMeshes.Where(x => x != null));

#if PROBUILDER_4_0_OR_NEWER
            // delete & undo causes cases where object is not null but the reference to it's pb_Object is
            HashSet<ProBuilderMesh> remove = new HashSet<ProBuilderMesh>();

            foreach (ProBuilderMesh pb in modifiedPbMeshes)
            {
                try
                {
                    pb.ToMesh();
                    pb.Refresh();
                    pb.Optimize();
                }
                catch
                {
                    remove.Add(pb);
                }

            }

            if (remove.Count() > 0)
                modifiedPbMeshes.SymmetricExceptWith(remove);
#endif

            foreach (PolyMesh m in modifiedMeshes)
			{
                m.UpdateMeshFromData();
			}

			base.UndoRedoPerformed(modified);
		}
	}
}
