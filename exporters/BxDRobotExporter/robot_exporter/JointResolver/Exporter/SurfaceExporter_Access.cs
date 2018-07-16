using Inventor;
using System.IO;
using System;
using System.Collections.Generic;
using System.Threading;

public partial class SurfaceExporter
{
    private const int MAX_WAITING_EVENTS = 64;

    /// <summary>
    /// Exports all the components in this group to the in-RAM mesh.
    /// </summary>
    /// <param name="group">Group to export from</param>
    /// <param name="reporter">Progress reporter</param>
    public BXDAMesh ExportAll(CustomRigidGroup group, Guid guid, BXDIO.ProgressReporter reporter = null)
    {
        // Create output mesh
        BXDAMesh outputMesh = new BXDAMesh(guid);

        // Collect faces to export
        List<SurfaceBody> plannedSurfaces = GenerateExportList(group, outputMesh);

        // Export faces, multithreaded
        reporter?.Invoke(0, plannedSurfaces.Count);

        ManualResetEvent[] doneEvents = new ManualResetEvent[MAX_WAITING_EVENTS];
        ExportJob[] jobs = new ExportJob[plannedSurfaces.Count];

        // Create wait events
        for (int i = 0; i < MAX_WAITING_EVENTS; i++)
            doneEvents[i] = new ManualResetEvent(true); // Start with all events triggered to fill up event space

        // Start jobs
        for (int i = 0; i < plannedSurfaces.Count; i++)
        {
            int waitSlot = WaitHandle.WaitAny(doneEvents); // Get next available done event handle
            doneEvents[waitSlot].Reset(); // Reset the event

            jobs[i] = new ExportJob(plannedSurfaces[i], outputMesh, false, SynthesisGUI.PluginSettings.GeneralUseFancyColors);

            ThreadPool.QueueUserWorkItem(jobs[i].ThreadPoolCallback, new ExportJob.JobContext { doneEvent = doneEvents[waitSlot] }); // Add the job to the queue
            reporter?.Invoke((i + 1), plannedSurfaces.Count);
        }

        // Wait for all jobs to finish
        WaitHandle.WaitAll(doneEvents);

        return outputMesh;
    }
}