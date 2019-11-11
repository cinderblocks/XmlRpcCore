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
  * Complete single class embeddable server (XmlRpcServer) 
  * XML-RPC System object implemented (XmlRpcSystemObject) 
  * Method level exposure granularity (XmlRpcExposedAttribute)
  * Mono support (See [Mono](docs/MONO.html)) 
  * .NET Compact Framework support (See [CF](docs/CF.html)). 
  * Option of dynamic local proxies.

## Basic Test

Open two command shells, in one:

    
         SampleServer
    

In the other:

    
         SampleClient
    

## Documentation

  * The API documentation: [Documentation](docs/classes/XmlRpcCS.html)
  * The descriptions of the type mapping: [Types](docs/TYPES.html)
  * A UML doodle about the serialize/deserialize class inheritance: [Serialization](docs/XmlRpcSerialization.png)

## Sample Code

Under src/samples there are examples. Read the [Examples](docs/EXAMPLES.html)
for more info.

## Unit Tests

Under tests/ there are some basic unit tests. To run them have NUnit
installed, edit the path to NUnit in xmlrpccs.build and then:

    
    	nant -find unit-tests

Why didn't I use nant's nunit task you ask? It's rather broken.

## License

XmlRpcCS is under the BSD license. See: [License](LICENSE.html)

## References

  * [xmlrpc.org](http://xmlrpc.org) To learn more about XML-RPC. 
  * [nant.sourceforge.net](http://nant.sourceforge.net) To learn more about nant. 
  * [nunit.sourceforge.net](http://nunit.sourceforge.net) To learn more about NUnit unit testing.
  * [go-mono.net](http://go-mono.net) To learn more about the mono project 

## To Do

  * Support system object "capabilities" method
  * Improve system object's "methodHelp" support - rip from XML docs somehow.
  * Method overloading based on arguements
  * More unit tests
  * Tutorial doc
