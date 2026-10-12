#!/usr/bin/env bash 

cd ./src/OfficeDays.Web || exit
npm run build
dotnet build ../OfficeDays/OfficeDays.csproj