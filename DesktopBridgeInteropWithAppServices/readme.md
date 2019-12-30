This sample shows inter process communication using App Services. 

This sample is a desktop bridge app that includes the following projects:
* UWPApp -> this is the main UWP app. it exposes two app services : SumAppService, MulAppService
* Launcher -> full trust process launched from the UWP app. 
* SumApp -> C# console app. it receives two integers from UWPApp through SumAppService connection, performs the addition and returns the result through the app service connection
* MulApp -> C# console app. it receives two integers from UWPApp through MulAppService connection, performs the multiplication and returns the result through the app service connection
* Package -> this is the Windows Application Package project used to deploy this desktop bridge app

Launcher will start either SumApp.exe or MulApp.exe C# console apps depending on user's choice (do an addition or do a multiplication).

The console	apps will connect to the relevant app service when they start (for instance, SumApp will connect to SumAppService).

