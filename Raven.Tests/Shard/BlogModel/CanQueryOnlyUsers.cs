using System.Linq;
using Raven.Abstractions.Extensions;
using Xunit;

namespace Raven.Tests.Shard.BlogModel
{
	public class CanQueryOnlyUsers : ShardingScenario
	{
		[Fact]
		public void WhenQueryingForUserById()
		{
			using (var session = shardedDocumentStore.OpenSession())
			{
				var user = session.Load<User>("users/1");

				Assert.Equal(1, Servers["Users"].Server.NumberOfRequests);
				Assert.Null(user);
				Servers.Where(ravenDbServer => ravenDbServer.Key != "Users")
					.ForEach(server => Assert.Equal(0, server.Value.Server.NumberOfRequests));
			}
		}

		[Fact]
		public void WhenStoringUser()
		{
			using (var session = shardedDocumentStore.OpenSession())
			{
				session.Store(new User { Name = "Fitzchak Yitzchaki" });

				Assert.Equal(1, Servers["Users"].Server.NumberOfRequests);
				Servers.Where(ravenDbServer => ravenDbServer.Key != "Users")
					.ForEach(server => Assert.Equal(0, server.Value.Server.NumberOfRequests));
			}

			using (var session = shardedDocumentStore.OpenSession())
			{
				var user = session.Load<User>("users/1");

				Assert.Equal(2, Servers["Users"].Server.NumberOfRequests);
				Assert.NotNull(user);
				Assert.Equal("Fitzchak Yitzchaki", user.Name);			
				Servers.Where(ravenDbServer => ravenDbServer.Key != "Users")
					.ForEach(server => Assert.Equal(0, server.Value.Server.NumberOfRequests));
			}
		}

		[Fact]
		public void WhenQueryingForUserByName()
		{
			using (var session = shardedDocumentStore.OpenSession())
			{
				var user = session.Query<User>()
					.FirstOrDefault(x => x.Name == "Fitzchak");

				Assert.Equal(1, Servers["Users"].Server.NumberOfRequests);
				Servers.Where(ravenDbServer => ravenDbServer.Key != "Users")
					.ForEach(server => Assert.Equal(0, server.Value.Server.NumberOfRequests));
			}
		}
	}
}