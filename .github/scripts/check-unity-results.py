"""Fail CI if Unity did not execute the expected EditMode tests."""
import argparse
from pathlib import Path
import xml.etree.ElementTree as ET


ASSEMBLIES = {
    'Game.Core.Tests.EditMode',
    'Game.AI.Tests.EditMode',
    'Game.Data.Tests.EditMode',
    'Game.Presentation.Tests.EditMode',
}
VALIDATION_TEST = 'Game.Data.Tests.FirstCardSetTests.ProjectCardDatabaseValidation'


def verify(report: Path, validation: bool) -> None:
    root = ET.parse(report).getroot()
    if root.tag != 'test-run':
        raise ValueError('Expected a Unity NUnit test-run report.')

    cases = root.findall('.//test-case')
    failed = [case for case in cases if case.get('result') == 'Failed']
    for case in failed:
        print(f"{case.get('fullname')}: {case.findtext('failure/message', '')}")
    passed = sum(case.get('result') == 'Passed' for case in cases)
    print(f'{len(cases)} test cases, {passed} passed, {len(failed)} failed.')

    if root.get('result') != 'Passed' or not passed or failed or int(root.get('failed', 0)):
        raise ValueError('Unity EditMode tests did not pass. See the XML and Editor log.')

    if validation:
        if len(cases) != 1 or cases[0].get('fullname') != VALIDATION_TEST:
            raise ValueError('The card database validation test was not executed in isolation.')
        return

    executed = {
        Path(suite.attrib['name'].replace('\\', '/')).name.removesuffix('.dll')
        for suite in root.iter('test-suite')
        if suite.get('type') == 'Assembly'
        and any(case.get('result') == 'Passed' for case in suite.iter('test-case'))
    }
    if missing := ASSEMBLIES - executed:
        raise ValueError(f"Missing passing tests in assemblies: {', '.join(sorted(missing))}")


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('report', type=Path)
    parser.add_argument('--validation', action='store_true')
    args = parser.parse_args()
    try:
        verify(args.report, args.validation)
    except (ET.ParseError, OSError, ValueError) as error:
        parser.exit(1, f'{error}\n')
