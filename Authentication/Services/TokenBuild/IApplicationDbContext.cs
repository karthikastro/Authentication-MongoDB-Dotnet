using System.Collections.Generic;

namespace Authentication.Services.TokenBuild
{

    //public interface IApplicationDbContext
    //{
    //    public DbSet<RefreshToken> RefreshTokens { get; set; }

    //    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    //}
    using System.Threading.Tasks;
    using System.Threading;
    using MongoDB.Driver;
    using MongoDB.Bson.Serialization.Attributes;
    using MongoDB.Bson;

    public interface IApplicationDbContext
    {
        IMongoCollection<RefreshToken> RefreshTokens { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }

    public class RefreshToken
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public Guid UserId { get; set; }
        public string Token { get; set; }

        // Add other properties if needed

        public RefreshToken(Guid userId, string token)
        {
            UserId = userId;
            Token = token;
        }
    }

    // Your implementation of IApplicationDbContext for MongoDB
    public class ApplicationDbContext : IApplicationDbContext
    {
        private readonly IMongoDatabase _database;

        public ApplicationDbContext(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<RefreshToken> RefreshTokens => _database.GetCollection<RefreshToken>("RefreshTokens");

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // MongoDB doesn't have the concept of SaveChangesAsync as in Entity Framework,
            // so you may not need this method for MongoDB. It's included here for consistency.
            // You can decide how to handle this in your application.
            return Task.FromResult(0);
        }
    }


}
