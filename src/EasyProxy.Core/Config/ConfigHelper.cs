using Dapper;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace EasyProxy.Core.Config
{
    public class ConfigHelper
    {
        private async Task<IDbConnection> GetConnectionAsync()
        {
            var sb = new SqliteConnectionStringBuilder
            {
                DataSource = @"easyproxy.db"
            };
            var conn = new SQLiteConnection(sb.ToString());
            await conn.OpenAsync();
            return conn;
        }

        public async Task AddClientAsync(ClientConfig client)
        {
            var conn = await GetConnectionAsync();

            await conn.ExecuteAsync("Insert into Client values(?,@Name,@SecretKey);", client);

            client.ClientId = await GetInsertedClientIdAsync(conn);
        }

        public async Task RemoveClientAsync(int clientId)
        {
            var conn = await GetConnectionAsync();

            await conn.ExecuteAsync("Delete from Client where ClientId=@clientId", new { clientId });
        }

        public async Task UpdateClientAsync(ClientConfig client)
        {
            var conn = await GetConnectionAsync();

            await conn.ExecuteAsync("UPDATE Client SET Name=@Name,SecretKey=@SecretKey WHERE ClientId = @ClientId", client);
        }

        public async Task<List<ClientConfig>> GetAllClientAsync()
        {
            var conn = await GetConnectionAsync();
            var list = (await conn.QueryAsync<ClientConfig>("Select * From Client", new { })).AsList();

            foreach (var client in list)
            {
                client.Channels = await GetChannelsAsync(client.ClientId);
            }
            return list;
        }

        public async Task<ClientConfig> GetClientAsync(int clientId)
        {
            var conn = await GetConnectionAsync();
            var client = await conn.QueryFirstOrDefaultAsync<ClientConfig>("Select * From Client Where clientId=@clientId", new { clientId });
            if (client != null)
            {
                client.Channels = await GetChannelsAsync(client.ClientId);
            }
            return client;
        }

        public async Task AddChannelAsync(ChannelConfig channel)
        {
            var conn = await GetConnectionAsync();

            await conn.ExecuteAsync("INSERT INTO Channel(ClientId,Name,FrontendPort,BackendPort,FrontendIp) VALUES (@ClientId,@Name,@FrontendPort,@BackendPort,@FrontendIp)", channel);

            channel.ChannelId = await GetInsertedChannelIdAsync(conn);
        }

        public async Task RemoveChannelAsync(int channelId)
        {
            var conn = await GetConnectionAsync();

            await conn.ExecuteAsync("Delete from Channel where ChannelId=@channelId", new { channelId });
        }

        public async Task UpdateChannelAsync(ChannelConfig channel)
        {
            var conn = await GetConnectionAsync();

            await conn.ExecuteAsync("UPDATE Channel SET Name=@Name,BackendPort=@BackendPort,FrontendPort=@FrontendPort,FrontendIp=@FrontendIp WHERE channelId = @ChannelId", channel);
        }

        public async Task<ChannelConfig> GetChannelAsync(int channelId)
        {
            var conn = await GetConnectionAsync();
            return await conn.QueryFirstAsync<ChannelConfig>("Select * From Channel Where ChannelId = @channelId", new { channelId });
        }

        public async Task<List<ChannelConfig>> GetChannelsAsync(int clientId)
        {
            var conn = await GetConnectionAsync();
            return (await conn.QueryAsync<ChannelConfig>("Select * From Channel Where ClientId = @clientId", new { clientId })).AsList();
        }

        private async Task<int> GetInsertedClientIdAsync(IDbConnection conn)
        {
            return (int)(long)await conn.ExecuteScalarAsync("select last_insert_rowid() from Client");
        }

        private async Task<int> GetInsertedChannelIdAsync(IDbConnection conn)
        {
            return (int)(long)await conn.ExecuteScalarAsync("select last_insert_rowid() from Channel");
        }


        public async Task<bool> CheckClientAsync(int clientId, string secretKey)
        {
            var client = await GetClientAsync(clientId);

            if (client == null)
            {
                return false;
            }

            return client.SecretKey == secretKey;
        }
    }
}
