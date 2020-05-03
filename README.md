# Server for Client-Server chatting application

## Build requirements:
- .NET CORE 2.1 SDK (src: https://dotnet.microsoft.com/download/dotnet-core/2.1)

## Demo HOW-TO:
1. Clone repository
2. Start server host
    1. Run command line (Win/Linux)
    2. Navigate to cloned repository location and `/Server ` folder
    3. Run command `dotnet run -c RELEASE`
    4. type `run` to start listening
3. Start demo client CLI
    1. Run command line in new window (Win/Linux)
    2. Navigate to cloned repository location and `/ClientDemo` folder
    3. Run command `dotnet run -c RELEASE`
4. Repeat step **3** for required number of clients
5. In client CLI window type username and password htat will be used by client
6. Request user registration pressing **2** (*if user is already registerd go to* **8**)
7. After registration you will be requested to pass credentials again
8. Request user authetication pressing **1**
9. Message sending
    1. Press **1** to enter message sending
    2. Enter recipient name (*if recipient will not be connected appropriate  error will occure*)
    3. Type message (*for demo purpose  it is single line message*)
    4. Press *Enter* to send message
    5. `[MSG FROM: xyz]` info will occure on recipient console window
10. Press **2** to exit
