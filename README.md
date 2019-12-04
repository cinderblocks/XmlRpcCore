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
  * Simple client (XmlRpcRequest) 
  * Method level exposure granularity (XmlRpcExposedAttribute)
  * Option of dynamic local proxies.

## Documentation

This needs to be regenerated and documentation needs to be updated.

  * The API documentation: [Documentation](docs/classes/XmlRpcCS.html)
  * The descriptions of the type mapping: [Types](docs/TYPES.html)
  * A UML doodle about the serialize/deserialize class inheritance: [Serialization](docs/XmlRpcSerialization.png)

## Sample Code

These need to be updated...

## Unit Tests

These need to be updated...

## License

XmlRpcCore is under the BSD license. See: [License](LICENSE.html)

## References

  * [xmlrpc.org](http://xmlrpc.org) To learn more about XML-RPC. 

## To Do

  * Support system object "capabilities" method
  * Improve system object's "methodHelp" support - rip from XML docs somehow.
  * Method overloading based on arguments
  * More unit tests
  * Tutorial doc
