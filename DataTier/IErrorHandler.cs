using System;

namespace DataTier
{
    public interface IErrorHandler
    {
        void LogError(Exception ex, int UserID, string Page, string Process, string Subject, string SQL);
    }
}
