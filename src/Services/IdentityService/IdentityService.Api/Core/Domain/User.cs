﻿using System;

namespace IdentityService.Api.Core.Domain
{
    public class User
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}