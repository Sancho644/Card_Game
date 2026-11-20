"""Local tests for the argument contract of GameCI v4.3.2."""

import contextlib
import io
from pathlib import Path
import runpy
import sys
import unittest
from unittest.mock import patch

WRAPPER = Path(__file__).with_name("unity-editor-no-coverage.py")
FILTER = "Game.Data.Tests.FirstCardSetTests.ProjectCardDatabaseValidation"
GAMECI_ARGS = [
    "-batchmode", "-logFile", "/github/workspace/Logs/CI/EditMode/editmode.log",
    "-projectPath", "/github/workspace/.", "-coverageResultsPath", "/github/workspace/Code Coverage",
    "-runTests", "-testPlatform", "editmode",
    "-testResults", "/github/workspace/Logs/CI/EditMode/editmode-results.xml",
    "-enableCodeCoverage", "-debugCodeOptimization",
    "-coverageOptions", "generateAdditionalMetrics;generateHtmlReport;generateBadgeReport;dontClear",
    "-nographics", "-job-worker-count", "2",
]
EXPECTED = [
    "-batchmode", "-logFile", "/github/workspace/Logs/CI/EditMode/editmode.log",
    "-projectPath", "/github/workspace/.", "-runTests", "-testPlatform", "editmode",
    "-testResults", "/github/workspace/Logs/CI/EditMode/editmode-results.xml",
    "-debugCodeOptimization", "-nographics", "-job-worker-count", "2",
]


class LauncherTests(unittest.TestCase):
    def execute(self, args):
        output = io.StringIO()
        with patch.object(sys, "argv", [str(WRAPPER), *args]), \
                patch("os.execv") as execv, contextlib.redirect_stdout(output):
            runpy.run_path(str(WRAPPER), run_name="__main__")
        execv.assert_called_once()
        executable, command = execv.call_args.args
        self.assertEqual(executable, "/usr/bin/unity-editor")
        self.assertEqual(command[0], executable)
        return command[1:], output.getvalue()

    def test_full_editmode_keeps_noncoverage_flags_and_order(self):
        args, log = self.execute(GAMECI_ARGS)
        self.assertEqual(args, EXPECTED)
        self.assertIn("Wrapper invoked", log)
        self.assertIn("Coverage arguments detected: 3", log)
        self.assertIn("-job-worker-count 2=True", log)

    def test_card_filter_kept(self):
        filter_args = ["-testFilter", FILTER]
        args, log = self.execute(GAMECI_ARGS + filter_args)
        self.assertEqual(args, EXPECTED + filter_args)
        self.assertIn("-testFilter=True", log)

    def test_license_activation_and_return_are_unmodified_and_silent(self):
        for mode in ["-serial", "-returnlicense"]:
            with self.subTest(mode=mode):
                command = [mode, "-username", "test-email", "-password", "test-password", "-quit"]
                self.assertEqual(self.execute(command), (command, ""))

    def test_empty_or_missing_value_never_eats_following_switch(self):
        self.assertEqual(self.execute(["-runTests", "-coverageOptions", "", "-nographics"])[0],
                         ["-runTests", "-nographics"])
        self.assertEqual(self.execute(["-runTests", "-coverageResultsPath", "-nographics"])[0],
                         ["-runTests", "-nographics"])

    def test_values_have_no_place_in_diagnostic_output(self):
        args, log = self.execute(GAMECI_ARGS + ["-otherFlag", "hidden-value"])
        self.assertEqual(args, EXPECTED + ["-otherFlag", "hidden-value"])
        self.assertNotIn("hidden-value", log)
        self.assertNotIn("/github/workspace/", log)


if __name__ == "__main__":
    unittest.main()
