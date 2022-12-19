using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[Serializable]
public class CutsceneData
{
    public bool finished = false;
    public Action finish;

    public virtual void onLoad()
    {

    }

    public virtual void abort()
    {

    }

    public virtual void startSegment()
    {
        finished = false;
    }

    public virtual void finishedSegment()
    {
        finished = true;
        finish?.Invoke();
    }

    public CutsceneData()
    {

    }
}

public class CutsceneNPCDialog : CutsceneData
{
    [SerializeField]
    public DialogNPC npc;
    public GameObject interactor;

    public CutsceneNPCDialog(DialogNPC npc, GameObject user)
    {
        this.npc = npc;
        interactor = user;
    }

    public CutsceneNPCDialog()
    {

    }
    public override void onLoad()
    {
        npc.closedDialog += finishedSegment;
    }

    public override void startSegment()
    {
        base.startSegment();
        npc.interact(interactor);
    }

    public override void abort()
    {
        base.abort();
        npc.exitDialog();
    }

}

public class SimultaneousCustscene : CutsceneData
{
    private int _numCompleted;

    public int numCompleted
    {
        get { return _numCompleted; }
        set
        {
            if (numCompleted >= cutscenes.Length && !finished)
                finishedSegment();
        }
    }

    public CutsceneData[] cutscenes;
    public SimultaneousCustscene(CutsceneData[] cutscenes)
    {
        this.cutscenes = cutscenes;
    }

    public SimultaneousCustscene()
    {

    }

    public override void startSegment()
    {
        base.startSegment();
        numCompleted = 0;

        foreach (CutsceneData data in cutscenes)
        {
            data.startSegment();
        }
        
    }

    public override void onLoad()
    {
        base.onLoad();

        foreach(CutsceneData data in cutscenes)
        {
            data.finish += (() => numCompleted++);
        }
    }
}

public class WaitCutscene : CutsceneData
{
    public float waitTime;

    public WaitCutscene(float time)
    {
        waitTime = time;
    }

    public WaitCutscene()
    {

    }

    public override void startSegment()
    {
        base.startSegment();
        wait();
    }

    public IEnumerator wait()
    {
        yield return new WaitForSeconds(waitTime);
        finishedSegment();
    }
}

public class InterruptibleCutscenes : CutsceneData
{
    public CutsceneData[] cutscenes;

    public InterruptibleCutscenes(CutsceneData[] cutscenes)
    {
        this.cutscenes = cutscenes;
    }

    public InterruptibleCutscenes()
    {

    }

    public override void onLoad()
    {
        base.onLoad();

        foreach (CutsceneData data in cutscenes)
        {
            data.finish += (() => finishedSegment());
        }
    }

    public override void finishedSegment()
    {
        base.finishedSegment();

        foreach(CutsceneData data in cutscenes)
        {
            data.abort();
        }
    }

}