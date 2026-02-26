using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System;

namespace BL_Grid
{
    [Serializable]
    public struct ResourceEntry
    {
        public string Name;
        public Mesh Mesh;
    }

    public class Ore : GridEntity
    {
        public List<ResourceEntry> Resources = new List<ResourceEntry>();
    }
}

