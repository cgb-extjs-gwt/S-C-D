using System;
using System.Linq;
using Gdc.Scd.Core.Helpers;
using Gdc.Scd.MigrationTool.Interfaces;
using Ninject;

namespace Gdc.Scd.MigrationTool
{
    class Program
    {
        static readonly StandardKernel kernel;

        static readonly IMigrationService migrationService;

        static Program()
        {
            NinjectExt.IsConsoleApplication = true;

            kernel = CreateKernel();
            migrationService = kernel.Get<IMigrationService>();
        }

        static void Main(string[] args)
        {
            try
            {
                var migrationActions = migrationService.GetMigrationActions().ToArray();

                Console.WriteLine($"{migrationActions.Length} migrations for executing");
                Console.WriteLine();

                var defaultColor = Console.ForegroundColor;

                foreach (var migrationAction in migrationActions)
                {
                    Console.WriteLine($"Executing migration:");
                    Console.WriteLine($"Number: {migrationAction.Number}");
                    Console.WriteLine($"Description: {migrationAction.Description}");

                    try
                    {
                        migrationAction.Execute();
                    }
                    catch(Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Error during execution migration:");
                        Console.WriteLine(ex.Message);
                        Console.WriteLine();

                        Console.ForegroundColor = defaultColor;
                        Console.WriteLine("Executing migration was stoped");

                        break;
                    }

                    migrationService.SaveMigrationAsExecuted(migrationAction);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Migration succefully executed");
                    Console.ForegroundColor = defaultColor;
                    Console.WriteLine();
                }
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
            }

            Console.ReadKey();
        }

        private static StandardKernel CreateKernel()
        {
            return new StandardKernel(
                new Core.Module(), 
                new DataAccessLayer.Module(),
                new BusinessLogicLayer.Module(),
                new MigrationTool.Module());
        }
    }
}
