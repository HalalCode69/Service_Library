README: Setting Up the Database for the MVC Project
Prerequisites
1. Visual Studio with ASP.NET MVC installed.
2. SQL Server Management Studio (SSMS) installed.

Step 1: Restoring the Database from EbookDB2.bak :

1. Open SQL Server Management Studio (SSMS).
2. Connect to the local SQL Server instance.
3. Right-click on Databases in the Object Explorer, then select Restore Database.
4. In the Restore Database window:
 Select Device and click the [...] button to browse for the .bak file.
 Click Add, navigate to the project folder, and select EbookDB2.bak.
 Click OK to confirm.
 
5. In the Destination Database field, enter a name for the database (e.g., EbookDB2).
6. Under the Files section, ensure the logical file names are correct.
7. Click OK to start the restoration process.
8. Once the restoration is complete, you should see the EbookDB2 database in the Object Explorer.


Step 2: Configure the MVC Project to Connect to the Database
Open your project in Visual Studio.

Locate the appsettings.json file in your project.

Find the <connectionStrings> section and update it to include your database connection. Use the following format:

It should look like this after the changes: 
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=EbookDB2;Trusted_Connection=True;Encrypt=False;MultipleActiveResultSets=true"
  },

And done, you are now officially using our database in the project .