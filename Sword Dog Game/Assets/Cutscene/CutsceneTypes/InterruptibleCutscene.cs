using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InterruptibleCutscene : CutsceneData
{
    public CutsceneData[] cutscenes;

    public InterruptibleCutscene(CutsceneData[] cutscenes)
    {
        this.cutscenes = cutscenes;
    }

    public InterruptibleCutscene()
    {

    }

    public override void cycleExecution()
    {
        base.cycleExecution();
        foreach (CutsceneData data in cutscenes)
            data.cycleExecution();
    }

    public override void abort()
    {
        base.abort();
        foreach (CutsceneData data in cutscenes)
            data.abort();
    }

    public override void startSegment()
    {
        base.startSegment();
        foreach (CutsceneData data in cutscenes)
            data.startSegment();
    }

    public override void Start()
    {
        base.Start();

        foreach (CutsceneData data in cutscenes)
        {
            if(data != null)
                data.finish += (() => finishedSegment());
        }
    }

    public override void finishedSegment()
    {
        base.finishedSegment();

        foreach (CutsceneData data in cutscenes)
        {
            if(data != null)
                data.abort();
        }
    }
}
