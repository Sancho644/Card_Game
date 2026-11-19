# CI проекта Card_Game

Workflow [Unity CI](../.github/workflows/unity-ci.yml) запускается при push в `main`, при открытии, обновлении и повторном открытии PR в `main`, а также вручную. Для одной ветки или PR новый запуск отменяет предыдущий. Используются временные GitHub-hosted runners Ubuntu 24.04 и Unity из `ProjectSettings/ProjectVersion.txt` (сейчас `6000.6.7f1`).

| Job | Что проверяет | Артефакт |
| --- | --- | --- |
| Unity Tests | Импорт пакетов и компиляция Editor-кода, все EditMode-тесты без фильтра, наличие пройденных тестов во всех четырёх текущих сборках | `unity-editmode-results` |
| Card Database Validation | Отдельный прогон `ProjectCardDatabaseValidation`. Использует общий код меню **Tools → Game → Validate Cards** и `CardDatabase.Validate()`. Проверяет каталог, эффекты, именные способности, варианты баланса и колоды | `unity-card-validation` |
| Windows Build | Release-игрок `StandaloneWindows64`, текущий Mono backend, включённые сцены `Bootstrap`, `MainMenu` и `Match` из Editor Build Settings | `CardGame-Windows-x64-<run>-<attempt>` и `unity-windows-build-log` |

Проверки запускаются последовательно, чтобы один запуск CI не занимал два места лицензии одновременно. Проверка каталога публикует свой отчёт даже после падения общего прогона. Сборка требует успеха обеих проверок. Все три job пропускаются для PR из внешних fork.

Unity Library кэшируется отдельно для тестов и Windows. В ключ входит версия редактора, содержимое `Packages/` и хэш исходников проекта. При изменении исходников Unity может переиспользовать предыдущий импорт для той же версии редактора и пакетов, а изменившиеся ассеты импортирует заново. После job GitHub удаляет временную машину. В Git хранятся workflow и скрипты проверки, артефакты хранятся 14 дней в Actions.

## Лицензия и первый запуск

В репозитории откройте **Settings → Secrets and variables → Actions → New repository secret**. Добавьте набор секретов, соответствующий вашей лицензии.

| Лицензия | Секреты |
| --- | --- |
| Unity Personal | `UNITY_LICENSE` (полное содержимое действующего `.ulf` из Unity Hub), `UNITY_EMAIL`, `UNITY_PASSWORD` |
| Unity Pro / подходящая лицензия с серийным номером | `UNITY_SERIAL`, `UNITY_EMAIL`, `UNITY_PASSWORD` |

Для Personal оставьте `UNITY_SERIAL` незаданным, для Pro используйте серийный номер вместо `UNITY_LICENSE`. Подробности активации и возврата места лицензии описаны в [документации GameCI](https://game.ci/docs/github/activation/). Передавайте значения через настройки GitHub, проверяйте изменения в доверенных ветках и оставляйте файлы лицензий и пароли вне репозитория. Сетевые тесты протокола выполняются локально внутри процесса и не требуют учётных данных Unity Gaming Services.

Для проверки до слияния отправьте feature-ветку в этот же репозиторий и откройте PR в `main`. После появления workflow в основной ветке его можно запустить через **Actions → Unity CI → Run workflow** и выбрать ветку. Для защиты слияния отметьте **Unity Tests** и **Card Database Validation** как обязательные проверки для `main` после первого успешного прогона. GitHub может считать пропущенные job успешными, поэтому PR из внешнего fork нужно отдельно проверить на доверенной ветке перед слиянием.

## Результаты и диагностика

В **Actions → Unity CI → нужный запуск** откройте job для просмотра шагов. В разделе **Artifacts** скачайте архив с NUnit XML и логом Editor. Для запуска игры скачайте архив `CardGame-Windows-x64-…`, распакуйте целиком и запустите `CardGame.exe`. Для сетевой игры передайте обоим игрокам один архив.

- **Лицензия.** Проверьте названия секретов и соответствие набора типу лицензии. Если активация не дошла до старта Editor, внутри артефакта может не быть Editor.log. Диагностика в таком случае находится в шаге GameCI. Проверьте доступность места лицензии при одновременных запусках CI на разных ветках.
- **Импорт или компиляция.** В логе найдите первое сообщение Package Manager или `error CS…`. Проверьте версию Unity и согласованность `manifest.json` с `packages-lock.json`. При ошибке компиляции NUnit XML может отсутствовать.
- **Тесты.** NUnit XML содержит имя, сообщение и stack trace теста. Неполный прогон или отсутствие пройденных тестов в одной из обязательных сборок также завершает job ошибкой.
- **Каталог.** Ошибка теста указывает каталог и затронутый ассет или колоду. Для воспроизведения запустите `ProjectCardDatabaseValidation` в Unity Test Runner.
- **Сборка.** Проверьте лог job и наличие модуля Windows Mono в образе GameCI для версии из `ProjectVersion.txt`. При недоступности образа сборка завершается ошибкой до запуска Editor.

## Новые EditMode-тесты

Размещайте NUnit-тесты под `Assets/Scripts/Tests/EditMode/` в подходящей сборке `Game.Core.Tests.EditMode`, `Game.Data.Tests.EditMode`, `Game.AI.Tests.EditMode` или `Game.Presentation.Tests.EditMode`. Все EditMode-сборки Unity обнаруживает автоматически. Для новой сборки настройте Editor-only asmdef по образцу существующих. Добавьте её в список обязательных сборок в [проверке XML](../.github/scripts/check-unity-results.py), если CI должен отлавливать и её исчезновение.

Локальный запуск отдельной копии проекта с уже активированной лицензией:

```powershell
New-Item -ItemType Directory -Force Logs/CI/EditMode | Out-Null
Start-Process -FilePath 'D:/Unity/Editors/6000.6.7f1/Editor/Unity.exe' -ArgumentList "-batchmode -nographics -projectPath `"$PWD`" -runTests -testPlatform EditMode -testResults Logs/CI/EditMode/editmode-results.xml -logFile Logs/CI/EditMode/editmode.log" -Wait -PassThru
python .github/scripts/check-unity-results.py Logs/CI/EditMode/editmode-results.xml
```

См. также [GameCI Test Runner](https://game.ci/docs/github/test-runner/), [GameCI Builder](https://game.ci/docs/github/builder/) и [справочник GitHub Actions](https://docs.github.com/en/actions/reference/workflows-and-actions/workflow-syntax).
