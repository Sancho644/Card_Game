# Карты поля: стиль 2

Применён общий шаблон второго концепта: арка из золота и бронзы, арт окружения, вертикальная полоса бонусов слева, отдельные панели названия и условия. Текст, стихии, числа и редкость рисуются интерфейсом из данных карты, они не запечены в изображение.

## Ассеты и настройки

- `Assets/Art/Cards/Fields/field-frame-v1.png` — общая рамка с прозрачным окном арта.
- `Assets/Art/Cards/Fields/bonus-panel-v1.png` — отдельная панель модификатора; число панелей соответствует каталогу (2–4).
- `Assets/Art/Cards/Fields/forge-art-v1.png` — первый готовый арт, подключён к `field.forge`.
- `Assets/Data/Config/FieldCardStyle.asset` — рамка, панель, цвет текста, нормализованные области элементов и размер в мире. Подключён к `VisualSettings`.

Для всех 16 полей готовы индивидуальные арты окружений. Пятнадцать новых изображений дополняют «Кузницу», «Вечный маяк» использует v2 с поправленной композицией под арку. [Полный список файлов и запросы ImageGen](field-art-prompts.md). Арт подключается полем Art нужного FieldDef без изменений кода.

## Отображение

`FieldCardFaceView` используется в коллекции, в руке и на столе, а также в крупном просмотре в битве. `CardFaceBuilder` переносит модификаторы отдельно от строки Stats. Название стихии и число модификатора находятся в отдельных однострочных областях: при четырёх стихиях число не вытесняется названием. Заголовок условия отделяется от основного текста по первому двоеточию; текст каталога сохранён. При отмене условия заголовок и текст перечёркиваются, а модификаторы остаются видимыми. Рубашка и видимость скрытых карт сохранены. На 3D-карте остаётся непрозрачная подложка, чтобы соседние карты корректно перекрывались. Подписи стопок в TacticalHudView рисуются в мировом Canvas с сохранением размера на экране, поэтому карта перекрывает их по глубине, в том числе при подъёме из руки.

## Проверка

- 287 EditMode-тестов, включая перенос всех модификаторов каталога, отсутствие данных у скрытой карты и видимость четырёх подписанных чисел при высоте карты 160 и 800.
- Play Mode: все 16 полей — число модификаторов, перечёркивание и восстановление условия, затемнение, непрозрачная подложка, скрытие лицевой стороны неизвестной карты.
- Крупный просмотр коллекции 1920×1080: у всех 16 полей текст помещается целиком; минимальный размер основного текста 19,7.
- Крупный просмотр в битве: переключения поле → существо → поле → способность → поле без наложения шаблонов.
- После исправления размещения бонусов: крупный просмотр четырёх стихий в битве при 1280×720 и 2100×900 без обрезки текста; все 16 полей в коллекции при 1920×1080.
- Play Mode: подсказка первоначального выбора поля скрыта, обводка места выкладки сохранена. Увеличенная 3D-карта перекрывает подпись стопки; крупный просмотр интерфейса также перекрывает подписи.
- Консоль Unity без ошибок и предупреждений после проверок.
- Полный набор артов: все 16 ссылок уникальны, каталог проходит Validate, правильные текстуры показываются в коллекции и просмотре битвы, заглушек нет, UV находятся внутри изображения. Временная галерея всех карт использовала настоящий FieldCardFaceView; после проверки удалена из Play Mode. Повторно прошли 287 EditMode-тестов.

Проверить вручную: Коллекция → Карты поля → «Кузница» (арт и три модификатора); «Вечный маяк» (четыре модификатора и длинный текст); открытие карты поля в битве и её просмотр крупно.

## Генерация

Использован встроенный инструмент ImageGen, без CLI/API. Рамка получена редактированием выбранного концепта 2; арт и панель бонуса созданы отдельно. Файлы импортированы через Unity MCP как Sprite Single, альфа сохранена. Финальные запросы:

### Рамка

```text
Edit the supplied approved fantasy FIELD CARD concept into a reusable EMPTY FRAME sprite. Preserve the gold-bronze carved arch, dark teal stone, brown name plaque and bottom type/rarity plaques faithfully. Remove ALL text, letters, numbers, element icons, rarity gemstone and all forge illustration. Make the whole upper art window genuinely TRANSPARENT, including the former left bonus rail: the game places the art and variable-count bonus medallions there. Keep the dark teal rules tablet opaque and BLANK. Remove the small separate condition-heading plaque so the rules tablet is one large uninterrupted dark teal readable area. Keep the wide brown name plaque directly below the art blank. Portrait 7:10, straight front view, outer gold frame fills canvas edges, no margins or drop shadow, transparent outside clipped corners. Approximate vertical zones from TOP: upper arched art window 3%-60%, brown name plaque 60%-69%, dark teal rules panel 70%-92%, two footer plaques 94%-99%. No fixed bonus panels, no watermarks. Crisp painterly beveled fantasy game texture.
```

### Кузница

```text
Use case: stylized-concept. Paint ONLY a fantasy environment illustration for a card named Кузница. Ancient magical forge inside a volcanic mountain, monumental dark basalt arches and bridges, a large glowing anvil on a raised platform, molten orange lava channels, fine turquoise rune accents, cinematic warm light, clear focal anvil center-right because bonus icons will cover left side in the game. Same polished stylized painterly fantasy as illustrated gold-and-teal card game creatures. Portrait 4:5 artwork, full-bleed environment, no frame, card layout, labels, letters, numbers, UI icons, gemstones, people or foliage. High detail with bold shapes readable at small size.
```

### Панель бонуса

```text
Use case: stylized-concept. A SINGLE EMPTY portrait medallion panel for fantasy card bonus stats, front view. Clipped octagonal corners, thick beveled antique bronze-gold border, inset quiet DARK TEAL stone fill, painterly crisp highlights matching fantasy card frames. Entirely blank: no icon, gem, text, letters, numbers, runes or decoration inside center. Compact portrait proportions 3:4. Panel fills canvas to edges with no padding, fully opaque center, transparent outside corner cuts, no shadow or background.
```
