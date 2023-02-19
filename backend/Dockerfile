FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /opt/build

RUN curl -fsSL https://sh.rustup.rs | bash && rustup -y default stable && cargo install wizer --all-features && curl -fsSL https://developer.fermyon.com/downloads/install.sh | bash && sudo mv spin /usr/local/bin/

COPY ./Project.csproj .
RUN dotnet restore

COPY . .
RUN spin build