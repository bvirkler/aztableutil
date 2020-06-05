using System;
using CommandLine;
using System.IO;
using Azure.Identity;
using Microsoft.Azure.Cosmos.Table;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using System.Collections.Generic;

namespace aztableutil
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Parser.Default.ParseArguments<RenameColumnOptions>(args)
                .MapResult(
                    (RenameColumnOptions opts) => RenameTableColumnAsync(opts),
                    errs => Task.FromResult(1));
        }

        private static async Task<int> RenameTableColumnAsync(RenameColumnOptions options)
        {
            // get table name
            string tableName = options.TableUri.AbsolutePath.Substring(1);
            Console.WriteLine($"Renaming {options.From} to {options.To} on table {options.TableUri}.");

            // build credentials
            CloudTableClient client = GetTableClient(options);

            var table = client.GetTableReference(tableName);
            if (!await table.ExistsAsync())
            {
                Console.WriteLine($"Table {tableName} does not exist.");
                return 1;
            }

            string updateMessage = "Updating entities, this could take a while.";
            Console.Write(updateMessage);
            long updateCount = 0;

            TableContinuationToken token = null;
            do
            {
                var queryResult = await table.ExecuteQuerySegmentedAsync(new TableQuery<DynamicTableEntity>(), token);
                token = queryResult.ContinuationToken;

                // move value to new attribute name
                var entities = queryResult.Results;
                var entitiesToUpdate = new List<DynamicTableEntity>();
                foreach (var entity in entities)
                {
                    if (entity.Properties.ContainsKey(options.From))
                    {
                        // TODO: check if options.To already exists

                        entity[options.To] = entity[options.From];
                        //entity[options.From] = null;
                        entity.Properties.Remove(options.From);
                        entitiesToUpdate.Add(entity);
                    }
                }

                // apply updates
                // group by partition
                var partitionGroups = entitiesToUpdate.ToLookup(e => e.PartitionKey, e => e);
                foreach (var partitionGroup in partitionGroups)
                {
                    var batch = new TableBatchOperation();
                    foreach (var entity in partitionGroup)
                    {
                        batch.Add(TableOperation.Replace(entity));

                        // TODO: check batch size
                        // limit: "total payload may be no more than 4 MB in size" https://docs.microsoft.com/en-us/rest/api/storageservices/performing-entity-group-transactions
                        // estimate: https://docs.microsoft.com/en-us/archive/blogs/avkashchauhan/how-the-size-of-an-entity-is-caclulated-in-windows-azure-table-storage
                        if (batch.Count == 100)
                        {
                            bool success = await ExecuteBatchAsync(table, batch);
                            updateCount += batch.Count;
                            Console.Write($"\r{updateMessage} {updateCount} rows updated.");
                            batch.Clear();
                        }
                    }

                    if (batch.Count > 0)
                    {
                        bool success = await ExecuteBatchAsync(table, batch);
                        updateCount += batch.Count;
                        Console.Write($"\r{updateMessage} {updateCount} rows updated.");
                    }
                }
            } while (token != null);

            Console.WriteLine("\n\nDone!");
            return 0;
        }

        private static CloudTableClient GetTableClient(RenameColumnOptions options)
        {
            if (options.UseDevStorage)
            {
                return CloudStorageAccount.DevelopmentStorageAccount.CreateCloudTableClient();
            }
            else
            {
                StorageCredentials credentials;
                if (!string.IsNullOrEmpty(options.Key))
                {
                    string host = options.TableUri.Host;
                    string account = host.Substring(0, host.IndexOf('.'));
                    credentials = new StorageCredentials(account, options.Key);
                }
                else
                    credentials = new StorageCredentials(options.SasToken);

                CloudTableClient client = new CloudTableClient(new Uri(options.TableUri.GetLeftPart(UriPartial.Authority)), credentials);
                return client; 
            }
        }

        private static async Task<bool> ExecuteBatchAsync(CloudTable table, TableBatchOperation batch)
        {
            // TODO: 
            var batchResult = await table.ExecuteBatchAsync(batch);
            return batchResult.All(result => result.HttpStatusCode == (int)HttpStatusCode.NoContent);
        }
    }
}
