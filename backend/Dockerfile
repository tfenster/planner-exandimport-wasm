FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /opt/build

RUN curl -fsSL https://raw.githubusercontent.com/devcontainers/features/main/src/rust/install.sh | bash && export PATH=$PATH:/usr/local/cargo/bin && rustup default stable && cargo install wizer --all-features && curl -fsSL https://developer.fermyon.com/downloads/install.sh | bash && sudo mv spin /usr/local/bin/

COPY ./Project.csproj .
RUN dotnet restore

COPY . .
RUN spin build