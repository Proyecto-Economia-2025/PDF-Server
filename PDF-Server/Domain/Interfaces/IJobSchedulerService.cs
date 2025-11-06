using System.Threading.Tasks;
using PDF_Server.Domain.Models;

namespace PDF_Server.Domain.Interfaces
{
    public interface IJobSchedulerService
    {
        Task<bool> ScheduleNotificationsAsync(NotificationJobRequest request);
    }
}