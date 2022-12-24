using System;

namespace IdentityService.Api.Application.Services.Responses
{
    public class UserResponse
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
