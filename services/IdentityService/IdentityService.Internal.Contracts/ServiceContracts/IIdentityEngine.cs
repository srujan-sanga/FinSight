using FinSight.Contracts;
using IdentityService.Internal.Contracts.DataContracts.Entities;
using IdentityService.Internal.Contracts.DataContracts.Responses;

namespace IdentityService.Internal.Contracts.ServiceContracts;

public interface IIdentityEngine : IEngine
{
      public  Task<DbUser?> VerifyUserCredentialsAsync(string username, string rawPassword, CancellationToken cancellationToken);

}
