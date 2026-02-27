using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System;

namespace GameLogic
{
    [Serializable]
    public struct ResourceEntry
    {
        public string Name;
        public Mesh Mesh;
    }

    public class Ore : GPUEntity
    {
        public List<ResourceEntry> Resources = new List<ResourceEntry>();
    }
}

