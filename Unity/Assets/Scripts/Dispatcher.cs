using System;
using System.Collections.Generic;
using UnityEngine;

public class Dispatcher : MonoBehaviour
{
    private static Dispatcher instance;

    private List<Action> pending = new List<Action>();

    public static Dispatcher Instance
    {
        get
        {
            return instance;
        }
    }

    public void Invoke(Action fn)
    {
        lock (this.pending)
        {
            this.pending.Add(fn);
        }
    }

    private void InvokePending()
    {
        lock (this.pending)
        {
            foreach (Action action in this.pending)
            {
                action();
            }

            this.pending.Clear();
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
    }

    private void Update()
    {
        this.InvokePending();
    }
}