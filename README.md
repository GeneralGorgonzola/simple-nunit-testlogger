# Custom nunit-logger for unittests
Crude and simple.

Can be used with the Jenkins plugin for nunit.

Written in .NET 5.0.

Probably has a few bugs and shortcomings... should be easy to fix.

## Tools, prerequisites
XSCGEN. Tool to generate code from XSDs.

https://www.nuget.org/packages/dotnet-xscgen/

https://github.com/mganss/XmlSchemaClassGenerator

This is useful when generating XML that should conform to some specific format.

A working XSD for nunit2.5 can be found here: https://nunit.org/files/testresult_schema_25.txt

Install the tool, download the XSD and do this:

    xscgen -0 -n "|nunit2.xsd=Zool" nunit2.xsd

This generates a .cs file, called *Zool.cs*.

Using this file, we are *reasonably sure* to generate only valid XML when saving the test reports.
Beware, though, that the nunit-people seem to have used strings instead of booleans, dates, times, enumerations (for error-codes), and decimals instead of integers... but the XML will by itself will be valid, at least.

## Roll your own...
Create a class library:

    dotnet new classlib --name Whatever

Add this nuget package:

    dotnet add package Microsoft.TestPlatform.ObjectModel

And implement this interface:

    ITestLoggerWithParameters

**Note**: The assembly has to have **TestLogger** in the name.

Add these properties to the CSPROJ file:

    <PropertyGroup>
        .....various stuff removed for clarity.....
        <AssemblyName>MyNamespace.TestLogger</AssemblyName>
        <RootNamespace>MyNamespace.TestLogger</RootNamespace>
    </PropertyGroup>

Also, add these attributes to the class implementing the interface ITestLoggerWithParameters:

    [FriendlyName("whatever")]
    [ExtensionUri("logger://Microsoft/TestPlatform/AnythingGoes/v1")]
    public class Logger : ITestLoggerWithParameters
    {
        ......

Create a nuget package, and push it to some repository, and add it to any unittest-project, or use a project-reference.

When testing, invoke dotnet test like this:

    dotnet test --logger "whatever;LogFilePath=TestResults.xml;Suite=SomeCoolName"

(this matches the example-code above) or using the Uri instead:

    dotnet test --logger "logger://Microsoft/TestPlatform/AnythingGoes/v1;LogFilePath=TestResults.xml;Suite=SomeOtherCoolName"

In the Jenkins pipeline, for instance:

    steps {
        sh 'dotnet test --logger "whatever;LogFilePath=TestResults.xml;Suite=MyProject"'
    }

and pick up the test-reports:

    post {
        always {
            nunit testResultsPattern: '**/*TestResult*.xml'
        }
    }

These additional arguments:

    LogFilePath=TestResults.xml;Suite=SomeCoolName

are passed as a dictionary to the Initialize-method:

    public void Initialize(TestLoggerEvents events, Dictionary<string, string> parameters)
    {
        parameters.TryGetValue("TestRunDirectory", out var testRunDirectory);
        parameters.TryGetValue("LogFilePath", out var logFilePath);
        parameters.TryGetValue("Suite", out var suiteName);
        Setup(events, testRunDirectory, suiteName, logFilePath);
    }

## In this solution
Run the "tests":

    dotnet test --logger "nunit2;LogFilePath=TestResults.xml;Suite=MyGloriousTestSuite"

This will produce a set of testreports with a mix of failed and successful tests.

To understand the flow and the data, try this:

    dotnet test --logger "reverse;LogFilePath=TestResults.xml" > output.txt 2>&1

This logs, what the test-logger logs.

### Resulting TestResult.xml
The root is the result-node, *test-results*.
In the *results*-node underneath, place *test-suite* and/or *test-case* nodes.

Here, we place a *test-suite*, which corresponds to the test-assembly/project.
Underneath this *test-suite* node (in another *results*-node) we place another *test-suite* node, which corresponds to the individual test-classes in the assembly. Underneath this *test-suite*, we finally place the *test-case* nodes, which correspond to the individual tests.

The result is a hierarchy of tests and statistics, which can be understood/consumed by the Jenkins nunit-plugin.