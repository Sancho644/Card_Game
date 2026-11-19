ARG UNITY_VERSION=6000.6.7f1
FROM unityci/editor:ubuntu-${UNITY_VERSION}-windows-mono-3

RUN mkdir -p /opt/cardgame-ci && command -v unity-editor > /opt/cardgame-ci/unity-editor-path
COPY unity-editor /opt/cardgame-ci/unity-editor
RUN chmod +x /opt/cardgame-ci/unity-editor
ENV PATH="/opt/cardgame-ci:${PATH}"
