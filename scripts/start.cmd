@echo off
echo Starting web application on port 5001...
cd /D D:\source\repos\distributed_programming\Valuator
start "" dotnet run --urls "http://127.0.0.1:5001"

echo Starting another web application on port 5002...
cd /D D:\source\repos\distributed_programming\Valuator
start "" dotnet run --urls "http://127.0.0.1:5002"

echo Starting Nginx proxy server...
start "" /D "D:\nginx" nginx.exe
это start.cmd