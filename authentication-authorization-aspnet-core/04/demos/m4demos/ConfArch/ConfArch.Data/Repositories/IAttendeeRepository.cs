using System.Threading.Tasks;
using ConfArch.Data.Models;

namespace ConfArch.Data.Repositories
{
    public interface IAttendeeRepository
    {
        Task<int> Add(AttendeeModel attendee);
    }
}