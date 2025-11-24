XmlRpcCore
========

[![Build status](https://ci.appveyor.com/api/projects/status/1mw8qwd83q1u7l5d?svg=true)](https://ci.appveyor.com/project/cinderblocks57647/xmlrpccore)  
[![XmlRpcCore NuGet-Release](https://img.shields.io/nuget/v/xmlrpccore.svg?label=XmlRpcCore)](https://www.nuget.org/packages/XmlRpcCore/)  
[![NuGet Downloads](https://img.shields.io/nuget/dt/XmlRpcCore?label=NuGet%20downloads)](https://www.nuget.org/packages/XmlRpcCore/)  

## Introduction

This package provides a simple XML-RPC client and server for C# applications.

XmlRpcCore is a fork of [XmlRpcCS](http://xmlrpccs.sourceforge.net/) written to
take advantage of newer language features and conform to the .NET Standard spec.

The goals of XmlRpcCS were to keep it small and simple. The motivation was to
write something that was easy to use while being flexible.

## Notable Features

  * Fully XML-RPC specification compliant, including key extensions. 
  * Simple client (`XmlRpcRequest`) 
  * Method level exposure granularity (`XmlRpcExposedAttribute`)
  * Option of dynamic local proxies.

## Examples

Below are short examples showing common usage patterns. Replace `url` with your
server endpoint and add error handling as appropriate.

Basic request (legacy non-generic params):

```csharp
using System.Collections;
using System.Net.Http;

var req = new XmlRpcRequest("example.sum", new ArrayList { 1, 2, 3 });

// Serialize to XML string
var xml = new XmlRpcRequestSerializer().Serialize(req);

// Send using HttpClient (extension method included)
using var client = new HttpClient();
var response = await client.PostAsXmlRpcAsync("https://your.server/xmlrpc", req);
if (response.IsFault)
    throw new XmlRpcException(response.FaultCode, response.FaultString);

var value = response.Value; // result from remote call
```

Using the generic helper (`ParamsGeneric`) to create strongly-typed lists:

```csharp
var request = new XmlRpcRequest();
var genericParams = request.ParamsGeneric; // IList<object>
genericParams.Add(1);
genericParams.Add(2);
genericParams.Add(3);

// Ensure the underlying legacy Params is populated if needed by older APIs
if (request.Params is System.Collections.IList legacy)
{
    legacy.Clear();
    foreach (var v in genericParams) legacy.Add(v);
}

var xml = new XmlRpcRequestSerializer().Serialize(request);
```

Deserialize a request/response from XML text:

```csharp
using System.IO;

var xmlText = "..."; // xmlrpc request/response as string
var parsedRequest = (XmlRpcRequest)new XmlRpcRequestDeserializer().Deserialize(new StringReader(xmlText));
var parsedResponse = (XmlRpcResponse)new XmlRpcResponseDeserializer().Deserialize(new StringReader(xmlText));
```

POCO deserialization examples

```csharp
// Simple POCO mapped from a response value (struct)
public class Person { public string name { get; set; } public int age { get; set; } }

var respXml = "..."; // an XML-RPC response whose <value> is a struct { name, age }
var person = new XmlRpcResponseDeserializer().Deserialize<Person>(new StringReader(respXml));

// Constructor binding: map struct keys to constructor args
public class PersonCtor { public string Name { get; } public int Age { get; } public PersonCtor(string name, int age) { Name = name; Age = age; } }
var ctorPerson = new XmlRpcRequestDeserializer().Deserialize<PersonCtor>(new StringReader(requestXml));

// Attribute mapping: use [XmlRpcName] to bind differently named fields
public class PersonAttr { [XmlRpcName("fullname")] public string FullName { get; set; } public int Age { get; set; } }
var attrPerson = new XmlRpcRequestDeserializer().Deserialize<PersonAttr>(new StringReader(requestXml));

// Private setters and fields are supported by the binder
public class PersonPrivate { public string Name { get; private set; } private int age; public int GetAge() => age; }
var privatePerson = new XmlRpcRequestDeserializer().Deserialize<PersonPrivate>(new StringReader(requestXml));
```

Create and inspect a fault response:

```csharp
var fault = new XmlRpcResponse();
fault.SetFault(42, "Something went wrong");
var xml = XmlRpcResponseSerializer.Singleton.Serialize(fault);
// Parse it back
var parsed = (XmlRpcResponse)new XmlRpcResponseDeserializer().Deserialize(new StringReader(xml));
if (parsed.IsFault)
    Console.WriteLine($"Code {parsed.FaultCode}: {parsed.FaultString}");
```

Boxcar (system.multiCall) example:

```csharp
var box = new XmlRpcBoxcarRequest();
var r1 = new XmlRpcRequest("one.sum", new System.Collections.ArrayList { 1, 2 });
var r2 = new XmlRpcRequest("two.concat", new System.Collections.ArrayList { "x", "y" });
box.Requests.Add(r1);
box.Requests.Add(r2);

var xml = new XmlRpcRequestSerializer().Serialize(box);
// Send `box` like a normal request; method name will be `system.multiCall`.
```

Notes

  * The library supports both legacy non-generic collection types and newer
    generic collections. Use `ParamsGeneric` to access a strongly-typed
    `IList<object>` view. The old `Params` (`IList`) property is marked as
    obsolete and will be removed in a future major release.
  * Date/time values are serialized using an ISO 8601 format and parsed using
    the invariant culture for consistent round-tripping across cultures.

## Documentation

This needs to be regenerated and documentation needs to be updated.

  * The API documentation: [Documentation](docs/classes/XmlRpcCS.html)
  * The descriptions of the type mapping: [Types](docs/TYPES.html)
  * A UML doodle about the serialize/deserialize class inheritance: [Serialization](docs/XmlRpcSerialization.png)


## Unit Tests

Unit tests are included in `XmlRpcCore.Tests` and exercise both legacy and
modern APIs. Run them via `dotnet test XmlRpcCore.Tests`.

## License

XmlRpcCore is under the BSD license. See: [License](LICENSE.html)

## References

  * [xmlrpc.org](http://xmlrpc.org) To learn more about XML-RPC. 

## To Do

  * Support system object "capabilities" method
  * Improve system object's "methodHelp" support - rip from XML docs somehow.
  * Method overloading based on arguments
  * Tutorial doc
