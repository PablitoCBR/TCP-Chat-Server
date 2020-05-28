# Server for Client-Server chatting application

## Build requirements:
- .NET CORE 3.1 SDK (src: https://dotnet.microsoft.com/download/dotnet-core/3.1)

## Demo HOW-TO:
### SETUP
### Single self contained file (.NET CORE 3.1 UPDATE):
#### Compile project to single executable file using .NET Core 3.1. which will contain .NET runtime, libraries and application code.

#### LOCAL MODE
1. Compile server host
    1. Run command line 
    2. Navigate to cloned repository location and `/Server ` directory
    3. If first compilation run command `dotnet restore` and `dotnet build -c DEBUG`
    4. Run command `dotnet publish -r <runtime_id> -c Debug -o <output_directory>`
        - To choose correct runtime ID refer to: https://docs.microsoft.com/pl-pl/dotnet/core/rid-catalog
        - Compilation will also copy `appsettings.json`, `Chatty.db` and create `Server.pdb` files inside specified output directory.
2. Start server host
    1. Navigate to specified output directory and run executable `Server` file.
    2. Order server to listen for messages by typing `run`
3. Compile demo client CLI
    1. Run command line in new window 
    2. Navigate to cloned repository location and `/ClientDemo` directory
    3. If first compilation run command `dotnet restore` and `dotnet build -c DEBUG`
    4. Run command `dotnet publish -r <runtime_id> -c Debug -o <output_directory>`
        - To choose correct runtime ID refer to: https://docs.microsoft.com/pl-pl/dotnet/core/rid-catalog
        - Compilation will create `ClientDemo.pdb` 
4. Repeat step **3** for required number of clients
5. Start client demo
    1. Navigate to specified output directory and run executable `ClientDemo` file.
    2. In client CLI window type username and password that will be used by client
    3. Request user registration pressing **2** (*if user is already registerd go to* **8**)
    4. After registration you will be requested to pass credentials again
    5. Request user authetication pressing **1**

#### Local Network MODE
1. Compile server host
    1. Run command line 
    2. Navigate to cloned repository location and `/Server ` directory
    3. If first compilation run command `dotnet restore` and `dotnet build -c RELEASE`
    4. Run command `dotnet publish -r <runtime_id> -c RELEASE -o <output_directory>`
        - To choose correct runtime ID refer to: https://docs.microsoft.com/pl-pl/dotnet/core/rid-catalog
        - Compilation will also copy `appsettings.json`, `Chatty.db` and create `Server.pdb` files inside specified output directory.
2. Start server host
    1. Navigate to specified output directory and run executable `Server` file.
    2. Order server to listen for messages by typing `run`
3. Compile demo client CLI
    1. Run command line in new window 
    2. Navigate to cloned repository location and `/ClientDemo` directory
    3. If first compilation run command `dotnet restore` and `dotnet build -c RELEASE`
    4. Run command `dotnet publish -r <runtime_id> -c RELEASE -o <output_directory>`
        - To choose correct runtime ID refer to: https://docs.microsoft.com/pl-pl/dotnet/core/rid-catalog
        - Compilation will create `ClientDemo.pdb` 
4. Repeat step **3** for required number of clients
5. Start client demo
    1. Navigate to specified output directory and run executable `ClientDemo` file.
    2. In client CLI window type username and password that will be used by client
    3. Request user registration pressing **2** and enter IPv4 address from server host if required (*if user is already registerd go to* **6**)
    4. After registration you will be requested to pass credentials again
    5. Request user authetication pressing **1** and enter IPv4 address from server host if required

### Client menu

**Only after successful authentication**

Message sending:

1. Press **1** to enter message sending
2. Enter recipient name (*if recipient will not be connected appropriate  error will occure*)
3. Type message (*for demo purpose  it is single line message*)
4. Press *Enter* to send message
5. `[MSG FROM: xyz]` info will occure on recipient console window

Press **2** to print all recived messages in current session.

Press **3** to exit
