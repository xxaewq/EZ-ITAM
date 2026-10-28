namespace EzItam.Api.Services
{
    public interface IEmployeeService
    {
        public Task<int> SyncEmployeesToTempTableAsync(List<HrEmployeeDto> hrEmployees, CancellationToken cancellationToken = default);
    }



    public class EmployeeService(AppDbContext db) : IEmployeeService
    {
        private readonly AppDbContext _db = db;

        public Task<int> SyncEmployeesToTempTableAsync(List<HrEmployeeDto> hrEmployees, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
