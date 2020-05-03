# Server for Client-Server chatting application

## Build requirements:
- .NET CORE 2.1 SDK (src: https://dotnet.microsoft.com/download/dotnet-core/2.1)

## Demo HOW-TO:
### SETUP
#### LOCAL MODE
1. Clone repository
2. Start server host
    1. Run command line (Win/Linux)
    2. Navigate to cloned repository location and `/Server ` folder
    3. Run command `dotnet run -c DEBUG`
    4. Type `run` to start listening
3. Start demo client CLI
    1. Run command line in new window (Win/Linux)
    2. Navigate to cloned repository location and `/ClientDemo` folder
    3. Run command `dotnet run -c DEBUG`
4. Repeat step **3** for required number of clients
5. In client CLI window type username and password that will be used by client
6. Request user registration pressing **2** (*if user is already registerd go to* **8**)
7. After registration you will be requested to pass credentials again
8. Request user authetication pressing **1**

#### Local Network MODE
1. Setup server host
    1. Clone repository to target host machine
    2. Start server host
        1. Run command line (Win/Linux)
        2. Navigate to cloned repository location and `/Server ` folder
        3. Run command `dotnet run -c RELEASE`
        4. Type `run` to start listening
        5. IPv4 address and port of server will occure
2. Setup client
    1. Clone repository to target host machine
    2. Start demo client CLI
        1. Run command line in new window (Win/Linux)
        2. Navigate to cloned repository location and `/ClientDemo` folder
        3. Run command `dotnet run -c RELEASE`
    3. In client CLI window type username and password that will be used by client
    4. Request user registration pressing **2** and enter IPv4 address from server host if required (*if user is already registerd go to* **6**)
    5. After registration you will be requested to pass credentials again
    6. Request user authetication pressing **1** and enter IPv4 address from server host if required

### Message sending

**Only after successful authentication**

1. Press **1** to enter message sending
2. Enter recipient name (*if recipient will not be connected appropriate  error will occure*)
3. Type message (*for demo purpose  it is single line message*)
4. Press *Enter* to send message
5. `[MSG FROM: xyz]` info will occure on recipient console window

Press **2** to exit
