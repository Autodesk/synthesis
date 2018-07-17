using Inventor;
using System.IO;
using System;
using System.Collections.Generic;
using System.Threading;

public partial class SurfaceExporter
{
    public SurfaceExporter()
    {
        ExportJob.ResetAssets();
    }

    private const int MAX_WAITING_JOBS = 32;

    /// <summary>
    /// Exports all the components in this group to the in-RAM mesh.
    /// </summary>
    /// <param name="group">Group to export from</param>
    /// <param name="reporter">Progress reporter</param>
    public BXDAMesh ExportAll(CustomRigidGroup group, Guid guid, BXDIO.ProgressReporter reporter = null)
    {
        // Create output mesh
        MeshController outputMesh = new MeshController(guid);

        // Collect faces to export
        List<SurfaceBody> plannedSurfaces = GenerateExportList(group, outputMesh.Mesh);

        // Export faces, multithreaded
        reporter?.Invoke(0, plannedSurfaces.Count);

        ManualResetEvent[] doneEvents = new ManualResetEvent[MAX_WAITING_JOBS];
        ExportJob[] jobs = new ExportJob[plannedSurfaces.Count];

        // Create wait events
        for (int i = 0; i < MAX_WAITING_JOBS; i++)
            doneEvents[i] = new ManualResetEvent(true); // Start with all events triggered to fill up event space

        // Start jobs
        int totalJobsFinished = 0;
        object finishLock = new object(); // Used to prevent multiple threads from updating progress bar at the same time.

        for (int i = 0; i < plannedSurfaces.Count; i++)
        {
            int waitSlot = WaitHandle.WaitAny(doneEvents); // Get next available done event handle
            doneEvents[waitSlot].Reset(); // Reset the event

            jobs[i] = new ExportJob(plannedSurfaces[i], outputMesh, SynthesisGUI.PluginSettings.GeneralUseFancyColors);

            // Add the job to the queue
            ThreadPool.QueueUserWorkItem(jobs[i].ThreadPoolCallback, new ExportJob.JobContext { doneEvent = doneEvents[waitSlot], onFinish = () =>
                {
                    // Update the progress bar
                    lock (finishLock)
                    {
                        totalJobsFinished++;
                        reporter?.Invoke(totalJobsFinished, plannedSurfaces.Count);
                    }
                }
            });
        }

        // Wait for all jobs to finish
        WaitHandle.WaitAll(doneEvents);

        // Check for errors
        foreach (ExportJob job in jobs)
            if (job.error != null)
                throw job.error;

        outputMesh.DumpOutput();
        return outputMesh.Mesh;
    }
}