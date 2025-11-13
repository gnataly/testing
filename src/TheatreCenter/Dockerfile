FROM mcr.microsoft.com/dotnet/sdk:8.0

WORKDIR /src

COPY *.sln .
COPY TheatreCenter.UnitTests/*.csproj TheatreCenter.UnitTests/
COPY TheatreCenter.Backend/*.csproj TheatreCenter.Backend/
COPY TheatreCenter.Domain/*.csproj TheatreCenter.Domain/
COPY TheatreCenter.Data/*.csproj TheatreCenter.Data/
COPY TheatreCenter.DTOs/*.csproj TheatreCenter.DTOs/
COPY TheatreCenter.Services/*.csproj TheatreCenter.Services/

RUN dotnet restore

COPY . .
RUN dotnet build --configuration Release --no-restore


RUN echo '#!/bin/bash\n\
echo "Running Unit Tests from $(pwd)"\n\
ls -la\n\
dotnet test TheatreCenter.UnitTests/TheatreCenter.UnitTests.csproj --configuration Release --logger "trx;LogFileName=unit-tests.trx" --results-directory /test-results\n --filter "Category=Unit"\
exit $?' > /run-unit-tests.sh && chmod +x /run-unit-tests.sh

RUN echo '#!/bin/bash\n\
echo "Running Unit Tests from $(pwd)"\n\
ls -la\n\
dotnet test TheatreCenter.UnitTests/TheatreCenter.UnitTests.csproj --configuration Release --logger "trx;LogFileName=integration-tests.trx" --results-directory /test-results\n --filter "Category=Integration"\
exit $?' > /run-integration-tests.sh && chmod +x /run-integration-tests.sh

RUN echo '#!/bin/bash\n\
echo "Running Unit Tests from $(pwd)"\n\
ls -la\n\
dotnet test TheatreCenter.UnitTests/TheatreCenter.UnitTests.csproj --configuration Release --logger "trx;LogFileName=e2e-tests.trx" --results-directory /test-results\n --filter "Category=EndToEnd"\
exit $?' > /run-e2e-tests.sh && chmod +x /run-e2e-tests.sh


RUN echo '#!/bin/bash\n\
echo "Running All Tests from $(pwd)"\n\
/run-unit-tests.sh\n\
/run-integration-tests.sh\n\
/run-e2e-tests.sh\n\
exit $?' > /run-all-tests.sh && chmod +x /run-all-tests.sh

RUN mkdir -p /test-results

CMD ["/run-all-tests.sh"]