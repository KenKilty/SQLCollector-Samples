# SqlCollector-Samples

Sample application using Azure Functions 3.x to illustrate serverless data collection targeting Azure SQL and SQL Server on Virtual Machines.
Illustrates usage of AzureDefaultCredential in conjunction with Azure Active Directory Managed Identity Authentication for accessing Azure SQL. This is made possible using the enhancements to the dotnet Microsoft.Data.SqlClient 2.1 enhancements.

## Connection String Example

Employing [user assigned managed identity](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/overview#managed-identity-types) with Microsoft.Data.SqlClient v2.1 to managed token aquisition:

`Server=tcp:sqlcollector-contoso.database.windows.net,1433; Authentication=Active Directory Managed Identity; User Id=d3004180-edf4-4a9c-8fb6-19036d8c620b; Initial Catalog=SqlCollectorDb;`

## Reference

- [Azure Active Directory Managed Identity authentication for Azure SQL](https://github.com/dotnet/SqlClient/blob/master/release-notes/2.1/2.1.0.md#azure-active-directory-managed-identity-authentication)

## License

Copyright 2020 Kenneth Kilty

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
