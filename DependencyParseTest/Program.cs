// See https://aka.ms/new-console-template for more information

using System.Reflection;

static async Task Main(string[] args)
{
    string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestData");
    string packageJsonPath = Path.Combine(filePath, "Package_Test.json");
    string testProjectPath = Path.Combine(filePath, "Test_Project.xml");

    //Read Instructions.txt
    //Do Step 1
    //Do Step 2
    //Do Step 3
}