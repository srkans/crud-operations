﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    public partial class InsertPerson_StoredProcedure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string sp_InsertPerson = @"CREATE PROCEDURE [dbo].[InsertPerson]
                                       (@PersonID uniqueidentifier, @Name nvarchar(40), @Email nvarchar(50), @DateOfBirth datetime2(7),
                                        @Gender nvarchar(10), @CountryID uniqueidentifier, @Address nvarchar(200), @ReceiveNewsLetters bit)
                                        AS BEGIN
                                            INSERT INTO [dbo].[Persons] (PersonID, Name, Email, DateOfBirth, Gender, CountryID, Address, ReceiveNewsLetters) VALUES
                                            (@PersonID, @Name, @Email, @DateOfBirth, @Gender, @CountryID, @Address, @ReceiveNewsLetters)
                                        END
            ";

            migrationBuilder.Sql(sp_InsertPerson);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            string sp_InsertPerson = @"DROP PROCEDURE [dbo].[InsertPerson]";

            migrationBuilder.Sql(sp_InsertPerson);
        }
    }
}
