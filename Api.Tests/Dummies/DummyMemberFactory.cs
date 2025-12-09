using API.Entities;
using System;

namespace API.Tests.Dummies
{
    public static class DummyMemberFactory
    {
        /// <summary>
        /// Creates a dummy Member with a linked AppUser.
        /// Use optional parameters to override defaults.
        /// </summary>
        public static Member Create(
            string? id = null,
            string displayName = "Test User",
            string gender = "male",
            string city = "Test City",
            string country = "Test Country",
            string? description = "Test Description",
            DateOnly? dateOfBirth = null,
            string? imageUrl = null,
            bool includePhotos = true)
        {
            var memberId = id ?? Guid.NewGuid().ToString();

            var user = new AppUser
            {
                Id = memberId,
                DisplayName = displayName,
                Email = $"{displayName.Replace(" ", "").ToLower()}@example.com",
                PasswordHash = new byte[] { 1, 2, 3 },
                PasswordSalt = new byte[] { 4, 5, 6 }
            };

            var member = new Member
            {
                Id = memberId,
                DisplayName = displayName,
                Gender = gender,
                City = city,
                Country = country,
                Description = description,
                DateOfBirth = dateOfBirth ?? new DateOnly(1990, 1, 1),
                ImageUrl = imageUrl,
                User = user
            };

            user.Member = member;

            if (includePhotos)
            {
                member.Photos.Add(new Photo { Url = "/photos/test1.jpg" });
                member.Photos.Add(new Photo { Url = "/photos/test2.jpg" });
            }

            return member;
        }
    }
}
