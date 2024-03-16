@echo off
echo Starting web application on port 5001...

echo Start Nats server
start start "" /D "D:\nats" nats-server.exe

cd ../CalculateRank
start "" dotnet run

cd ../Valuator
start "" dotnet run --urls "http://127.0.0.1:5001"

echo Starting another web application on port 5002...
start "" dotnet run --urls "http://127.0.0.1:5002"

echo Starting Nginx proxy server...
start "" /D "D:\nginx" nginx.exe