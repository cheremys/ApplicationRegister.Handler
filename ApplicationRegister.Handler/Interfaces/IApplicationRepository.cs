using ApplicationRegister.Handler.Entities;
using System.Collections.Generic;

namespace ApplicationRegister.Handler.Interfaces
{
    public interface IApplicationRepository
    {
        public IEnumerable<Application> GetApplications(int clientId, string address = null);

        public int? CreateApplication(Application application);
    }
}
