using System;

namespace WebApp.Service
{
    public interface IProgressReporterQuery
    {
        public IProgress<ProgressEventData>? Progress { get; set; }
    }
}
