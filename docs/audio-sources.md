# Источники звуков P0

Все внешние семплы получены 20 ноября 2026 года, обработаны и сведены с оригинальными слоями Card_Game. Записи Kenney распространяются по CC0 1.0. Лицензия разрешает коммерческое использование, изменение и распространение, включая WAV в сборках игры. Атрибуция добровольная. [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/), [условия Kenney](https://kenney.nl/support).

| Набор | Автор | Лицензия | Страница оригинала и архив |
| --- | --- | --- | --- |
| Casino Audio 1.1 | Kenney Vleugels | CC0 1.0 | [Kenney](https://kenney.nl/assets/casino-audio), [архив с лицензией](https://gametech.byte888.com/vault/sfx/kenney/casino-audio.zip) |
| Impact Sounds 1.0 | Kenney | CC0 1.0 | [Kenney](https://kenney.nl/assets/impact-sounds), [архив с лицензией](https://www.nextgenrungames.com/free-assets/downloads/impact-sounds-v1.zip) |

SHA-256 скачанных архивов

- Casino Audio `f36250766ac5bc378c13708ddf12a23a8e54a3251f8d482c7536e51b5dbafa18`.
- Impact Sounds `5758f9aaeaa825b30960aa79c508c4f8c09259e7473bf09b15c916226c186a11`.

Оригинальные резонансы, тональные интервалы и шумы сделаны специально для Card_Game скриптом [render_p0.py](../AudioSources/P0/render_p0.py) на Python и NumPy. Код синтеза использует математические модели затухающих колебаний и фильтрованный шум. Модели генеративного ИИ и сторонней лицензии для этих слоёв нет. Готовые клипы смешанных источников включают исходное аудио Kenney под CC0 и слои Card_Game.

Ниже указаны исходные имена OGG и наборы. Соответствующие записи и тексты лицензий лежат в `AudioSources/P0/Foley/`. Мастер каждого клипа лежит в `AudioSources/P0/Masters/` и имеет то же имя файла, что и игровой WAV. Мастера записаны в PCM 24-bit, mono, 48 кГц. Игровые файлы в `Assets/Audio/SFX/` записаны в PCM 16-bit, mono, 44,1 кГц. Для повторного экспорта из корня проекта запусти `python AudioSources/P0/render_p0.py` с установленными NumPy и SoundFile. Скрипт также собирает файл для прослушивания и таблицы измерений в `Logs/P0/`.

| ID | Файл | Семплы Kenney | Обработка и характер звука |
| --- | --- | --- | --- |

| `sfx.card.deal` | `Cards/card-deal.wav` | `card-slide-1.ogg` (Casino) / `card-place-1.ogg` (Casino) | Скольжение, короткий взмах, лёгкая посадка. |
| `sfx.card.deal` | `Cards/card-deal-02.wav` | `card-slide-3.ogg` (Casino) / `card-place-2.ogg` (Casino) | Более сухое скольжение и высокий край карты. |
| `sfx.card.deal` | `Cards/card-deal-03.wav` | `card-slide-4.ogg` (Casino) / `card-place-3.ogg` (Casino) | Мягкий взмах и более поздний контакт. |
| `sfx.card.pick` | `Cards/card-pick.wav` | `card-slide-2.ogg` (Casino) / `card-fan-1.ogg` (Casino) | Отрыв и короткий бумажный щелчок. |
| `sfx.card.place.creature` | `Cards/card-place-creature.wav` | `card-place-1.ogg` (Casino) / `impactWood_medium_001.ogg` (Impact) | Карточный контакт и тёплое дерево. |
| `sfx.card.place.creature` | `Cards/card-place-creature-02.wav` | `card-place-4.ogg` (Casino) / `impactWood_medium_003.ogg` (Impact) | Другой удар дерева с более низким резонансом. |
| `sfx.card.place.field` | `Cards/card-place-field.wav` | `card-shove-3.ogg` (Casino) / `impactGeneric_light_002.ogg` (Impact) | Плотная посадка с каменным отзвуком. |
| `sfx.card.field.reveal` | `Cards/card-field-reveal.wav` | `card-slide-3.ogg` (Casino) / `impactMetal_light_003.ogg` (Impact) | Шелест переворота, бронзовые обертоны и вспышка. |
| `sfx.card.discard` | `Cards/card-discard.wav` | `card-slide-1.ogg` (Casino) / `card-shove-1.ogg` (Casino) / `card-place-4.ogg` (Casino) / `impactSoft_medium_001.ogg` (Impact) | Сухой шелест, приглушённая посадка на стопку. |
| `sfx.battle.start` | `Battle/battle-start.wav` | `impactPunch_heavy_002.ogg` (Impact) | Удар и медный зов из двух голосов. |
| `sfx.battle.power.up` | `Battle/battle-power-up.wav` | Оригинальные синтезированные слои | Яркие восходящие обертоны и серебряный акцент. |
| `sfx.battle.power.down` | `Battle/battle-power-down.wav` | Оригинальные синтезированные слои | Приглушённые нисходящие обертоны и рассеивание. |
| `sfx.battle.win` | `Battle/battle-win.wav` | `impactWood_medium_004.ogg` (Impact) | Два восходящих удара, верхний гармонический отклик. |
| `sfx.battle.lose` | `Battle/battle-lose.wav` | `impactSoft_medium_003.ogg` (Impact) | Мягкий удар, нисходящие фразы и тёмный отклик. |
| `sfx.battle.creature.out` | `Battle/battle-creature-out.wav` | `impactSoft_medium_000.ogg` (Impact) | Рассеивание и мягкий глухой контакт. |
| `sfx.battle.field.remove` | `Battle/battle-field-remove.wav` | `card-shove-2.ogg` (Casino) / `impactGeneric_light_003.ogg` (Impact) | Каменное скольжение, удар и короткий резонанс. |
| `sfx.turn.local` | `Battle/turn-local.wav` | Оригинальные синтезированные слои | Два ясных резонансных тона. |
| `sfx.ability.common` | `Abilities/ability-common.wav` | `impactMetal_light_002.ogg` (Impact) | Мягкий магический щелчок с яркой искрой. |
| `sfx.ability.common` | `Abilities/ability-common-02.wav` | `impactMetal_light_004.ogg` (Impact) | Мягкий магический щелчок со сдвоенной искрой. |
| `ui.click` | `UI/ui-click.wav` | `impactWood_medium_000.ogg` (Impact) | Короткий нейтральный деревянный щелчок. |
