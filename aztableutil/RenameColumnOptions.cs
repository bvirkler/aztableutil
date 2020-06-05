using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace aztableutil
{
    [Verb("rename-col")]
    internal class RenameColumnOptions
    {
        //[Option("sa-resource-id", Required = true, HelpText = "Storage account resource id")]
        //public string StorageAccountResourceId { get; set; }
        //[Option('t', "table-name", Required = true, HelpText = "The name of the table")]
        //public string TableName { get; set; }
        [Option('s', "sas-token", Group = "auth", HelpText = "Shared Access Signature Token")]
        public string SasToken { get; set; }
        [Option('k', "key", Group = "auth", HelpText = "Storage account key")]
        public string Key { get; set; }
        [Option('d', "dev", Group = "auth", HelpText = "Use local development storage")]
        public bool UseDevStorage { get; set; }
        [Option('u', "table-uri", Required = true, HelpText = "URL of table to update")]
        public Uri TableUri { get; set; }
        [Option('f', "from", Required = true, HelpText = "The current column name")]
        public string From { get; set; }
        [Option('t', "to", Required = true, HelpText = "The name to which the column should be renamed")]
        public string To { get; set; }
    }
}
