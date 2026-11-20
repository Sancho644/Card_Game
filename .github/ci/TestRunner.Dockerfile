ARG UNITY_VERSION=6000.6.7f1
FROM unityci/editor:ubuntu-${UNITY_VERSION}-linux-il2cpp-3

RUN test -x /usr/bin/unity-editor && command -v python3
COPY --chmod=755 unity-editor-no-coverage.py /opt/cardgame-tests/unity-editor
ENV PATH="/opt/cardgame-tests:${PATH}"
RUN /bin/bash -c 'test "$(command -v unity-editor)" = /opt/cardgame-tests/unity-editor'
