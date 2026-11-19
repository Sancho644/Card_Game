#!/usr/bin/env python3
"""Temporarily disable coverage in GameCI Test Runner v4.3.2."""

import os
import sys


def editor_arguments(arguments):
    if "-runTests" not in arguments:
        return arguments

    filtered = []
    options = iter(arguments)
    for option in options:
        if option == "-enableCodeCoverage":
            continue
        if option in ("-coverageOptions", "-coverageResultsPath"):
            next(options, None)
            continue
        filtered.append(option)
    return filtered


if __name__ == "__main__":
    editor = "/usr/bin/unity-editor"
    arguments = editor_arguments(sys.argv[1:])
    if "-runTests" in arguments:
        # Log only the experiment flags. Activation and return can carry secrets.
        workers = ("-job-worker-count" in arguments and
                   arguments[arguments.index("-job-worker-count") + 1:][:1] == ["2"])
        print(f"Coverage disabled; -nographics={'-nographics' in arguments}; "
              f"-job-worker-count 2={workers}", flush=True)
    os.execv(editor, [editor, *arguments])
