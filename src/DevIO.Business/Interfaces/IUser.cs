using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace DevIO.Business.Interfaces
{
    public  interface IUser
    {
        string Name { get; }

        string GetUserEmail();

        Guid GetUserId();

        bool IsAuthenticated();

        bool IsInRole(string role);

        IEnumerable<Claim> GetClaimsIdentity();
    }
}
