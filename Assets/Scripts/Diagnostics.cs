using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Diagnostics
{
    public static Dictionary<string, Stopwatch> Watches;

    public static void Init() {
        if (Watches == null) Watches = new Dictionary<string, Stopwatch>();
    }

    public static void CountStart(string name) {
        Init();
        Watches[name] = new Stopwatch();
        Watches[name].Start();
    }

    public static double CountEnd(string name) {
        Init();
        if (Watches.ContainsKey(name)) {
            Watches[name].Stop();
            return WatchTime(name);
        } else {
            throw new System.Exception("No such watch");
        }
    }

    public static double WatchTime(string name) {
        if (Watches.ContainsKey(name)) {
            return Watches[name].Elapsed.TotalSeconds;
        } else {
            throw new System.Exception("No such watch");
        }
    }

    public static double WatchAction(System.Action action) {
        string name = "_" + Random.Range(0f, 10000f).ToString();
        CountStart(name);
        action.Invoke();
        return CountEnd(name);
    }
}
