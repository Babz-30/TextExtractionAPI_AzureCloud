# ----------- Build Stage ----------- #
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files and restore dependencies
# This ensures dotnet restore can run successfully before copying all source.
COPY ["MyCloudProject/MyCloudProject.csproj", "MyCloudProject/"]
COPY ["MyCloudProject.Common/MyCloudProject.Common.csproj", "MyCloudProject.Common/"]
COPY ["MyExperiment/MyExperiment.csproj", "MyExperiment/"]

RUN dotnet restore "./MyCloudProject/MyCloudProject.csproj"

# Copy the rest of the application source code
COPY . .
WORKDIR "/src/MyCloudProject"

# Build the application
RUN dotnet build "./MyCloudProject.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ----------- Publish Stage ----------- #
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
# Publish the application, ensuring it's self-contained if needed, or just for deployment.
# /p:UseAppHost=false is good for environments where an app host isn't desired/needed.
RUN dotnet publish "./MyCloudProject.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ----------- Runtime Base Image ----------- #
# Using a slim Debian image is good for size, but requires explicit dependency installation.
FROM debian:bookworm-slim AS base

# Set environment variable for non-interactive apt-get
ENV DEBIAN_FRONTEND=noninteractive

# Install runtime dependencies and .NET runtime
RUN apt-get update && \
    apt-get install -y wget gnupg apt-transport-https software-properties-common ca-certificates && \
    wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    apt-get update && \
    apt-get install -y dotnet-runtime-8.0 \
                       tesseract-ocr \
                       tesseract-ocr-eng \
                       libleptonica-dev \
                       libtesseract-dev \
                       libc6-dev \
                       cabextract \
                       fontconfig \
                       # libc6-dev provides the 'libdl' library, which was the core missing dependency.
                       # It's essential for dynamically loading shared libraries on Linux.
                       libgdiplus && \
                       # libgdiplus is often required if your .NET application uses System.Drawing.Common
                       # for any image manipulation (e.g., resizing, converting formats) before passing
                       # to Tesseract. Even if not directly used by TesseractOCR, it's a common dependency
                       # for .NET image processing on Linux.
    rm -rf /var/lib/apt/lists/*

# Download and install Microsoft Core Fonts (Arial, etc.)
RUN wget https://www.freedesktop.org/software/fontconfig/webfonts/webfonts.tar.gz && \
    tar -xzf webfonts.tar.gz && \
    cd msfonts && \
    cabextract *.exe && \
    mkdir -p /usr/local/share/fonts && \
    cp *.ttf *.TTF /usr/local/share/fonts/ && \
    mkdir -p /usr/share/fonts && \
    cp *.ttf *.TTF /usr/share/fonts/ && \
    mkdir -p /root/.local/share/fonts && \
    cp *.ttf *.TTF /root/.local/share/fonts/ && \
    fc-cache -f -v && \
    cd .. && rm -rf msfonts webfonts.tar.gz

# Create folder for interop symlinks
RUN mkdir -p /app/x64

# Add symlinks for interop (adjust versions if needed)
# These symlinks help TesseractOCR.InteropDotNet find the correct library versions.
RUN ln -sf /usr/lib/x86_64-linux-gnu/liblept.so.5 /app/x64/libleptonica-1.85.0.dll.so && \
    ln -sf /usr/lib/x86_64-linux-gnu/libtesseract.so.5 /app/x64/libtesseract55.dll.so && \
    ln -sf /usr/lib/x86_64-linux-gnu/libdl.so.2 /usr/lib/x86_64-linux-gnu/libdl.so

WORKDIR /app

# ----------- Final Stage ----------- #
FROM base AS final
WORKDIR /app
# Copy the published application from the publish stage into the final image.
COPY --from=publish /app/publish .

# Define the entry point for the application.
ENTRYPOINT ["dotnet", "MyCloudProject.dll"]
