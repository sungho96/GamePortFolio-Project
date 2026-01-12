using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TFT/Wave Table", fileName = "WaveTable")]
public class WaveTableSo : ScriptableObject
{
    [Serializable]
    public class SpawnEntry
    {
        public UnitData unit;
        public int count = 1;
    }

    [Serializable]
    public class Wave
    {
        public int round = 1;
        public List<SpawnEntry> spawns = new List<SpawnEntry>();
    }

    public List<Wave> waves = new List<Wave>();
    public Wave GetWave(int round)
    {
        Wave last = null;
        foreach (var w in waves)
        {
            if (w == null) continue;
            if (w.round == round) return w;
            if (w.round <= round) last = w;
        }
        return last;
    }
}

