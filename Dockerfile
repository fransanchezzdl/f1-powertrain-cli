# Stage 1: Build, Test & Publish Self-Contained Binary
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG TARGETARCH
WORKDIR /src

COPY f1-powertrain-cli.sln ./
COPY src/PowertrainCli/PowertrainCli.csproj src/PowertrainCli/
COPY tests/PowertrainCli.Tests/PowertrainCli.Tests.csproj tests/PowertrainCli.Tests/
RUN dotnet restore

COPY src/ src/
COPY tests/ tests/

# Run unit tests
RUN dotnet test tests/PowertrainCli.Tests/PowertrainCli.Tests.csproj --no-restore -c Release

# Publish as self-contained executable for the target architecture
RUN dotnet publish src/PowertrainCli/PowertrainCli.csproj \
    -c Release \
    -r linux-$TARGETARCH \
    --self-contained true \
    -p:PublishSingleFile=true \
    -o /app/bin

# Stage 2: Final Runtime Environment (Python + Self-Contained Binary)
FROM python:3.11-slim
WORKDIR /app

# Install runtime dependencies required by self-contained .NET single-files
RUN apt-get update && apt-get install -y --no-install-recommends \
    libicu-dev \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*

# Install Python requirements
COPY ingestion/requirements.txt ./
RUN pip install --no-cache-dir -r requirements.txt

# Copy Python modules & self-contained .NET executable
COPY ingestion/ ./ingestion/
COPY --from=build /app/bin ./bin

# Ensure binary has execute permissions
RUN chmod +x /app/bin/PowertrainCli

# Create mount points
RUN mkdir -p /app/cache /app/data

ENTRYPOINT ["/app/bin/PowertrainCli"]