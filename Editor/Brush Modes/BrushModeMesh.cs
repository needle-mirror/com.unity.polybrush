using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Polybrush;

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

        private HashSet<GameObject> modifiedPbMeshes = new HashSet<GameObject>();

        internal override void OnBrushBeginApply(BrushTarget brushTarget, BrushSettings brushSettings)
		{
            base.OnBrushBeginApply(brushTarget, brushSettings);
		}

		internal override void OnBrushApply(BrushTarget brushTarget, BrushSettings brushSettings)
		{
			// false means no ToMesh or Refresh, true does.  Optional addl bool runs pb_Object.Optimize()
			brushTarget.editableObject.Apply(true);

            if (ProBuilderBridge.ProBuilderExists() && brushTarget.editableObject.isProBuilderObject)
                ProBuilderBridge.Refresh(brushTarget.gameObject);

            UpdateTempComponent(brushTarget, brushSettings);
		}

		internal override void RegisterUndo(BrushTarget brushTarget)
		{
            if (ProBuilderBridge.IsValidProBuilderMesh(brushTarget.gameObject))
            {
                UnityEngine.Object pbMesh = ProBuilderBridge.GetProBuilderComponent(brushTarget.gameObject);
                if (pbMesh != null)
                {
                    Undo.RegisterCompleteObjectUndo(pbMesh, UndoMessage);
                    modifiedPbMeshes.Add(brushTarget.gameObject);
                }
                else
                {
                    Undo.RegisterCompleteObjectUndo(brushTarget.editableObject.polybrushMesh, UndoMessage);
                    modifiedMeshes.Add(brushTarget.editableObject.polybrushMesh.polyMesh);
                }
            }
            else
            {
                Undo.RegisterCompleteObjectUndo(brushTarget.editableObject.polybrushMesh, UndoMessage);
                modifiedMeshes.Add(brushTarget.editableObject.polybrushMesh.polyMesh);
            }

            brushTarget.editableObject.isDirty = true;
		}

		internal override void UndoRedoPerformed(List<GameObject> modified)
		{
			modifiedMeshes = new HashSet<PolyMesh>(modifiedMeshes.Where(x => x != null));

            if (ProBuilderBridge.ProBuilderExists())
            {
                // delete & undo causes cases where object is not null but the reference to it's pb_Object is
                HashSet<GameObject> remove = new HashSet<GameObject>();

                foreach (GameObject pb in modifiedPbMeshes)
                {
                    try
                    {
                        ProBuilderBridge.ToMesh(pb);
                        ProBuilderBridge.Refresh(pb);
                        ProBuilderBridge.Optimize(pb);
                    }
                    catch
                    {
                        remove.Add(pb);
                    }

                }

                if (remove.Count() > 0)
                    modifiedPbMeshes.SymmetricExceptWith(remove);
            }

            foreach (PolyMesh m in modifiedMeshes)
			{
                m.UpdateMeshFromData();
			}

			base.UndoRedoPerformed(modified);
		}
	}
}
