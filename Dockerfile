FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /app
EXPOSE 8127
CMD [ "/bin/bash", "init.sh" ]