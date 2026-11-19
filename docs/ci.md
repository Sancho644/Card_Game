# CI проекта Card_Game

Workflow [Unity CI](../.github/workflows/unity-ci.yml) запускается при push в `main`, при открытии, обновлении и повторном открытии PR в `main`, а также вручную. Для одной ветки или PR новый запуск отменяет предыдущий. Используются временные GitHub-hosted runners Ubuntu 24.04 и Unity из `ProjectSettings/ProjectVersion.txt` (сейчас `6000.6.7f1`).

| Job | Что проверяет | Артефакт |
| --- | --- | --- |
| Unity Tests | Импорт пакетов и компиляция Editor-кода, все EditMode-тесты без фильтра, наличие пройденных тестов во всех четырёх текущих сборках | `unity-editmode-results`, `unity-tests-memory-diagnostics` |
| Card Database Validation | Отдельный прогон `ProjectCardDatabaseValidation`. Использует общий код меню **Tools → Game → Validate Cards** и `CardDatabase.Validate()`. Проверяет каталог, эффекты, именные способности, варианты баланса и колоды | `unity-card-validation` |
| Windows Build | Release-игрок `StandaloneWindows64`, текущий Mono backend, включённые сцены `Bootstrap`, `MainMenu` и `Match` из Editor Build Settings | `CardGame-Windows-x64-<run>-<attempt>` и `unity-windows-build-log` |

Проверки запускаются последовательно, чтобы один запуск CI не занимал два места лицензии одновременно. Проверка каталога публикует свой отчёт даже после падения общего прогона. Сборка требует успеха обеих проверок. Все три job пропускаются для PR из внешних fork.

Unity Library кэшируется отдельно для тестов и Windows. В ключ входит версия редактора, содержимое `Packages/` и хэш исходников проекта. При изменении исходников Unity может переиспользовать предыдущий импорт для той же версии редактора и пакетов, а изменившиеся ассеты импортирует заново. После job GitHub удаляет временную машину. В Git хранятся workflow и скрипты проверки, артефакты хранятся 14 дней в Actions.

В [первом запуске CI](https://github.com/Sancho644/Card_Game/actions/runs/44451051640) Windows runner начал с пустой Library и сохранил кэш после сборки. Сообщение GameCI об отсутствии Library ожидаемо, когда подходящего кэша ещё нет или его вытеснил GitHub. Проверить попадание в кэш можно в шаге `actions/cache`. Windows runner восстанавливает свою Library из предыдущих Windows-сборок, а Card Database Validation использует кэш EditMode. Ключи восстановления ограничены версией Unity и хэшем пакетов. Кэш, созданный внутри PR, доступен повторным запускам того же PR, поэтому первый запуск нового PR всё ещё может начать с пустой Library. [Область доступа к кэшу GitHub](https://docs.github.com/en/actions/reference/workflows-and-actions/dependency-caching#restrictions-for-accessing-a-cache)

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

### Unity Tests с кодом 137

В [запуске 44461564171](https://github.com/Sancho644/Card_Game/actions/runs/44461564171) Editor завершился с кодом 137 после компиляции, при загрузке сборок и до записи NUnit XML. Было включено Code Coverage. Runner сообщил 15 701 МБ памяти, а GameCI задал контейнеру лимит 14 916 МБ и 4 CPU. Сам журнал не подтверждает OOM. Отдельная проверка каталога завершилась успешно.

В основном **Unity Tests** Code Coverage временно отключён для диагностики. В теге v4.3.2 параметра `coverageEnabled` ещё нет, а скрипт запуска всегда передаёт `-enableCodeCoverage`. Тестовый образ использует ту же версию Unity и убирает только аргументы Coverage из запуска `-runTests`. GameCI передаёт `-nographics -job-worker-count 2` через `customParameters`. Подтверждение этих флагов появится в выводе обёртки и в командной строке тестового Editor в `editmode.log`. В этом задании по-прежнему выполняются все EditMode-тесты и проверяется NUnit XML. Card Database Validation продолжает работать в исходной конфигурации. [Входы тега GameCI](https://raw.githubusercontent.com/game-ci/unity-test-runner/v4.3.2/action.yml) · [Скрипт запуска](https://raw.githubusercontent.com/game-ci/unity-test-runner/v4.3.2/dist/platforms/ubuntu/run_tests.sh)

Артефакт `unity-tests-memory-diagnostics` сохраняется и при падении тестов. `memory.log` содержит замеры во время работы контейнера, включая RSS процессов в КиБ, счётчики cgroup v2 и `docker stats`. `events.log` сохраняет события Docker, в том числе `oom`, `kill` и `die`, даже если GameCI удалил контейнер. Сборщик читает только имена процессов, числовые показатели и отдельные поля Docker. Если следующий прогон пройдёт, его результат покажет, что сработала комбинация настроек. Сам по себе он не определит вклад Coverage и числа рабочих потоков по отдельности.

### Журнал Windows Build

GameCI Builder 5.0.1 передаёт редактору `-logfile /dev/stdout`. Второй `-logFile` из `customParameters` в первом запуске дошёл до Unity, но журнал остался в stdout. Windows-задача теперь использует тот же образ Unity Windows Mono с [обёрткой unity-editor](../.github/ci/unity-editor). Для сборки она заменяет путь в исходном аргументе на `Logs/CI/Windows/Editor.log`, выводит журнал в job и возвращает исходный код завершения Unity.

Если Editor записал непустой файл, Actions загружает его и при успешной, и при неудачной сборке. Успешная сборка без журнала считается ошибкой CI. Если лицензирование или запуск контейнера прервались до записи файла, подробности остаются в логе job.

### Windows Build с кодом 137

В [запуске 44457445880](https://github.com/Sancho644/Card_Game/actions/runs/44457445880) GameCI восстановил около 935 МиБ Windows Library по резервному ключу. Runner сообщил 15 701 МБ памяти, контейнер получил лимит 14 916 МБ и 4 CPU. GameCI последовательно запустил Unity для активации лицензии на `/BlankProject`, один раз для сборки Card_Game и затем для возврата лицензии. В журнале сборки зарегистрированы Licensing Client, UPM, предварительный запуск VBCSCompiler и графика llvmpipe. Он обрывается после регистрации модулей Windows и Linux до отчёта BuildPipeline.

`Killed` и код 137 означают SIGKILL. Источник сигнала в журнале этого запуска отсутствует. Размер кэша и лимит контейнера не показывают фактическое потребление памяти. Windows Build теперь ограничивает Unity JobQueue двумя worker и запускает импорт Asset Database последовательно внутри Editor. Это сокращает возможную конкуренцию за память ценой более медленного импорта. Схема лицензирования, лимиты контейнера и отдельный Windows-кэш остаются прежними. [Параметры Unity 6000.6](https://docs.unity.com/en-us/engine/6000.6/manual/unity-editor/command-line-arguments/editor)

Артефакт `unity-windows-build-resources-<attempt>` содержит RSS процессов в КиБ, `memory.current`, `memory.peak`, `memory.max` и `memory.events` контейнера, если доступен cgroup v2. Начальный снимок включает состояние после активации, поэтому `memory.peak` может уже отражать предыдущий запуск Editor в том же контейнере. Файл `runner-resources.log` содержит снимки памяти runner и события Docker. Рост `oom_kill` подтверждает, что OOM killer завершил процесс контейнера. Чтобы отличить собственный лимит контейнера от нехватки памяти на runner, сопоставьте также события `max` и `oom` с пиком памяти. [Счётчики cgroup v2](https://www.kernel.org/doc/html/latest/admin-guide/cgroup-v2.html#memory-interface-files)

Unity поддерживает `-nographics` в batchmode, включая запуск команды сборки Windows x64, но предупреждает, что этот режим не поддерживает запекание GI. Локальная попытка headless-сборки остановилась на соединении с Unity Licensing Client. Проверка материалов и карт в Player, собранном таким способом, ещё не выполнена. В CI сохранён прежний графический режим. Перед публикацией Windows-архива проверяются файлы всех включённых сцен и общих ресурсов.

### Unity Services и URP

- **Services Core.** Предсборочная проверка версии 1.18.0 требует одновременно привязки проекта и входа пользователя в редактор. У открытого локального проекта оба условия выполнялись, а `Application.cloudProjectId` совпадал с ID в Player Settings. Активация лицензии GameCI сама по себе не создаёт сессию входа пользователя Unity Editor для этой проверки. Если перестанут работать комнаты, откройте **Edit → Project Settings → Services** в редакторе после входа через Unity Hub. Сверьте ID с **Unity Dashboard → Projects → Card → Settings**, затем проверьте доступ к Authentication и Relay для того же проекта и окружения. Если редактор действительно отвязан, выберите существующий проект с ID `77fc0387-f067-49c9-9e11-c212a8fa25aa`. [Инструкция Unity](https://docs.unity.com/en-us/services/getting-started)
- **Terrain shader stripping.** Предупреждения об удалении всех вариантов `Universal Render Pipeline/Terrain/Lit` и его вспомогательных проходов допустимы для текущих сцен. В сценах и префабах игры нет Terrain, а стол построен на мешах. Если в будущем появится Terrain или видимый материал станет розовым, проверьте конкретный шейдер и настройки URP для Windows. [Документация URP](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@17.0/manual/shader-stripping.html)

## Новые EditMode-тесты

Размещайте NUnit-тесты под `Assets/Scripts/Tests/EditMode/` в подходящей сборке `Game.Core.Tests.EditMode`, `Game.Data.Tests.EditMode`, `Game.AI.Tests.EditMode` или `Game.Presentation.Tests.EditMode`. Все EditMode-сборки Unity обнаруживает автоматически. Для новой сборки настройте Editor-only asmdef по образцу существующих. Добавьте её в список обязательных сборок в [проверке XML](../.github/scripts/check-unity-results.py), если CI должен отлавливать и её исчезновение.

Локальный запуск отдельной копии проекта с уже активированной лицензией:

```powershell
New-Item -ItemType Directory -Force Logs/CI/EditMode | Out-Null
Start-Process -FilePath 'D:/Unity/Editors/6000.6.7f1/Editor/Unity.exe' -ArgumentList "-batchmode -nographics -projectPath `"$PWD`" -runTests -testPlatform EditMode -testResults Logs/CI/EditMode/editmode-results.xml -logFile Logs/CI/EditMode/editmode.log" -Wait -PassThru
python .github/scripts/check-unity-results.py Logs/CI/EditMode/editmode-results.xml
```

См. также [GameCI Test Runner](https://game.ci/docs/github/test-runner/), [GameCI Builder](https://game.ci/docs/github/builder/) и [справочник GitHub Actions](https://docs.github.com/en/actions/reference/workflows-and-actions/workflow-syntax).
