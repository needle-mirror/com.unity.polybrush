using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.Polybrush
{
    /// <summary>
    /// Holds the affected mesh attribute arrays
    /// </summary>
	internal class SplatSet
	{
		const float WEIGHT_EPSILON = 0.0001f;

		// how many vertices are stored in this splatset
		private int weightCount;

		// channel to index in weights array
		private Dictionary<MeshChannel, int> channelMap;

		// splatset doesn't store an array of splatweight because it's too slow
		// to reconstruct mesh arrays from selecting each component of a splatweight.
		private Vector4[][] weights;

		// Assigns where each weight is applied on the mesh.
		internal AttributeLayout[] attributeLayout;

		// The number of values being passed to the mesh (ex, color.rgba = 4)
		internal int attributeCount { get { return attributeLayout.Length; } }

        /// <summary>
        /// Initialize a new SplatSet with vertex count and attribute layout.  Attributes should
        /// match the length of weights applied (one attribute per value).
        /// Weight values are initialized to zero (unless preAlloc is false, then only the channel
        /// container array is initialized and arrays aren't allocated)
        /// </summary>
        /// <param name="vertexCount"></param>
        /// <param name="attributes"></param>
        /// <param name="preAlloc">Allocate and initialize Weight values to 0 ?</param>
        internal SplatSet(int vertexCount, AttributeLayout[] attributes, bool preAlloc = true)
		{
			this.channelMap = SplatWeight.GetChannelMap(attributes);
			int channels = channelMap.Count;
			this.attributeLayout = attributes;
			this.weights = new Vector4[channels][];
			this.weightCount = vertexCount;

			if(preAlloc)
			{
				for(int i = 0; i < channels; i++)
					this.weights[i] = new Vector4[vertexCount];
			}
		}

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="other">The copied SplatSet</param>
        internal SplatSet(SplatSet other)
		{
			int attribCount = other.attributeCount;
			this.attributeLayout = new AttributeLayout[attribCount];
			System.Array.Copy(other.attributeLayout, 0, this.attributeLayout, 0, attribCount);

			this.channelMap = new Dictionary<MeshChannel, int>();

			foreach(var kvp in other.channelMap)
				this.channelMap.Add(kvp.Key, kvp.Value);

			int channelCount = other.channelMap.Count;
			this.weightCount = other.weightCount;
			this.weights = new Vector4[channelCount][];

			for(int i = 0; i < channelCount; i++)
			{
                if (other.weights[i] != null)
                {
				    this.weights[i] = new Vector4[weightCount];
				    System.Array.Copy(other.weights[i], this.weights[i], weightCount);
                }
			}
		}

        internal void SetChannelBaseTextureWeights(Dictionary<MeshChannel, List<int>> channelsToBaseTex, Dictionary<int, int> baseTexToMask, Dictionary<int, List<int>> maskToIndices)
        {
            foreach(var channelKvp in channelsToBaseTex)
            {
                var channelWeights = weights[channelMap[channelKvp.Key]];
                for(int i = 0; i < channelWeights.Length; i++)
                {
                    var vertexWeight = channelWeights[i];
                    foreach(var baseTexIndex in channelKvp.Value)
                    {
                        if(!baseTexToMask.ContainsKey(baseTexIndex))
                            continue;
                        // Calculate the weight out of one already used for this mask,
                        // and set the weight of the base texture to the remainder.
                        // e.g. if all textures are on the same mask, the base texture is at
                        //      index 0 and the vector looks like: (0, 0.2, 0.3, 0.4).
                        //      Then 1 - (0.2 + 0.3 + 0.4) = 0.1, and the vector becomes: (0.1, 0.2, 0.3, 0.4).
                        float value = 0f;
                        List<int> indices;
                        if(maskToIndices.TryGetValue(baseTexToMask[baseTexIndex], out indices))
                        {
                            foreach(var ind in indices)
                            {
                                value += vertexWeight[ind];
                            }
                        }

                        vertexWeight[baseTexIndex] = 1 - value;
                    }

                    channelWeights[i] = vertexWeight;
                }
                weights[channelMap[channelKvp.Key]] = channelWeights;
            }
        }

        /// <summary>
        /// Initialize a SplatSet with mesh and attribute layout.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="attributes"></param>
        internal SplatSet(PolyMesh mesh, AttributeLayout[] attributes) : this(mesh.vertexCount, attributes, false)
		{
			foreach(var kvp in channelMap)
			{
				switch(kvp.Key)
				{
					case MeshChannel.UV0:
					case MeshChannel.UV2:
					case MeshChannel.UV3:
					case MeshChannel.UV4:
					{
						List<Vector4> uv = mesh.GetUVs( MeshChannelUtility.UVChannelToIndex(kvp.Key) );
						weights[kvp.Value] = uv.Count == weightCount ? uv.ToArray() : new Vector4[weightCount];
					}
					break;

					case MeshChannel.Color:
					{
						Color[] color = mesh.colors;
						weights[kvp.Value] = color != null && color.Length == weightCount ? System.Array.ConvertAll(color, x => (Vector4)x ) : new Vector4[weightCount];
					}
					break;

					case MeshChannel.Tangent:
					{
						Vector4[] tangent = mesh.tangents;
						weights[kvp.Value] = tangent != null && tangent.Length == weightCount ? tangent : new Vector4[weightCount];
					}
					break;
				}
			}
		}

        /// <summary>
        /// Get the default weights for each channel (the minimum)
        /// </summary>
        /// <returns></returns>
		internal SplatWeight GetMinWeights()
		{
			SplatWeight min = new SplatWeight(channelMap);

			foreach(AttributeLayout al in attributeLayout)
			{
				Vector4 v = min[al.channel];
				v[(int)al.index] = al.min;
				min[al.channel] = v;
			}

			return min;
		}

        /// <summary>
        /// Lerp each attribute value with matching `mask` to `rhs`.
        /// weights, lhs, and rhs must have matching layout attributes.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <param name="mask"></param>
        /// <param name="strength"></param>
        internal void LerpWeights(SplatSet lhs, SplatSet rhs, int mask, float[] strength)
		{
			Dictionary<int, uint> affected = new Dictionary<int, uint>();

			foreach(AttributeLayout al in attributeLayout)
			{
				int mapIndex = channelMap[al.channel];

				if(al.mask == mask)
				{
					if(!affected.ContainsKey(mapIndex))
						affected.Add(mapIndex, al.index.ToFlag());
					else
						affected[mapIndex] |= al.index.ToFlag();
				}
			}

			foreach(var v in affected)
			{
				Vector4[] a = lhs.weights[v.Key];
				Vector4[] b = rhs.weights[v.Key];
				Vector4[] c = weights[v.Key];

				for(int i = 0; i < weightCount; i++)
				{
					if((v.Value & 1) != 0) c[i].x = Mathf.Lerp(a[i].x, b[i].x, strength[i]);
					if((v.Value & 2) != 0) c[i].y = Mathf.Lerp(a[i].y, b[i].y, strength[i]);
					if((v.Value & 4) != 0) c[i].z = Mathf.Lerp(a[i].z, b[i].z, strength[i]);
					if((v.Value & 8) != 0) c[i].w = Mathf.Lerp(a[i].w, b[i].w, strength[i]);
				}
			}
		}

        /// <summary>
        /// Lerp weights between lhs and rhs
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <param name="strength"></param>
		internal void LerpWeights(SplatSet lhs, SplatWeight rhs, float strength)
		{
			for(int i = 0; i < weightCount; i++)
			{
				foreach(var cm in channelMap)
					this.weights[cm.Value][i] = Vector4.LerpUnclamped(lhs.weights[cm.Value][i], rhs[cm.Key], strength);
			}
		}

        /// <summary>
        /// Lerp weights between lhs and rhs for value at the given index.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <param name="strength"></param>
        /// <param name="index"></param>
        internal void LerpWeightOnSingleChannel(SplatSet lhs, SplatWeight rhs, float strength, MeshChannel channel, int index, int baseTexIndex)
        {
            if(!channelMap.ContainsKey(channel))
                return;

            var channelIndex = channelMap[channel];
            for (int i = 0; i < weightCount; i++)
            {
                float lerpedValue = Mathf.LerpUnclamped(lhs.weights[channelIndex][i][index], rhs[channel][index], strength);

                // replace the original value at index with the lerped value
                var newWeightVector = lhs.weights[channelIndex][i];
                newWeightVector[index] = lerpedValue;

                if(baseTexIndex > -1)
                {
                    newWeightVector[baseTexIndex] += (lhs.weights[channelIndex][i][index] - newWeightVector[index]);
                }

                this.weights[channelIndex][i] = newWeightVector;
            }
        }

        /// <summary>
        /// Copy values to another SplatSet
        /// </summary>
        /// <param name="other">The other SplatSet we want to copy our values to</param>
		internal void CopyTo(SplatSet other)
		{
			if(other.weightCount != weightCount)
			{
				Debug.LogError("Copying splat set to mis-matched container length");
				return;
			}

			for(int i = 0; i < channelMap.Count; i++)
				System.Array.Copy(this.weights[i], other.weights[i], weightCount);
		}


        /// <summary>
        /// Apply the weights to "mesh"
        /// </summary>
        /// <param name="mesh">The mesh we want to apply weights to</param>
		internal void Apply(PolyMesh mesh)
		{
			foreach(AttributeLayout al in attributeLayout)
			{
				switch(al.channel)
				{
					case MeshChannel.UV0:
					case MeshChannel.UV2:
					case MeshChannel.UV3:
					case MeshChannel.UV4:
					{
						List<Vector4> uv = new List<Vector4>(weights[channelMap[al.channel]]);
						mesh.SetUVs(MeshChannelUtility.UVChannelToIndex(al.channel), uv);
					}
					break;

					case MeshChannel.Color:
					{
						// @todo consider storing Color array separate from Vec4 since this cast costs ~5ms
						mesh.colors = System.Array.ConvertAll(weights[channelMap[al.channel]], x => (Color)x);
						break;
					}

					case MeshChannel.Tangent:
					{
						mesh.tangents = weights[channelMap[MeshChannel.Tangent]];
						break;
					}
				}
			}
		}

		public override string ToString()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			foreach(AttributeLayout al in attributeLayout)
				sb.AppendLine(al.ToString());

			sb.AppendLine("--");

			for(int i = 0; i < weightCount; i++)
				sb.AppendLine(weights[i].ToString());

			return sb.ToString();
		}
	}
}
