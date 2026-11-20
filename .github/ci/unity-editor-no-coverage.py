#!/usr/bin/env python3
"""Adapt the test invocation of GameCI Test Runner v4.3.2."""

import os
import sys

EDITOR = "/usr/bin/unity-editor"
COVERAGE_SWITCH = "-enableCodeCoverage"
COVERAGE_VALUES = {"-coverageOptions", "-coverageResultsPath"}


def editor_arguments(arguments):
    if "-runTests" not in arguments:
        return arguments, []

    filtered = []
    removed = []
    index = 0
    while index < len(arguments):
        argument = arguments[index]
        index += 1
        name, equals, _ = argument.partition("=")
        if argument == COVERAGE_SWITCH:
            removed.append(name)
        elif name in COVERAGE_VALUES:
            removed.append(name)
            if not equals and index < len(arguments) and not arguments[index].startswith("-"):
                index += 1
        else:
            filtered.append(argument)

    return filtered, removed


def has_value(arguments, flag):
    return any(arg == flag for arg in arguments[:-1])


def log_test_launch(arguments, removed):
    workers = any(arguments[i:i + 2] == ["-job-worker-count", "2"]
                  for i in range(len(arguments) - 1))
    print(f"Wrapper invoked; Unity launcher: {EDITOR}", flush=True)
    print(f"Coverage arguments detected: {len(removed)}; removed: {', '.join(removed) or '(none)'}", flush=True)
    print(
        "Final safe test flags: "
        f"-runTests={'-runTests' in arguments}; "
        f"-testPlatform={has_value(arguments, '-testPlatform')}; "
        f"-testResults={has_value(arguments, '-testResults')}; "
        f"-logFile={has_value(arguments, '-logFile')}; "
        f"-nographics={'-nographics' in arguments}; "
        f"-job-worker-count 2={workers}; "
        f"-testFilter={has_value(arguments, '-testFilter')}; "
        f"-debugCodeOptimization={'-debugCodeOptimization' in arguments}",
        flush=True,
    )


if __name__ == "__main__":
    arguments, removed = editor_arguments(sys.argv[1:])
    if "-runTests" in arguments:
        log_test_launch(arguments, removed)
    os.execv(EDITOR, [EDITOR, *arguments])
