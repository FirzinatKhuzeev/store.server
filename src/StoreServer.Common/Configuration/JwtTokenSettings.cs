namespace StoreServer
{
    using System;

    public class JwtTokenSettings
  {
        public string Key { get; set; }

        public string Issuer { get; set; }

        public string Audience { get; set; }

        public DateTime Expiration => IssuedAt.Add(ValidFor);

        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        public TimeSpan ValidFor { get; set; } = TimeSpan.FromMinutes(60);
    }
}
