## About
This is a command line utility for Azure Table Storage.

## Commands

### rename-col

Rename a column.  Because Azure Table Storage (ATS) is schemaless, the only way to do this is to update each entity individually.
This command automates that process.

#### Options

```
 -s, --sas-token    (Group: auth) Shared Access Signature Token
 -k, --key          (Group: auth) Storage account key
 -d, --dev          (Group: auth) Use local development storage
 -u, --table-uri    Required. URL of table to update
 -f, --from         Required. The current column name
 -t, --to           Required. The name to which the column should be renamed
 --help             Display this help screen.
 --version          Display version information.
 ```
 
#### Examples

**Development Storage**

`aztableutil rename-col -u https://canbeanythingwhenusing-d.com/MyTable -d -f OldColumnName -t NewColumnName`

`-d` tells it to use the local storage emulator.  When using `-d` the value of `-u` is ignored except for the table name at the end.

**Storage Account Key**

`aztableutil rename-col -u https://yourstorageacct.table.core.windows.net/MyTable -k "storage acct key" -f OldColumnName -t NewColumnName`
