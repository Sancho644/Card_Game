"""Verify the arguments recorded by the real Unity EditMode process."""

import argparse
from pathlib import Path

VALIDATION_TEST = "Game.Data.Tests.FirstCardSetTests.ProjectCardDatabaseValidation"
COVERAGE_FLAGS = {"-enableCodeCoverage", "-coverageOptions", "-coverageResultsPath"}


def verify(log, validation):
    contents = log.read_text(encoding="utf-8", errors="replace")
    _, header, command = contents.partition("COMMAND LINE ARGUMENTS:\n")
    if not header:
        raise ValueError("Unity did not log its command line.")

    # In Linux GameCI the command line precedes the first project-path message.
    command, _, _ = command.partition("\nSuccessfully changed project path to:")
    arguments = [line.strip() for line in command.splitlines()]
    if arguments[:1] != ["/opt/unity/Editor/Unity"]:
        raise ValueError("Unexpected Unity executable in Editor.log.")
    if COVERAGE_FLAGS.intersection(arg.partition("=")[0] for arg in arguments):
        raise ValueError("A Code Coverage flag reached the Unity executable.")
    if "Code Coverage is enabled." in contents:
        raise ValueError("Unity reported enabled Code Coverage.")

    for flag in ("-batchmode", "-runTests", "-nographics", "-debugCodeOptimization"):
        if flag not in arguments:
            raise ValueError(f"Missing {flag} in Editor.log.")
    workspace_results = f"/github/workspace/{log.parent.as_posix()}"
    expected_values = {
        "-testPlatform": "editmode",
        "-job-worker-count": "2",
        "-logFile": f"{workspace_results}/editmode.log",
        "-testResults": f"{workspace_results}/editmode-results.xml",
    }
    if validation:
        expected_values["-testFilter"] = VALIDATION_TEST
    elif "-testFilter" in arguments:
        raise ValueError("The full EditMode run must not have a filter.")
    for flag, value in expected_values.items():
        positions = [i for i, arg in enumerate(arguments[:-1]) if arg == flag]
        if len(positions) != 1 or arguments[positions[0] + 1] != value:
            raise ValueError(f"Unexpected or missing {flag} in Editor.log.")
    print("Verified real Unity Editor arguments, NUnit output path and disabled Coverage.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("log", type=Path)
    parser.add_argument("--validation", action="store_true")
    args = parser.parse_args()
    try:
        verify(args.log, args.validation)
    except (OSError, ValueError) as error:
        parser.exit(1, f"{error}\n")
