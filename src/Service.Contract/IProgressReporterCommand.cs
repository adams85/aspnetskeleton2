using System;

namespace WebApp.Service
{
    public interface IProgressReporterCommand
    {
        public IProgress<ProgressEventData>? Progress { get; set; }
    }
}
