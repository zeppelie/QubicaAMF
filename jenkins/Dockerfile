FROM jenkins/jenkins:lts-jdk21

USER root

RUN apt-get update \
    && apt-get install -y --no-install-recommends libicu-dev \
    && rm -rf /var/lib/apt/lists/*

COPY --from=mcr.microsoft.com/dotnet/sdk:10.0 /usr/share/dotnet /usr/share/dotnet
COPY --from=docker:cli /usr/local/bin/docker /usr/local/bin/docker
COPY --from=docker:cli /usr/local/libexec/docker/cli-plugins /usr/local/libexec/docker/cli-plugins

RUN ln -s /usr/share/dotnet/dotnet /usr/local/bin/dotnet \
    && git config --system --add safe.directory /repo \
    && git config --system --add safe.directory /repo/.git

COPY plugins.txt /usr/share/jenkins/plugins.txt
RUN jenkins-plugin-cli --plugin-file /usr/share/jenkins/plugins.txt

COPY casc.yaml /usr/share/jenkins/casc.yaml

ENV CASC_JENKINS_CONFIG=/usr/share/jenkins/casc.yaml
ENV JAVA_OPTS="-Djenkins.install.runSetupWizard=false -Dhudson.plugins.git.GitSCM.ALLOW_LOCAL_CHECKOUT=true"
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
ENV DOTNET_NOLOGO=1
