using System;
using System.Collections.Generic;
using UnityEngine;

public interface ITickable
{
    void Tick(float dt, int tick);
}

[DefaultExecutionOrder(-10000)]
public sealed class TickManager : MonoBehaviour
{
    public static TickManager I;

    [Header("Tick")]
    public int tickRate = 60;
    public bool paused = false;

    [Header("Catch-up")]
    public bool catchUp = true;
    public int maxTicksPerFrame = 8;

    [NonSerialized] public int tick;
    [NonSerialized] public float dt;
    [NonSerialized] public float time;

    readonly List<ITickable> list = new();

    //to avoid concurrent modification
    readonly List<ITickable> pendingAdd = new();
    readonly List<ITickable> pendingRemove = new();
    bool iterating;

    float accumulator;

    void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;
        dt = tickRate > 0 ? 1f / tickRate : 1f / 60f;
    }

    void OnEnable()
    {
        accumulator = 0f;
    }

    void Update()
    {
        if (paused) return;

        if (tickRate <= 0) tickRate = 1;
        float targetDt = 1f / tickRate;
        if (!Mathf.Approximately(dt, targetDt)) dt = targetDt;

        accumulator += Time.unscaledDeltaTime;

        int steps = 0;
        while (accumulator >= dt)
        {
            Step();
            accumulator -= dt;

            if (!catchUp) { accumulator = 0f; break; }
            if (++steps >= maxTicksPerFrame) { accumulator = 0f; break; }
        }
    }

    void Step()
    {
        tick++;
        time += dt;

        FlushPending();

        iterating = true;
        for (int i = 0; i < list.Count; i++)
            list[i].Tick(dt, tick);
        iterating = false;

        FlushPending();
    }



    //to avoid concurrent modification
    void FlushPending()
    {
        if (pendingRemove.Count > 0)
        {
            for (int i = 0; i < pendingRemove.Count; i++)
                list.Remove(pendingRemove[i]);
            pendingRemove.Clear();
        }

        if (pendingAdd.Count > 0)
        {
            for (int i = 0; i < pendingAdd.Count; i++)
            {
                var t = pendingAdd[i];
                if (!list.Contains(t)) list.Add(t);
            }
            pendingAdd.Clear();
        }
    }

    public static void Register(ITickable t)
    {
        if (!I || t == null) return;
        if (I.iterating) { if (!I.pendingAdd.Contains(t)) I.pendingAdd.Add(t); }
        else if (!I.list.Contains(t)) I.list.Add(t);
    }

    public static void Unregister(ITickable t)
    {
        if (!I || t == null) return;
        if (I.iterating) { if (!I.pendingRemove.Contains(t)) I.pendingRemove.Add(t); }
        else I.list.Remove(t);
    }

    public static void ResetClock()
    {
        if (!I) return;
        I.tick = 0;
        I.time = 0f;
        I.accumulator = 0f;
    }
}