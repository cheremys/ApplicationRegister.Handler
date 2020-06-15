using ApplicationRegister.Handler.Entities;
using ApplicationRegister.Handler.Interfaces;
using Microsoft.Data.SqlClient;
using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace ApplicationRegister.Handler.Repositories
{
    public class ApplicationRepository : IApplicationRepository
    {
        private const string applicationsSelectProcedure = "dbo.ApplicationsSelect";
        private const string applicationInsertProcedure = "dbo.ApplicationInsert";
        private readonly string connectionString;

        public ApplicationRepository(IConfigurationRoot config)
        {
            connectionString = config.GetSection("connectionString").Get<string>();
        }

        public int? CreateApplication(Application application)
        {
            int? id = null;
            IDbConnection db = new SqlConnection(connectionString);

            var value = new
            {
                ClientId = application.ClientId,
                Address = application.DepartmentAddress,
                Amount = application.Amount,
                Currency = application.Currency,
                Ip = application.Currency
            };


            try
            {
                var result = db.Query(applicationInsertProcedure, value, commandType: CommandType.StoredProcedure).FirstOrDefault();

                id = (int)((result != null && result?.Id == null) ? null : result?.Id);
            }
            catch (SqlException exception)
            {
                throw exception;
            }
            catch (Exception exception)
            {
                throw exception;
            }
            finally
            {
                db.Dispose();
            }

            return id;
        }

        public IEnumerable<Application> GetApplications(int clientId, string address = null)
        {
            IDbConnection db = new SqlConnection(connectionString);
            var values = new { ClientId = clientId, Address = address };
            List<Application> applications = null;

            try
            {
                var results = db.Query(applicationsSelectProcedure, values, commandType: CommandType.StoredProcedure).ToList();

                applications = new List<Application>();

                results.ForEach(application =>
                   applications.Add(
                       new Application
                       {
                           Id = application.Id,
                           ClientId = application.ClientId,
                           DepartmentAddress = application.DepartmentAddress,
                           Amount = application.Amount,
                           Currency = application.Currency,
                       })
                );
            }
            catch (SqlException exception)
            {
                throw exception;
            }
            catch (Exception exception)
            {
                throw exception;
            }
            finally
            {
                db.Dispose();
            }

            return applications;
        }
    }
}
