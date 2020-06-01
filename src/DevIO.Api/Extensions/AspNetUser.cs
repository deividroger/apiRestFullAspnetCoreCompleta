using DevIO.Business.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace DevIO.Api.Extensions
{
    public class AspNetUser : IUser
    {
        private readonly IHttpContextAccessor _accessor;

        public AspNetUser(IHttpContextAccessor accessor) => _accessor = accessor;
        

        public string Name => _accessor.HttpContext.User.Identity.Name;

        public IEnumerable<Claim> GetClaimsIdentity()
        {
            return _accessor.HttpContext.User.Claims;
        }

        public string GetUserEmail()
        {
            return IsAuthenticated() ? _accessor.HttpContext.User.GetUserEmail() : "";
        }

        public Guid GetUserId()
        {
            return IsAuthenticated() ? Guid.Parse(_accessor.HttpContext.User.GetUserId()) : Guid.NewGuid();
        }

        public bool IsAuthenticated()
        {
          return  _accessor.HttpContext.User.Identity.IsAuthenticated;
        }

        public bool IsInRole(string role)
        {
            return _accessor.HttpContext.User.IsInRole(role);
        }
    }

    public static class ClaimsPrincipalExtensions
    {
        public static string GetUserId(this ClaimsPrincipal principal)
        {
            return GetValueClaimType(principal, ClaimTypes.NameIdentifier);
        }

        public static string GetUserEmail(this ClaimsPrincipal principal)
        {
            return GetValueClaimType(principal, ClaimTypes.Email);
        }

        private static string GetValueClaimType(ClaimsPrincipal principal, string claimType)
        {
            if (principal == null)
            {
                throw new ArgumentException(nameof(principal));
            }

            var claim = principal.FindFirst(claimType);

            return claim?.Value;
        }
    }

}
