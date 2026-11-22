# P0 Audio Library — QA и приёмка

## Повторная проверка 22 ноября 2026 года

Unity 6000.6.7f1, `feature/audio-assets-p0`. Исправления проверены в рабочей копии `D:\Unity\Projects\Card_Game` поверх `5706e6a`.

**Технический статус — PASS.** Все три замечания закрыты в Play Mode на ×1, ×2 и ×8. Акустическая приёмка на устройствах остаётся открытой.

| Замечание | До исправления | ×1 | ×2 | ×8 |
| --- | --- | --- | --- | --- |
| Повтор `PowerUp` | Два сигнала за один пересчёт с интервалом около 6 мс | Один на направление | Один на направление | Один на направление |
| Ранний `FieldRemove` | На 1,011–1,014 с раньше движения при ×1 и на 0,508 с при ×2 | 2,35–2,49 мс | 2,61–2,70 мс | 2,48–2,58 мс |
| Поздний `CreatureOut` | Около 1,03 с после начала исхода при ×1. Яркость уже 0,25 при длительности затемнения 0,65 с | 4,77–7,26 мс | 4,00–5,43 мс | 4,01–5,28 мс |

После исправления для двух последних строк измерен интервал от вызова `ISoundPlayer.Play` до первого замера видимого изменения. Вызов и изменение попали в один и тот же кадр. Замер отражает порядок событий Unity и не измеряет задержку звукового устройства.

### Причины и исправления

- **`PowerUp` и `PowerDown`**. В `BattlePowerPresenter.cs` вызов в цикле по существам повторял сигнал, когда менялись оба показателя. Каждый пересчёт теперь воспроизводит первый звук каждого направления. Анимации чисел, подсветка и всплывающие сообщения остаются индивидуальными. Первое отображение и повтор того же значения проходят без звуков силы.
- **`FieldRemove`**. Раньше сигнал запускался на 67% длительности последовательности исхода, а карта начинала движение после неё. `BattleOutcomeAudio.cs` и `MatchPresenter.cs` передают в `TableSync.cs` подтверждённый переход активного поля в стопку удалённых. `TableSync` расходует этот сигнал один раз в `OnStart` движения уже отображённой карты. Мгновенное восстановление, повторная синхронизация и сохранение поля проходят без сигнала.
- **`CreatureOut`**. Раньше сигнал запускался на 34% длительности всей последовательности исхода. `BattleResolvedPresenter.cs` и `BattleOutcomeAudio.cs` запускают его с первым tween затемнения, созданным для фактического события `CreatureOut`. Одно или два одновременных выбытия дают один сигнал. Исход без затемняющихся существ проходит без `CreatureOut`. `BattleWin` и `BattleLose` звучат в начале показа исхода.

### Проверки

- **EditMode и Card Database Validation — PASS.** 405 из 405 тестов, 0 падений и пропусков. В набор входит `ProjectCardDatabaseValidation`. `BattleAudioTests.cs` покрывает изменение силы одного существа, оба одинаковых направления, смешанный пересчёт и повтор снимка. Также проверены начало движения поля, мгновенная синхронизация, повторы и поле, сохранённое «Плацдармом». `AudioRuntimeTests.cs` проверяет первый кадр затемнения одного и двух существ, а также исход без затемнения.
- **Проверка регрессий — PASS.** Контрольный прогон с прежним вызовом силы, прежней долей времени для `CreatureOut` и опережающим вызовом `FieldRemove` дал ровно 5 ожидаемых падений из 29. Упали проверки двух повышений и двух понижений, одного и двух затемнений и удаления поля до движения. Затем исправления восстановлены побайтно.
- **Play Mode и сетевые повторы — PASS.** 12 битв в сцене Match. Команды выполнял `OnlineAuthority`, сцена получала снимки через `OnlineCodec` и `OnlineMatchSession`. Аудиопроба фиксировала время и передавала каждый вызов штатному плееру с назначенными клипами. Из 357 доставок 123 новые версии породили события, 123 повтора и 111 устаревших версий пропущены. `PowerUp` и `PowerDown` прозвучали по одному разу при смешанном пересчёте. «Родовая крепость» дала один `PowerUp` для двух существ, а «Своя земля» дала отдельный `PowerUp` для одного. «Плацдарм» сохранил победителя-гостя и поле с одним выбытием. «Нерушимая» сохранила поле при двух выбытиях. На «Бездне» и «Родовой крепости» понадобилось по одному звуку удаления. В консоли редактора после Play Mode ошибок и предупреждений 0.
- **Windows x64 Release — PASS.** Сборка Unity завершилась за 17,55 с, ошибок и предупреждений 0. Подтверждены PE x64, `CardGame.exe`, `CardGame_Data` и сцены Bootstrap, MainMenu, Match. В `SoundSettings` назначены 16 сигналов и 20 уникальных клипов. Все 20 PCM-клипов прочитаны из файлов Windows Player, для каждого проверен непустой сегмент FSB5. Суммарный размер звуковых сегментов — 494 784 байта.
- **NOT VERIFIED.** Прослушивание на наушниках, колонках и физическом Android, Android Player, внешний Relay между устройствами и повторный интерактивный hot-seat на этой ревизии. Сетевая проверка охватывает обработку кадров в одном процессе редактора.

Локальные доказательства находятся в `Logs/AudioP0Fix/`. `play-mode.tsv` содержит таймкоды и номера кадров, `play-mode-summary.txt` содержит результаты сверки, `packed-audio.json` содержит назначения и подтверждение упаковки, а Windows Player находится в `Windows/`.

## Исходный отчёт от 20 ноября 2026 года


20 ноября 2026 года. Unity 6000.6.7f1. Ветка `feature/audio-assets-p0`, проверенный HEAD `4a5f7b3`, коммит библиотеки `39fa5c4`. Проверка выполнена в `D:\Unity\Projects\Card_Game`.

**Итог — FAIL по критерию отсутствия повторов. Готовность к merge пока не подтверждена.** В Play Mode один пересчёт силы вызвал два одинаковых `PowerUp`. Звук ухода поля прозвучал примерно за секунду до начала движения при ×1. Для `CreatureOut` акцент пришёлся на уже затемнённые карты. WAV, лицензии, назначения, импорт, микшер, тесты и Windows Release прошли технические проверки. Акустическая приёмка остаётся открытой.

| Область | Статус | Результат |
| --- | --- | --- |
| Файлы | PASS | Декодированы все 65 WAV/OGG. 20 игровых WAV и 20 мастеров прошли технические проверки |
| Лицензии | PASS | 25 исходных OGG побайтно подтверждены, совпали SHA‑256 архивов и тексты лицензий |
| SoundSettings и импорт | PASS | 16 сигналов, 20 уникальных клипов, 40 платформенных записей |
| AudioMixer | PASS | Маршруты и независимость уровней измерены в Unity. Сохранение Mute покрыто тестами |
| Игровой звук | FAIL / WARNING | Дублирование `PowerUp` и расхождение двух акцентов с визуальной фазой |
| Повтор сетевых кадров | PASS | Проверены реальный кодек и обработчик версий с двумя получателями |
| Внешний Relay | NOT VERIFIED | Внешнее подключение не запускалось |
| EditMode и Card Database Validation | PASS | 396/396, 0 пропусков, 0 проблем базы карт |
| Windows x64 Release | PASS | 0 ошибок и предупреждений, 20/20 клипов подтверждены по файлам Player |
| Android | PASS / NOT VERIFIED | Настройки импорта проверены. APK, память устройства и физическое прослушивание открыты |
| Акустическая приёмка | NOT VERIFIED | Анализ файлов и выход DSP измерены. Для оценки на слух нужен слушатель |

Сверены [аудиоплан](audio-production-plan.md), [аудиоархитектура](audio-system.md), [происхождение](audio-sources.md) и [предыдущий отчёт](audio-p0-review.md). Проверки редактора выполнялись через Unity MCP на подтверждённом пути проекта.

## Инвентаризация и измерения

Измерены все 65 WAV/OGG. В инвентаризацию сопутствующих материалов входят два текста лицензий, `AudioSources/P0/render_p0.py` и метаданные игровых ассетов. Peak, RMS, хвост и последний сэмпл указаны в dBFS. True peak — оценка в dBTP с четырёхкратной передискретизацией. Атака — первый сэмпл выше −52 dBFS. Хвост — RMS последних 5 мс. «Полная шкала» для OGG отмечает значение ±1 или выше после декодирования Vorbis.

**PASS для игровых файлов и мастеров.** Рабочие WAV дают −19,0…−6,5 dBTP, RMS −37,6…−19,6 dBFS, атаку до 13,1 мс и уровень хвоста не выше −68,5 dBFS. Сэмплов на полной шкале и резких срезов по заданному порогу нет. Не найдено идентичных файлов по SHA‑256 и идентичного декодированного PCM при равных частоте и каналах. Повторный экспорт в памяти из сохранённых OGG и `render_p0.py` побайтно совпал со всеми 20 мастерами и 20 игровыми WAV.

У 16 сырых OGG есть технические отметки по пикам или уровню на срезе. Эти файлы служат исходниками. После обработки все готовые WAV прошли контроль запаса и затухания. Все 25 исходных OGG использованы.

### Игровые WAV

Корень `Assets/Audio/SFX/`.

| Файл | Формат | Гц | Кан. | Бит | с | Байты | Peak | RMS | True peak | Атака, мс | Хвост / финал | Сэмплы на полной шкале | Состояние |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| `Abilities/ability-common-02.wav` | WAV PCM | 44 100 | 1 | 16 | 0,291 | 25 712 | −15,8 | −27,3 | −15,8 | 0,0 | −∞ / −∞ | 0 | PASS |
| `Abilities/ability-common.wav` | WAV PCM | 44 100 | 1 | 16 | 0,267 | 23 594 | −15,5 | −26,8 | −15,5 | 0,0 | −99,0 / −∞ | 0 | PASS |
| `Battle/battle-creature-out.wav` | WAV PCM | 44 100 | 1 | 16 | 0,309 | 27 298 | −14,0 | −25,5 | −14,0 | 7,1 | −95,8 / −∞ | 0 | PASS |
| `Battle/battle-field-remove.wav` | WAV PCM | 44 100 | 1 | 16 | 0,409 | 36 118 | −12,0 | −26,7 | −12,0 | 0,0 | −∞ / −∞ | 0 | PASS |
| `Battle/battle-lose.wav` | WAV PCM | 44 100 | 1 | 16 | 0,579 | 51 112 | −11,0 | −22,3 | −11,0 | 0,1 | −91,9 / −∞ | 0 | PASS |
| `Battle/battle-power-down.wav` | WAV PCM | 44 100 | 1 | 16 | 0,237 | 20 948 | −14,0 | −24,8 | −14,0 | 0,1 | −91,7 / −∞ | 0 | PASS |
| `Battle/battle-power-up.wav` | WAV PCM | 44 100 | 1 | 16 | 0,218 | 19 272 | −12,0 | −22,7 | −12,0 | 0,1 | −91,9 / −∞ | 0 | PASS |
| `Battle/battle-start.wav` | WAV PCM | 44 100 | 1 | 16 | 0,726 | 64 078 | −6,5 | −19,6 | −6,5 | 10,2 | −98,4 / −∞ | 0 | PASS |
| `Battle/battle-win.wav` | WAV PCM | 44 100 | 1 | 16 | 0,562 | 49 614 | −8,0 | −20,1 | −8,0 | 0,1 | −∞ / −∞ | 0 | PASS |
| `Battle/turn-local.wav` | WAV PCM | 44 100 | 1 | 16 | 0,384 | 33 914 | −13,0 | −23,8 | −13,0 | 0,1 | −91,2 / −∞ | 0 | PASS |
| `Cards/card-deal-02.wav` | WAV PCM | 44 100 | 1 | 16 | 0,112 | 9 924 | −16,6 | −35,6 | −16,6 | 8,1 | −68,5 / −∞ | 0 | PASS |
| `Cards/card-deal-03.wav` | WAV PCM | 44 100 | 1 | 16 | 0,138 | 12 216 | −16,1 | −37,6 | −16,1 | 0,0 | −103,3 / −∞ | 0 | PASS |
| `Cards/card-deal.wav` | WAV PCM | 44 100 | 1 | 16 | 0,122 | 10 806 | −16,0 | −35,4 | −16,0 | 9,9 | −73,4 / −∞ | 0 | PASS |
| `Cards/card-discard.wav` | WAV PCM | 44 100 | 1 | 16 | 0,145 | 12 834 | −15,0 | −29,3 | −15,0 | 13,0 | −∞ / −∞ | 0 | PASS |
| `Cards/card-field-reveal.wav` | WAV PCM | 44 100 | 1 | 16 | 0,335 | 29 592 | −11,0 | −25,7 | −11,0 | 0,2 | −∞ / −∞ | 0 | PASS |
| `Cards/card-pick.wav` | WAV PCM | 44 100 | 1 | 16 | 0,095 | 8 424 | −17,2 | −32,9 | −17,0 | 0,0 | −∞ / −∞ | 0 | PASS |
| `Cards/card-place-creature-02.wav` | WAV PCM | 44 100 | 1 | 16 | 0,171 | 15 128 | −12,0 | −27,4 | −12,0 | 0,0 | −98,5 / −∞ | 0 | PASS |
| `Cards/card-place-creature.wav` | WAV PCM | 44 100 | 1 | 16 | 0,153 | 13 540 | −11,5 | −25,5 | −11,5 | 4,1 | −89,8 / −∞ | 0 | PASS |
| `Cards/card-place-field.wav` | WAV PCM | 44 100 | 1 | 16 | 0,254 | 22 448 | −9,0 | −25,7 | −9,0 | 0,0 | −91,8 / −∞ | 0 | PASS |
| `UI/ui-click.wav` | WAV PCM | 44 100 | 1 | 16 | 0,074 | 6 572 | −19,0 | −31,3 | −19,0 | 0,1 | −95,3 / −∞ | 0 | PASS |

### WAV-мастера

Корень `AudioSources/P0/Masters/`.

| Файл | Формат | Гц | Кан. | Бит | с | Байты | Peak | RMS | True peak | Атака, мс | Хвост / финал | Сэмплы на полной шкале | Состояние |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| `ability-common-02.wav` | WAV PCM | 48 000 | 1 | 24 | 0,291 | 41 948 | −15,8 | −27,3 | −15,8 | 0,0 | −∞ / −∞ | 0 | PASS |
| `ability-common.wav` | WAV PCM | 48 000 | 1 | 24 | 0,267 | 38 492 | −15,5 | −26,8 | −15,5 | 0,0 | −111,2 / −∞ | 0 | PASS |
| `battle-creature-out.wav` | WAV PCM | 48 000 | 1 | 24 | 0,309 | 44 540 | −14,0 | −25,5 | −14,0 | 7,1 | −142,8 / −∞ | 0 | PASS |
| `battle-field-remove.wav` | WAV PCM | 48 000 | 1 | 24 | 0,409 | 58 940 | −12,0 | −26,7 | −12,0 | 0,0 | −∞ / −∞ | 0 | PASS |
| `battle-lose.wav` | WAV PCM | 48 000 | 1 | 24 | 0,579 | 83 420 | −11,0 | −22,3 | −11,0 | 0,1 | −140,1 / −∞ | 0 | PASS |
| `battle-power-down.wav` | WAV PCM | 48 000 | 1 | 24 | 0,237 | 34 172 | −14,0 | −24,8 | −14,0 | 0,1 | −127,0 / −∞ | 0 | PASS |
| `battle-power-up.wav` | WAV PCM | 48 000 | 1 | 24 | 0,218 | 31 436 | −12,0 | −22,7 | −12,0 | 0,1 | −99,1 / −∞ | 0 | PASS |
| `battle-start.wav` | WAV PCM | 48 000 | 1 | 24 | 0,726 | 104 588 | −6,5 | −19,6 | −6,5 | 10,2 | −146,7 / −∞ | 0 | PASS |
| `battle-win.wav` | WAV PCM | 48 000 | 1 | 24 | 0,562 | 80 972 | −8,0 | −20,1 | −8,0 | 0,1 | −∞ / −∞ | 0 | PASS |
| `card-deal-02.wav` | WAV PCM | 48 000 | 1 | 24 | 0,112 | 16 172 | −16,7 | −35,6 | −16,6 | 8,1 | −68,2 / −∞ | 0 | PASS |
| `card-deal-03.wav` | WAV PCM | 48 000 | 1 | 24 | 0,138 | 19 916 | −16,3 | −37,6 | −16,1 | 0,0 | −105,1 / −∞ | 0 | PASS |
| `card-deal.wav` | WAV PCM | 48 000 | 1 | 24 | 0,122 | 17 612 | −16,0 | −35,4 | −16,0 | 9,9 | −73,1 / −∞ | 0 | PASS |
| `card-discard.wav` | WAV PCM | 48 000 | 1 | 24 | 0,145 | 20 924 | −15,0 | −29,3 | −15,0 | 13,0 | −104,6 / −∞ | 0 | PASS |
| `card-field-reveal.wav` | WAV PCM | 48 000 | 1 | 24 | 0,335 | 48 284 | −11,0 | −25,7 | −11,0 | 0,2 | −∞ / −∞ | 0 | PASS |
| `card-pick.wav` | WAV PCM | 48 000 | 1 | 24 | 0,095 | 13 724 | −17,0 | −32,9 | −17,0 | 0,0 | −128,7 / −∞ | 0 | PASS |
| `card-place-creature-02.wav` | WAV PCM | 48 000 | 1 | 24 | 0,171 | 24 668 | −12,0 | −27,4 | −12,0 | 0,0 | −146,5 / −∞ | 0 | PASS |
| `card-place-creature.wav` | WAV PCM | 48 000 | 1 | 24 | 0,153 | 22 076 | −11,5 | −25,5 | −11,5 | 4,1 | −92,3 / −∞ | 0 | PASS |
| `card-place-field.wav` | WAV PCM | 48 000 | 1 | 24 | 0,254 | 36 620 | −9,0 | −25,7 | −9,0 | 0,0 | −138,2 / −∞ | 0 | PASS |
| `turn-local.wav` | WAV PCM | 48 000 | 1 | 24 | 0,384 | 55 340 | −13,0 | −23,8 | −13,0 | 0,1 | −139,3 / −∞ | 0 | PASS |
| `ui-click.wav` | WAV PCM | 48 000 | 1 | 24 | 0,074 | 10 700 | −19,0 | −31,3 | −19,0 | 0,1 | −140,0 / −∞ | 0 | PASS |

### Исходные OGG

Корень `AudioSources/P0/Foley/`.

| Файл | Формат | Гц | Кан. | Бит | с | Байты | Peak | RMS | True peak | Атака, мс | Хвост / финал | Сэмплы на полной шкале | Состояние |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| `Casino/Audio/card-fan-1.ogg` | OGG Vorbis | 44 100 | 2 | — | 0,721 | 12 489 | −0,8 | −21,8 | −0,8 | 0,1 | −50,4 / −56,5 | 0 | WARNING · малый запас true peak |
| `Casino/Audio/card-place-1.ogg` | OGG Vorbis | 44 100 | 2 | — | 0,689 | 9 624 | −1,1 | −27,1 | −1,1 | 2,0 | −56,4 / −58,9 | 0 | PASS |
| `Casino/Audio/card-place-2.ogg` | OGG Vorbis | 44 100 | 2 | — | 0,459 | 8 046 | −1,8 | −31,7 | −1,6 | 0,7 | −64,5 / −62,7 | 0 | PASS |
| `Casino/Audio/card-place-3.ogg` | OGG Vorbis | 44 100 | 2 | — | 0,883 | 11 503 | 0,1 | −32,6 | 0,4 | 0,4 | −57,8 / −59,0 | 1 | WARNING · полная шкала, малый запас true peak |
| `Casino/Audio/card-place-4.ogg` | OGG Vorbis | 44 100 | 2 | — | 0,601 | 9 618 | −1,1 | −29,2 | −1,1 | 0,0 | −46,0 / −44,3 | 0 | WARNING · уровень на срезе хвоста |
| `Casino/Audio/card-shove-1.ogg` | OGG Vorbis | 44 100 | 2 | — | 0,766 | 10 690 | −2,3 | −30,6 | −2,0 | 0,0 | −37,0 / −39,3 | 0 | WARNING · уровень на срезе хвоста |
| `Casino/Audio/card-shove-2.ogg` | OGG Vorbis | 44 100 | 2 | — | 0,767 | 10 445 | −1,4 | −31,9 | −1,4 | 0,0 | −44,4 / −53,5 | 0 | WARNING · уровень на срезе хвоста |
| `Casino/Audio/card-shove-3.ogg` | OGG Vorbis | 44 100 | 2 | — | 0,693 | 9 948 | −0,8 | −27,9 | −0,7 | 0,0 | −44,1 / −36,8 | 0 | WARNING · уровень на срезе хвоста, малый запас true peak |
| `Casino/Audio/card-slide-1.ogg` | OGG Vorbis | 44 100 | 2 | — | 0,601 | 8 833 | −1,5 | −27,6 | −1,5 | 4,0 | −61,9 / −52,4 | 0 | PASS |
| `Casino/Audio/card-slide-2.ogg` | OGG Vorbis | 44 100 | 2 | — | 0,584 | 9 042 | −1,0 | −25,5 | −1,0 | 0,0 | −54,0 / −60,4 | 0 | WARNING · малый запас true peak |
| `Casino/Audio/card-slide-3.ogg` | OGG Vorbis | 44 100 | 2 | — | 0,598 | 9 164 | −1,4 | −26,9 | −1,3 | 0,0 | −55,4 / −48,0 | 0 | PASS |
| `Casino/Audio/card-slide-4.ogg` | OGG Vorbis | 44 100 | 2 | — | 0,459 | 7 912 | −1,7 | −27,7 | −1,6 | 0,0 | −49,0 / −47,5 | 0 | WARNING · уровень на срезе хвоста |
| `Impact/media/audio/impactGeneric_light_002.ogg` | OGG Vorbis | 44 100 | 2 | — | 0,140 | 5 795 | −1,0 | −20,6 | −1,0 | 0,0 | −37,5 / −37,5 | 0 | WARNING · уровень на срезе хвоста, малый запас true peak |
| `Impact/media/audio/impactGeneric_light_003.ogg` | OGG Vorbis | 44 100 | 2 | — | 0,138 | 5 792 | −1,1 | −20,7 | −1,1 | 0,0 | −37,3 / −37,3 | 0 | WARNING · уровень на срезе хвоста |
| `Impact/media/audio/impactMetal_light_002.ogg` | OGG Vorbis | 44 100 | 2 | — | 0,236 | 6 804 | −1,4 | −20,6 | −1,3 | 0,0 | −54,4 / −54,7 | 0 | PASS |
| `Impact/media/audio/impactMetal_light_003.ogg` | OGG Vorbis | 44 100 | 2 | — | 0,482 | 9 023 | −1,5 | −20,7 | −1,5 | 0,0 | −60,2 / −60,4 | 0 | PASS |
| `Impact/media/audio/impactMetal_light_004.ogg` | OGG Vorbis | 44 100 | 2 | — | 0,213 | 6 675 | −1,3 | −20,7 | −1,1 | 0,0 | −51,8 / −52,1 | 0 | PASS |
| `Impact/media/audio/impactPunch_heavy_002.ogg` | OGG Vorbis | 44 100 | 2 | — | 0,457 | 9 025 | −1,0 | −16,6 | −1,0 | 0,4 | −72,8 / −79,5 | 0 | WARNING · малый запас true peak |
| `Impact/media/audio/impactSoft_medium_000.ogg` | OGG Vorbis | 44 100 | 2 | — | 0,118 | 5 351 | −1,0 | −14,3 | −1,0 | 0,0 | −33,8 / −34,0 | 0 | WARNING · уровень на срезе хвоста, малый запас true peak |
| `Impact/media/audio/impactSoft_medium_001.ogg` | OGG Vorbis | 44 100 | 2 | — | 0,183 | 5 506 | −0,9 | −14,9 | −0,9 | 0,1 | −37,0 / −36,9 | 0 | WARNING · уровень на срезе хвоста, малый запас true peak |
| `Impact/media/audio/impactSoft_medium_003.ogg` | OGG Vorbis | 44 100 | 2 | — | 0,140 | 5 392 | −0,9 | −14,3 | −0,9 | 0,0 | −34,5 / −34,5 | 0 | WARNING · уровень на срезе хвоста, малый запас true peak |
| `Impact/media/audio/impactWood_medium_000.ogg` | OGG Vorbis | 44 100 | 2 | — | 0,333 | 6 575 | −1,0 | −22,6 | −1,0 | 0,0 | −49,8 / −50,0 | 0 | WARNING · уровень на срезе хвоста |
| `Impact/media/audio/impactWood_medium_001.ogg` | OGG Vorbis | 44 100 | 2 | — | 0,333 | 6 555 | −1,0 | −22,6 | −1,0 | 0,0 | −49,5 / −49,6 | 0 | WARNING · уровень на срезе хвоста |
| `Impact/media/audio/impactWood_medium_003.ogg` | OGG Vorbis | 44 100 | 2 | — | 0,333 | 6 598 | −1,0 | −22,7 | −1,0 | 0,0 | −51,2 / −51,3 | 0 | PASS |
| `Impact/media/audio/impactWood_medium_004.ogg` | OGG Vorbis | 44 100 | 2 | — | 0,333 | 6 481 | −1,1 | −22,2 | −1,1 | 0,0 | −52,1 / −52,1 | 0 | PASS |


## Лицензирование

Обе страницы Kenney указывают CC0 1.0. Для соответствующих файлов допускаются коммерческое использование, обработка и распространение. Атрибуция добровольная. С архивами сверены именно байты каждого OGG и тексты приложенных лицензий после нормализации отступов. [Casino Audio 1.1](https://kenney.nl/assets/casino-audio) · [Impact Sounds 1.0](https://kenney.nl/assets/impact-sounds) · [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/)

| Архив | SHA‑256 | Лицензия в репозитории | Статус |
| --- | --- | --- | --- |
| Casino Audio | `f36250766ac5bc378c13708ddf12a23a8e54a3251f8d482c7536e51b5dbafa18` | [License.txt](../AudioSources/P0/Foley/Casino/License.txt) | PASS |
| Impact Sounds | `5758f9aaeaa825b30960aa79c508c4f8c09259e7473bf09b15c916226c186a11` | [LICENSE-AND-CREDITS.txt](../AudioSources/P0/Foley/Impact/LICENSE-AND-CREDITS.txt) | PASS |

Фактическое использование внешних файлов получено из скрипта сведения. Все строки таблицы используют CC0 1.0.

| Файл в `AudioSources/P0/Foley/` | Путь в архиве | Автор | Игровые клипы | Статус |
| --- | --- | --- | --- | --- |
| `Casino/Audio/card-fan-1.ogg` | `Audio/card-fan-1.ogg` | Kenney Vleugels | `card-pick.wav` | PASS |
| `Casino/Audio/card-place-1.ogg` | `Audio/card-place-1.ogg` | Kenney Vleugels | `card-deal.wav`, `card-place-creature.wav` | PASS |
| `Casino/Audio/card-place-2.ogg` | `Audio/card-place-2.ogg` | Kenney Vleugels | `card-deal-02.wav` | PASS |
| `Casino/Audio/card-place-3.ogg` | `Audio/card-place-3.ogg` | Kenney Vleugels | `card-deal-03.wav` | PASS |
| `Casino/Audio/card-place-4.ogg` | `Audio/card-place-4.ogg` | Kenney Vleugels | `card-place-creature-02.wav`, `card-discard.wav` | PASS |
| `Casino/Audio/card-shove-1.ogg` | `Audio/card-shove-1.ogg` | Kenney Vleugels | `card-discard.wav` | PASS |
| `Casino/Audio/card-shove-2.ogg` | `Audio/card-shove-2.ogg` | Kenney Vleugels | `battle-field-remove.wav` | PASS |
| `Casino/Audio/card-shove-3.ogg` | `Audio/card-shove-3.ogg` | Kenney Vleugels | `card-place-field.wav` | PASS |
| `Casino/Audio/card-slide-1.ogg` | `Audio/card-slide-1.ogg` | Kenney Vleugels | `card-deal.wav`, `card-discard.wav` | PASS |
| `Casino/Audio/card-slide-2.ogg` | `Audio/card-slide-2.ogg` | Kenney Vleugels | `card-pick.wav` | PASS |
| `Casino/Audio/card-slide-3.ogg` | `Audio/card-slide-3.ogg` | Kenney Vleugels | `card-deal-02.wav`, `card-field-reveal.wav` | PASS |
| `Casino/Audio/card-slide-4.ogg` | `Audio/card-slide-4.ogg` | Kenney Vleugels | `card-deal-03.wav` | PASS |
| `Impact/media/audio/impactGeneric_light_002.ogg` | `media/audio/impactGeneric_light_002.ogg` | Kenney | `card-place-field.wav` | PASS |
| `Impact/media/audio/impactGeneric_light_003.ogg` | `media/audio/impactGeneric_light_003.ogg` | Kenney | `battle-field-remove.wav` | PASS |
| `Impact/media/audio/impactMetal_light_002.ogg` | `media/audio/impactMetal_light_002.ogg` | Kenney | `ability-common.wav` | PASS |
| `Impact/media/audio/impactMetal_light_003.ogg` | `media/audio/impactMetal_light_003.ogg` | Kenney | `card-field-reveal.wav` | PASS |
| `Impact/media/audio/impactMetal_light_004.ogg` | `media/audio/impactMetal_light_004.ogg` | Kenney | `ability-common-02.wav` | PASS |
| `Impact/media/audio/impactPunch_heavy_002.ogg` | `media/audio/impactPunch_heavy_002.ogg` | Kenney | `battle-start.wav` | PASS |
| `Impact/media/audio/impactSoft_medium_000.ogg` | `media/audio/impactSoft_medium_000.ogg` | Kenney | `battle-creature-out.wav` | PASS |
| `Impact/media/audio/impactSoft_medium_001.ogg` | `media/audio/impactSoft_medium_001.ogg` | Kenney | `card-discard.wav` | PASS |
| `Impact/media/audio/impactSoft_medium_003.ogg` | `media/audio/impactSoft_medium_003.ogg` | Kenney | `battle-lose.wav` | PASS |
| `Impact/media/audio/impactWood_medium_000.ogg` | `media/audio/impactWood_medium_000.ogg` | Kenney | `ui-click.wav` | PASS |
| `Impact/media/audio/impactWood_medium_001.ogg` | `media/audio/impactWood_medium_001.ogg` | Kenney | `card-place-creature.wav` | PASS |
| `Impact/media/audio/impactWood_medium_003.ogg` | `media/audio/impactWood_medium_003.ogg` | Kenney | `card-place-creature-02.wav` | PASS |
| `Impact/media/audio/impactWood_medium_004.ogg` | `media/audio/impactWood_medium_004.ogg` | Kenney | `battle-win.wav` | PASS |

`battle-power-up.wav`, `battle-power-down.wav` и `turn-local.wav` полностью синтезированы скриптом проекта. Остальные клипы используют указанные семплы и при необходимости синтезированные слои. Случаев `License Unverified` в P0 нет.

## SoundCue и игровые вызовы

| SoundCue | AudioClip | Точка вызова | Группа Mixer | Повторные события и отмена | Результат |
| --- | --- | --- | --- | --- | --- |
| `CardDeal` | `Cards/card-deal.wav`, `Cards/card-deal-02.wav`, `Cards/card-deal-03.wav` | `DealPresenter` → `TableSync.Add`. Старт движения новых карт при `HandDealt`. | SFX | Доставка защищена версией кадра. Внутри раздачи действует интервал 45 мс между стартами клипов. | PASS |
| `CardPick` | `Cards/card-pick.wav` | `TableInteraction.OnTap` и `OnDragStarted`. Захват доступной карты. | SFX | Локальный сигнал жеста. При отмене перетаскивания захват один, успешного размещения нет. `CardDrop` пуст. | PASS для перетаскивания · NOT VERIFIED для tap в Play Mode |
| `CardPlace` | `Cards/card-place-creature.wav`, `Cards/card-place-creature-02.wav` | `CardSoundPresenter.Arrivals` → `TableSync.Move.OnComplete`. Размещение и подтверждённый вызов. | SFX | Предыдущий и новый снимки должны показать переход в целевой слот. ID извлекается один раз. Есть тест повторной доставки. | PASS для размещения · NOT VERIFIED для ручного вызова |
| `FieldPlace` | `Cards/card-place-field.wav` | `FieldsDealt` с фактическим выходом из резерва → завершение движения в слоте. | SFX | Одна запись на ID и пропуск неизменившегося снимка. Повтор кадра проверен тестом. | PASS |
| `FieldFlip` | `Cards/card-field-reveal.wav` | `FieldOpenedPresenter`. Подтверждённое `FieldOpened`, начало анимации переворота или пульса видимой карты. | SFX | Один вызов после принятого события. Звук появляется в начале переворота. Для оборотной стороны лицо становится видимым в ходе вращения. | PASS для старта раскрытия |
| `AbilityDiscard` | `Cards/card-discard.wav` | `DiscardAudio.NewCards` → `TableSync.Move.OnComplete`. Только новые карты в публичном сбросе. | SFX | При повторе, отклонении команды и одном `AbilityCancelled` нового прибытия нет. Отмена эффекта сыгранной карты всё ещё допускает её подтверждённый сброс. | PASS |
| `BattleStart` | `Battle/battle-start.wav` | `BattleStartedPresenter`. Синхронизация после посадки вступившего существа, затем баннер. | SFX | Наблюдался один вызов на битву в Play Mode. Повтор кадра не публикует событие. | PASS |
| `PowerUp` | `Battle/battle-power-up.wav` | `BattlePowerPresenter.Show`. Перед счётчиком по каждому существу со выросшей силой. | SFX | Дважды за один снимок при росте обеих сил. Не связано с повторной доставкой. | FAIL |
| `PowerDown` | `Battle/battle-power-down.wav` | `BattlePowerPresenter.Show`. Перед счётчиком существа с падением силы. | SFX | Один вызов на уменьшившуюся силу в сценарии разнонаправленных изменений. При падении обеих сил код выполняет тот же цикл, такой матч отдельно не воспроизводился. | PASS для одного уменьшения · WARNING для обоих |
| `BattleWin` | `Battle/battle-win.wav` | `BattleResolvedPresenter` → `BattleOutcomeAudio.Schedule` на старте последовательности. Победа зрителя или общий победный сигнал hot-seat. | SFX | Первый исход в пакете. Победа подтверждена за хозяина в отдельной сцене. | PASS |
| `BattleLose` | `Battle/battle-lose.wav` | Та же последовательность, отдельный клип при победе соперника. | SFX | Подтверждено в реальном матче с AI и со стороны гостя. Сетевые replay-фреймы не повторяют результат. | PASS |
| `CreatureOut` | `Battle/battle-creature-out.wav` | `BattleOutcomeAudio.Schedule`. Один акцент при одном или нескольких `CreatureOut`. | SFX | Два выбывания дали один вызов. При обычной скорости сигнал прозвучал после визуального угасания, до движения в стопку. | WARNING |
| `FieldRemove` | `Battle/battle-field-remove.wav` | `BattleOutcomeAudio.Schedule`. Акцент при фактическом `FieldRemoved` в последствиях битвы. | SFX | Один вызов. Упреждает начало видимого движения карты примерно на 1,01 с при ×1 и 0,51 с при ×2. | WARNING |
| `TurnStart` | `Battle/turn-local.wav` | `TurnBannerPresenter` для зрителя. В hot-seat `MatchPresenter.RequestDecision` потребляет ожидающий локальный ход после передачи. | SFX | В Play Mode звучал при своём ходе. Дополнительная проверка двух сетевых получателей пропускает повторные `TurnStarted`. | PASS для локального/сетевого события · NOT VERIFIED для нового прогона hot-seat |
| `AbilityPlay` | `Abilities/ability-common.wav`, `Abilities/ability-common-02.wav` | `AbilityPlayedPresenter`. Только Common на старте полёта подтверждённой способности. | SFX | Два разных Common дали по одному вызову на ×1 и ×2. Необычная способность в AI-сценарии ушла в сброс без Common-звука. | PASS |
| `Button` | `UI/ui-click.wav` | `UiButtons.Assign`. Активная и доступная кнопка ищет `UiAudioScope`. | UI | Локальный. Тест повторного `Bind` и выключенной кнопки прошёл. Меню и коллекция проверены живыми вызовами привязанных кнопок. | PASS |

## SoundSettings и импорт

**PASS.** Через `AssetDatabase` и `AudioImporter` прочитаны 21 запись `SoundCue` и 40 настроек импорта для 20 клипов. Прежние числовые значения 0–18 сохранены. `FieldPlace=19`, `BattleLose=20`. Каждая ссылка уникальна и ведёт в `Assets/Audio/SFX/`. Все варианты раздачи, размещения существа и Common последовательно проиграны. Громкость каждой из 16 заполненных записей и контентный master trim равны 1. Уровни заложены в самих WAV. `CardDrop`, `Banner`, `Victory`, `Defeat`, `Draw`, Menu Music и Match Music пусты.

| Платформа | Force To Mono | Load Type | Compression | Quality | Sample rate | Preload | Ambisonic | Normalize | Load in background |
| --- | --- | --- | --- | ---: | --- | --- | --- | --- | --- |
| Windows Standalone | Да | Decompress On Load | PCM | 1 | Preserve, 44 100 Гц | Да | Нет | Нет | Нет |
| Android | Да | Decompress On Load | PCM | 1 | Preserve, 44 100 Гц | Да | Нет | Нет | Нет |

Параметры соответствуют диапазонам аудиоплана. Значение Quality хранится в настройках PCM и не задаёт степень сжатия несжатого клипа. ASTC и ETC2 применяются к текстурам и не изменяют аудиокомпрессию. [Unity 6000.6 · Audio Clip Import Settings](https://docs.unity.com/en-us/engine/6000.6/manual/audio/reference/class-audio-clip) · [Audio file compression](https://docs.unity.com/en-us/engine/6000.6/manual/audio/files/compression)

## AudioMixer

**PASS.** `CardGameAudioMixer.mixer` содержит `Master → Music, SFX, UI`. Доступны `MasterVolume`, `MusicVolume`, `SfxVolume` и `UiVolume`. У реального `AppRoot` наблюдались 4 источника SFX, 1 UI, 2 Music. Все 2D, Play On Awake выключен. В меню активен один AudioListener, частота вывода 48 кГц. Для проверки Music временно проигран существующий клип через `MusicPlayer`.

| Измерение | Контрольный пик | Пик после изменения | Результат |
| --- | ---: | ---: | --- |
| UI при UI −12 дБ | 0,079338 | 0,019919 | PASS |
| SFX при UI −12 дБ | 0,334377 | 0,334377 | PASS |
| Music при SFX −12 дБ | 0,334377 | 0,334377 | PASS |
| SFX при Master −6 дБ | 0,334377 | 0,167543 | PASS, ослабление применено однократно |

Все 20 клипов дали выход DSP. Реальные кнопки открытия коллекции и её вкладки каждая дали пик 0,079338. Сохранение ползунков при Mute, fallback при отсутствии параметра и пустой клип покрыты существующими тестами. Временные уровни микшера восстановлены. Проверка обходилась без записи пользовательских настроек. Аудиоплан предлагает начать с трёх источников SFX и одного UI. Текущая реализация использует четыре SFX, как зафиксировано в аудиоархитектуре. Бюджет голосов и DSP CPU в стрессовом сценарии остался **NOT VERIFIED**. [Unity 6000.6 · Audio Mixer](https://docs.unity.com/en-us/engine/6000.6/manual/audio/mixer)

## Синхронизация и геймплей

В Play Mode использовался рабочий проигрыватель со временной записью таймкодов вызовов `ISoundPlayer`. Игра с AI прошла на обычной скорости. Детерминированные сценарии победы хозяина и поражения гостя прошли при ×1 и ×2. Изменение положения поля фиксировалось при первом наблюдаемом смещении более 0,015 единицы Unity.

| Сценарий | Статус | Наблюдение |
| --- | --- | --- |
| Раздача, захват и отмена | PASS | Реальная анимация раздачи. Перетаскивание с отменой дало один `CardPick` и пустой `CardDrop`. `FieldPlace` прозвучал только после отдельной принятой команды |
| Размещение и старт битвы | PASS | Поле и существа дали по одному акценту прибытия. `BattleStart` прозвучал после посадки второго существа |
| Открытие и разнонаправленные силы | PASS | На «Бездне» зарегистрированы `FieldOpened`, `FieldFlip` и по одному `PowerUp` и `PowerDown` |
| Два одновременных роста | FAIL | После открытия поля в AI-матче `PowerUp` в 13,796 с и 13,802 с. Один пересчёт вызывает звук для каждого выросшего показателя |
| Common и сброс | PASS | Приняты `ability.home-ground` и `ability.onslaught`. Каждая дала отдельный `AbilityPlay`, затем `AbilityDiscard` по прибытии. Интервал около 1,50 с при ×1 и 0,75 с при ×2 |
| Победа и поражение | PASS | AI-матч и детерминированная проверка подтвердили разные клипы и порядок `BattleWin/BattleLose → CreatureOut → FieldRemove` |
| Одно и два выбытия | PASS по числу · WARNING по времени | Одно выбытие покрыто EditMode. Два в живой битве дали один звук через ~1,03 с после начала исхода. Обе карты уже были затемнены до 0,25. Длительность затемнения 0,65 с |
| Удаление поля | WARNING | AI ×1: сигнал 21,964 с, движение 22,978 с, разница 1,014 с. Сценарий ×1: 39,099 → 40,110 с. При ×2: 49,961 → 50,469 с |
| Свой ход и итог матча | PASS по доступной проверке | `TurnStart` на локальном ходе. Единичный итоговый сигнал подтверждён `MatchResultAudio` в EditMode. Итоговый клип пуст в соответствии с этапом |
| Tap, ручной вызов, новая проверка hot-seat и ускорение нажатием | NOT VERIFIED | Эти отдельные пути не воспроизводились в данном прогоне |

В измеренных матчах максимальный пик DSP 0,334549. Художественная приемлемость наложения коротких UI и карточных звуков на боевые эффекты остаётся отдельным пунктом прослушивания.

## Локальные и сетевые проверки

**PASS.** EditMode вновь выполнен на `4a5f7b3`: 396 из 396, падений 0, пропусков 0. `CardValidationMenu.FindProblems()` вернул 0 проблем. Проверены сценарии `AudioRuntimeTests`, `DiscardAudioTests`, `P0AudioTests`, вспомогательный `RecordingSoundPlayer` и `OnlineProtocolTests`. Существующие ожидания тестов сохранены. Дополнительные проверки проведены временными сценариями, без записи тестовых компонентов в проект.

Прогон `OnlineAuthority → OnlineCodec → OnlineMatchSession` достиг `Ended`. 2 клиента получили по 95 доставок, включая повтор текущего и устаревшие кадры, и опубликовали по 32 пакета событий, ровно по одному на новую версию. У каждого было 3 своих и 3 чужих `TurnStarted`, 3 `FieldOpened`, 5 `AbilityPlayed`, 3 `BattleResolved` и 1 `MatchEnded`. Для размещения и сброса есть отдельные тесты с повтором доставки и сравнением снимков. Устойчивость доставки и игровые таймкоды проверялись отдельными сценариями. Реальный внешний Relay и слышимый микс между устройствами — **NOT VERIFIED**.

## Windows x64 Release и Android

**Windows — PASS.** Unity вернула `Succeeded` за 13,35 с, ошибок и предупреждений 0. Проверены PE x64, `CardGame.exe`, `CardGame_Data` и сцены Bootstrap, MainMenu, Match. Из файлов Player прочитаны 20 объектов `AudioClip` в `sharedassets0.assets`. У каждого объект ссылается на непустой сегмент FSB5 в `sharedassets0.resource` в пределах файла. Все 20 имеют PCM, mono, 44,1 кГц, суммарный размер сегментов 494 784 байта. Инкрементальный `BuildReport` не перечислил клипы, поэтому упаковку подтвердил разбор самого Player.

**Android Import Settings — PASS.** Все 20 настроек используют поддерживаемый формат mono PCM 44,1 кГц. Размер игровых WAV 493 144 байта. Расчётный PCM-буфер без служебных расходов 492 264 байта, около 481 КиБ. Размер APK и фактическая память на устройстве — **NOT VERIFIED**. Android Player и физическое устройство не запускались.

## Приоритет исправлений и приёмка на слух

| Приоритет | Статус | Замечание и предлагаемая проверка |
| --- | --- | --- |
| Высокий | FAIL | В `BattlePowerPresenter.Show` два роста в одном снимке дают два одинаковых клипа. Проверить один звук на направление за снимок с сохранением обоих счётчиков. Добавить регрессию для двух одинаковых и двух разных направлений |
| Средний | WARNING | Сверить `FieldRemove` с началом ухода карты и `CreatureOut` с затемнением или уходом существа. Повторить измерения на ×1 и ×2 |
| До художественного утверждения | NOT VERIFIED | Прослушать на наушниках, колонках и физическом Android. Сравнить BattleStart, BattleWin/Lose, FieldFlip, CardPlace и AbilityPlay. Проверить щелчки, тембр, комфортную громкость и слышимость карточных и UI-сигналов в плотной битве и при ускорении нажатием |

Локальные доказательства в `Logs/AudioP0QA/`: `preview-p0.wav`, `inventory.json`, `mixer-play.txt`, `ui-play.txt`, `gameplay.txt`, `gameplay-common.txt`, `packed-audio.json`, `technical-summary.json`, `windows-build.txt`. Сборка находится в `Logs/AudioP0QA/Windows/`.
