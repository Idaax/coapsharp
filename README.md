# CoAPSharp
CoAPSharp was the world's first [Constraind Application Protocol (CoAP)](https://www.rfc-editor.org/rfc/rfc7252) library that was built for Microsoft's [.NET Micro Framework](https://netmf.github.io/), popularly known as NETMF. As work on NETMF halted, CoAPSharp's future too was impacted. Thankfully, the folks at the [nanoFramework](https://github.com/nanoframework) team recreated possibilities of writing embedded code on microcontrollers using C#. 

This release of CoAPSharp is now qualified for the nanoFramework runtime. It contains the following:
* The core CoAPSharp library
* Samples that outline how to use the library

Do follow us on Twitter [@CoAPSharp](https://twitter.com/coapsharp) for latest news, updates, samples and announcements related to this library.

## Free e-book on CoAP with examples in CoAPSharp
Our developers have created an e-book for you to get started on CoAP and CoAPSharp. All examples in the book are also provided in this repository.

## Examples (For .NET Standard)
1. [Block transfer in CoAP - Client](https://github.com/Idaax/coapsharp/blob/main/Idaax.NetStandard.CoAPSharp.Samples/BasicCoAPBlockTransferPUTClient.cs)
2. [Block transfer in CoAP - Server](https://github.com/Idaax/coapsharp/blob/main/Idaax.NetStandard.CoAPSharp.Samples/BasicCoAPBlockTransferPUTServer.cs)
3. [CON client in CoAP](https://github.com/Idaax/coapsharp/blob/main/Idaax.NetStandard.CoAPSharp.Samples/BasicCoAPCONClient.cs)
4. [CON server in CoAP](https://github.com/Idaax/coapsharp/blob/main/Idaax.NetStandard.CoAPSharp.Samples/BasicCoAPCONServer.cs)
5. [NON client in CoAP](https://github.com/Idaax/coapsharp/blob/main/Idaax.NetStandard.CoAPSharp.Samples/BasicCoAPNONClient.cs)
6. [NON server in CoAP](https://github.com/Idaax/coapsharp/blob/main/Idaax.NetStandard.CoAPSharp.Samples/BasicCoAPNONServer.cs)
7. [Observable server in CoAP](https://github.com/Idaax/coapsharp/blob/main/Idaax.NetStandard.CoAPSharp.Samples/BasicCoAPObservableServer.cs)
8. [Observable client in CoAP](https://github.com/Idaax/coapsharp/blob/main/Idaax.NetStandard.CoAPSharp.Samples/BasicCoAPObservableClient.cs)
9. [Separate response client in CoAP](https://github.com/Idaax/coapsharp/blob/main/Idaax.NetStandard.CoAPSharp.Samples/BasicCoAPSeparateResponseClient.cs)
10. [Separate response server in CoAP](https://github.com/Idaax/coapsharp/blob/main/Idaax.NetStandard.CoAPSharp.Samples/BasicCoAPSeparateResponseServer.cs)
11. [Well-known NON client in CoAP](https://github.com/Idaax/coapsharp/blob/main/Idaax.NetStandard.CoAPSharp.Samples/BasicCoAPWellKnownNONClient.cs)
12. [Well-known NON server in CoAP](https://github.com/Idaax/coapsharp/blob/main/Idaax.NetStandard.CoAPSharp.Samples/BasicCoAPWellKnownNONServer.cs)

## Examples (For nanoFramework on ESP32)
A basic CoAP NON Server is implemented [here](https://github.com/Idaax/coapsharp/tree/main/Idaax.nanoFramework.CoAPSharp.Samples)
You follow the same process as .NET , however, you need to ensure that you are connected to the network before you begin.
If you have only one ESP32, you can flash the code in ESP32 (the sample shows how to get the assigned IP address). Then, you can create the corresponding client or server on another machine and see the magic!
