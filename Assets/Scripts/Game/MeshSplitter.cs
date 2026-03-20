using System.Collections.Generic;
using UnityEngine;

public static class MeshSplitter
{
    /// <summary>
    /// Splits a mesh into solid-looking fragments using Voronoi nearest-seed assignment.
    /// Each fragment is made double-sided (reversed-winding duplicates) so cut faces
    /// never show through as paper-thin gaps.
    /// </summary>
    public static Mesh[] Split(Mesh source, int fragments = 8)
    {
        if (source == null || fragments < 2) return null;
        if (!source.isReadable)
        {
            Debug.LogWarning($"MeshSplitter: '{source.name}' is not readable. Enable Read/Write in the mesh import settings.");
            return null;
        }

        source.RecalculateBounds();

        var verts = source.vertices;
        var normals = source.normals;
        var uvs = source.uv;
        bool hasNormals = normals != null && normals.Length == verts.Length;
        bool hasUVs = uvs != null && uvs.Length == verts.Length;
        int subCount = source.subMeshCount;

        Vector3[] seeds = GenerateSeedsFromVertices(verts, fragments, source.name.GetHashCode());

        // Per-fragment data
        var fragVerts = new List<Vector3>[fragments];
        var fragNormals = new List<Vector3>[fragments];
        var fragUVs = new List<Vector2>[fragments];
        var fragSubTris = new List<int>[fragments, subCount];
        var vertexMaps = new Dictionary<int, int>[fragments];

        for (int f = 0; f < fragments; f++)
        {
            fragVerts[f] = new List<Vector3>();
            fragNormals[f] = new List<Vector3>();
            fragUVs[f] = new List<Vector2>();
            vertexMaps[f] = new Dictionary<int, int>();
            for (int sub = 0; sub < subCount; sub++)
                fragSubTris[f, sub] = new List<int>();
        }

        // Assign each triangle to the nearest seed based on centroid
        for (int sub = 0; sub < subCount; sub++)
        {
            var tris = source.GetTriangles(sub);
            for (int t = 0; t < tris.Length; t += 3)
            {
                int i0 = tris[t], i1 = tris[t + 1], i2 = tris[t + 2];
                Vector3 centroid = (verts[i0] + verts[i1] + verts[i2]) / 3f;
                int nearest = FindNearestSeed(centroid, seeds);

                int r0 = RemapVertex(nearest, i0, verts, normals, uvs, hasNormals, hasUVs, fragVerts, fragNormals, fragUVs, vertexMaps);
                int r1 = RemapVertex(nearest, i1, verts, normals, uvs, hasNormals, hasUVs, fragVerts, fragNormals, fragUVs, vertexMaps);
                int r2 = RemapVertex(nearest, i2, verts, normals, uvs, hasNormals, hasUVs, fragVerts, fragNormals, fragUVs, vertexMaps);

                fragSubTris[nearest, sub].Add(r0);
                fragSubTris[nearest, sub].Add(r1);
                fragSubTris[nearest, sub].Add(r2);
            }
        }

        // Build meshes — make each double-sided so cut faces look solid
        var result = new List<Mesh>(fragments);
        for (int f = 0; f < fragments; f++)
        {
            if (fragVerts[f].Count == 0) continue;

            // Duplicate all verts with flipped normals for the back side
            int frontCount = fragVerts[f].Count;
            for (int v = 0; v < frontCount; v++)
            {
                fragVerts[f].Add(fragVerts[f][v]);
                if (hasNormals) fragNormals[f].Add(-fragNormals[f][v]);
                if (hasUVs) fragUVs[f].Add(fragUVs[f][v]);
            }

            // Add reversed-winding triangles referencing the back-side verts
            for (int sub = 0; sub < subCount; sub++)
            {
                var tris = fragSubTris[f, sub];
                int triCount = tris.Count;
                for (int t = 0; t < triCount; t += 3)
                {
                    tris.Add(tris[t + 2] + frontCount);
                    tris.Add(tris[t + 1] + frontCount);
                    tris.Add(tris[t]     + frontCount);
                }
            }

            var m = new Mesh { name = $"{source.name}_frag{f}" };
            m.SetVertices(fragVerts[f]);
            if (hasNormals) m.SetNormals(fragNormals[f]);
            if (hasUVs) m.SetUVs(0, fragUVs[f]);
            m.subMeshCount = subCount;
            for (int sub = 0; sub < subCount; sub++)
                m.SetTriangles(fragSubTris[f, sub], sub);
            if (!hasNormals) m.RecalculateNormals();
            m.RecalculateBounds();
            result.Add(m);
        }

        return result.ToArray();
    }

    static Vector3[] GenerateSeedsFromVertices(Vector3[] verts, int count, int hashSeed)
    {
        if (verts.Length <= count)
        {
            var all = new Vector3[verts.Length];
            System.Array.Copy(verts, all, verts.Length);
            return all;
        }

        var seeds = new Vector3[count];
        var minDist = new float[verts.Length];
        for (int i = 0; i < minDist.Length; i++) minDist[i] = float.MaxValue;

        uint state = (uint)(hashSeed ^ 0x5A5A5A5A);
        state = XorShift(state);
        int firstIdx = (int)(state % (uint)verts.Length);
        seeds[0] = verts[firstIdx];

        for (int v = 0; v < verts.Length; v++)
            minDist[v] = (verts[v] - seeds[0]).sqrMagnitude;

        for (int s = 1; s < count; s++)
        {
            int farthest = 0;
            float bestDist = -1f;
            for (int v = 0; v < verts.Length; v++)
            {
                if (minDist[v] > bestDist)
                {
                    bestDist = minDist[v];
                    farthest = v;
                }
            }
            seeds[s] = verts[farthest];

            for (int v = 0; v < verts.Length; v++)
            {
                float d = (verts[v] - seeds[s]).sqrMagnitude;
                if (d < minDist[v]) minDist[v] = d;
            }
        }

        return seeds;
    }

    static uint XorShift(uint s)
    {
        s ^= s << 13;
        s ^= s >> 17;
        s ^= s << 5;
        return s;
    }

    static int FindNearestSeed(Vector3 point, Vector3[] seeds)
    {
        int best = 0;
        float bestDist = float.MaxValue;
        for (int i = 0; i < seeds.Length; i++)
        {
            float d = (point - seeds[i]).sqrMagnitude;
            if (d < bestDist) { bestDist = d; best = i; }
        }
        return best;
    }

    static int RemapVertex(int fragment, int origIdx,
        Vector3[] verts, Vector3[] normals, Vector2[] uvs,
        bool hasNormals, bool hasUVs,
        List<Vector3>[] fVerts, List<Vector3>[] fNormals, List<Vector2>[] fUVs,
        Dictionary<int, int>[] maps)
    {
        if (!maps[fragment].TryGetValue(origIdx, out int newIdx))
        {
            newIdx = fVerts[fragment].Count;
            maps[fragment][origIdx] = newIdx;
            fVerts[fragment].Add(verts[origIdx]);
            if (hasNormals) fNormals[fragment].Add(normals[origIdx]);
            if (hasUVs) fUVs[fragment].Add(uvs[origIdx]);
        }
        return newIdx;
    }
}
